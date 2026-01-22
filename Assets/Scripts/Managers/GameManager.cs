using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Pool;
using UnityEngine.InputSystem;

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
    ChapterData currentChapter;
    [SerializeField] int currentStageIndex = 0;

    [Header("Target Points")]
    [SerializeField] TargetPointManager targetPointManager;

    [Header("Spawn Settings")]
    [SerializeField] float minDistanceFromTargetPoint = 30f;

    private StuckObj currentKnife;
    private bool isGameOver = false;
    private bool isGameActive = false;
    private List<StuckObj> allKnives = new List<StuckObj>(50);
    private int stuckAmount = 0;
    private int targetStuckVal = 10;
    private CircleCollider2D targetCollider;
    private WaitForSeconds spawnWait;
    private WaitForSeconds transitionWait;
    private WaitForSeconds gameOverWait;
    private List<float> occupiedAngles = new List<float>(20);
    private Camera mainCam;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        mainCam = Camera.main;
        spawnWait = new WaitForSeconds(spawnDelay);
        transitionWait = new WaitForSeconds(stageTransitionDelay);
        gameOverWait = new WaitForSeconds(gameOverDelay);

        if (LobbyManager.SelectedChapter != null)
        {
            currentChapter = LobbyManager.SelectedChapter;
        }
    }

    void Start()
    {
        isGameActive = true;
        InitializeStage();
        SpawnNewKnife();
        UpdateUI();

        if (GoogleAdmobManager.Instance != null)
        {
            GoogleAdmobManager.Instance.LoadBannerAd();
        }
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SkipToNextStage();
        }
    }

    void SkipToNextStage()
    {
        if (isGameOver || !isGameActive) return;

        StopAllCoroutines();

        if (currentKnife != null)
        {
            LeanPool.Despawn(currentKnife);
            currentKnife = null;
        }

        if (targetCharacter != null)
        {
            targetCharacter.StopRotationOnly();
        }

        StartCoroutine(TransitionToNextStage());
    }

    public void SetGameActive(bool active) => isGameActive = active;

    void InitializeStage()
    {
        if (currentChapter == null) return;

        ChapterData.StageSettings stageSettings = currentChapter.GetStageSettings(currentStageIndex);
        targetStuckVal = stageSettings.requiredKnives;
        currentStuckObjPrefab = stageSettings.stuckObjPrefab;

        if (currentStuckObjPrefab == null) return;

        ApplyStageVisuals(stageSettings);

        if (targetCharacter != null)
        {
            targetCharacter.InitializeStage(stageSettings);
            if (targetCollider == null)
            {
                targetCollider = targetCharacter.GetComponent<CircleCollider2D>();
            }
        }

        stuckAmount = 0;
        isGameOver = false;

        occupiedAngles.Clear();
        SpawnObstacles(stageSettings.obstacleCount);

        if (targetPointManager != null)
        {
            targetPointManager.SetTargetCharacter(targetCharacter);
            targetPointManager.InitializeTargetPoints(stageSettings.targetPointCount, occupiedAngles);

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
            targetCharacter.GetComponent<SpriteRenderer>().sprite = stageSettings.targetImage;
        }
        if (UIManager.Instance != null && stageSettings.bgImage != null)
        {
            UIManager.Instance.UpdateBgImage(stageSettings.bgImage);
        }
    }

    public void OnTargetPointCompleted()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveTargetPointIcon();
        }
    }

    public List<StuckObj> GetAllKnives() => allKnives;

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
        yield return gameOverWait;
        GameOver();
    }

    void SpawnObstacles(int count)
    {
        if (count <= 0 || targetCharacter == null) return;

        float targetRadius = targetCollider != null
            ? targetCollider.radius * targetCharacter.transform.localScale.x
            : 1f;

        float stickOffset = currentStuckObjPrefab.GetTargetStickOffset();
        const float minAngleGap = 30f;
        const int maxAttempts = 100;

        for (int i = 0; i < count; i++)
        {
            float angle = FindValidAngle(minAngleGap, maxAttempts);
            occupiedAngles.Add(angle);

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * Vector3.up;
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
    }

    float FindValidAngle(float minGap, int maxAttempts)
    {
        float angle = Random.Range(0f, 360f);
        int count = occupiedAngles.Count;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            bool valid = true;

            for (int i = 0; i < count; i++)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(angle, occupiedAngles[i])) < minGap)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return angle;
            angle = Random.Range(0f, 360f);
        }

        return angle;
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
        if (isGameOver || !isGameActive || currentStuckObjPrefab == null || spawnPoint == null) return;

        currentKnife = LeanPool.Spawn(currentStuckObjPrefab, spawnPoint.position, Quaternion.identity);
        if (currentKnife != null) allKnives.Add(currentKnife);
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
        yield return spawnWait;
        SpawnNewKnife();
    }

    public void OnKnifeStuck(StuckObj knife)
    {
        if (isGameOver) return;

        stuckAmount++;
        UpdateUI();

        if (stuckAmount >= targetStuckVal)
        {
            if (targetPointManager != null && !targetPointManager.AreAllPointsCompleted())
            {
                return;
            }
            StageComplete();
        }
    }

    void StageComplete()
    {
        isGameActive = false;

        if (currentKnife != null)
        {
            LeanPool.Despawn(currentKnife);
            currentKnife = null;
        }

        if (targetCharacter != null)
        {
            targetCharacter.StopRotationOnly();
        }

        StartCoroutine(TransitionToNextStage());
    }

    IEnumerator TransitionToNextStage()
    {
        currentStageIndex++;

        if (currentStageIndex >= currentChapter.TotalStages)
        {
            ChapterComplete();
            yield break;
        }

        if (targetCharacter != null)
        {
            targetCharacter.TransitionToNextStage();
        }

        yield return new WaitForSeconds(0.15f);

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
        for (int i = allKnives.Count - 1; i >= 0; i--)
        {
            if (allKnives[i] != null)
            {
                allKnives[i].transform.SetParent(null);
                LeanPool.Despawn(allKnives[i]);
            }
        }
        allKnives.Clear();

        if (targetCharacter != null)
        {
            StuckObj[] remainingKnives = targetCharacter.GetComponentsInChildren<StuckObj>();
            for (int i = 0; i < remainingKnives.Length; i++)
            {
                if (remainingKnives[i] != null)
                {
                    remainingKnives[i].transform.SetParent(null);
                    LeanPool.Despawn(remainingKnives[i]);
                }
            }
        }

        if (targetPointManager != null)
        {
            targetPointManager.ClearAllPoints();
        }

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
            targetCharacter.StopRotation();
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
        int count = allKnives.Count;

        for (int i = 0; i < count - 1; i++)
        {
            Collider2D col1 = allKnives[i].GetCollider();
            if (col1 == null) continue;

            for (int j = i + 1; j < count; j++)
            {
                Collider2D col2 = allKnives[j].GetCollider();
                if (col2 != null)
                {
                    Physics2D.IgnoreCollision(col1, col2, true);
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
            Vector2 screenPos = mainCam.WorldToScreenPoint(targetCharacter.transform.position);
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
            Vector2 screenPos = mainCam.WorldToScreenPoint(targetCharacter.transform.position);
            UIManager.Instance.circleMask.ShowAndFocus(screenPos, () =>
            {
                UIManager.Instance.ShowLoseUI();
            });
        }
    }

    public void OnContinueButtonPressed()
    {
        if (GoogleAdmobManager.Instance != null)
        {
            if (GoogleAdmobManager.Instance.IsRewardedAdReady())
            {
                GoogleAdmobManager.Instance.ShowRewardedAd(
                    onCompleted: ContinueGameAfterAd,
                    onFailed: null
                );
            }
            else
            {
                ContinueGameAfterAd();
            }
        }
        else
        {
            ContinueGameAfterAd();
        }
    }

    void ContinueGameAfterAd()
    {
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

        occupiedAngles.Clear();
        SpawnObstacles(stageSettings.obstacleCount);

        if (targetPointManager != null)
        {
            targetPointManager.SetTargetCharacter(targetCharacter);
            targetPointManager.InitializeTargetPoints(stageSettings.targetPointCount, occupiedAngles);

            if (UIManager.Instance != null && stageSettings.targetPointCount > 0)
            {
                UIManager.Instance.InitializeTargetPointUI(stageSettings.targetPointCount);
            }
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