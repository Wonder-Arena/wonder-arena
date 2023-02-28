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
    TextMeshProUGUI bloctoAddress;

    [SerializeField]
    GameObject confirmButton;
    [SerializeField]
    GameObject custodyButton;
    [SerializeField]
    GameObject linkAccountButton;

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
        NetworkManager.Instance.ClearUserData();
        LevelManager.Instance.LoadScene("ConnectingWallet");
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Instance.linkedSuccesfully || FlowInterfaceBB.Instance.hasParentAddress)
        {
            confirmButton.SetActive(false);
            linkAccountButton.SetActive(false);
            custodyButton.SetActive(true);
            //bloctoAddress.text = NetworkManager.Instance.parentAddressPublic;
            bloctoAddress.text = PlayerPrefs.GetString("ParentAddress");
        }
        else
        {
            confirmButton.SetActive(true);
            linkAccountButton.SetActive(true);
            bloctoAddress.text = null;
        }
    }

    private IEnumerator Start()
    {
        while (!CoroutineHelper.Instance.AreAllCoroutinesFinished())
        {
            yield return null;
        }
        int totalWinsInt = 0;
        int attackerRecordsInt = 0;
        int defenderRecordsInt = 0;
        foreach (var challenge in NetworkManager.Instance.userChallengeData)
        {
            if (NetworkManager.Instance.userFlowAddress == challenge.winner)
            {
                totalWinsInt += 1;
            }
            if (NetworkManager.Instance.userFlowAddress == challenge.attacker.address)
            {
                attackerRecordsInt += 1;
            }
            else
            {
                defenderRecordsInt += 1;
            }
        }
        totalWins.text = totalWinsInt.ToString();
        attackRecords.text = attackerRecordsInt.ToString();
        defendRecords.text = defenderRecordsInt.ToString();
        totalScore.text = NetworkManager.Instance.userTotalScore;
        totalFights.text = NetworkManager.Instance.userChallengeData.Count.ToString();
        username.text = PlayerPrefs.GetString("Username");
        email.text = PlayerPrefs.GetString("Email");
    }
}
