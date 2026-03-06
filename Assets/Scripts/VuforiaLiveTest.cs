using UnityEngine;
using Vuforia;

public class VuforiaLiveTest : MonoBehaviour
{
    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f) return;
        timer = 0f;

        if (VuforiaApplication.Instance == null)
        {
            Debug.Log("❌ VuforiaApplication non initialisée");
            return;
        }

        if (!VuforiaApplication.Instance.IsRunning)
        {
            Debug.Log("❌ Vuforia pas encore démarré");
            return;
        }

        if (VuforiaBehaviour.Instance == null)
        {
            Debug.Log("❌ VuforiaBehaviour introuvable");
            return;
        }

        Camera cam = VuforiaBehaviour.Instance.GetComponent<Camera>();
        if (cam == null || !cam.enabled)
        {
            Debug.Log("❌ Caméra Vuforia inactive");
            return;
        }

        Debug.Log("✅ Vuforia tourne + caméra active (temps réel OK)");
    }
}
