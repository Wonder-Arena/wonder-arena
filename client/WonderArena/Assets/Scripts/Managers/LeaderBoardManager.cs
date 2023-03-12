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

    [SerializeField]
    TextMeshProUGUI battlePlayed;

    [SerializeField]
    TextMeshProUGUI battleWon;

    Dictionary<string, int> allPlayersScore = new();
    //string selectedFlowAddress;
    public bool gotPlayer;
    
    FlowInterfaceBB flowInterface;

    private void Awake()
    {
        flowInterface = FlowInterfaceBB.Instance;
        challengePlayerButton = challengePlayerButton.GetComponent<Button>();
        battlePlayed = battlePlayed.GetComponent<TextMeshProUGUI>();
        battleWon = battleWon.GetComponent<TextMeshProUGUI>();
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
        yield return StartCoroutine(FlowInterfaceBB.Instance.GetAllPlayers());

        foreach (CadenceDictionaryItem player in flowInterface.allPlayers_ListDictionaryItems)
        {
            string name = (player.Value as CadenceComposite).CompositeFieldAs<CadenceString>("name").Value;
            int score = int.Parse((player.Value as CadenceComposite).CompositeFieldAs<CadenceNumber>("score").Value);
            bool isChallengable = (player.Value as CadenceComposite).CompositeFieldAs<CadenceBool>("isChallengable").Value;


            if (isChallengable)
            {
                if (allPlayersScore.ContainsKey(name))
                {
                    allPlayersScore[name] = score;
                }
                else
                {
                    allPlayersScore.Add(name, score);
                }
            }  
        }

        int index = 0;
        GameObject userRow = null;
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

            if (player.Key == NetworkManager.Instance.userUsername)
            {
                userRow = newRow;
            }

            if (place % 2 == 0)
            {
                newRow.transform.GetComponent<TabButton>().basicIdleSprite = newRow.transform.GetChild(0).GetComponent<Image>().sprite;
            }
            else
            {
                newRow.transform.GetComponent<TabButton>().basicIdleSprite = contentParent.GetComponent<TabGroup>().tabIdle;
            }

            index += 1;
        }

        contentParent.GetComponent<TabGroup>().ResetTabs();

        userRow.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = $"{NetworkManager.Instance.userUsername} (You)";
        userRow.transform.GetChild(4).GetComponent<TextMeshProUGUI>().color = Color.green;
    }

    public void ChallengePlayer()
    {
        LevelManager.Instance.LoadScene("TeamMakingAttacking");
    }

    public void GetPlayer(string selectedRightNow)
    {
        if (selectedRightNow == PlayerPrefs.GetString("Username"))
        {
            challengePlayerButton.gameObject.SetActive(false);
        }
        else
        {
            challengePlayerButton.gameObject.SetActive(false);
            gotPlayer = false;
            CoroutineHelper.Instance.RunCoroutine("GetPlayerLeaderBoard",
                NetworkManager.Instance.GetPlayerForLeaderBoard(selectedRightNow));
        }
    }

    public void SetPlayedData(List<NetworkManager.Player.ChallengeData> 
        challenges, string selectedAddress)
    {
        int battleWonInt = 0;
        foreach (var challenge in challenges)
        {
            if (challenge.winner == selectedAddress)
            {
                battleWonInt += 1;
            }
        }
        
        battlePlayed.text = challenges.Count().ToString();
        battleWon.text = battleWonInt.ToString();
    }
}

