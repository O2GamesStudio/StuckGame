// ChapterData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Chapter Data", menuName = "Game/Chapter Data")]
public class ChapterData : ScriptableObject
{
    [System.Serializable]
    public class StageSettings
    {
        [Header("Visual Settings")]
        public Sprite bgImage;
        public Sprite targetImage;
        public StuckObj stuckObjPrefab;

        [Header("Rotation Settings")]
        [HideInInspector] public float minStartSpeed = 0f;
        [HideInInspector] public float maxStartSpeed = 80f;
        [HideInInspector] public float minSpeedChangeRate = 40f;
        [HideInInspector] public float maxSpeedChangeRate = 80f;
        public float minMaxSpeed = 80f;
        public float maxMaxSpeed = 200f;
        [Range(0f, 1f)]
        public float accelerationRatio = 0.3f;
        public bool rotateClockwise = true;

        [Header("Reverse Rotation Settings")]
        public float minHoldTime = 1f;
        public float maxHoldTime = 3f;
        public float reverseDeceleration = 100f;
        public float reverseWaitTime = 0.3f;
        public bool reverseDirection = true;

        [Header("Stage Info")]
        public int requiredKnives = 10;
        [Range(0, 20)]
        public int obstacleCount = 0;
        [Range(0, 10)]
        public int targetPointCount = 0;
    }

    [Header("Chapter Info")]
    [SerializeField] private string chapterName;
    [SerializeField] private int chapterNumber;
    [SerializeField] private Sprite chapterImage;
    [SerializeField] private Sprite chapterBgImage;

    [Header("Stage Settings")]
    [SerializeField] private StageSettings[] stages = new StageSettings[10];

    public string ChapterName => chapterName;
    public int ChapterNumber => chapterNumber;
    public Sprite ChapterImage => chapterImage;
    public Sprite ChapterBgImage => chapterBgImage;
    public int TotalStages => stages.Length;

    public StageSettings GetStageSettings(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < stages.Length)
        {
            return stages[stageIndex];
        }

        Debug.LogError($"Invalid stage index: {stageIndex}");
        return stages[0];
    }
}