using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI totalScore;
    [SerializeField]
    TextMeshProUGUI defendRecords;
    [SerializeField]
    TextMeshProUGUI attackRecords;
    [SerializeField]
    TextMeshProUGUI totalFights;
    [SerializeField]
    TextMeshProUGUI totalWins;
    [SerializeField]
    TextMeshProUGUI username;
    [SerializeField]
    TextMeshProUGUI email;

    [SerializeField]
    GameObject confirmButton;

    private void Awake()
    {
        username = username.GetComponent<TextMeshProUGUI>();
        email = email.GetComponent<TextMeshProUGUI>();
        totalScore = totalScore.GetComponent<TextMeshProUGUI>();
        defendRecords = defendRecords.GetComponent<TextMeshProUGUI>();
        attackRecords = attackRecords.GetComponent<TextMeshProUGUI>();
        totalFights = totalFights.GetComponent<TextMeshProUGUI>();
        totalWins = totalWins.GetComponent<TextMeshProUGUI>();
    }

    public void LogOutClicked()
    {
        PlayerPrefs.DeleteAll();
        GameManager.Instance.ClearUserData();
        LevelManager.Instance.LoadScene("ConnectingWallet");
    }

    public void FixedUpdate()
    {
        totalScore.text = GameManager.Instance.userTotalScore;
        totalFights.text = GameManager.Instance.userChallengeData.Count.ToString();
        username.text = PlayerPrefs.GetString("Username");
        email.text = PlayerPrefs.GetString("Email");
        int totalWinsInt = 0;
        foreach (var challenge in GameManager.Instance.userChallengeData)
        {
            if (GameManager.Instance.userFlowAddress == challenge.winner)
            {
                totalWinsInt += 1;
            }
        }
        totalWins.text = totalWinsInt.ToString();

        if (GameManager.Instance.linkedSuccesfully)
        {
            confirmButton.SetActive(false);
        }
        else
        {
            confirmButton.SetActive(true);
        }

    }

}
