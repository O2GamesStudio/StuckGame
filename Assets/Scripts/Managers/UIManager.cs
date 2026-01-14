using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] Button screenBtn;
    [SerializeField] Image targetImage;
    [SerializeField] TextMeshProUGUI targetText;
    [SerializeField] float fillDuration = 0.3f; // 채워지는 시간
    [SerializeField] Ease fillEase = Ease.OutQuad; // 이징 타입

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

    public void TargetUIUpdate(int targetVal, int nowVal)
    {
        float targetFillAmount = (float)nowVal / targetVal;

        targetImage.DOKill();
        targetImage.DOFillAmount(targetFillAmount, fillDuration)
            .SetEase(fillEase);

        targetText.text = nowVal + "/" + targetVal;
    }

    void ScreenOnClick()
    {
        GameManager.Instance.OnClick();
    }
}