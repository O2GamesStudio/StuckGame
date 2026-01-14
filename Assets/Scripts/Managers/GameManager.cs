using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] StuckObj stuckObjPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float throwForce = 10f;
    [SerializeField] float spawnDelay = 0.5f;
    [SerializeField] TargetCtrl targetCharacter;
    [SerializeField] int targetStuckVal = 10;
    [SerializeField] bool isInfiniteMode = false;

    private StuckObj currentKnife;
    private bool isGameOver = false;
    private List<StuckObj> allKnives = new List<StuckObj>();
    private int stuckAmount = 0;

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
        SpawnNewKnife();
        UpdateUI();
    }

    void SpawnNewKnife()
    {
        if (isGameOver) return;

        currentKnife = Instantiate(stuckObjPrefab, spawnPoint.position, Quaternion.identity);
        allKnives.Add(currentKnife);
    }

    public void OnClick()
    {
        if (currentKnife != null && !isGameOver)
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
            ClearStage();
        }
    }

    public void ClearStage()
    {
        Debug.Log("Stage Clear!");
        isGameOver = true;

        // 발사 대기 중인 칼 제거
        if (currentKnife != null)
        {
            Destroy(currentKnife.gameObject);
            currentKnife = null;
        }

        targetCharacter.ClearStage();
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
}