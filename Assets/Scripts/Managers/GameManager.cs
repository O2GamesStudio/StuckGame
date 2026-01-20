using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Objects")]
    private StuckObj currentStuckObjPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] TargetCtrl targetCharacter;

    [Header("Game Settings")]
    [SerializeField] float throwForce = 10f;
    [SerializeField] float spawnDelay = 0.5f;
    [SerializeField] float stageTransitionDelay = 1f;
    [SerializeField] float gameOverDelay = 0.5f;

    [Header("Chapter & Stage")]
    [SerializeField] ChapterData currentChapter;
    [SerializeField] int currentStageIndex = 0;

    [Header("Target Points")]
    [SerializeField] TargetPointManager targetPointManager;

    [Header("Spawn Settings")]
    [SerializeField] float minDistanceFromTargetPoint = 30f;

    private StuckObj currentKnife;
    private bool isGameOver = false;
    private bool isGameActive = false;
    private List<StuckObj> allKnives = new List<StuckObj>();
    private int stuckAmount = 0;
    private int targetStuckVal = 10;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        isGameActive = true;
        InitializeStage();
        SpawnNewKnife();
        UpdateUI();

        // Load and show banner ad when game starts (in-game only)
        if (GoogleAdmobManager.Instance != null)
        {
            GoogleAdmobManager.Instance.LoadBannerAd();
        }
    }

    public void SetGameActive(bool active) => isGameActive = active;

    void InitializeStage()
    {
        if (currentChapter == null)
        {
            Debug.LogError("Chapter Data is not assigned!");
            return;
        }

        ChapterData.StageSettings stageSettings = currentChapter.GetStageSettings(currentStageIndex);
        targetStuckVal = stageSettings.requiredKnives;

        currentStuckObjPrefab = stageSettings.stuckObjPrefab;

        if (currentStuckObjPrefab == null)
        {
            Debug.LogError($"StuckObj Prefab is not assigned in Stage {currentStageIndex + 1}!");
            return;
        }

        ApplyStageVisuals(stageSettings);

        if (targetCharacter != null)
        {
            targetCharacter.InitializeStage(stageSettings);
        }

        stuckAmount = 0;
        isGameOver = false;

        List<float> occupiedAngles = SpawnObstacles(stageSettings.obstacleCount);

        if (targetPointManager != null)
        {
            targetPointManager.SetTargetCharacter(targetCharacter);
            targetPointManager.InitializeTargetPoints(stageSettings.targetPointCount, occupiedAngles);

            // Initialize target point UI
            if (UIManager.Instance != null && stageSettings.targetPointCount > 0)
            {
                UIManager.Instance.InitializeTargetPointUI(stageSettings.targetPointCount);
            }
        }

        UpdateStageText();
    }

    void ApplyStageVisuals(ChapterData.StageSettings stageSettings)
    {
        if (targetCharacter != null && stageSettings.targetImage != null)
        {
            SpriteRenderer targetRenderer = targetCharacter.GetComponent<SpriteRenderer>();
            if (targetRenderer != null)
            {
                targetRenderer.sprite = stageSettings.targetImage;
            }
        }
        if (UIManager.Instance != null && stageSettings.bgImage != null)
        {
            UIManager.Instance.UpdateBgImage(stageSettings.bgImage);
        }
    }

    public void OnTargetPointCompleted()
    {
        // Remove one target point icon from UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveTargetPointIcon();
        }
    }

    public List<StuckObj> GetAllKnives()
    {
        return allKnives;
    }

    public void OnKnifeCollision()
    {
        isGameActive = false;

        if (targetCharacter != null)
        {
            targetCharacter.StopRotationOnly();
            targetCharacter.OnGameOverHit();
        }
        StartCoroutine(GameOverAfterDelay());
    }

    IEnumerator GameOverAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);
        GameOver();
    }

    List<float> SpawnObstacles(int count)
    {
        List<float> usedAngles = new List<float>();

        if (count <= 0 || targetCharacter == null) return usedAngles;

        CircleCollider2D targetCollider = targetCharacter.GetComponent<CircleCollider2D>();
        float targetRadius = 1f;

        if (targetCollider != null)
        {
            targetRadius = targetCollider.radius * targetCharacter.transform.localScale.x;
        }

        float stickOffset = currentStuckObjPrefab.GetTargetStickOffset();
        float minAngleGap = 30f;

        for (int i = 0; i < count; i++)
        {
            float angle = 0f;
            bool validAngle = false;
            int maxAttempts = 100;
            int attempts = 0;

            while (!validAngle && attempts < maxAttempts)
            {
                angle = Random.Range(0f, 360f);
                validAngle = true;

                foreach (float usedAngle in usedAngles)
                {
                    float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angle, usedAngle));
                    if (angleDiff < minAngleGap)
                    {
                        validAngle = false;
                        break;
                    }
                }

                attempts++;
            }

            usedAngles.Add(angle);

            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
            Vector3 spawnPosition = targetCharacter.transform.position + direction * (targetRadius + stickOffset);

            StuckObj obstacle = LeanPool.Spawn(currentStuckObjPrefab, spawnPosition, Quaternion.identity);

            obstacle.transform.SetParent(targetCharacter.transform);

            Vector3 localDir = obstacle.transform.parent.InverseTransformDirection(direction);
            float localAngle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg - 90f;
            obstacle.transform.localRotation = Quaternion.Euler(0, 0, localAngle + 180f);

            obstacle.SetupAsObstacle();
            obstacle.StickAsObstacle(targetCharacter.transform);

            allKnives.Add(obstacle);
        }

        return usedAngles;
    }

    List<float> GetTargetPointAngles()
    {
        List<float> angles = new List<float>();

        if (targetCharacter == null) return angles;

        TargetPoint[] targetPoints = targetCharacter.GetComponentsInChildren<TargetPoint>();

        foreach (TargetPoint point in targetPoints)
        {
            Vector3 localPos = point.transform.localPosition;
            float angle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg - 90f;
            if (angle < 0) angle += 360f;
            angles.Add(angle);
        }

        return angles;
    }

    void UpdateStageText()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStageText(currentStageIndex + 1);
        }
    }

    void SpawnNewKnife()
    {
        if (isGameOver || !isGameActive || currentStuckObjPrefab == null || spawnPoint == null)
        {
            return;
        }

        currentKnife = LeanPool.Spawn(currentStuckObjPrefab, spawnPoint.position, Quaternion.identity);

        if (currentKnife == null) return;

        allKnives.Add(currentKnife);
    }

    public void OnClick()
    {
        if (currentKnife != null && !isGameOver && isGameActive)
        {
            currentKnife.Throw(throwForce);
            currentKnife = null;

            StartCoroutine(SpawnKnifeAfterDelay());
        }
    }

    IEnumerator SpawnKnifeAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnNewKnife();
    }

    public void OnKnifeStuck()
    {
        stuckAmount++;
        UpdateUI();

        if (stuckAmount >= targetStuckVal)
        {
            StageComplete();
        }
    }

    void StageComplete()
    {
        if (targetPointManager != null && targetPointManager.GetRequiredCount() > 0)
        {
            if (!targetPointManager.AreAllPointsCompleted())
            {
                return;
            }
        }

        isGameActive = false;

        if (currentKnife != null)
        {
            LeanPool.Despawn(currentKnife);
            currentKnife = null;
        }

        if (targetCharacter != null)
        {
            targetCharacter.StopRotation();
        }

        StartCoroutine(TransitionToNextStage());
    }

    IEnumerator TransitionToNextStage()
    {
        yield return new WaitForSeconds(stageTransitionDelay);

        currentStageIndex++;

        if (currentStageIndex >= currentChapter.TotalStages)
        {
            ChapterComplete();
            yield break;
        }

        LoadNextStage();
    }

    void LoadNextStage()
    {
        ClearAllKnives();

        isGameActive = true;

        InitializeStage();
        SpawnNewKnife();
        UpdateUI();
    }

    void ClearAllKnives()
    {
        foreach (var knife in allKnives)
        {
            if (knife != null)
            {
                knife.transform.SetParent(null);
                LeanPool.Despawn(knife);
            }
        }
        allKnives.Clear();

        if (targetCharacter != null)
        {
            StuckObj[] remainingKnives = targetCharacter.GetComponentsInChildren<StuckObj>();

            foreach (StuckObj knife in remainingKnives)
            {
                if (knife != null)
                {
                    knife.transform.SetParent(null);
                    LeanPool.Despawn(knife);
                }
            }
        }

        if (targetPointManager != null)
        {
            targetPointManager.ClearAllPoints();
        }

        // Clear target point UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClearTargetPointUI();
        }
    }

    void ChapterComplete()
    {
        isGameOver = true;

        if (targetCharacter != null)
        {
            targetCharacter.ClearStage();
        }

        FocusOnCharacterWin();
    }

    void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TargetUIUpdate(targetStuckVal, stuckAmount);
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        isGameActive = false;

        DisableKnifeCollisions();

        if (targetCharacter != null)
        {
            targetCharacter.ExplodeKnives();
        }

        StartCoroutine(FocusAfterExplosion());
    }

    void DisableKnifeCollisions()
    {
        allKnives.RemoveAll(knife => knife == null);

        for (int i = 0; i < allKnives.Count; i++)
        {
            for (int j = i + 1; j < allKnives.Count; j++)
            {
                if (allKnives[i] != null && allKnives[j] != null)
                {
                    Collider2D col1 = allKnives[i].GetCollider();
                    Collider2D col2 = allKnives[j].GetCollider();

                    if (col1 != null && col2 != null)
                    {
                        Physics2D.IgnoreCollision(col1, col2, true);
                    }
                }
            }
        }
    }

    IEnumerator FocusAfterExplosion()
    {
        yield return null;
        FocusOnCharacterLose();
    }

    void FocusOnCharacterWin()
    {
        if (targetCharacter != null && UIManager.Instance.circleMask != null)
        {
            Vector3 worldPos = targetCharacter.transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            UIManager.Instance.circleMask.ShowAndFocus(screenPos, () =>
            {
                UIManager.Instance.ShowWinUI();
            });
        }
    }

    void FocusOnCharacterLose()
    {
        if (targetCharacter != null && UIManager.Instance.circleMask != null)
        {
            Vector3 worldPos = targetCharacter.transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            UIManager.Instance.circleMask.ShowAndFocus(screenPos, () =>
            {
                UIManager.Instance.ShowLoseUI();
            });
        }
    }

    /// <summary>
    /// Continue game after watching rewarded ad
    /// Called from UIManager's ContinueOnClick
    /// </summary>
    public void OnContinueButtonPressed()
    {
        if (GoogleAdmobManager.Instance != null)
        {
            // Check if rewarded ad is ready
            if (GoogleAdmobManager.Instance.IsRewardedAdReady())
            {
                // Show rewarded ad
                GoogleAdmobManager.Instance.ShowRewardedAd(
                    onCompleted: () =>
                    {
                        // Ad watched successfully - continue game
                        Debug.Log("Rewarded ad watched - continuing game");
                        ContinueGameAfterAd();
                    },
                    onFailed: () =>
                    {
                        // Ad failed or user closed early
                        Debug.Log("Rewarded ad failed or closed early");
                        // Optionally show a message to the user
                        if (UIManager.Instance != null)
                        {
                            Debug.Log("Please watch the ad to continue");
                        }
                    }
                );
            }
            else
            {
                // Ad not ready - optionally allow continue anyway or show message
                Debug.LogWarning("Rewarded ad not ready yet");

                // Option 1: Allow continue anyway (generous to player)
                ContinueGameAfterAd();

                // Option 2: Show message and don't allow continue (uncomment if preferred)
                // if (UIManager.Instance != null)
                // {
                //     Debug.Log("Ad not ready, please try again");
                // }
            }
        }
        else
        {
            // AdMob Manager not available - allow continue anyway
            Debug.LogWarning("GoogleAdmobManager not found - continuing without ad");
            ContinueGameAfterAd();
        }
    }

    private void ContinueGameAfterAd()
    {
        // UI 정리
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ContinueGameUI();
        }

        ClearAllKnives();

        isGameOver = false;
        isGameActive = true;
        stuckAmount = 0;

        if (currentChapter == null) return;

        ChapterData.StageSettings stageSettings = currentChapter.GetStageSettings(currentStageIndex);

        currentStuckObjPrefab = stageSettings.stuckObjPrefab;

        if (currentStuckObjPrefab == null) return;

        if (targetCharacter != null)
        {
            targetCharacter.InitializeStage(stageSettings);
        }

        List<float> occupiedAngles = SpawnObstacles(stageSettings.obstacleCount);

        if (targetPointManager != null)
        {
            targetPointManager.SetTargetCharacter(targetCharacter);
            targetPointManager.InitializeTargetPoints(stageSettings.targetPointCount, occupiedAngles);
        }

        SpawnNewKnife();
        UpdateUI();
    }

    public void RestartStage()
    {
        DOTween.KillAll();

        ClearAllKnives();

        isGameActive = true;
        isGameOver = false;

        InitializeStage();
        SpawnNewKnife();
        UpdateUI();
    }

    public void RestartChapter()
    {
        currentStageIndex = 0;
        RestartStage();
    }
}