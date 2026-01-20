// TargetPointManager.cs
using UnityEngine;
using System.Collections.Generic;

public class TargetPointManager : MonoBehaviour
{
    public static TargetPointManager Instance { get; private set; }

    [SerializeField] private GameObject targetPointPrefab;
    [SerializeField] private TargetCtrl targetCharacter;

    private List<TargetPoint> activePoints = new List<TargetPoint>();
    private int completedPointsCount = 0;
    private int requiredPointsCount = 0;

    [Header("Spawn Settings")]
    [SerializeField] float pointOffset = 2f;
    [SerializeField] float minAngleGap = 30f;

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
    }

    public void InitializeTargetPoints(int count, List<float> occupiedAngles)
    {
        ClearAllPoints();

        if (count <= 0 || targetCharacter == null)
        {
            Debug.Log($"TargetPointManager: count={count}, targetCharacter={(targetCharacter != null ? "exists" : "null")}");
            return;
        }

        requiredPointsCount = count;
        completedPointsCount = 0;

        Debug.Log($"TargetPointManager: Initializing {count} target points");

        CircleCollider2D targetCollider = targetCharacter.GetComponent<CircleCollider2D>();
        float targetRadius = 1f;

        if (targetCollider != null)
        {
            targetRadius = targetCollider.radius * targetCharacter.transform.localScale.x;
        }

        List<float> usedAngles = new List<float>(occupiedAngles);

        for (int i = 0; i < count; i++)
        {
            float angle = 0f;
            bool validAngle = false;
            int maxAttempts = 100;
            int attempts = 0;

            while (!validAngle && attempts < maxAttempts)
            {
                angle = Random.Range(0f, 360f);
                validAngle = true;

                foreach (float usedAngle in usedAngles)
                {
                    float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angle, usedAngle));
                    if (angleDiff < minAngleGap)
                    {
                        validAngle = false;
                        break;
                    }
                }

                attempts++;
            }

            usedAngles.Add(angle);

            // Instantiate 사용 (LeanPool 대신)
            GameObject pointObj = Instantiate(targetPointPrefab);
            pointObj.name = $"TargetPoint_{i}";

            Debug.Log($"TargetPoint instantiated: {pointObj.name}, Active: {pointObj.activeSelf}");

            // 부모 설정
            pointObj.transform.SetParent(targetCharacter.transform);

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * Vector3.up;
            pointObj.transform.localPosition = direction * (targetRadius + pointOffset);
            pointObj.transform.localRotation = rotation;

            Debug.Log($"TargetPoint positioned: {pointObj.name}, LocalPos: {pointObj.transform.localPosition}, WorldPos: {pointObj.transform.position}");

            TargetPoint point = pointObj.GetComponent<TargetPoint>();
            if (point != null)
            {
                activePoints.Add(point);
                Debug.Log($"TargetPoint {i} created at angle {angle}, Active: {pointObj.activeSelf}");
            }
            else
            {
                Debug.LogError($"TargetPoint component not found on {pointObj.name}");
            }
        }

        Debug.Log($"TargetPointManager: Total {activePoints.Count} points created, required: {requiredPointsCount}");
    }

    public void OnPointCompleted(TargetPoint point)
    {
        if (!activePoints.Contains(point)) return;

        completedPointsCount++;
        GameManager.Instance?.OnTargetPointCompleted();
    }

    public bool AreAllPointsCompleted()
    {
        return completedPointsCount >= requiredPointsCount;
    }

    public int GetCompletedCount()
    {
        return completedPointsCount;
    }

    public int GetRequiredCount()
    {
        return requiredPointsCount;
    }

    public void ClearAllPoints()
    {
        Debug.Log($"TargetPointManager: Clearing {activePoints.Count} points");

        foreach (var point in activePoints)
        {
            if (point != null)
            {
                // Destroy 사용 (LeanPool 대신)
                Destroy(point.gameObject);
            }
        }

        activePoints.Clear();
        completedPointsCount = 0;
        requiredPointsCount = 0;

        Debug.Log("TargetPointManager: All points cleared");
    }

    public void SetTargetCharacter(TargetCtrl target)
    {
        targetCharacter = target;
    }
}