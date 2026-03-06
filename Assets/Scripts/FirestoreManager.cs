using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance;

    public FirebaseAuth auth;
    public FirebaseUser user;
    public FirebaseFirestore db;

    public bool IsSeller { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void Login(string email, string password, Action<bool, string> callback)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                callback(false, task.Exception?.Message);
                return;
            }

            user = task.Result.User;
            callback(true, "Login success");

            // Vérifier si vendeur en lisant Firestore
            CheckIfSeller();
        });
    }

    private void CheckIfSeller()
    {
        var docRef = db.Collection("users").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (!task.Result.Exists) return;

            var data = task.Result.ToDictionary();
            if (data.ContainsKey("isSeller"))
                IsSeller = (bool)data["isSeller"];

            Debug.Log("IsSeller: " + IsSeller);
        });
    }
}
