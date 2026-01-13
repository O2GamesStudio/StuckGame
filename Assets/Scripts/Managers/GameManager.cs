using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] StuckObj stuckObjPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float throwForce = 10f;
    [SerializeField] float spawnDelay = 0.5f;
    [SerializeField] TargetCtrl targetCharacter; // 캐릭터 참조
    [SerializeField] CircleMaskController circleMask; // 마스크 참조

    private StuckObj currentKnife;
    private bool isGameOver = false;

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
    }

    void SpawnNewKnife()
    {
        if (isGameOver) return;

        currentKnife = Instantiate(stuckObjPrefab, spawnPoint.position, Quaternion.identity);
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

    public void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");

        FocusOnCharacter();
    }

    void FocusOnCharacter()
    {
        if (targetCharacter != null && circleMask != null)
        {
            // 월드 좌표 → 스크린 좌표 변환
            Vector3 worldPos = targetCharacter.transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // 활성화 + 포커스 애니메이션 (한 번에)
            circleMask.ShowAndFocus(screenPos, () =>
            {
                Debug.Log("포커스 애니메이션 완료!");
                // 여기서 게임오버 UI 표시
            });
        }
    }
}