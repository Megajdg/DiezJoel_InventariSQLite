using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;
    public GameObject errorPanel;
    public DatabaseManager dbManager;

    private void Start()
    {
        if (PlayerPrefs.GetString("RegisterSuccess", "") == "OK")
        {
            errorText.text = "User registered. Log in, please.";
            errorPanel.SetActive(true);
            PlayerPrefs.DeleteKey("RegisterSuccess");
        }
    }

    public void OnLoginButton()
    {
        string error;
        if (dbManager.LoginUser(usernameInput.text, passwordInput.text, out error))
        {
            PlayerPrefs.SetString("LoggedUser", usernameInput.text);

            // Obtener inventario del usuario
            int inventariID = dbManager.GetInventariID(usernameInput.text);
            PlayerPrefs.SetInt("InventariID", inventariID);

            Debug.Log(inventariID);

            UnityEngine.SceneManagement.SceneManager.LoadScene("HomeScene");
        }
        else
        {
            errorText.text = error;
            errorPanel.SetActive(true);
        }
    }

    public void GoToRegister()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RegisterScene");
    }
}