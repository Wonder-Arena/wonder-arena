using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamsManager : MonoBehaviour
{
    [SerializeField]
    GameObject teamCardsParent;
    [SerializeField]
    GameObject teamCardPrefab;
    public Dictionary<string, List<GameObject>> userDefenderTeam = new();

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

            foreach (KeyValuePair<string, List<GameObject>> defenderGroup in userDefenderTeam)
            {
                GameObject newTeamCard = Instantiate(teamCardPrefab, teamCardsParent.transform);
                for (int i = 0; i < 3; i++)
                {
                    GameObject newBeastIcon = Instantiate(defenderGroup.Value[i], newTeamCard.transform.GetChild(3).GetChild(i));
                }
            }           
        }
    }
}
