using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Objects")]
    [SerializeField] StuckObj stuckObjPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] TargetCtrl targetCharacter;

    [Header("Game Settings")]
    [SerializeField] float throwForce = 10f;
    [SerializeField] float spawnDelay = 0.5f;
    [SerializeField] float stageTransitionDelay = 1f; // 스테이지 전환 딜레이

    [Header("Chapter & Stage")]
    [SerializeField] ChapterData currentChapter;
    [SerializeField] int currentStageIndex = 0; // 0~9 (스테이지 1~10)

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
    }

    void Start()
    {
        isGameActive = true; // 먼저 활성화
        InitializeStage();
        SpawnNewKnife();
        UpdateUI();
    }
    void InitializeStage()
    {
        if (currentChapter == null)
        {
            Debug.LogError("Chapter Data is not assigned!");
            return;
        }

        // 현재 스테이지 설정 가져오기
        ChapterData.StageSettings stageSettings = currentChapter.GetStageSettings(currentStageIndex);

        // 목표 칼 개수 설정
        targetStuckVal = stageSettings.requiredKnives;

        // 타겟 캐릭터 초기화
        if (targetCharacter != null)
        {
            targetCharacter.InitializeStage(stageSettings);
        }

        // 게임 상태 초기화
        stuckAmount = 0;
        isGameOver = false;

        // UI 스테이지 텍스트 업데이트 추가
        UpdateStageText();
    }

    // GameManager.cs - 새로운 메서드 추가
    void UpdateStageText()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStageText(currentStageIndex + 1);
        }
    }

    void SpawnNewKnife()
    {
        if (isGameOver || !isGameActive)
        {
            return;
        }

        currentKnife = Instantiate(stuckObjPrefab, spawnPoint.position, Quaternion.identity);
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
        isGameActive = false;

        if (currentKnife != null)
        {
            Destroy(currentKnife.gameObject);
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

        // 챕터의 모든 스테이지를 완료한 경우
        if (currentStageIndex >= currentChapter.TotalStages)
        {
            ChapterComplete();
            yield break;
        }

        // 다음 스테이지 로드
        LoadNextStage();
    }

    void LoadNextStage()
    {
        // 모든 칼 제거
        ClearAllKnives();

        // 타겟에 박혀있는 칼들도 제거
        StuckObj[] stuckKnives = targetCharacter.GetComponentsInChildren<StuckObj>();
        foreach (StuckObj knife in stuckKnives)
        {
            if (knife != null)
            {
                Destroy(knife.gameObject);
            }
        }

        // 게임 활성화
        isGameActive = true;

        // 새 스테이지 초기화
        InitializeStage();
        SpawnNewKnife();
        UpdateUI();
    }

    void ClearAllKnives()
    {
        // 리스트에 있는 모든 칼 제거
        foreach (var knife in allKnives)
        {
            if (knife != null)
            {
                Destroy(knife.gameObject);
            }
        }
        allKnives.Clear();
    }

    void ChapterComplete()
    {
        Debug.Log($"Chapter {currentChapter.ChapterNumber} Complete!");
        isGameOver = true;

        // 챕터 완료 처리
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

    public void RestartStage()
    {
        // DOTween 애니메이션 정리
        DOTween.KillAll();

        ClearAllKnives();

        isGameActive = true;

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