using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> listOfAttackerGroup = new(3);
    public bool haveAttackercomp; 

    [SerializeField]
    GameObject ui_CharaterSelection;
    [SerializeField]
    GameObject ui_SelectedUnits;
    FlowInterfaceBB flowInterface;
    [SerializeField]
    List<GameObject> allBeastsPrefabs;


    private void Awake()
    {
        flowInterface = FlowInterfaceBB.Instance.GetComponent<FlowInterfaceBB>();
    }

    private IEnumerator Start()
    {
        // Waiting for all scripts to be done before trying to get Beasts
        bool completed = false;
        while (!completed)
        {
            completed = true;
            completed = completed && flowInterface.isScriptsCompleted;
            yield return null;
        }

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
            string nftId = nft.CompositeFieldAs<CadenceNumber>("id").Value;
            CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
            string nameOfPawn = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
            nameOfPawn += "_"+beastTemplate.CompositeFieldAs<CadenceString>("skin").Value; 
            foreach (GameObject selectionBeast in allBeastsPrefabs)
            {
                if (selectionBeast.name == nameOfPawn)
                {
                    GameObject newSelectionBeast = Instantiate(selectionBeast, ui_CharaterSelection.transform);
                    newSelectionBeast.name = $"{selectionBeast.name}_{hpOfPawn}_{nftId}";
                }
            }
        }
    }

    // Selecting out Beasts from UI to actual list
    public void SelectUnit(GameObject unit)
    {
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

        // Deleting all of the previous selected units and refilling it again
        foreach (Transform child in ui_SelectedUnits.transform)
        {
            foreach (Transform childInChild in child)
            {
                Destroy(childInChild.gameObject);
            }
        }
        for (int i = 0; i < listOfAttackerGroup.Count; i++)
        {
            if (listOfAttackerGroup[i] != null)
            {
                GameObject newSelectedUnit = Instantiate(listOfAttackerGroup[i], ui_SelectedUnits.transform.GetChild(i));
                newSelectedUnit.name = listOfAttackerGroup[i].name;
                Destroy(newSelectedUnit.transform.GetChild(1).gameObject);
            }
        }
    }

    public void StartFight()
    {
        haveAttackercomp = true;
        for (int i = 0; i < listOfAttackerGroup.Count; i++)
        {
            if (listOfAttackerGroup[i] == null)
            {
                haveAttackercomp = false;
                break;
            }
        }

        if (haveAttackercomp)
        {
            for (int i = 0; i < listOfAttackerGroup.Count; i++)
            {
                if (listOfAttackerGroup[i] != null)
                {
                    GameManager.Instance.attackerComp.Add(listOfAttackerGroup[i].name);
                }
            }
            LevelManager.Instance.LoadScene("FightScene");
        }
        else
        {
            Debug.Log("Select your comp");
        }
        
    }
}
