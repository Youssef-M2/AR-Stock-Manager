using UnityEngine;
using Unity.Barracuda;
using System.Linq;

public class OnnxProductModel : MonoBehaviour, IProductModel
{
    [Header("ONNX Model")]
    public NNModel modelAsset;

    private IWorker worker;

    // Exemple labels (à adapter à ton modèle)
    public string[] labels = { "coca", "fanta", "sprite" };

    void Awake()
    {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
    }

    public string Predict(Texture2D inputImage)
    {
        // Prétraitement (ex: 224x224)
        Texture2D resized = ResizeTexture(inputImage, 224, 224);

        Tensor input = new Tensor(resized, 3);
        worker.Execute(input);

        Tensor output = worker.PeekOutput();

        int bestIndex = 0;
        float maxVal = output[0];

        for (int i = 1; i < output.length; i++)
        {
            if (output[i] > maxVal)
            {
                maxVal = output[i];
                bestIndex = i;
            }
        }

        input.Dispose();
        output.Dispose();

        return labels[bestIndex];
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }

    // Simple resize helper
    Texture2D ResizeTexture(Texture2D src, int w, int h)
    {
        RenderTexture rt = RenderTexture.GetTemporary(w, h);
        Graphics.Blit(src, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return tex;
    }
}
