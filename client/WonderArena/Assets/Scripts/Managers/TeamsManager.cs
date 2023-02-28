using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text;

public class TeamsManager : MonoBehaviour
{
    [SerializeField]
    GameObject teamCardsParent;
    [SerializeField]
    GameObject teamCardPrefab;
    [SerializeField]
    List<GameObject> allBeastsPrefabs;

    CoroutineHelper coroutineHelper;

    public Transform selectedTeam = null;

    public Dictionary<string, List<string>> userDefenderTeam = new();

    private void Awake()
    {
        coroutineHelper = CoroutineHelper.Instance;
        userDefenderTeam = NetworkManager.Instance.userDefenderGroups;
    }

    private void Start()
    {
        coroutineHelper.RunCoroutine("GetAllBeastsInTeamManager", FlowInterfaceBB.Instance.GetAllBeastsIDs());
    }

    private void Update()
    {
        if (teamCardsParent.transform.childCount != userDefenderTeam.Count)
        {
            foreach (Transform child in teamCardsParent.transform.GetComponentInChildren<Transform>())
            {
                Destroy(child.gameObject);
            }

            foreach (KeyValuePair<string, List<string>> defenderGroup in userDefenderTeam)
            {
                GameObject newTeamCard = Instantiate(teamCardPrefab, teamCardsParent.transform);
                newTeamCard.transform.Find("TeamName").GetComponent<TextMeshProUGUI>().text = defenderGroup.Key;
                newTeamCard.transform.GetComponent<TabButton>().tabGroup = teamCardsParent.transform.GetComponent<TabGroup>();
                for (int i = 0; i < 3; i++)
                {
                    foreach (GameObject beastPrefab in allBeastsPrefabs)
                    {
                        if (defenderGroup.Value[i].Split("_")[0] + "_" + defenderGroup.Value[i].Split("_")[1] == beastPrefab.name)
                        {
                            GameObject newBeastIcon = Instantiate(beastPrefab, newTeamCard.transform.Find("BeastGroup").GetChild(i));
                            Destroy(newBeastIcon.transform.Find("Background").gameObject);
                            Destroy(newBeastIcon.transform.Find("Platform").gameObject);
                            newBeastIcon.name = beastPrefab.name;
                        }
                    }
                }
            }           
        }
        else
        {
            userDefenderTeam = NetworkManager.Instance.userDefenderGroups;
        }
    }

    public void SetPlatforms()
    {
        List<string> beastsNames = new();
        Transform beastGroup = selectedTeam.Find("BeastGroup");
        for (int i = 0; i < beastGroup.childCount; i++)
        {
            beastsNames.Add(beastGroup.GetChild(i).GetChild(0).name);
        }
        PlatformSetter.Instance.SetAllBeast(beastsNames);
    }

    public void DeleteTeam()
    {
        coroutineHelper.RunCoroutine("RemoveDefenderGroup", 
            NetworkManager.Instance.RemoveDefenderGroup(selectedTeam));
        LevelManager.Instance.LoadScene("DefendTeam");
    }
}
