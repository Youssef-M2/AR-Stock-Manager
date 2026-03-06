using Firebase.Firestore;

[FirestoreData]
public class ProductData
{
    [FirestoreDocumentId]
    public string id { get; set; }

    [FirestoreProperty] public string name { get; set; }
    [FirestoreProperty] public double price { get; set; }
    [FirestoreProperty] public int stock { get; set; }
    [FirestoreProperty] public string expiry { get; set; }
    [FirestoreProperty] public string category { get; set; }
    [FirestoreProperty] public string type { get; set; }
}
