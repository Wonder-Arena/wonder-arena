using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string userFlowAddress = null;
    public string userAccessToken = null;
    public string lastFightRecord = null;
    public string lastDefenderAddress = null;

    public List<string> attackerComp = new();
    public List<string> lastDefenderNamesOfPawns = new();
    public Dictionary<string, List<GameObject>> userDefenderGroups = new();

    public static GameManager Instance { get; private set; }
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


    public IEnumerator GetPlayer()
    {
        string url = "https://wonder-arena-production.up.railway.app/auth/players/" + PlayerPrefs.GetString("Username");
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {userAccessToken}");
        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        Player.Response response = JsonUtility.FromJson<Player.Response>(request.downloadHandler.text);

        if (response.status == true)
        {
            userFlowAddress = response.data.flowAccount.address;
        }
        else
        {
            Debug.Log(response.message);
        }

    }

    public IEnumerator ClaimBBs()
    {
        string url = "https://wonder-arena-production.up.railway.app/auth/wonder_arena/get_bbs";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {userAccessToken}");
        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

        ClaimedBeast.Response response = JsonUtility.FromJson<ClaimedBeast.Response>(request.downloadHandler.text);

        Debug.Log(response.message);

    }

    #endregion



}
