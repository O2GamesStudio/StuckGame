using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] StuckObj stuckObjPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float throwForce = 10f;
    [SerializeField] float spawnDelay = 0.5f;

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

    }
}