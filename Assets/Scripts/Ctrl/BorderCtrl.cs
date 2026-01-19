using UnityEngine;

public class BorderCtrl : MonoBehaviour
{
    [SerializeField] GameObject topBorder, bottomBorder, leftBorder, rightBorder;
    [SerializeField] float borderOffset = 0.1f;

    void Start()
    {
        SetupBorders();
    }

    void SetupBorders()
    {
        Camera mainCamera = Camera.main;
        float screenHeight = 2f * mainCamera.orthographicSize;
        float screenWidth = screenHeight * mainCamera.aspect;

        float top = screenHeight / 2f - borderOffset;
        float bottom = -screenHeight / 2f + borderOffset;
        float left = -screenWidth / 2f + borderOffset;
        float right = screenWidth / 2f - borderOffset;

        if (topBorder != null)
            topBorder.transform.position = new Vector3(0, top, 0);

        if (bottomBorder != null)
            bottomBorder.transform.position = new Vector3(0, bottom, 0);

        if (leftBorder != null)
            leftBorder.transform.position = new Vector3(left, 0, 0);

        if (rightBorder != null)
            rightBorder.transform.position = new Vector3(right, 0, 0);
    }
}