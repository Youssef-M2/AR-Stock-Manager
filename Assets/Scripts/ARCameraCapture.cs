using UnityEngine;

public class ARCameraCapture : MonoBehaviour
{
    public Camera vuforiaCamera;
    public RenderTexture cameraRT;

    private Texture2D capturedImage;

    void Start()
    {
        if (vuforiaCamera == null)
        vuforiaCamera = GetComponent<Camera>();

    capturedImage = new Texture2D(
        cameraRT.width,
        cameraRT.height,
        TextureFormat.RGB24,
        false
    );

    Debug.Log("✅ ARCameraCapture Vuforia prêt");

    }

    public bool TryGetCameraImage(out Texture2D image)
{
    image = null;

    if (cameraRT == null || vuforiaCamera == null)
        return false;

    var prevRT = vuforiaCamera.targetTexture;

    vuforiaCamera.targetTexture = cameraRT;
    vuforiaCamera.Render();

    RenderTexture.active = cameraRT;
    capturedImage.ReadPixels(
        new Rect(0, 0, cameraRT.width, cameraRT.height),
        0, 0
    );
    capturedImage.Apply();

    RenderTexture.active = null;
    vuforiaCamera.targetTexture = prevRT;

    image = capturedImage;
    return true;
}
}
