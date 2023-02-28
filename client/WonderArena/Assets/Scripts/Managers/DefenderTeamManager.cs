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
    //[System.Serializable]
    //public class DefenderGroup
    //{
    //    [System.Serializable]
    //    public class Request
    //    {
    //        public string groupName;
    //        public List<int> beastIDs;
    //    }

    //    [System.Serializable]
    //    public class Response
    //    {
    //        public bool status;
    //        public string message;
    //        public Data data;
    //    }

    //    [System.Serializable]
    //    public class Data
    //    {

    //    }
    //}

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

    AudioClip clickSound;

    FlowInterfaceBB flowInterface;

    CoroutineHelper coroutineHelper;


    private void Awake()
    {
        coroutineHelper = CoroutineHelper.Instance;
        sceneButton = sceneButton.GetComponent<ChangeSceneButton>();
        flowInterface = FlowInterfaceBB.Instance;
        teamNameField = teamNameField.GetComponent<TMP_InputField>();
        clickSound = transform.GetComponent<AudioSource>().clip;
    }

    private IEnumerator Start()
    {
        haveDefenderComp = false;
        listOfDefenderGroup = new(3);
        // Waiting for all scripts to be done before trying to get Beasts

        //coroutineHelper.RunCoroutine("GetAllBeastsInTeamManager", flowInterface.GetAllBeastsIDs());
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
        yield return null;
    }

    // Setting all of out Beasts from Blockchain to UI to let player choose them
    public void SetAllAvailableBeasts()
    {
        if (ui_CharaterSelection.transform.childCount > 0)
        {
            foreach (GameObject beast in ui_CharaterSelection.transform)
            {
                if (beast != null)
                {
                    Destroy(beast);
                }
            }
        }
        
        List<CadenceComposite> allPawns = flowInterface.playerAllPawns_ListCadenceComposite;
        foreach (CadenceComposite pawn in allPawns)
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
        AudioSource.PlayClipAtPoint(clickSound, transform.position);
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

        List<string> beastsNames = new();

        foreach (GameObject attacker in listOfDefenderGroup)
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

    public void ConfirmDefenderTeam()
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

        if (NetworkManager.Instance.userDefenderGroups.Count > 3)
        {
            MessageManager.Instance.ShowMessage("You can only have maximum of 4 teams!", 2f);
        }

        else if (haveDefenderComp)
        {
            StartCoroutine(WaitForDefenders());
        }

        else
        {
            Debug.Log("Select your comp");
        }
    }  

    private IEnumerator WaitForDefenders()
    {
        coroutineHelper.RunCoroutine("SetDefenderTeam",
                NetworkManager.Instance.SetDefenderTeam(teamNameField.text, listOfDefenderGroup));
                
        LevelManager.Instance.LoadScene("DefendTeam");

        while (coroutineHelper.IsCoroutineRunning("SetDefenderTeam"))
        {
            yield return null;
        }
            
        coroutineHelper.RunCoroutine("GetUserDefenderGroup", FlowInterfaceBB.Instance.GetUserDefenderGroups());   
    }

}