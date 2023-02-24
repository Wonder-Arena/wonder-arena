using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;

public class FlowInterfaceBB : MonoBehaviour
{
    // Test addresses - Alice and Bob
    public string alicesAddress = "0x1801c3f618a511e6";
    public string bobsAddress = "0x771519260bbe1ee6";

    // Actual address;
    public string userFlowAddress = null;

    public CadenceComposite challengeRecords;
    public List<CadenceComposite> allPlayers_ListCadenceComposite = new();
    public CadenceBase[] playerAllBeastsIDs_CadenceBaseArray;
    public List<CadenceComposite> playerAllPawns_ListCadenceComposite = new();
    public bool isScriptsCompleted = false;

    // FLOW account object - set via Login screen.
    [Header("FLOW Account")]
    public FlowControl.Account FLOW_ACCOUNT = null;

    // Cadence scripts to get the data for Beasts
    [Header("Beasts Scripts")]
    [SerializeField] TextAsset GetAllPlayersTxn;
    [SerializeField] TextAsset GetBeastsIdsTxn;
    [SerializeField] TextAsset GetChallengeRecordsTxn;
    [SerializeField] TextAsset GetDefenderGroupsTxn;
    [SerializeField] TextAsset GetPlayerTxn;
    [SerializeField] TextAsset GetPawnsTxn;

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
        // Change it Later // or no....
        //*****************//
        foreach (var account in FlowControl.Data.Accounts)
        {
            if (account.Name == "testnet_service_account")
            {
                FLOW_ACCOUNT = account;
            }
        }

        userFlowAddress = GameManager.Instance.userFlowAddress;
        userFlowAddress = "0x2edad002a7c6a41d";

        if (PlayerPrefs.HasKey("Username"))
        {
            yield return StartCoroutine(GetAllPlayers());
            yield return StartCoroutine(GetAllBeastsIDs());
            yield return StartCoroutine(GetAllPlayerPawns());
            yield return StartCoroutine(GetFightsRecords());

            //yield return StartCoroutine(GetDefenderGroups());

            isScriptsCompleted = true;
        }
        else
        {
            Debug.LogError("User is not registred!");
        }
    }

    // Getting all players
    private IEnumerator GetAllPlayers()
    {
        Task<FlowScriptResponse> getAllPLayers = FLOW_ACCOUNT.ExecuteScript(GetAllPlayersTxn.text);

        yield return new WaitUntil(() => getAllPLayers.IsCompleted);

        if (getAllPLayers.Result.Error != null)
        {
            Debug.LogError($"Error:  {getAllPLayers.Result.Error.Message}");
            yield break;
        }

        CadenceBase[] allPLayers = (getAllPLayers.Result.Value as CadenceArray).Value;

        foreach (CadenceComposite player in allPLayers)
        {
            allPlayers_ListCadenceComposite.Add(player);
        }
    }

    // Executing script to get all Beasts IDs that user has and adding them to allPlayersBeastsIDs list
    private IEnumerator GetAllBeastsIDs()
    {
        Task<FlowScriptResponse> getBeastsIDs = FLOW_ACCOUNT.ExecuteScript(GetBeastsIdsTxn.text, new CadenceAddress(userFlowAddress));

        yield return new WaitUntil(() => getBeastsIDs.IsCompleted);

        if (getBeastsIDs.Result.Error != null)
        {
            Debug.LogError($"Error:  {getBeastsIDs.Result.Error.Message}");
            yield break;
        }

        playerAllBeastsIDs_CadenceBaseArray = (getBeastsIDs.Result.Value as CadenceArray).Value;
    }

    private IEnumerator GetAllPlayerPawns()
    {
        // Executing script to get all Pawns from account
        Task<FlowScriptResponse> getPawns = FLOW_ACCOUNT.ExecuteScript(GetPawnsTxn.text, 
            new CadenceAddress(userFlowAddress), new CadenceArray(playerAllBeastsIDs_CadenceBaseArray));

        yield return new WaitUntil(() => getPawns.IsCompleted);

        if (getPawns.Result.Error != null)
        {
            Debug.LogError($"Error:  {getPawns.Result.Error.Message}");
            yield break;
        }

        CadenceBase[] allPawns = (getPawns.Result.Value as CadenceArray).Value;

        // Adding all pawns to List of all pawns that user has
        foreach (CadenceComposite pawn in allPawns)
        {
            playerAllPawns_ListCadenceComposite.Add(pawn);
        }
    }

    private IEnumerator GetFightsRecords()
    {
        Task<FlowScriptResponse> getChallengeRecords = FLOW_ACCOUNT.ExecuteScript(GetChallengeRecordsTxn.text,
            new CadenceAddress("0x2edad002a7c6a41d"), new CadenceAddress("0x2c105ca4d4e260b1"), new CadenceNumber(CadenceNumberType.UInt64, "133415704"));

        yield return new WaitUntil(() => getChallengeRecords.IsCompleted);

        if (getChallengeRecords.Result.Error != null)
        {
            Debug.LogError($"Error:  {getChallengeRecords.Result.Error.Message}");
            yield break;
        }

        //challengeRecords = (getChallengeRecords.Result.Value as CadenceComposite);
        CadenceOptional challengeRecord = (getChallengeRecords.Result.Value as CadenceOptional);

        if (challengeRecord.Value != null)
        {
            CadenceComposite challengeComposite = challengeRecord.Value as CadenceComposite;
            foreach (CadenceCompositeField field in challengeComposite.Value.Fields)
            {
                Debug.Log(field.Name);
            }
        }   
    }
}