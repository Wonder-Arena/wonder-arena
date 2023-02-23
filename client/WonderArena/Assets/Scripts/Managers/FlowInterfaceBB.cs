using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class FlowInterfaceBB : MonoBehaviour
{
    #region Classes and NetworkManager

    [System.Serializable]
    public class GetPlayer
    {
        [System.Serializable]
        public class Player
        {
            public bool status;
            public string message;
            public PlayerData data;
        }   
        
        public class PlayerData
        {

        }
    }

    #endregion


    // Test addresses - Alice and Bob
    public string alicesAddress = "0x1801c3f618a511e6";
    public string bobsAddress = "0x771519260bbe1ee6";

    public List<CadenceNumber> allPlayersBeastsIDs = new();
    public List<CadenceComposite> allAvailableBeasts = new();
    public bool isScriptsCompleted = false;

    // FLOW account object - set via Login screen.
    [Header("FLOW Account")]
    public FlowControl.Account FLOW_ACCOUNT = null;

    // The TextAssets containing Cadence scripts and transactions that will be used for the game
    [Header("Scripts and Transactions")]
    [SerializeField] TextAsset RegisterNewPlayerTxn;
    [SerializeField] TextAsset AddDefenderGroupTxn;
    [SerializeField] TextAsset RemoveDefenderGroupTxn;
    [SerializeField] TextAsset Fight_OnlyForAdminTxn;

    // Cadence scripts to get the data for Beasts
    [Header("Beasts Scripts")]
    [SerializeField] TextAsset GetAllBeastsTxn;
    [SerializeField] TextAsset GetAllPlayersTxn;
    [SerializeField] TextAsset GetBeastsIdsTxn;
    [SerializeField] TextAsset GetChallengeRecordsTxn;
    [SerializeField] TextAsset GetDefenderGroupsTxn;
    [SerializeField] TextAsset GetPlayerTxn;
    [SerializeField] TextAsset GetBeastByIdTxn;

    private static FlowInterfaceBB m_instance = null;
    public static FlowInterfaceBB Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<FlowInterfaceBB>();
                DontDestroyOnLoad(m_instance);
            }

            return m_instance;
        }
    }

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(this);
        }

        // Register DevWallet
        FlowSDK.RegisterWalletProvider(ScriptableObject.CreateInstance<DevWalletProvider>());
    }

    private IEnumerator Start()
    {
        //*****************//
        // Change it Later //
        //*****************//
        foreach (var account in FlowControl.Data.Accounts)
        {
            if (account.Name == "testnet_service_account")
            {
                FLOW_ACCOUNT = account;
            }
        }

        isScriptsCompleted = true;

        //StartCoroutine(GetPawns());
        StartCoroutine(GetAllBeastsIDs());
        //StartCoroutine(GetDefenderGroups());

        yield return null;
    }

    // Executing script to get all Beasts IDs that user has and adding them to allPlayersBeastsIDs list
    private IEnumerator GetAllBeastsIDs()
    {
        Task<FlowScriptResponse> getBeastsIDs = FLOW_ACCOUNT.ExecuteScript(GetBeastsIdsTxn.text, new CadenceAddress(alicesAddress));

        yield return new WaitUntil(() => getBeastsIDs.IsCompleted);

        if (getBeastsIDs.Result.Error != null)
        {
            Debug.LogError($"Error:  {getBeastsIDs.Result.Error.Message}");
            yield break;
        }

        isScriptsCompleted = true;

        CadenceBase[] allBeastsIDs = (getBeastsIDs.Result.Value as CadenceArray).Value;

        foreach (CadenceNumber beastID in allBeastsIDs)
        {
            allPlayersBeastsIDs.Add(beastID);
        }
    }

    private IEnumerator GetPawns()
    {
        // Executing script to get all Pawns from account
        Task<FlowScriptResponse> getPawns = FLOW_ACCOUNT.ExecuteScript(GetAllBeastsTxn.text, new CadenceAddress(bobsAddress));

        yield return new WaitUntil(() => getPawns.IsCompleted);

        if (getPawns.Result.Error != null)
        {
            Debug.LogError($"Error:  {getPawns.Result.Error.Message}");
            yield break;
        }

        isScriptsCompleted = true;

        CadenceBase[] allPawns = (getPawns.Result.Value as CadenceArray).Value;

        // Adding all beasts to List of all beasts that user has
        foreach (CadenceComposite pawn in allPawns)
        {
            allAvailableBeasts.Add(pawn);
        }
    }

    private IEnumerator GetDefenderGroups()
    {
        Task<FlowScriptResponse> getDefenderGroups = FLOW_ACCOUNT.ExecuteScript(GetDefenderGroupsTxn.text, new CadenceAddress(alicesAddress));

        yield return new WaitUntil(() => getDefenderGroups.IsCompleted);

        if (getDefenderGroups.Result.Error != null)
        {
            Debug.LogError($"Error:  {getDefenderGroups.Result.Error.Message}");
            yield break;
        }

        CadenceBase[] allDefenderGroups = (getDefenderGroups.Result.Value as CadenceArray).Value;

        foreach (CadenceArray defenderGroup in allDefenderGroups)
        {
            foreach (CadenceNumber defenderBeastId in defenderGroup.Value)
            {
                StartCoroutine(GetBeastById(defenderBeastId));
            }
        }
    }

    private IEnumerator GetBeastById(CadenceNumber idOfBeast)
    {
        Task<FlowScriptResponse> getBeastById = FLOW_ACCOUNT.ExecuteScript(GetBeastByIdTxn.text, 
            new CadenceAddress(alicesAddress), idOfBeast);

        yield return new WaitUntil(() => getBeastById.IsCompleted);

        if (getBeastById.Result.Error != null)
        {
            Debug.LogError($"Error:  {getBeastById.Result.Error.Message}");
            yield break;
        }

        CadenceComposite beastById = (getBeastById.Result.Value as CadenceComposite);
        
        foreach (CadenceCompositeField beastField in beastById.Value.Fields)
        {
            switch (beastField.Name)
            {
                case ("beastTemplate"):
                    foreach (CadenceCompositeField beastTemplateField in (beastField.Value as CadenceComposite).Value.Fields)
                    {
                        if (beastTemplateField.Name == "name")
                        {
                            
                        }

                        if (beastTemplateField.Name == "skin")
                        {
                            
                        }
                    }
                    break;            
            }
        }
    }
}
