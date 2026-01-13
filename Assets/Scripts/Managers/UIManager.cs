using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] Button screenBtn;

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

        screenBtn.onClick.AddListener(ScreenOnClick);
    }

    void ScreenOnClick()
    {
        GameManager.Instance.OnClick();
    }
}