using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;

public class ProductManager : MonoBehaviour
{
    public static ProductManager Instance;
    private FirebaseFirestore db;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        db = FirebaseFirestore.DefaultInstance;
    }

    // 🔹 Charger un produit
    public async Task<ProductData> GetProduct(string productId)
    {
        DocumentReference docRef = db.Collection("products").Document(productId);
        DocumentSnapshot snap = await docRef.GetSnapshotAsync();

        if (!snap.Exists)
        {
            Debug.LogWarning("❌ Produit non trouvé: " + productId);
            return null;
        }

        ProductData product = snap.ConvertTo<ProductData>();
        product.id = snap.Id;

        return product;
    }

    // 🔹 Mettre à jour le stock
    public async Task UpdateStock(string productId, int newStock)
    {
        if (!FirestoreManager.Instance.IsSeller)
        {
            Debug.LogError("❌ Permission refusée : vous n'êtes pas vendeur.");
            return;
        }

        DocumentReference docRef = db.Collection("products").Document(productId);
        await docRef.UpdateAsync("stock", newStock);

        Debug.Log("✔ Stock mis à jour : " + productId);
    }
}
