using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    public static ChapterData SelectedChapter { get; private set; }

    [SerializeField] Image chapterImage, bgImage;
    [SerializeField] Button startBtn, infiniteModeBtn;
    [SerializeField] Button preBtn, nextBtn;
    [SerializeField] float rotationDuration = 0.3f;

    int chapterNum = 0;
    [SerializeField] ChapterData[] chapterDatas;
    bool isAnimating = false;

    void Awake()
    {
        startBtn.onClick.AddListener(StartOnClick);
        infiniteModeBtn.onClick.AddListener(InfiniteModeOnClick);
        preBtn.onClick.AddListener(PreBtnOnClick);
        nextBtn.onClick.AddListener(NextBtnOnClick);

        UpdateChapterDisplay();
    }

    void PreBtnOnClick()
    {
        if (isAnimating || chapterDatas == null || chapterDatas.Length == 0) return;

        chapterNum--;
        if (chapterNum < 0) chapterNum = chapterDatas.Length - 1;

        AnimateChapterChange();
    }

    void NextBtnOnClick()
    {
        if (isAnimating || chapterDatas == null || chapterDatas.Length == 0) return;

        chapterNum++;
        if (chapterNum >= chapterDatas.Length) chapterNum = 0;

        AnimateChapterChange();
    }

    void AnimateChapterChange()
    {
        isAnimating = true;

        chapterImage.transform.DORotate(new Vector3(0, 90, 0), rotationDuration * 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                UpdateChapterDisplay();

                chapterImage.transform.DORotate(Vector3.zero, rotationDuration * 0.5f)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => isAnimating = false);
            });
    }

    void UpdateChapterDisplay()
    {
        if (chapterDatas == null || chapterDatas.Length == 0) return;

        ChapterData currentChapter = chapterDatas[chapterNum];
        if (currentChapter == null) return;

        if (chapterImage != null && currentChapter.ChapterImage != null)
        {
            chapterImage.sprite = currentChapter.ChapterImage;
        }

        if (bgImage != null && currentChapter.ChapterBgImage != null)
        {
            bgImage.sprite = currentChapter.ChapterBgImage;
        }
    }

    void StartOnClick()
    {
        if (chapterDatas != null && chapterDatas.Length > 0)
        {
            SelectedChapter = chapterDatas[chapterNum];
        }
        SceneLoader.LoadGameScenes(1, 2);
    }

    void InfiniteModeOnClick() { }

    void OnDestroy()
    {
        if (chapterImage != null)
        {
            chapterImage.transform.DOKill();
        }
    }
}