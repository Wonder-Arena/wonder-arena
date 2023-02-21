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
    public static CharacterManager Instance { get; private set; }

    public List<CadenceNumber> listOfBeastsIDs = new();

    public List<GameObject> listOfAllSelectedUnits = new(3);

    [SerializeField]
    GameObject ui_CharaterSelection;
    [SerializeField]
    GameObject ui_SelectedUnits;
    //[SerializeField]
    //GameObject flowInterfaceReferenceObject;
    FlowInterfaceBB flowInterface;
    [SerializeField]
    List<GameObject> allBeastsPrefabs;


    private void Awake()
    {
        // Make our Instance to be the only one and for all scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        //flowInterface = flowInterfaceReferenceObject.GetComponent<FlowInterfaceBB>();
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
            listOfAllSelectedUnits.Add(null);
        }
    }

    // Setting all of out Beasts from Blockchain to UI to let player choose them
    public void SetAllAvailableBeasts()
    {
        string beastID = null;
        List<CadenceComposite> copy_allBeasts = new(flowInterface.allAvailableBeasts);
        foreach (CadenceComposite beast in copy_allBeasts)
        {
            foreach (CadenceCompositeField beastField in beast.Value.Fields)
            {
                switch (beastField.Name)
                {
                    case "id":
                        beastID = (beastField.Value as CadenceNumber).Value;
                        break;
                    case "name":
                        string nameOfBeast = (beastField.Value as CadenceString).Value;
                        foreach (GameObject selectionBeast in allBeastsPrefabs)
                        {
                            if (selectionBeast.name == nameOfBeast)
                            {
                                GameObject newSelectionBeast = Instantiate(selectionBeast, ui_CharaterSelection.transform);
                                newSelectionBeast.name = $"{selectionBeast.name}_{beastID}";
                            }
                        }
                        break;
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

        for (int i = 0; i < listOfAllSelectedUnits.Count; i++)
        {
            if (isSelectedCheck && listOfAllSelectedUnits[i] == null)
            {
                listOfAllSelectedUnits[i] = unit;
                break;
            }
            if (!isSelectedCheck && listOfAllSelectedUnits[i] != null && listOfAllSelectedUnits[i].name == unit.name)
            {
                listOfAllSelectedUnits[i] = null;
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
        for (int i = 0; i < listOfAllSelectedUnits.Count; i++)
        {
            if (listOfAllSelectedUnits[i] != null)
            {
                GameObject newSelectedUnit = Instantiate(listOfAllSelectedUnits[i], ui_SelectedUnits.transform.GetChild(i));
                newSelectedUnit.transform.localScale /= 1.5f;
                newSelectedUnit.name = listOfAllSelectedUnits[i].name;
                Destroy(newSelectedUnit.transform.GetChild(1).gameObject);
            }
        }
        SendSelectedUnitsToBlockchain();
    }

    public void SendSelectedUnitsToBlockchain()
    {
        // Sending all our Units to Blockchain
    }
}
