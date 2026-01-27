// TargetPointManager.cs
using UnityEngine;
using System.Collections.Generic;
using Lean.Pool;

public class TargetPointManager : MonoBehaviour
{
    public static TargetPointManager Instance { get; private set; }

    [SerializeField] private GameObject targetPointPrefab;
    [SerializeField] private TargetCtrl targetCharacter;

    private List<TargetPoint> activePoints = new List<TargetPoint>(10);
    private int completedPointsCount = 0;
    private int requiredPointsCount = 0;

    [Header("Spawn Settings")]
    [SerializeField] float pointOffset = 2f;
    [SerializeField] float minAngleGap = 30f;

    private CircleCollider2D targetCollider;

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

        if (count <= 0 || targetCharacter == null) return;

        requiredPointsCount = count;
        completedPointsCount = 0;

        if (targetCollider == null)
        {
            targetCollider = targetCharacter.GetComponent<CircleCollider2D>();
        }

        float targetRadius = targetCollider != null
            ? targetCollider.radius * targetCharacter.transform.localScale.x
            : 1f;

        float scaleFactor = GameManager.Instance != null ? GameManager.Instance.GetScaleFactor() : 1f;
        float scaledPointOffset = pointOffset / (scaleFactor * scaleFactor);

        int occupiedCount = occupiedAngles.Count;

        for (int i = 0; i < count; i++)
        {
            float angle = FindValidAngle(occupiedAngles, occupiedCount);
            occupiedAngles.Add(angle);
            occupiedCount++;

            GameObject pointObj = LeanPool.Spawn(targetPointPrefab, targetCharacter.transform);
            pointObj.name = $"TargetPoint_{i}";
            pointObj.transform.localScale = Vector3.one * scaleFactor;

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * Vector3.up;
            pointObj.transform.localPosition = direction * (targetRadius + scaledPointOffset);
            pointObj.transform.localRotation = rotation;

            TargetPoint point = pointObj.GetComponent<TargetPoint>();
            if (point != null)
            {
                activePoints.Add(point);
            }
        }
    }

    float FindValidAngle(List<float> usedAngles, int count)
    {
        const int maxAttempts = 100;
        float angle = Random.Range(0f, 360f);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            bool valid = true;

            for (int i = 0; i < count; i++)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(angle, usedAngles[i])) < minAngleGap)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return angle;
            angle = Random.Range(0f, 360f);
        }

        return angle;
    }

    public void OnPointCompleted(TargetPoint point)
    {
        if (!activePoints.Contains(point)) return;

        activePoints.Remove(point);
        completedPointsCount++;
        GameManager.Instance?.OnTargetPointCompleted();
    }

    public bool AreAllPointsCompleted() => completedPointsCount >= requiredPointsCount;

    public int GetCompletedCount() => completedPointsCount;

    public int GetRequiredCount() => requiredPointsCount;

    public void ClearAllPoints()
    {
        for (int i = activePoints.Count - 1; i >= 0; i--)
        {
            if (activePoints[i] != null && activePoints[i].gameObject != null)
            {
                LeanPool.Despawn(activePoints[i].gameObject);
            }
        }

        activePoints.Clear();
        completedPointsCount = 0;
        requiredPointsCount = 0;
    }

    public void SetTargetCharacter(TargetCtrl target)
    {
        targetCharacter = target;
        targetCollider = null;
    }
}