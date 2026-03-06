using UnityEngine;
using TMPro;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Linq;

public interface IProductModel
{
    string Predict(Texture2D image);
}

public class ARProductRecognizer : MonoBehaviour
{
    [Header("Camera Capture")]
    public ARCameraCapture cameraCapture;

    [Header("UI Elements")]
    public TMP_Text productNameText;
    public TMP_Text priceText;
    public TMP_Text stockText;
    public TMP_Text expiryText;
    public TMP_Text categoryText;
    public TMP_Text typeText;
    public GameObject productPanel;

    [Header("Buttons")]
    public GameObject scanButton; // 🔥 Toujours visible

    [Header("Login")]
    public GameObject loginCanvas;

    [Header("Model Inference")]
    public MonoBehaviour modelScript;
    private IProductModel model;

    private FirebaseFirestore db;
    private DocumentSnapshot currentProduct;

    private readonly string[] productCollections =
    {
        "Chocolat",
        "Fruits",
        "Boissons",
        "Snacks",
        "Chips"
    };

    // ==================================================
    void Start()
    {
        model = modelScript as IProductModel;
        db = FirebaseFirestore.DefaultInstance;

        productPanel.SetActive(false);
        loginCanvas.SetActive(false);
        scanButton.SetActive(true); // ✅ TOUJOURS ACTIF

        Debug.Log("✅ ARProductRecognizer prêt");
    }

    // ==================================================
    // 🔘 SCAN BUTTON
    // ==================================================
    public void OnScanButtonClicked()
    {
        Debug.Log("📸 Scan button clicked");
        RecognizeProduct();
    }

    // ==================================================
    // 🔘 EXIT BUTTON
    // ==================================================
    public void OnExitButtonClicked()
    {
        productPanel.SetActive(false);
        loginCanvas.SetActive(false);
    }

    // ==================================================
    // 🔍 RECONNAISSANCE PRODUIT
    // ==================================================
    private async void RecognizeProduct()
    {
        if (cameraCapture == null || model == null)
        {
            Debug.LogError("❌ Camera ou modèle manquant");
            return;
        }

        if (!cameraCapture.TryGetCameraImage(out Texture2D image))
        {
            Debug.LogError("❌ Image caméra non capturée");
            return;
        }

        string productName = model.Predict(image)?.Trim().ToLower();
        Debug.Log("📦 Produit détecté IA : " + productName);

        if (string.IsNullOrEmpty(productName))
            return;

        currentProduct = await FindProductInAllCollections(productName);

        if (currentProduct == null)
        {
            Debug.LogError("❌ Produit non trouvé Firestore");
            productPanel.SetActive(false);
            return;
        }

        // ✅ UI
        productNameText.text = currentProduct.GetValue<string>("name");
        priceText.text = currentProduct.GetValue<object>("price").ToString();
        stockText.text = currentProduct.GetValue<object>("stock").ToString();
        expiryText.text = currentProduct.GetValue<object>("expiry").ToString();
        categoryText.text = currentProduct.GetValue<string>("category");
        typeText.text = currentProduct.GetValue<string>("type");

        productPanel.SetActive(true);
    }

    // ==================================================
    // 🔎 RECHERCHE FIRESTORE
    // ==================================================
    private async Task<DocumentSnapshot> FindProductInAllCollections(string productName)
    {
        foreach (string collection in productCollections)
        {
            Query query = db.Collection(collection)
                .WhereEqualTo("name", productName)
                .Limit(1);

            QuerySnapshot snap = await query.GetSnapshotAsync();

            if (snap.Count > 0)
            {
                Debug.Log($"✅ Produit trouvé dans {collection}");
                return snap.Documents.First();
            }
        }
        return null;
    }

    // ==================================================
    // 🔐 SELLER CHECK — BLOQUANT
    // ==================================================
    private bool IsSeller()
{
    if (AuthManager.Instance == null)
        return false;

    // 🔒 Utilisateur non connecté
    if (AuthManager.Instance.CurrentUser == null)
        return false;

    // 🔒 Pas vendeur
    if (!AuthManager.Instance.IsSeller)
        return false;

    return true;
}


    // ==================================================
    // ➕ ADD STOCK
    // ==================================================
    public async void AddStock()
{
    if (!IsSeller())
    {
        Debug.Log("🔐 Accès refusé → login requis");
        loginCanvas.SetActive(true);
        return;
    }

    if (currentProduct == null) return;

    DocumentReference docRef = currentProduct.Reference;

    await db.RunTransactionAsync(async transaction =>
    {
        DocumentSnapshot snap = await transaction.GetSnapshotAsync(docRef);
        long stock = long.Parse(snap.GetValue<object>("stock").ToString());
        transaction.Update(docRef, "stock", stock + 1);
    });

    stockText.text = (int.Parse(stockText.text) + 1).ToString();
}

    // ==================================================
    // ➖ REMOVE STOCK
    // ==================================================
    public async void RemoveStock()
{
    if (!IsSeller())
    {
        Debug.Log("🔐 Accès refusé → login requis");
        loginCanvas.SetActive(true);
        return;
    }

    if (currentProduct == null) return;

    DocumentReference docRef = currentProduct.Reference;

    await db.RunTransactionAsync(async transaction =>
    {
        DocumentSnapshot snap = await transaction.GetSnapshotAsync(docRef);
        long stock = long.Parse(snap.GetValue<object>("stock").ToString());

        if (stock > 0)
            transaction.Update(docRef, "stock", stock - 1);
    });

    int s = int.Parse(stockText.text);
    if (s > 0) stockText.text = (s - 1).ToString();
}
}

