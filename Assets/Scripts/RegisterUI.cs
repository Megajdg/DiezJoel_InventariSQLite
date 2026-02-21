using TMPro;
using UnityEngine;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;
    public GameObject errorPanel;
    public DatabaseManager dbManager;

    public void OnRegisterButton()
    {
        string result = dbManager.RegisterUser(usernameInput.text, passwordInput.text);

        if (result == "OK")
        {
            PlayerPrefs.SetString("RegisterSuccess", "OK");
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }
        else
        {
            errorText.text = result;
            errorPanel.SetActive(true);
        }
    }

    public void GoToLogin()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}