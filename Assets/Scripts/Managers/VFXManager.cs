using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX Prefabs")]
    [SerializeField] GameObject gameOverVFXPrefab;

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

    public GameObject GetGameOverVFX()
    {
        return gameOverVFXPrefab;
    }
}