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

    [SerializeField] float pointOffset = 2f;
    [SerializeField] float minAngleGap = 20f;

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
        // 기존 포인트 정리
        ClearAllPoints();

        if (count <= 0 || targetCharacter == null) return;

        requiredPointsCount = count;
        completedPointsCount = 0;

        // 타겟의 Collider 반지름 가져오기
        CircleCollider2D targetCollider = targetCharacter.GetComponent<CircleCollider2D>();
        float targetRadius = 1f;

        if (targetCollider != null)
        {
            targetRadius = targetCollider.radius * targetCharacter.transform.localScale.x;
        }

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

                foreach (float usedAngle in occupiedAngles)
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

            occupiedAngles.Add(angle);

            // 목표 지점 생성
            GameObject pointObj = Instantiate(targetPointPrefab, targetCharacter.transform);
            pointObj.name = $"TargetPoint_{i}";

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * Vector3.up;
            pointObj.transform.localPosition = direction * (targetRadius + pointOffset);
            pointObj.transform.localRotation = rotation;

            TargetPoint point = pointObj.GetComponent<TargetPoint>();
            if (point != null)
            {
                activePoints.Add(point);
            }
        }

    }

    public void OnPointCompleted(TargetPoint point)
    {
        if (!activePoints.Contains(point)) return;

        completedPointsCount++;
        Debug.Log($"Point completed! {completedPointsCount}/{requiredPointsCount}");

        // GameManager에 알림
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
        foreach (var point in activePoints)
        {
            if (point != null)
            {
                Destroy(point.gameObject);
            }
        }

        activePoints.Clear();
        completedPointsCount = 0;
        requiredPointsCount = 0;
    }

    public void SetTargetCharacter(TargetCtrl target)
    {
        targetCharacter = target;
    }
}