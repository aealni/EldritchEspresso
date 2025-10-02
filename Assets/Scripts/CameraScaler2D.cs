using UnityEngine;

[ExecuteAlways]
public class CameraScaler2D : MonoBehaviour
{
    public float referenceWidth = 1920f;
    public float referenceHeight = 1080f;
    public float minWidth = 1280f;
    public float minHeight = 720f;
    public Camera cam;

    private float initialOrthoSize;
    public float targetAspect = 16.0f / 9.0f;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        initialOrthoSize = cam.orthographicSize;
        
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
    }

    void Update()
    {
        ScaleCamera();
        EnforceAspectRatio();
    }

    void ScaleCamera()
    {
        cam.orthographicSize = initialOrthoSize;
    }

    void EnforceAspectRatio()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}
