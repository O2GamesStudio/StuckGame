using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] Button toLobbyBtn, resumeBtn;
    [SerializeField] Button soundBtn, musicBtn;
    [SerializeField] Image soundToggleImage, musicToggleImage;
    [SerializeField] Image soundHandleImage, musicHandleImage;
    [SerializeField] Sprite[] toggleImages;

    [SerializeField] bool isSoundOn = true;
    [SerializeField] bool isMusicOn = true;

    void Awake()
    {
        if (resumeBtn != null)
            resumeBtn.onClick.AddListener(ResumeOnClick);
        if (toLobbyBtn != null)
            toLobbyBtn.onClick.AddListener(ToLobbyOnClick);
        if (soundBtn != null)
            soundBtn.onClick.AddListener(SoundOnClick);
        if (musicBtn != null)
            musicBtn.onClick.AddListener(MusicOnClick);
    }

    void Start()
    {
        isSoundOn = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;

        UpdateUI();
    }

    public void UpdateUI()
    {
        UpdateSoundUI();
        UpdateMusicUI();
    }

    void UpdateSoundUI()
    {
        if (soundToggleImage != null && toggleImages != null && toggleImages.Length >= 2)
        {
            soundToggleImage.sprite = isSoundOn ? toggleImages[0] : toggleImages[1];
        }

        if (soundHandleImage != null)
        {
            RectTransform handleRect = soundHandleImage.GetComponent<RectTransform>();
            float targetX = isSoundOn ? 60f : -60f;
            handleRect.DOAnchorPosX(targetX, 0.15f).SetEase(Ease.OutCubic);

            soundHandleImage.color = isSoundOn ? new Color(0x36 / 255f, 0x26 / 255f, 0x7E / 255f) : Color.white;
        }
    }

    void UpdateMusicUI()
    {
        if (musicToggleImage != null && toggleImages != null && toggleImages.Length >= 2)
        {
            musicToggleImage.sprite = isMusicOn ? toggleImages[0] : toggleImages[1];
        }

        if (musicHandleImage != null)
        {
            RectTransform handleRect = musicHandleImage.GetComponent<RectTransform>();
            float targetX = isMusicOn ? 60f : -60f;
            handleRect.DOAnchorPosX(targetX, 0.15f).SetEase(Ease.OutCubic);

            musicHandleImage.color = isMusicOn ? new Color(0x36 / 255f, 0x26 / 255f, 0x7E / 255f) : Color.white;
        }
    }

    void SoundOnClick()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("IsSoundOn", isSoundOn ? 1 : 0);

        UpdateSoundUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSoundEnabled(isSoundOn);
            if (isSoundOn)
                SoundManager.Instance.PlayUIClickSFX();
        }
    }

    void MusicOnClick()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("IsMusicOn", isMusicOn ? 1 : 0);

        UpdateMusicUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicEnabled(isMusicOn);

            if (isMusicOn)
            {
                SoundManager.Instance.PlayGameBGM();
            }
            else
            {
                SoundManager.Instance.PauseBGM();
            }

            SoundManager.Instance.PlayUIClickSFX();
        }
    }

    void ResumeOnClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClickSFX();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }

        gameObject.SetActive(false);
    }

    void ToLobbyOnClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClickSFX();
        SceneManager.LoadScene(0);
    }
}