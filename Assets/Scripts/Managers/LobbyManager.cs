using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] Button startBtn, infiniteModeBtn;


    void Awake()
    {
        startBtn.onClick.AddListener(StartOnClick);
        infiniteModeBtn.onClick.AddListener(InfiniteModeOnClick);
    }
    void StartOnClick()
    {
        SceneManager.LoadScene(1);
    }
    void InfiniteModeOnClick()
    {

    }
}
