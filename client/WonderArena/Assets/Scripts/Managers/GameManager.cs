using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public string userFlowAddress = null;
    public string userAccessToken = null;
    public string lastFightRecord = null;
    public string lastDefenderAddress = null;
    public string userTotalScore = null;

    public List<string> attackerComp = new();
    public List<string> lastDefenderNamesOfPawns = new();
    public List<Player.ChallengeData> userChallengeData = new(); 
    public Dictionary<string, List<string>> userDefenderGroups = new();

    public Dictionary<string, bool> gameManagerEnumerators = new();

    public bool linkedSuccesfully;

    #region APIs

    [Header("API URLs")]
    public string endpointPATH = "https://wonder-arena-production.up.railway.app";
    public string registerPATH = "/auth";
    public string loginPATH = "/auth/login";
    public string getPlayerPATH = "/auth/wonder_arena/players/";
    public string claimBBsPATH = "/auth/wonder_arena/get_bbs";
    public string addDefenderTeamPATH = "/auth/wonder_arena/add_defender_group";
    public string removeDefenderTeamPATH = "/auth/wonder_arena/remove_defender_group";
    public string fightPATH = "/auth/wonder_arena/fight";
    public string accountLinkingPATH = "/auth/flow/account_link";
    public string claimRewardsPATH = "/auth/wonder_arena/claim_reward";

    #endregion


    public static GameManager Instance { get; set; }
    private void Awake()
    {
        // Make our Instance only for and for all scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public bool HaveAttackerComp()
    {
        return true;
    }

    public void ClearUserData()
    {
        userFlowAddress = null;
        userAccessToken = null;
        lastFightRecord = null;
        lastDefenderAddress = null;
        userTotalScore = null;

        attackerComp = new();
        lastDefenderNamesOfPawns = new();
        userDefenderGroups = new();
        userChallengeData = new();
    }


    #region Networking
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
            public List<ChallengeData> challenges;
            public int score;
        }

        [System.Serializable]
        public class ChallengeData
        {
            public string id;
            public string winner;
            public List<string> attackerBeasts;
            public List<string> defenderBeasts;
            public string attackerScoreChange;
            public string defenderScoreChange;
        }

        [System.Serializable]
        public class FlowAccount
        {
            public string address;
        }
    }

    [System.Serializable]
    public class ClaimedBeast
    {
        [System.Serializable]
        public class Response
        {
            public bool status;
            public string message;
        }
    }

    [System.Serializable]
    public class ParentAddress
    {
        public string parentAddress;
    }

    [System.Serializable]
    public class AccountLinkResponse
    {
        public bool status;
        public string message;
    }

    public IEnumerator GetPlayer()
    {
        string url = endpointPATH + getPlayerPATH + PlayerPrefs.GetString("Username");
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {userAccessToken}");
        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        Player.Response response = JsonUtility.FromJson<Player.Response>(request.downloadHandler.text);

        if (response.status == true)
        {
            userTotalScore = response.data.score.ToString();
            userFlowAddress = response.data.flowAccount.address;
            userChallengeData = response.data.challenges;
        }
        else
        {
            Debug.Log(response.message);
        }

        request.Dispose();
    }

    public IEnumerator GetPlayerUpdate(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        while (userFlowAddress == null)
        {
            StartCoroutine(GetPlayer());
            yield return null;
        }    
    }

    public IEnumerator ClaimBBs()
    {
        var request = new UnityWebRequest(endpointPATH + claimBBsPATH, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {userAccessToken}");
        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        ClaimedBeast.Response response = JsonUtility.FromJson<ClaimedBeast.Response>(request.downloadHandler.text);

        Debug.Log(response.message);
        
        request.Dispose();
    }

    
    public void LinkAccount(string address)
    {
        ParentAddress parentAddress = JsonUtility.FromJson<ParentAddress>(@"{ ""parentAddress"": ""0xbca26f5091cd39ec""}");
        parentAddress.parentAddress = address;

        string newJson = JsonUtility.ToJson(parentAddress);
        Debug.Log(newJson);

        StartCoroutine(LinkAccountPost(endpointPATH + accountLinkingPATH, newJson));
    }


    private IEnumerator LinkAccountPost(string url, string bodyJsonString)
    {
        linkedSuccesfully = true;
        GameObject confirmationWindow = GameObject.Find("Confirmation Window");
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {userAccessToken}");
        request.SendWebRequest();

        while (!request.isDone)
        {
            int dots = ((int)(Time.time * 2.0f) % 4);
            confirmationWindow.transform.Find("MessageField").transform.GetComponent<TextMeshProUGUI>().text 
                = "Linking accounts" + new string('.', dots);
            yield return null;
        }

        Debug.Log("Status Code: " + request.responseCode);
        Debug.Log(request.downloadHandler.text);

        AccountLinkResponse response = JsonUtility.FromJson<AccountLinkResponse>(request.downloadHandler.text);

        string message;
        if (response.status == true)
        {
            message = "Succesfully linked accounts!";
        }
        else
        {
            linkedSuccesfully = false;
            confirmationWindow.transform.Find("MessageField").transform.GetComponent<TextMeshProUGUI>().color = Color.red;
            message = response.message;
        }

        confirmationWindow.transform.Find("MessageField").transform.GetComponent<TextMeshProUGUI>().text = message;

        request.Dispose();
    }

    #endregion



}
