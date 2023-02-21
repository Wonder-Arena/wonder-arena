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
    // Test addresses - Alice and Bob
    public string alicesAddress = "0x1801c3f618a511e6";
    public string bobsAddress = "0x771519260bbe1ee6";

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

        StartCoroutine(GetPawns());

        yield return null;
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
        Task<FlowScriptResponse> getDefenderGroups = FLOW_ACCOUNT.ExecuteScript(GetDefenderGroupsTxn.text, new CadenceAddress(bobsAddress));

        yield return new WaitUntil(() => getDefenderGroups.IsCompleted);

        if (getDefenderGroups.Result.Error != null)
        {
            Debug.LogError($"Error:  {getDefenderGroups.Result.Error.Message}");
            yield break;
        }

        CadenceBase[] allDefenderGroups = (getDefenderGroups.Result.Value as CadenceArray).Value;

        // Adding all beasts to List of all beasts that user has
        foreach (CadenceComposite pawn in allPawns)
        {
            allAvailableBeasts.Add(pawn);
        }
    }
}
