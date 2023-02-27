using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using TMPro;

public class RedirectButton : MonoBehaviour
{
    [SerializeField]
    ConfirmationWindow confirmationWindow;

    private void Awake()
    {
        confirmationWindow = confirmationWindow.GetComponent<ConfirmationWindow>();
    }

    public void OpenConfirmationWindow(string message)
    {
        confirmationWindow.gameObject.SetActive(true);
        confirmationWindow.yesButton.onClick.AddListener(YesClicked);
        confirmationWindow.noButton.onClick.AddListener(NoClicked);
        confirmationWindow.messageText.text = Regex.Unescape(message);
    }

    private void YesClicked()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Application.OpenURL("https://www.bakalabs.com/wonder-arena");
            confirmationWindow.gameObject.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().name == "Profile")
        {
            string address = confirmationWindow.transform.Find("Input address").GetComponent<TMP_InputField>().text;
            NetworkManager.Instance.LinkAccount(address);
        }
    }

    private void NoClicked()
    {
        confirmationWindow.gameObject.SetActive(false);
    }
}