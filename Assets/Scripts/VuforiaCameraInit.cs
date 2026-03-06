using UnityEngine;
using Vuforia;

public class VuforiaCameraInit : MonoBehaviour
{
    void Start()
    {
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(
                PixelFormat.RGB888, true);

            Debug.Log("✅ Vuforia RGB888 activé");
        }
    }
}
