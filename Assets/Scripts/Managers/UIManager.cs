using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] Button retryBtn, exitBtn, continueBtn, settingBtn;
    [SerializeField] Button nextBtn, exitWinBtn;
    [SerializeField] Button screenBtn;
    [SerializeField] Image bgImage;
    [SerializeField] Image targetImage;
    [SerializeField] SettingPanel settingPanel;
    [SerializeField] StageUISet stageUISet;
    [SerializeField] TextMeshProUGUI targetText;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] float fillDuration = 0.3f;
    [SerializeField] Ease fillEase = Ease.OutQuad;

    [Header("Button Animation Settings")]
    [SerializeField] float buttonMoveDistance = 300f;
    [SerializeField] float buttonMoveDuration = 0.4f;
    [SerializeField] Ease buttonMoveEase = Ease.OutCubic;
    [SerializeField] float bounceUpAmount = 20f;
    [SerializeField] float bounceDownAmount = 10f;
    [SerializeField] float bounceDuration = 0.15f;

    [Header("Scale Animation Settings")]
    [SerializeField] float scaleUpValue = 1.2f;
    [SerializeField] float scaleUpDuration = 0.2f;
    [SerializeField] float scaleDownDuration = 0.15f;
    [SerializeField] float exitButtonDelay = 0.3f;
    [SerializeField] float exitButtonScaleDuration = 0.5f;

    [Header("Target Point UI")]
    [SerializeField] GameObject targetPointIconPrefab;
    [SerializeField] Transform targetPointIconContainer;

    [Header("Mode Settings")]
    private bool isInfiniteMode = false;
    [SerializeField] TextMeshProUGUI infiniteCountText;
    [SerializeField] GameObject[] chapterModeObjects;
    [SerializeField] GameObject[] infiniteModeObjects;

    private List<GameObject> targetPointIcons = new List<GameObject>(10);

    public CircleMaskController circleMask;

    private Vector3 nextBtnHiddenPos, exitWinBtnHiddenPos;
    private Vector3 nextBtnTargetPos, exitWinBtnTargetPos;
    private static readonly Vector3 zeroScale = Vector3.zero;
    private static readonly Vector3 oneScale = Vector3.one;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        settingBtn.onClick.AddListener(SettingOnClick);
        screenBtn.onClick.AddListener(ScreenOnClick);
        exitBtn.onClick.AddListener(ExitOnClick);
        retryBtn.onClick.AddListener(RetryOnClick);
        continueBtn.onClick.AddListener(ContinueOnClick);
        exitWinBtn.onClick.AddListener(ExitOnClick);
        nextBtn.onClick.AddListener(NextOnClick);

        nextBtnHiddenPos = nextBtn.transform.localPosition;
        exitWinBtnHiddenPos = exitWinBtn.transform.localPosition;

        nextBtnTargetPos = nextBtnHiddenPos + new Vector3(0, buttonMoveDistance, 0);
        exitWinBtnTargetPos = exitWinBtnHiddenPos + new Vector3(0, buttonMoveDistance, 0);

        retryBtn.transform.localScale = zeroScale;
        exitBtn.transform.localScale = zeroScale;
        continueBtn.transform.localScale = zeroScale;
        nextBtn.transform.localScale = zeroScale;
        exitWinBtn.transform.localScale = zeroScale;
    }


    public void SetInfiniteMode(bool infinite)
    {
        isInfiniteMode = infinite;

        if (stageText != null) stageText.gameObject.SetActive(!infinite);
        if (targetImage != null) targetImage.gameObject.SetActive(!infinite);
        if (targetText != null) targetText.gameObject.SetActive(!infinite);

        if (infiniteCountText != null)
        {
            infiniteCountText.gameObject.SetActive(infinite);
            if (infinite)
            {
                infiniteCountText.text = "0";
            }
        }

        SetModeObjects(infinite);
    }
    void SettingOnClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        settingPanel.gameObject.SetActive(true);
    }
    void SetModeObjects(bool infinite)
    {
        if (chapterModeObjects != null)
        {
            for (int i = 0; i < chapterModeObjects.Length; i++)
            {
                if (chapterModeObjects[i] != null)
                {
                    chapterModeObjects[i].SetActive(!infinite);
                }
            }
        }

        if (infiniteModeObjects != null)
        {
            for (int i = 0; i < infiniteModeObjects.Length; i++)
            {
                if (infiniteModeObjects[i] != null)
                {
                    infiniteModeObjects[i].SetActive(infinite);
                }
            }
        }
    }

    public void UpdateInfiniteCount(int count)
    {
        if (isInfiniteMode && infiniteCountText != null)
        {
            infiniteCountText.text = count.ToString();
        }
    }


    public void UpdateStageText(int stageNumber)
    {
        if (!isInfiniteMode && stageText != null)
        {
            stageText.text = "Stage " + stageNumber;
        }

        if (!isInfiniteMode && stageUISet != null)
        {
            stageUISet.UpdateStageVisual(stageNumber);
        }
    }

    public void ShowWinUI()
    {
        nextBtn.transform.localScale = zeroScale;
        exitWinBtn.transform.localScale = zeroScale;

        nextBtn.transform.DOScale(1f, buttonMoveDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                nextBtn.transform.DOLocalMove(nextBtnTargetPos, buttonMoveDuration)
                    .SetEase(buttonMoveEase)
                    .OnComplete(() => StartButtonBounce(nextBtn, nextBtnTargetPos));
            });

        exitWinBtn.transform.DOScale(1f, buttonMoveDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                exitWinBtn.transform.DOLocalMove(exitWinBtnTargetPos, buttonMoveDuration)
                    .SetEase(buttonMoveEase)
                    .OnComplete(() => StartButtonBounce(exitWinBtn, exitWinBtnTargetPos));
            });
    }

    public void ShowLoseUI()
    {
        continueBtn.transform.localScale = zeroScale;
        retryBtn.transform.localScale = zeroScale;
        exitBtn.transform.localScale = zeroScale;

        StartMainButtonScaleAnimation(continueBtn);
        StartMainButtonScaleAnimation(retryBtn);

        DOVirtual.DelayedCall(scaleUpDuration + scaleDownDuration + exitButtonDelay,
            () => StartExitButtonScaleAnimation(exitBtn));
    }

    void StartMainButtonScaleAnimation(Button button)
    {
        button.transform.localScale = zeroScale;
        button.transform.DOScale(scaleUpValue, scaleUpDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => button.transform.DOScale(1f, scaleDownDuration).SetEase(Ease.InOutQuad));
    }

    void StartExitButtonScaleAnimation(Button button)
    {
        button.transform.localScale = zeroScale;
        button.transform.DOScale(1f, exitButtonScaleDuration).SetEase(Ease.OutQuad);
    }

    public void UpdateBgImage(Sprite newBgSprite)
    {
        if (bgImage != null && newBgSprite != null)
        {
            bgImage.sprite = newBgSprite;
        }
    }

    void StartButtonBounce(Button button, Vector3 targetPos)
    {
        Vector3 upPos = targetPos;
        upPos.y += bounceUpAmount;

        button.transform.DOLocalMove(upPos, bounceDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => button.transform.DOLocalMove(targetPos, bounceDuration).SetEase(Ease.OutQuad));
    }

    public void TargetUIUpdate(int targetVal, int nowVal)
    {
        if (isInfiniteMode) return;

        int clampedVal = Mathf.Min(nowVal, targetVal);
        float targetFillAmount = (float)clampedVal / targetVal;

        targetImage.DOKill();
        targetImage.DOFillAmount(targetFillAmount, fillDuration).SetEase(fillEase);

        targetText.text = clampedVal + "/" + targetVal;
    }

    public void InitializeTargetPointUI(int count)
    {
        ClearTargetPointUI();

        if (targetPointIconPrefab == null || targetPointIconContainer == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(targetPointIconPrefab, targetPointIconContainer);
            targetPointIcons.Add(icon);
        }
    }

    public void RemoveTargetPointIcon()
    {
        if (targetPointIcons.Count == 0) return;

        GameObject iconToRemove = targetPointIcons[0];
        targetPointIcons.RemoveAt(0);

        if (iconToRemove != null)
        {
            iconToRemove.transform.DOKill();
            iconToRemove.transform.DOScale(0f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (iconToRemove != null)
                    {
                        Destroy(iconToRemove);
                    }
                });
        }
    }

    public void ClearTargetPointUI()
    {
        for (int i = targetPointIcons.Count - 1; i >= 0; i--)
        {
            if (targetPointIcons[i] != null)
            {
                targetPointIcons[i].transform.DOKill();
                Destroy(targetPointIcons[i]);
            }
        }
        targetPointIcons.Clear();
    }

    void ContinueOnClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnContinueButtonPressed();
        }
    }

    public void ContinueGameUI()
    {
        DOTween.KillAll();
        circleMask.Hide();
        retryBtn.transform.localScale = zeroScale;
        exitBtn.transform.localScale = zeroScale;
        continueBtn.transform.localScale = zeroScale;
    }

    void NextOnClick() { }

    void ExitOnClick()
    {
        DOTween.KillAll();
        SceneLoader.LoadSingleScene(0);
    }

    void RetryOnClick()
    {
        DOTween.KillAll();

        int scene1Index = -1;
        int scene2Index = -1;
        int sceneCount = SceneManager.sceneCount;

        for (int i = 0; i < sceneCount; i++)
        {
            int buildIndex = SceneManager.GetSceneAt(i).buildIndex;

            if (scene1Index == -1)
                scene1Index = buildIndex;
            else if (scene2Index == -1)
            {
                scene2Index = buildIndex;
                break;
            }
        }

        SceneLoader.LoadGameScenes(scene1Index, scene2Index);
    }

    void ScreenOnClick() => GameManager.Instance?.OnClick();
}