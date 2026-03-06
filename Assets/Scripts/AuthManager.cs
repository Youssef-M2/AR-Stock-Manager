using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Newtonsoft.Json;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    private FirebaseAuth auth;
    public FirebaseUser CurrentUser;

    public bool IsSeller { get; private set; } = false;

    public event Action OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action OnLogout;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeFirebase();
        Logout();
    }

    // -------------------------------------------------
    private async Task InitializeFirebase()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync();
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            CurrentUser = auth.CurrentUser;
            await LoadClaims();
            OnLoginSuccess?.Invoke();
        }
    }

    // -------------------------------------------------
    public async void Login(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            CurrentUser = result.User;

            await LoadClaims(); // 👈 IMPORTANT

            OnLoginSuccess?.Invoke();
        }
        catch (Exception e)
        {
            OnLoginFailed?.Invoke(e.Message);
        }
    }

    // -------------------------------------------------
    public void Logout()
    {
        auth.SignOut();
        CurrentUser = null;
        IsSeller = false;
        OnLogout?.Invoke();
    }

    // -------------------------------------------------
    private async Task LoadClaims()
    {
        if (CurrentUser == null) return;

        // 🔥 FORCE REFRESH DU TOKEN
        string idToken = await CurrentUser.TokenAsync(true);

        var claims = DecodeFirebaseToken(idToken);

        if (claims != null && claims.ContainsKey("isSeller"))
        {
            IsSeller = claims["isSeller"].ToString().ToLower() == "true";
        }
        else
        {
            IsSeller = false;
        }

        Debug.Log("CLAIMS => " + JsonConvert.SerializeObject(claims));
        Debug.Log("IsSeller = " + IsSeller);
    }

    // -------------------------------------------------
    private Dictionary<string, object> DecodeFirebaseToken(string token)
    {
        try
        {
            string[] parts = token.Split('.');
            if (parts.Length < 2) return null;

            string payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("JWT decode error: " + e.Message);
            return null;
        }
    }
}
