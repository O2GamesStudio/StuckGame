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
    [SerializeField] CircleMaskController circleMask;
    [SerializeField] int targetStuckVal = 10; // 목표 개수

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
        targetCharacter.ClearStage();
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
        yield return new WaitForSeconds(0.2f);
        FocusOnCharacter();
    }

    void FocusOnCharacter()
    {
        if (targetCharacter != null && circleMask != null)
        {
            Vector3 worldPos = targetCharacter.transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            circleMask.ShowAndFocus(screenPos, () =>
            {
                Debug.Log("포커스 애니메이션 완료!");
            });
        }
    }
}