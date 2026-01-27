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

    [SerializeField] Button startBtn;
    [SerializeField] Button nextBtn, prevBtn;
    [SerializeField] TextMeshProUGUI highestStageText;
    [SerializeField] TextMeshProUGUI modeText;
    [SerializeField] SettingPanel settingPanel;
    [SerializeField] ChapterData[] chapterDatas;
    [SerializeField] Image characterImage;

    [Header("Character Animation")]
    [SerializeField] float scaleYUp = 1.05f;
    [SerializeField] float scaleXDown = 0.95f;
    [SerializeField] float scaleYDown = 0.95f;
    [SerializeField] float scaleXUp = 1.05f;
    [SerializeField] float scaleDuration = 0.3f;
    [SerializeField] float delayBetweenLoops = 1f;

    private Vector3 originalScale;

    void Awake()
    {
        AllChapters = chapterDatas;

        startBtn.onClick.AddListener(StartOnClick);
        if (nextBtn != null) nextBtn.onClick.AddListener(NextModeOnClick);
        if (prevBtn != null) prevBtn.onClick.AddListener(PrevModeOnClick);

        UpdateHighestStageText();
        UpdateModeUI();

        if (characterImage != null)
        {
            originalScale = characterImage.transform.localScale;
            StartCharacterAnimation();
        }
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(0);
        }
    }

    void StartCharacterAnimation()
    {
        Sequence bounceSequence = DOTween.Sequence();

        bounceSequence.Append(characterImage.transform.DOScale(new Vector3(scaleXDown, scaleYUp, 1f), scaleDuration).SetEase(Ease.InOutSine));
        bounceSequence.Append(characterImage.transform.DOScale(new Vector3(scaleXUp, scaleYDown, 1f), scaleDuration).SetEase(Ease.InOutSine));
        bounceSequence.Append(characterImage.transform.DOScale(originalScale, scaleDuration).SetEase(Ease.InOutSine));
        bounceSequence.AppendInterval(delayBetweenLoops);

        bounceSequence.SetLoops(-1, LoopType.Restart);
    }

    void UpdateHighestStageText()
    {
        if (highestStageText == null) return;

        if (SelectedGameMode == GameMode.Story)
        {
            int savedStage = PlayerPrefs.GetInt("HighestStage", 1);
            highestStageText.text = $"Stage {savedStage}";
        }
        else
        {
            int highestInfinite = PlayerPrefs.GetInt("HighestInfinite", 0);
            highestStageText.text = $"{highestInfinite}";
        }
    }

    void NextModeOnClick()
    {
        if (SelectedGameMode == GameMode.Story)
        {
            SelectedGameMode = GameMode.Infinite;
            UpdateModeUI();
        }
    }

    void PrevModeOnClick()
    {
        if (SelectedGameMode == GameMode.Infinite)
        {
            SelectedGameMode = GameMode.Story;
            UpdateModeUI();
        }
    }

    void UpdateModeUI()
    {
        if (prevBtn != null)
        {
            prevBtn.interactable = SelectedGameMode != GameMode.Story;
        }

        if (nextBtn != null)
        {
            nextBtn.interactable = SelectedGameMode != GameMode.Infinite;
        }

        if (modeText != null)
        {
            modeText.text = SelectedGameMode == GameMode.Story ? "Normal" : "Challenge";
        }

        UpdateHighestStageText();
    }

    void StartOnClick()
    {
        SceneLoader.LoadGameScenes(1, 2);
    }
}