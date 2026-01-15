using UnityEngine;

[CreateAssetMenu(fileName = "New Chapter Data", menuName = "Game/Chapter Data")]
public class ChapterData : ScriptableObject
{
    [System.Serializable]
    public class StageSettings
    {
        [Header("Rotation Settings")]
        [HideInInspector] public float minStartSpeed = 0f;
        [HideInInspector] public float maxStartSpeed = 80f;
        [HideInInspector] public float minSpeedChangeRate = 20f;
        [HideInInspector] public float maxSpeedChangeRate = 60f;
        public float minMaxSpeed = 100f;
        public float maxMaxSpeed = 300f;
        public bool rotateClockwise = true;

        [Header("Reverse Rotation Settings")]
        public float minHoldTime = 1f;
        public float maxHoldTime = 3f;
        public float reverseDeceleration = 60f;
        public float reverseWaitTime = 0.3f;
        public bool reverseDirection = true;

        [Header("Stage Info")]
        public int requiredKnives = 10;
    }

    [SerializeField] private string chapterName;
    [SerializeField] private int chapterNumber;
    [SerializeField] private StageSettings[] stages = new StageSettings[10];

    public string ChapterName => chapterName;
    public int ChapterNumber => chapterNumber;
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