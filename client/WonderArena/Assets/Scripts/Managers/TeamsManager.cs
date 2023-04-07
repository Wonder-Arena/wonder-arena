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

    public Dictionary<string, List<Beast.BeastStats>> userDefenderTeam = new();

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

            foreach (KeyValuePair<string, List<Beast.BeastStats>> defenderGroup in userDefenderTeam)
            {
                GameObject newTeamCard = Instantiate(teamCardPrefab, teamCardsParent.transform);
                newTeamCard.transform.Find("TeamName").GetComponent<TextMeshProUGUI>().text = defenderGroup.Key;
                newTeamCard.transform.GetComponent<TabButton>().tabGroup = teamCardsParent.transform.GetComponent<TabGroup>();
                for (int i = 0; i < 3; i++)
                {
                    foreach (GameObject beastPrefab in allBeastsPrefabs)
                    {
                        if (defenderGroup.Value[i].nameOfBeast + "_" + defenderGroup.Value[i].skin == beastPrefab.name)
                        {
                            GameObject newBeastIcon = Instantiate(beastPrefab, newTeamCard.transform.Find("BeastGroup").GetChild(i));
                            newBeastIcon.AddComponent<Beast>().CopyFrom(defenderGroup.Value[i]);
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
        List<Beast.BeastStats> beastsStats = new();
        Transform beastGroup = selectedTeam.Find("BeastGroup");
        for (int i = 0; i < beastGroup.childCount; i++)
        {
            beastsStats.Add(beastGroup.GetChild(i).GetChild(0).GetComponent<Beast>().beastStats);
        }
        PlatformSetter.Instance.SetAllBeast(beastsStats);
    }

    public void DeleteTeam()
    {
        coroutineHelper.RunCoroutine("RemoveDefenderGroup", 
            NetworkManager.Instance.RemoveDefenderGroup(selectedTeam));
        LevelManager.Instance.LoadScene("DefendTeam");
    }
}
