using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamsManager : MonoBehaviour
{
    [SerializeField]
    GameObject teamCardsParent;
    [SerializeField]
    GameObject teamCardPrefab;
    [SerializeField]
    List<GameObject> allBeastsPrefabs;

    public Dictionary<string, List<string>> userDefenderTeam = new();

    private void Awake()
    {
        userDefenderTeam = GameManager.Instance.userDefenderGroups;
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
                        }
                    }
                }
            }           
        }
    }

    public void SetPlatforms(Transform buttonTransform)
    {
        Transform beastGroup = buttonTransform.Find("BeastGroup");

        //foreach (GameObject beastPrefab in allBeastsPrefabs)
        //{
        //    if (defenderGroup.Value[i].Split("_")[0] + "_" + defenderGroup.Value[i].Split("_")[1] == beastPrefab.name)
        //    {
        //        GameObject newBeastIcon = Instantiate(beastPrefab, newTeamCard.transform.Find("BeastGroup").GetChild(i));
        //        Destroy(newBeastIcon.transform.Find("Background").gameObject);
        //        Destroy(newBeastIcon.transform.Find("Platform").gameObject);
        //    }
        //}
    }
}
