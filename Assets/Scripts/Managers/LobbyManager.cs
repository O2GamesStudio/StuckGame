using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static ChapterData SelectedChapter { get; private set; }

    [SerializeField] Image chapterImage, bgImage;
    [SerializeField] Button startBtn, infiniteModeBtn;
    [SerializeField] Button preBtn, nextBtn;

    int chapterNum = 0;
    [SerializeField] ChapterData[] chapterDatas;

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
        if (chapterDatas == null || chapterDatas.Length == 0) return;

        chapterNum--;
        if (chapterNum < 0) chapterNum = chapterDatas.Length - 1;

        UpdateChapterDisplay();
    }

    void NextBtnOnClick()
    {
        if (chapterDatas == null || chapterDatas.Length == 0) return;

        chapterNum++;
        if (chapterNum >= chapterDatas.Length) chapterNum = 0;

        UpdateChapterDisplay();
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
}