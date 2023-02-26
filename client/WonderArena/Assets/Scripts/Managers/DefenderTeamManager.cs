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
using TMPro;

public class DefenderTeamManager : MonoBehaviour
{
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

    public List<GameObject> listOfDefenderGroup = new(3);
    public bool haveDefenderComp;

    [SerializeField]
    GameObject ui_CharaterSelection;
    [SerializeField]
    GameObject ui_SelectedUnits;
    [SerializeField]
    TMP_InputField teamNameField;
    [SerializeField]
    List<GameObject> allBeastsPrefabs;
    [SerializeField]
    ChangeSceneButton sceneButton;

    [SerializeField]
    TextAsset AddDefenderGroupTxn;

    FlowInterfaceBB flowInterface;


    private void Awake()
    {
        sceneButton = sceneButton.GetComponent<ChangeSceneButton>();
        flowInterface = FlowInterfaceBB.Instance.GetComponent<FlowInterfaceBB>();
        teamNameField = teamNameField.GetComponent<TMP_InputField>();
    }

    private IEnumerator Start()
    {
        haveDefenderComp = false;
        listOfDefenderGroup = new(3);
        // Waiting for all scripts to be done before trying to get Beasts
        bool completed = false;
        while (!completed)
        {
            completed = true;
            completed = completed && flowInterface.isScriptsCompleted;
            yield return null;
        }

        yield return StartCoroutine(flowInterface.GetAllBeastsIDs());
        SetAllAvailableBeasts();

        // Adding Listeners to all Beasts Buttons
        foreach (Transform ui_Beast in ui_CharaterSelection.GetComponentInChildren<Transform>())
        {
            ui_Beast.GetComponent<Button>().onClick.AddListener(() => SelectDefender(ui_Beast.gameObject));
        }

        // Filling list of Selected Beasts with nulls
        for (int i = 0; i < 3; i++)
        {
            listOfDefenderGroup.Add(null);
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
            string nftId = nft.CompositeFieldAs<CadenceNumber>("id").Value;
            CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
            string nameOfPawn = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
            nameOfPawn += "_" + beastTemplate.CompositeFieldAs<CadenceString>("skin").Value;
            foreach (GameObject selectionBeast in allBeastsPrefabs)
            {
                if (selectionBeast.name == nameOfPawn)
                {
                    GameObject newSelectionBeast = Instantiate(selectionBeast, ui_CharaterSelection.transform);
                    newSelectionBeast.name = $"{selectionBeast.name}_{hpOfPawn}_{nftId}";
                    newSelectionBeast.transform.Find("Platform").gameObject.SetActive(false);
                }
            }
        }
    }

    // Selecting out Defenders from UI to actual list
    public void SelectDefender(GameObject unit)
    {
        bool isSelectedCheck;

        // Setting check mark to our unit
        Transform unitChildren = unit.transform.GetChild(1);
        isSelectedCheck = !unitChildren.gameObject.activeInHierarchy;
        unitChildren.gameObject.SetActive(isSelectedCheck);

        for (int i = 0; i < listOfDefenderGroup.Count; i++)
        {
            if (isSelectedCheck && listOfDefenderGroup[i] == null)
            {
                listOfDefenderGroup[i] = unit;
                break;
            }
            if (!isSelectedCheck && listOfDefenderGroup[i] != null && listOfDefenderGroup[i].name == unit.name)
            {
                listOfDefenderGroup[i] = null;
            }
        }

        // Deleting all of the previous selected units and refilling it again
        foreach (Transform child in ui_SelectedUnits.transform)
        {
            foreach (Transform childInChild in child)
            {
                Destroy(childInChild.gameObject);
            }
        }
        for (int i = 0; i < listOfDefenderGroup.Count; i++)
        {
            if (listOfDefenderGroup[i] != null)
            {
                GameObject newSelectedUnit = Instantiate(listOfDefenderGroup[i], ui_SelectedUnits.transform.GetChild(i));
                newSelectedUnit.name = listOfDefenderGroup[i].name;
                newSelectedUnit.transform.Find("SelectedBackground").gameObject.SetActive(false);
                newSelectedUnit.transform.Find("Background").gameObject.SetActive(false);
                newSelectedUnit.transform.Find("Shadow").gameObject.SetActive(false);
                newSelectedUnit.transform.Find("Platform").gameObject.SetActive(true);
            }
        }
    }

    public void ConfirmDefenderTeam()
    {
        StartCoroutine(waitForConfirmDefender());
    }

    private IEnumerator waitForConfirmDefender()
    {
        yield return StartCoroutine(SetDefenderTeam());
        sceneButton.ToDefendGroup();
    }

    private IEnumerator SetDefenderTeam()
    {
        haveDefenderComp = true;
        for (int i = 0; i < listOfDefenderGroup.Count; i++)
        {
            if (listOfDefenderGroup[i] == null)
            {
                haveDefenderComp = false;
                break;
            }
        }

        if (haveDefenderComp)
        {
            string _teamName = teamNameField.text;
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

            yield return StartCoroutine(AddDefenderTeam(GameManager.Instance.endpointPATH + GameManager.Instance.addDefenderTeamPATH,
                newJson, GameManager.Instance.userAccessToken));
        }
        else
        {
            Debug.Log("Select your comp");
        }
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

        request.Dispose();
    }
}
