using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] Button retryBtn, exitBtn;
    [SerializeField] Button nextBtn, exitWinBtn;
    [SerializeField] Button screenBtn;
    [SerializeField] Image targetImage;
    [SerializeField] TextMeshProUGUI targetText;
    [SerializeField] TextMeshProUGUI stageText;
    [SerializeField] float fillDuration = 0.3f;
    [SerializeField] Ease fillEase = Ease.OutQuad;

    [Header("Button Animation Settings")]
    [SerializeField] float buttonMoveDistance = 300f; // 위로 올라가는 거리
    [SerializeField] float buttonMoveDuration = 0.4f; // 이동 시간
    [SerializeField] Ease buttonMoveEase = Ease.OutCubic; // 이동 이징
    [SerializeField] float bounceUpAmount = 20f; // 위로 튕기는 크기
    [SerializeField] float bounceDownAmount = 10f; // 아래로 내려가는 크기
    [SerializeField] float bounceDuration = 0.15f; // 각 단계별 시간

    public CircleMaskController circleMask;

    private Vector3 retryBtnHiddenPos, exitBtnHiddenPos;
    private Vector3 retryBtnTargetPos, exitBtnTargetPos;
    private Vector3 nextBtnHiddenPos, exitWinBtnHiddenPos;
    private Vector3 nextBtnTargetPos, exitWinBtnTargetPos;

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
        exitBtn.onClick.AddListener(ExitOnClick);
        retryBtn.onClick.AddListener(RetryOnClick);
        exitWinBtn.onClick.AddListener(ExitOnClick);
        nextBtn.onClick.AddListener(NextOnClick);

        retryBtnHiddenPos = retryBtn.transform.localPosition;
        exitBtnHiddenPos = exitBtn.transform.localPosition;
        nextBtnHiddenPos = nextBtn.transform.localPosition;
        exitWinBtnHiddenPos = exitWinBtn.transform.localPosition;

        retryBtnTargetPos = retryBtnHiddenPos + new Vector3(0, buttonMoveDistance, 0);
        exitBtnTargetPos = exitBtnHiddenPos + new Vector3(0, buttonMoveDistance, 0);
        nextBtnTargetPos = nextBtnHiddenPos + new Vector3(0, buttonMoveDistance, 0);
        exitWinBtnTargetPos = exitWinBtnHiddenPos + new Vector3(0, buttonMoveDistance, 0);
    }
    public void UpdateStageText(int stageNumber)
    {
        if (stageText != null)
        {
            stageText.text = "Stage " + stageNumber;
        }
    }
    public void ShowWinUI()
    {
        Sequence nextSeq = DOTween.Sequence();
        Sequence exitSeq = DOTween.Sequence();

        nextSeq.Append(nextBtn.transform.DOLocalMove(nextBtnTargetPos, buttonMoveDuration).SetEase(buttonMoveEase))
                .AppendCallback(() => StartButtonBounce(nextBtn, nextBtnTargetPos));

        exitSeq.Append(exitWinBtn.transform.DOLocalMove(exitWinBtnTargetPos, buttonMoveDuration).SetEase(buttonMoveEase))
               .AppendCallback(() => StartButtonBounce(exitWinBtn, exitWinBtnTargetPos));
    }
    public void ShowLoseUI()
    {
        Sequence retrySeq = DOTween.Sequence();
        Sequence exitSeq = DOTween.Sequence();

        retrySeq.Append(retryBtn.transform.DOLocalMove(retryBtnTargetPos, buttonMoveDuration).SetEase(buttonMoveEase))
                .AppendCallback(() => StartButtonBounce(retryBtn, retryBtnTargetPos));

        exitSeq.Append(exitBtn.transform.DOLocalMove(exitBtnTargetPos, buttonMoveDuration).SetEase(buttonMoveEase))
               .AppendCallback(() => StartButtonBounce(exitBtn, exitBtnTargetPos));
    }

    void StartButtonBounce(Button button, Vector3 targetPos)
    {
        Sequence bounceSeq = DOTween.Sequence();

        Vector3 upPos = targetPos + new Vector3(0, bounceUpAmount, 0);
        bounceSeq.Append(button.transform.DOLocalMove(upPos, bounceDuration).SetEase(Ease.OutQuad));

        bounceSeq.Append(button.transform.DOLocalMove(targetPos, bounceDuration).SetEase(Ease.OutQuad));
    }

    public void TargetUIUpdate(int targetVal, int nowVal)
    {
        float targetFillAmount = (float)nowVal / targetVal;

        targetImage.DOKill();
        targetImage.DOFillAmount(targetFillAmount, fillDuration)
            .SetEase(fillEase);

        targetText.text = nowVal + "/" + targetVal;
    }
    void NextOnClick()
    {
        //Todo : 다음 챕터 시작

    }
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

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            int buildIndex = scene.buildIndex;

            if (scene1Index == -1)
                scene1Index = buildIndex;
            else if (scene2Index == -1)
                scene2Index = buildIndex;
        }

        SceneLoader.LoadGameScenes(scene1Index, scene2Index);
    }

    void ScreenOnClick()
    {
        GameManager.Instance.OnClick();
    }
}