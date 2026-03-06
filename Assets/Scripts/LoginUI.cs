using UnityEngine;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;

    [Header("References")]
    public GameObject loginCanvas;

    public void Login()
    {
        // 🔒 Sécurité références
        if (emailInput == null || passwordInput == null || errorText == null || loginCanvas == null)
        {
            Debug.LogError("❌ Références UI non assignées dans LoginUI");
            return;
        }

        if (AuthManager.Instance == null)
        {
            errorText.text = "AuthManager manquant";
            Debug.LogError("❌ AuthManager.Instance == null");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Email et mot de passe requis";
            return;
        }

        errorText.text = "Connexion...";

        AuthManager.Instance.OnLoginSuccess += OnLoginSuccess;
        AuthManager.Instance.OnLoginFailed += OnLoginFailed;

        AuthManager.Instance.Login(email, password);
    }
    public void CancelLogin()
    {
        loginCanvas.SetActive(false);
    }

    private void OnLoginSuccess()
    {
        Cleanup();

        if (!AuthManager.Instance.IsSeller)
        {
            errorText.text = "Accès refusé (non vendeur)";
            return;
        }

        loginCanvas.SetActive(false);
        Debug.Log("✅ Login vendeur réussi");
    }

    private void OnLoginFailed(string msg)
    {
        Cleanup();
        errorText.text = "Erreur de connexion";
        Debug.LogError(msg);
    }

    private void Cleanup()
    {
        if (AuthManager.Instance == null) return;

        AuthManager.Instance.OnLoginSuccess -= OnLoginSuccess;
        AuthManager.Instance.OnLoginFailed -= OnLoginFailed;
    }
}
