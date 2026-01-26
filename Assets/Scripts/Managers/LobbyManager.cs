// LobbyManager.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public enum GameMode { Story, Infinite }

    public static GameMode SelectedGameMode { get; private set; } = GameMode.Story;
    public static ChapterData[] AllChapters { get; private set; }

    [SerializeField] Button startBtn, infiniteModeBtn;
    [SerializeField] TextMeshProUGUI highestStageText;
    [SerializeField] ChapterData[] chapterDatas;

    void Awake()
    {
        AllChapters = chapterDatas;

        startBtn.onClick.AddListener(StartOnClick);
        infiniteModeBtn.onClick.AddListener(InfiniteModeOnClick);

        UpdateHighestStageText();
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(0);
        }
    }

    void UpdateHighestStageText()
    {
        if (highestStageText == null) return;

        int savedStage = PlayerPrefs.GetInt("HighestStage", 1);
        highestStageText.text = $"Stage {savedStage}";
    }

    void StartOnClick()
    {
        SelectedGameMode = GameMode.Story;
        SceneLoader.LoadGameScenes(1, 2);
    }

    void InfiniteModeOnClick()
    {
        SelectedGameMode = GameMode.Infinite;
        SceneLoader.LoadGameScenes(1, 2);
    }
}