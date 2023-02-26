using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    GameObject plusButton;
    [SerializeField]
    GameObject airDroppingLoadPage;

    private IEnumerator Start()
    {
        airDroppingLoadPage.SetActive(true);
        yield return StartCoroutine(WaitForAccount());
        yield return StartCoroutine(FlowInterfaceBB.Instance.GetUserDefenderGroups());
        if (GameManager.Instance.userDefenderGroups.Count > 0)
        {
            SetPlatforms();
        }
        else
        {
            SetPlusSign();
        }       
    }

    private void SetPlatforms()
    {
        List<string> beastsNames;
        List<string> teamsNames = new();
        foreach (KeyValuePair<string, List<string>> defenderGroup in GameManager.Instance.userDefenderGroups)
        {
            teamsNames.Add(defenderGroup.Key);
        }
        int randomIndex = Random.Range(0, GameManager.Instance.userDefenderGroups.Count);
        beastsNames = new(GameManager.Instance.userDefenderGroups[teamsNames[randomIndex]]);

        PlatformSetter.Instance.SetAllBeast(beastsNames);
    }

    private void SetPlusSign()
    {
        plusButton.SetActive(true);
    }

    private IEnumerator WaitForAccount()
    {
        TextMeshProUGUI airDropTextField = airDroppingLoadPage.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        while (GameManager.Instance.userFlowAddress == null)
        {
            int dots = ((int)(Time.time * 2.0f) % 4);
            airDropTextField.text = $"Registrating Flow Account{new string('.', dots)}\nAnd Airdropping Beasts <3";
            yield return null;
        }
        airDroppingLoadPage.SetActive(false);
    }
}
