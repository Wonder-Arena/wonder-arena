using DapperLabs.Flow.Sdk.Cadence;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LeaderBoardManager : MonoBehaviour
{
    [SerializeField]
    GameObject leaderboardRowPrefab;
    [SerializeField]
    GameObject contentParent;
    [SerializeField]
    Button challengePlayerButton;

    Dictionary<string, int> allPlayersScore = new();
    string selectedFlowAddress;
    public bool gotPlayer;
    
    FlowInterfaceBB flowInterface;

    private void Awake()
    {
        flowInterface = FlowInterfaceBB.Instance;
        challengePlayerButton = challengePlayerButton.GetComponent<Button>();
    }

    public void Update()
    {
        if (!gotPlayer)
        {
            challengePlayerButton.gameObject.SetActive(false);
        }
        else
        {
            challengePlayerButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator Start()
    {
        bool completed = false;
        while (!completed)
        {
            completed = true;
            completed = completed && flowInterface.isScriptsCompleted;
            yield return null;
        }

        foreach (CadenceDictionaryItem player in flowInterface.allPlayers_ListDictionaryItems)
        {
            string name = (player.Value as CadenceComposite).CompositeFieldAs<CadenceString>("name").Value;
            int score = int.Parse((player.Value as CadenceComposite).CompositeFieldAs<CadenceNumber>("score").Value);

            allPlayersScore.Add(name, score);
        }

        int index = 0;
        foreach (KeyValuePair<string, int> player in allPlayersScore.OrderBy(key => key.Value))
        {
            GameObject newRow = Instantiate(leaderboardRowPrefab, contentParent.transform);
            newRow.transform.GetComponent<TabButton>().tabGroup = contentParent.GetComponent<TabGroup>();

            int place = allPlayersScore.Count - index;
            string name = player.Key;
            int score = player.Value;

            if (place == 1)
            {
                newRow.transform.GetChild(3).gameObject.SetActive(true);
                newRow.transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                newRow.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"#{place}";
            }

            newRow.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = $"{name}";
            newRow.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = $"{score}";

            if (place % 2 == 0)
            {
                newRow.transform.GetComponent<TabButton>().basicIdleSprite = newRow.transform.GetChild(0).GetComponent<Image>().sprite;
            }
            else
            {
                newRow.transform.GetComponent<TabButton>().basicIdleSprite = contentParent.GetComponent<TabGroup>().tabIdle;
            }

            contentParent.GetComponent<TabGroup>().ResetTabs();

            index += 1;
        }
    }

    public void ChallengePlayer()
    {
        SceneManager.LoadScene("TeamMakingAttacking");
    }

    public IEnumerator GetPlayer(string selectedRightNow)
    {
        gotPlayer = false;
        string url = GameManager.Instance.endpointPATH + GameManager.Instance.getPlayerPATH + selectedRightNow;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {GameManager.Instance.userAccessToken}");
        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        Player.Response response = JsonUtility.FromJson<Player.Response>(request.downloadHandler.text);

        if (response.status == true)
        {
            gotPlayer = true;
            selectedFlowAddress = response.data.flowAccount.address;
            GameManager.Instance.lastDefenderAddress = selectedFlowAddress;
            Debug.Log(selectedFlowAddress);
        }
        else
        {
            GameManager.Instance.lastDefenderAddress = null;
            Debug.Log(response.message);
        }

    }

    [System.Serializable]
    public class Player
    {
        [System.Serializable]
        public class Response
        {
            public bool status;
            public string message;
            public Data data;
        }

        [System.Serializable]
        public class Data
        {
            public string email;
            public string name;
            public bool claimedBBs;
            public FlowAccount flowAccount;
        }

        [System.Serializable]
        public class FlowAccount
        {
            public string address;
        }
    }
}
