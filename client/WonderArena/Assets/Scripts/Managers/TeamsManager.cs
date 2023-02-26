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

    public Transform selectedTeam = null;

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

    public void SetPlatforms()
    {
        Transform beastGroup = selectedTeam.Find("BeastGroup");

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

    public void DeleteTeam()
    {
        StartCoroutine(RemoveDefenderGroup());
    }

    private IEnumerator RemoveDefenderGroup()
    {
        if (selectedTeam != null)
        {
            string _teamName = selectedTeam.Find("TeamName").GetComponent<TextMeshProUGUI>().text;

            DefenderGroup.Request defenderGroupRequest = JsonUtility.FromJson<DefenderGroup.Request>(@"{""groupName"": ""Figters""}");

            defenderGroupRequest.groupName = _teamName;

            string newJson = JsonUtility.ToJson(defenderGroupRequest);

            yield return StartCoroutine(RemoveTeam(GameManager.Instance.endpointPATH + GameManager.Instance.removeDefenderTeamPATH,
                newJson, GameManager.Instance.userAccessToken));
        }
    }
        

    private IEnumerator RemoveTeam(string url, string bodyJsonString, string accessToken)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        yield return request.SendWebRequest();

        while (!request.isDone)
        {
            int dots = ((int)(Time.time * 2.0f) % 4);
            Debug.Log("Deleting a team" + new string('.', dots));
            yield return null;
        }

        Debug.Log("Status Code: " + request.responseCode);
        Debug.Log(request.downloadHandler.text);

        DefenderGroup.Response response = JsonUtility.FromJson<DefenderGroup.Response>(request.downloadHandler.text);

        if (response.status == true)
        {
            Debug.Log(response.message);
        }
        else
        {
            Debug.Log(response.message);
        }

        request.Dispose();
    }

    [System.Serializable]
    public class DefenderGroup
    {
        [System.Serializable]
        public class Request
        {
            public string groupName;
        }

        [System.Serializable]
        public class Response
        {
            public bool status;
            public string message;
        }
    }
}
