using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public TabButton selectedTab;
    public int index;
    [SerializeField]
    LeaderBoardManager leaderBoard;

    private void Awake()
    {
        leaderBoard = leaderBoard.GetComponent<LeaderBoardManager>();
    }

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
        button.transform.GetChild(4).GetComponent<TextMeshProUGUI>().color = Color.black;
        button.transform.GetChild(5).GetComponent<TextMeshProUGUI>().color = Color.black;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetComponent<TabGroup>().index == i)
            {
                leaderBoard.selectedRightNow = transform.GetChild(i).GetChild(4).GetComponent<TextMeshProUGUI>().text;
                Debug.Log(leaderBoard.selectedRightNow);
            }
        }
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
            button.transform.GetChild(4).GetComponent<TextMeshProUGUI>().color = Color.white;
            button.transform.GetChild(5).GetComponent<TextMeshProUGUI>().color = Color.white;
        }
    }
}
