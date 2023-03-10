using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    //public bool isFightDone;

    //[System.Serializable]
    //public class Fight
    //{
    //    [System.Serializable]
    //    public class Body
    //    {
    //        public List<int> attackerIDs;
    //        public string defenderAddress;
    //    }

    //    [System.Serializable]
    //    public class Response
    //    {
    //        public bool status;
    //        public string message;
    //        public ResponseData data;
    //    }

    //    [System.Serializable]
    //    public class ResponseData
    //    {
    //        public string attacker;
    //        public string defender;
    //        public string challengeUUID;
    //    }
    //}

    public List<GameObject> listOfAttackerGroup = new(3);
    public bool haveAttackerComp;
    public bool madeANewComp;

    AudioClip clickSound;

    [SerializeField]
    GameObject ui_CharaterSelection;
    [SerializeField]
    GameObject ui_SelectedUnits;


    [SerializeField]
    List<GameObject> allBeastsPrefabs;
    FlowInterfaceBB flowInterface;

    CoroutineHelper coroutineHelper;


    private void Awake()
    {
        coroutineHelper = CoroutineHelper.Instance;
        flowInterface = FlowInterfaceBB.Instance.GetComponent<FlowInterfaceBB>();
        clickSound = transform.GetComponent<AudioSource>().clip;
    }

    private IEnumerator Start()
    {
        madeANewComp = false;

        yield return StartCoroutine(flowInterface.GetAllBeastsIDs());

        SetAllAvailableBeasts();

        // Adding Listeners to all Beasts Buttons
        foreach (Transform ui_Beast in ui_CharaterSelection.GetComponentInChildren<Transform>())
        {
            ui_Beast.GetComponent<Button>().onClick.AddListener(() => SelectUnit(ui_Beast.gameObject));
        }

        // Filling list of Selected Beasts with nulls
        for (int i = 0; i < 3; i++)
        {
            listOfAttackerGroup.Add(null);
        }
    }

    // Setting all of out Beasts from Blockchain to UI to let player choose them
    public void SetAllAvailableBeasts()
    {
        List<CadenceComposite> copy_allPawns = new(flowInterface.playerAllPawns_ListCadenceComposite);
        foreach (CadenceComposite pawn in copy_allPawns)
        {
            string hpOfPawn = pawn.CompositeFieldAs<CadenceNumber>("hp").Value;          
            CadenceComposite nft = pawn.CompositeFieldAs<CadenceComposite>("nft");
            CadenceComposite skill = pawn.CompositeFieldAs<CadenceComposite>("skill");
            string manaRequired = skill.CompositeFieldAs<CadenceNumber>("manaRequired").Value;
            string nftId = nft.CompositeFieldAs<CadenceNumber>("id").Value;
            CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
            string nameOfPawn = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
            nameOfPawn += "_"+beastTemplate.CompositeFieldAs<CadenceString>("skin").Value; 
            foreach (GameObject selectionBeast in allBeastsPrefabs)
            {
                if (selectionBeast.name == nameOfPawn)
                {
                    GameObject newSelectionBeast = Instantiate(selectionBeast, ui_CharaterSelection.transform);
                    newSelectionBeast.name = $"{selectionBeast.name}_{hpOfPawn}_{nftId}_{manaRequired}";
                    newSelectionBeast.transform.Find("Platform").gameObject.SetActive(false);
                }
            }
        }
    }

    // Selecting out Beasts from UI to actual list
    public void SelectUnit(GameObject unit)
    {
        AudioSource.PlayClipAtPoint(clickSound, transform.position);
        bool isSelectedCheck;

        // Setting check mark to our unit
        Transform unitChildren = unit.transform.GetChild(1);
        isSelectedCheck = !unitChildren.gameObject.activeInHierarchy;
        unitChildren.gameObject.SetActive(isSelectedCheck);

        for (int i = 0; i < listOfAttackerGroup.Count; i++)
        {
            if (isSelectedCheck && listOfAttackerGroup[i] == null)
            {
                listOfAttackerGroup[i] = unit;
                break;
            }
            if (!isSelectedCheck && listOfAttackerGroup[i] != null && listOfAttackerGroup[i].name == unit.name)
            {
                listOfAttackerGroup[i] = null;
            }
        }

        List<string> beastsNames = new();

        foreach (GameObject attacker in listOfAttackerGroup)
        {
            if (attacker != null)
            {
                beastsNames.Add(attacker.name);
            }
            else
            {
                beastsNames.Add(null);
            }      
        }

        PlatformSetter.Instance.SetAllBeast(beastsNames);
    }

    public void OnConfirmClicked()
    {
        haveAttackerComp = true;
        for (int i = 0; i < listOfAttackerGroup.Count; i++)
        {
            if (listOfAttackerGroup[i] == null)
            {
                haveAttackerComp = false;
                break;
            }
        }

        if (haveAttackerComp)
        {
            List<int> attackerCompIntId = new();
            NetworkManager.Instance.attackerComp = new();
            for (int i = 0; i < listOfAttackerGroup.Count; i++)
            {
                if (listOfAttackerGroup[i] != null)
                {
                    NetworkManager.Instance.attackerComp.Add(listOfAttackerGroup[i].name);
                    attackerCompIntId.Add(int.Parse(listOfAttackerGroup[i].name.Split("_")[3]));
                }
            }
            coroutineHelper.RunCoroutine("StartFight", 
                NetworkManager.Instance.StartFight(attackerCompIntId));
            LevelManager.Instance.LoadScene("FightScene");
            
        }
        else
        {
            Debug.Log("Select your comp");
        }
    }

    //private IEnumerator StartFight()
    //{
    //    haveAttackerComp = true;
    //    for (int i = 0; i < listOfAttackerGroup.Count; i++)
    //    {
    //        if (listOfAttackerGroup[i] == null)
    //        {
    //            haveAttackerComp = false;
    //            break;
    //        }
    //    }

    //    if (haveAttackerComp)
    //    {
    //        List<int> attackerCompIntId = new();
    //        GameManager.Instance.attackerComp = new();
    //        for (int i = 0; i < listOfAttackerGroup.Count; i++)
    //        {
    //            if (listOfAttackerGroup[i] != null)
    //            {
    //                GameManager.Instance.attackerComp.Add(listOfAttackerGroup[i].name);
    //                attackerCompIntId.Add(int.Parse(listOfAttackerGroup[i].name.Split("_")[3]));
    //            }
    //        }

    //        Fight.Body fightBody = JsonUtility.FromJson<Fight.Body>(FightTxn.text);

    //        fightBody.attackerIDs = attackerCompIntId;
    //        fightBody.defenderAddress = GameManager.Instance.lastDefenderAddress;

    //        string newJson = JsonUtility.ToJson(fightBody);

    //        yield return StartCoroutine(GetFightRecord(GameManager.Instance.endpointPATH + GameManager.Instance.fightPATH,
    //            newJson, GameManager.Instance.userAccessToken));

    //        if (madeANewComp == true)
    //        {
    //            LevelManager.Instance.LoadScene("FightScene");
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("Select your comp");
    //    } 
    //}

    //private IEnumerator GetFightRecord(string url, string bodyJsonString, string accessToken)
    //{
    //    var request = new UnityWebRequest(url, "POST");
    //    byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
    //    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //    request.downloadHandler = new DownloadHandlerBuffer();
    //    request.SetRequestHeader("Content-Type", "application/json");
    //    request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
    //    yield return request.SendWebRequest();

    //    while (!request.isDone)
    //    {
    //        int dots = ((int)(Time.time * 2.0f) % 4);
    //        Debug.Log("Making new team" + new string('.', dots));
    //        yield return null;
    //    }

    //    Debug.Log("Status Code: " + request.responseCode);
    //    Debug.Log(request.downloadHandler.text);

    //    Fight.Response response = JsonUtility.FromJson<Fight.Response>(request.downloadHandler.text);

    //    if (response.status == true)
    //    {
    //        Debug.Log(response.message);
    //        madeANewComp = true;
    //        GameManager.Instance.lastDefenderAddress = response.data.defender;
    //        GameManager.Instance.lastFightRecord = response.data.challengeUUID;     
    //        yield return StartCoroutine(flowInterface.GetFightsRecords());
    //    }
    //    else
    //    {
    //        Debug.Log(response.message);
    //    }

    //    isFightDone = true;

    //    request.Dispose();
    //}
}
