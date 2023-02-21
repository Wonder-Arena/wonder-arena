using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class RedirectButton : MonoBehaviour
{
    [SerializeField]
    ConfirmationWindow confirmationWindow;

    public void OpenConfirmationWindow(string message)
    {
        confirmationWindow.gameObject.SetActive(true);
        confirmationWindow.yesButton.onClick.AddListener(YesClicked);
        confirmationWindow.noButton.onClick.AddListener(NoClicked);
        confirmationWindow.messageText.text = Regex.Unescape(message);
    }

    private void YesClicked()
    {
        Application.OpenURL("https://www.basicbeasts.io/drop");
        confirmationWindow.gameObject.SetActive(false);
    }

    private void NoClicked()
    {
        confirmationWindow.gameObject.SetActive(false);
    }
}