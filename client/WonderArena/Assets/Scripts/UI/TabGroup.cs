using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public TabButton selectedTab;
    public int index;
    [SerializeField]
    GameObject currentSceneManager;

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        tabButtons.Add(button);
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.sprite = tabActive;
        index = button.transform.GetSiblingIndex();

        if (SceneManager.GetActiveScene().name == "Leaderboard")
        {
            button.transform.GetChild(4).GetComponent<TextMeshProUGUI>().color = Color.black;
            button.transform.GetChild(5).GetComponent<TextMeshProUGUI>().color = Color.black;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetComponent<TabGroup>().index == i)
            {
                if (SceneManager.GetActiveScene().name == "Leaderboard")
                {
                    GetPlayerInLeaderBoard(i);
                }
                else if (SceneManager.GetActiveScene().name == "DefendTeam")
                {
                    GetTeamInDefendTeam(i);
                }
                else if (SceneManager.GetActiveScene().name == "BuyBeasts")
                {
                    GetToBuyBeast(i);
                }

            }
        }
    }

    private void GetPlayerInLeaderBoard(int index)
    {
        currentSceneManager.GetComponent<LeaderBoardManager>().GetPlayer(transform.GetChild(index).GetChild(4).GetComponent<TextMeshProUGUI>().text);
    }

    private void GetTeamInDefendTeam(int index)
    {
        currentSceneManager.transform.GetComponent<TeamsManager>().selectedTeam = transform.GetChild(index).transform;
        currentSceneManager.transform.GetComponent<TeamsManager>().SetPlatforms();
    }

    private void GetToBuyBeast(int index)
    {  
        string nameOfBeast = transform.GetChild(index).name.Split("_")[0] + "_" + transform.GetChild(index).name.Split("_")[1];
        Debug.Log(nameOfBeast);
        foreach (KeyValuePair<string, string> beast in FlowInterfaceBB.Instance.beastsForListingDictionary)
        {
            if (beast.Value == nameOfBeast)
            {
                Debug.Log(beast.Value + " " + beast.Key);
                currentSceneManager.GetComponent<ShopManager>().idOfBeast = beast.Key;
                return;
            }
        }
        currentSceneManager.GetComponent<ShopManager>().idOfBeast = null;
    }

    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab) 
            {
                continue;
            }

            button.background.sprite = button.basicIdleSprite;

            if (SceneManager.GetActiveScene().name == "Leaderboard")
            {
                button.transform.GetChild(4).GetComponent<TextMeshProUGUI>().color = Color.white;
                button.transform.GetChild(5).GetComponent<TextMeshProUGUI>().color = Color.white;
            } 
        }
    }
}
