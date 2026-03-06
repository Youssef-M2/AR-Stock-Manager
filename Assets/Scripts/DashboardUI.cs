using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class DashboardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;
    public TextMeshProUGUI expiryText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI typeText;

    private ProductData currentProduct;
    private string currentProductId;

    private ProductManager productManager;

    private void Awake()
    {
        productManager = FindObjectOfType<ProductManager>();
    }

    // Charger et afficher un produit
    public async void ShowProduct(string productId)
    {
        currentProductId = productId;
        currentProduct = await productManager.GetProduct(productId);

        if (currentProduct == null)
        {
            nameText.text = "NOT FOUND";
            priceText.text = "-";
            stockText.text = "-";
            expiryText.text = "-";
            categoryText.text = "-";
            typeText.text = "-";
            return;
        }

        nameText.text = currentProduct.name;
        priceText.text = currentProduct.price.ToString("0.00") + " DH";
        stockText.text = currentProduct.stock.ToString();
        expiryText.text = currentProduct.expiry;
        categoryText.text = currentProduct.category;
        typeText.text = currentProduct.type;
    }

    // Ajouter du stock
    public async void AddStock()
    {
        if (!FirestoreManager.Instance.IsSeller)
        {
            Debug.LogWarning("❌ Seul le vendeur peut modifier le stock.");
            return;
        }

        currentProduct.stock++;
        stockText.text = currentProduct.stock.ToString();
        await productManager.UpdateStock(currentProductId, currentProduct.stock);
    }

    // Retirer du stock
    public async void RemoveStock()
    {
        if (!FirestoreManager.Instance.IsSeller)
        {
            Debug.LogWarning("❌ Seul le vendeur peut modifier le stock.");
            return;
        }

        if (currentProduct.stock > 0)
            currentProduct.stock--;

        stockText.text = currentProduct.stock.ToString();
        await productManager.UpdateStock(currentProductId, currentProduct.stock);
    }
}
