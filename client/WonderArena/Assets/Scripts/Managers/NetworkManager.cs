using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    #region Variables

    public string userFlowAddress = null;
    public string userAccessToken = null;
    public string lastFightRecord = null;
    public string lastDefenderAddress = null;
    public string userTotalScore = null;
    public string parentAddressPublic = null;
    public string userUsername = null;

    public List<string> attackerComp = new();
    public List<Beast> lastDefenderBeasts = new();
    public List<Player.ChallengeData> userChallengeData = new();
    public Dictionary<string, List<string>> userDefenderGroups = new();

    public bool linkedSuccesfully;

    private bool madeANewComp;

    // Text Assets
    [SerializeField]
    TextAsset FightTxn;
    [SerializeField]
    TextAsset AddDefenderGroupTxn;

    #endregion

    #region APIs

    [Header("API URLs")]
    public string endpointPATH = "https://wonder-arena-production.up.railway.app";
    public string registerPATH = "/auth";
    public string loginPATH = "/auth/login";
    public string getPlayerPATH = "/auth/wonder_arena/players/";
    public string addDefenderTeamPATH = "/auth/wonder_arena/add_defender_group";
    public string removeDefenderTeamPATH = "/auth/wonder_arena/remove_defender_group";
    public string fightPATH = "/auth/wonder_arena/fight";
    public string accountLinkingPATH = "/auth/flow/account_link";
    public string claimRewardsPATH = "/auth/wonder_arena/claim_reward";
    public string stripePATH = "/auth/stripe/create_checkout_session";
    public string getPlayerWithChallengesPATH = "?basicOnly=false";
    public string googleLoginPATH = "/auth/google_login";

    public string responseStripeURL;

    #endregion

    #region Instance

    public static NetworkManager Instance { get; set; }
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

    #endregion

    #region Classes
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
            public Attacker attacker;
            public List<string> attackerBeasts;
            public Defender defender;
            public List<string> defenderBeasts;
            public string attackerScoreChange;
            public string defenderScoreChange;
        }

        [System.Serializable]
        public class FlowAccount
        {
            public string address;
        }

        [System.Serializable]
        public class Attacker
        {
            public string name;
            public string address;
        }

        [System.Serializable]
        public class Defender
        {
            public string name;
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

    [System.Serializable]
    public class Fight
    {
        [System.Serializable]
        public class Body
        {
            public List<int> attackerIDs;
            public string defenderAddress;
        }

        [System.Serializable]
        public class Response
        {
            public bool status;
            public string message;
            public ResponseData data;
        }

        [System.Serializable]
        public class ResponseData
        {
            public string attacker;
            public string defender;
            public string challengeUUID;
        }
    }

    [System.Serializable]
    public class DefenderGroup
    {
        [System.Serializable]
        public class Request
        {
            public string groupName;
            public List<int> beastIDs;
        }

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

        }
    }

    [System.Serializable]
    public class Stripe
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
            public string sessionID;
            public string sessionURL;
        }
    }

    #endregion

    #region JsonUtility

    // Get player's information
    public IEnumerator GetPlayer()
    {
        string url = endpointPATH + getPlayerPATH + PlayerPrefs.GetString("Username") + getPlayerWithChallengesPATH;
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
        do
        {
            CoroutineHelper.Instance.RunCoroutine("GetPlayerUpdate", GetPlayer());
            yield return new WaitForSeconds(seconds);
            yield return null;
        } while (userFlowAddress == null);
    }

    public IEnumerator LinkAccountPost(string url, string bodyJsonString)
    {
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
            confirmationWindow.transform.Find("MessageField").transform.GetComponent<TextMeshProUGUI>().color = Color.white;
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
            linkedSuccesfully = true;
            message = "Succesfully linked accounts!";
            PlayerPrefs.SetString("ParentAddress", parentAddressPublic);
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

    // Fight and Attacker comp
    public IEnumerator StartFight(List<int> attackerCompIntId)
    {
        Fight.Body fightBody = JsonUtility.FromJson<Fight.Body>(FightTxn.text);

        fightBody.attackerIDs = attackerCompIntId;
        fightBody.defenderAddress = lastDefenderAddress;

        string newJson = JsonUtility.ToJson(fightBody);

        CoroutineHelper.Instance.RunCoroutine("GetFightRecord", GetFightRecord(endpointPATH + fightPATH,
            newJson, userAccessToken));
        yield return null;
    }

    public IEnumerator GetFightRecord(string url, string bodyJsonString, string accessToken)
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
            Debug.Log("Making new team" + new string('.', dots));
            yield return null;
        }

        Debug.Log("Status Code: " + request.responseCode);
        Debug.Log(request.downloadHandler.text);

        Fight.Response response = JsonUtility.FromJson<Fight.Response>(request.downloadHandler.text);

        if (response.status == true)
        {
            Debug.Log(response.message);
            madeANewComp = true;
            lastDefenderAddress = response.data.defender;
            lastFightRecord = response.data.challengeUUID;
            CoroutineHelper.Instance.RunCoroutine("GetFightRecordsInsideFlowInterface", FlowInterfaceBB.Instance.GetFightsRecords());
        }
        else
        {
            MessageManager.Instance.SendMessage("Something went wrong: " + response.message);
            yield return new WaitForSeconds(1f);
            LevelManager.Instance.LoadScene("MainMenu");
        }

        request.Dispose();
    }

    // Defender team

    public IEnumerator SetDefenderTeam(string teamNameField, List<GameObject> listOfDefenderGroup)
    {
        string _teamName = teamNameField;
        List<int> _beastIds = new();

        foreach (GameObject defender in listOfDefenderGroup)
        {
            int.TryParse(defender.name.Split("_")[3], out int intID);
            _beastIds.Add(intID);
        }

        DefenderGroup.Request defenderGroupRequest = JsonUtility.FromJson<DefenderGroup.Request>(AddDefenderGroupTxn.text);

        defenderGroupRequest.groupName = _teamName;
        defenderGroupRequest.beastIDs = _beastIds;

        string newJson = JsonUtility.ToJson(defenderGroupRequest);

        yield return StartCoroutine(AddDefenderTeam(endpointPATH + addDefenderTeamPATH,
            newJson, userAccessToken));

    }

    private IEnumerator AddDefenderTeam(string url, string bodyJsonString, string accessToken)
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
            Debug.Log("Making new team" + new string('.', dots));
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

        yield return StartCoroutine(FlowInterfaceBB.Instance.GetUserDefenderGroups());

        request.Dispose();
    }

    // Leaderboard
    public IEnumerator GetPlayerForLeaderBoard(string selectedRightNow)
    {
        yield return new WaitForSeconds(0.5f);
        string url = endpointPATH + getPlayerPATH + selectedRightNow  + getPlayerWithChallengesPATH;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {userAccessToken}");
        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        Player.Response response = JsonConvert.DeserializeObject<Player.Response>(request.downloadHandler.text);

        if (response.status == true)
        {
            GameObject.FindObjectOfType<LeaderBoardManager>().gotPlayer = true;
            string selectedFlowAddress = response.data.flowAccount.address;
            lastDefenderAddress = selectedFlowAddress;
            GameObject.FindObjectOfType<LeaderBoardManager>().SetPlayedData(response.data.challenges,
                selectedFlowAddress);
            Debug.Log(selectedFlowAddress);
        }
        else
        {
            lastDefenderAddress = null;
            Debug.Log(response.message);
        }

        request.Dispose();

    }

    // Teams manager
    public IEnumerator RemoveDefenderGroup(Transform selectedTeam)
    {
        if (selectedTeam != null)
        {
            string _teamName = selectedTeam.Find("TeamName").GetComponent<TextMeshProUGUI>().text;

            selectedTeam.gameObject.SetActive(false);

            DefenderGroup.Request defenderGroupRequest = JsonUtility.FromJson<DefenderGroup.Request>(@"{""groupName"": ""Figters""}");

            defenderGroupRequest.groupName = _teamName;

            string newJson = JsonUtility.ToJson(defenderGroupRequest);

            yield return StartCoroutine(RemoveTeam(endpointPATH + removeDefenderTeamPATH,
                newJson, userAccessToken, selectedTeam));
        }
        else
        {
            Debug.Log("Please, select team to delete!");
        }
    }

    private IEnumerator RemoveTeam(string url, string bodyJsonString, 
        string accessToken, Transform selectedTeam)
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
            if (selectedTeam != null)
            {
                Destroy(selectedTeam.gameObject);
            }
            Debug.Log(response.message);
        }
        else
        {
            selectedTeam.gameObject.SetActive(true);
            Debug.Log(response.message);
        }

        request.Dispose();
    }

    public IEnumerator GetStripeCheckout(string tokenId)
    {
        // Create a dictionary with the "tokenId" key and the given value
        var data = new Dictionary<string, int>();
        data.Add("tokenId", int.Parse(tokenId));

        // Serialize the dictionary as a JSON string
        var jsonData = JsonConvert.SerializeObject(data);

        Debug.Log(jsonData);

        var request = new UnityWebRequest(endpointPATH + stripePATH, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + userAccessToken);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error posting JSON: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("JSON posted successfully!");
            Debug.Log(request.downloadHandler.text);

            // Parse the response JSON as a dictionary
            Stripe.Response response = JsonConvert.DeserializeObject<Stripe.Response>(request.downloadHandler.text);

            // Get the "sessionURL" value from the dictionary
            if (response.status == true)
            {
                responseStripeURL = response.data.sessionURL;
                Debug.Log("Session URL: " + responseStripeURL);
            }
            else
            {
                Debug.Log(response.message);
            }
        }
    }

    #endregion

    #region Functions

    public void LinkAccount(string address)
    {
        ParentAddress parentAddress = JsonUtility.FromJson<ParentAddress>(@"{ ""parentAddress"": ""0xbca26f5091cd39ec""}");
        parentAddress.parentAddress = address;
        parentAddressPublic = address;

        string newJson = JsonUtility.ToJson(parentAddress);
        Debug.Log(newJson);

        StartCoroutine(LinkAccountPost(endpointPATH + accountLinkingPATH, newJson));
    }

    public void ClearUserData()
    {
        userFlowAddress = null;
        userAccessToken = null;
        lastFightRecord = null;
        lastDefenderAddress = null;
        userTotalScore = null;
        responseStripeURL = null;
        parentAddressPublic = null;
        userUsername = null;

        attackerComp = new();
        lastDefenderNamesOfPawns = new();
        userDefenderGroups = new();
        userChallengeData = new();
    }

    #endregion
}
