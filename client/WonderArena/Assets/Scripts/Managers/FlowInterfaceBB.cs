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

    public CadenceOptional challengeRecords;
    public List<CadenceComposite> allPlayers_ListCadenceComposite = new();
    public List<CadenceDictionaryItem> allPlayers_ListDictionaryItems = new();
    public CadenceBase[] playerAllBeastsIDs_CadenceBaseArray;
    public List<CadenceComposite> playerAllPawns_ListCadenceComposite = new();
    public bool isScriptsCompleted = false;
    public Dictionary<string, bool> isScriptsCompletedDictionary = new();

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

        while (GameManager.Instance.userFlowAddress == null)
        {
            int dots = ((int)(Time.time * 2.0f) % 4);
            Debug.Log("Registrating Flow Account" + new string('.', dots));
            StartCoroutine(GameManager.Instance.GetPlayer());
            yield return null;
        }

        StartCoroutine(GameManager.Instance.ClaimBBs());

        userFlowAddress = GameManager.Instance.userFlowAddress;

        if (PlayerPrefs.HasKey("Username"))
        {
            yield return StartCoroutine(GetAllPlayers());
            //yield return StartCoroutine(GetAllBeastsIDs());

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
        
        CadenceDictionary allPLayers = (getAllPLayers.Result.Value as CadenceDictionary);

        foreach (CadenceDictionaryItem player in allPLayers.Value)
        {
            allPlayers_ListDictionaryItems.Add(player);
        }
    }

    // Executing script to get all Beasts IDs that user has and adding them to allPlayersBeastsIDs list
    public IEnumerator GetAllBeastsIDs()
    {
        Task<FlowScriptResponse> getBeastsIDs = FLOW_ACCOUNT.ExecuteScript(GetBeastsIdsTxn.text, new CadenceAddress(userFlowAddress));
        
        yield return new WaitUntil(() => getBeastsIDs.IsCompleted);

        if (getBeastsIDs.Result.Error != null)
        {
            Debug.LogError($"Error:  {getBeastsIDs.Result.Error.Message}");
            yield break;
        }

        playerAllBeastsIDs_CadenceBaseArray = (getBeastsIDs.Result.Value as CadenceArray).Value;

        yield return StartCoroutine(GetAllPlayerPawns());
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

    public IEnumerator GetDefenderPawnsNames(CadenceBase[] beastIds)
    {
        GameManager.Instance.lastDefenderNamesOfPawns = new();
        // Executing script to get all Pawns from account
        Task<FlowScriptResponse> getPawns = FLOW_ACCOUNT.ExecuteScript(GetPawnsTxn.text,
            new CadenceAddress(GameManager.Instance.lastDefenderAddress), new CadenceArray(beastIds));

        yield return new WaitUntil(() => getPawns.IsCompleted);

        if (getPawns.Result.Error != null)
        {
            Debug.LogError($"Error:  {getPawns.Result.Error.Message}");
            yield break;
        }

        CadenceBase[] allPawns = (getPawns.Result.Value as CadenceArray).Value;

        Debug.Log("Start adding names");
        // Adding all pawns to List of all pawns that user has
        foreach (CadenceComposite pawn in allPawns)
        {
            string nameOfPawn = null;
            string idOfPawn = null;
            string hpOfPawn = null;
            hpOfPawn = pawn.CompositeFieldAs<CadenceNumber>("hp").Value;
            CadenceComposite nft = pawn.CompositeFieldAs<CadenceComposite>("nft");
            idOfPawn = nft.CompositeFieldAs<CadenceNumber>("id").Value;
            CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
            nameOfPawn = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
            nameOfPawn += "_" + beastTemplate.CompositeFieldAs<CadenceString>("skin").Value;

            nameOfPawn += "_" + hpOfPawn + "_" + idOfPawn;
            Debug.Log(nameOfPawn);

            GameManager.Instance.lastDefenderNamesOfPawns.Add(nameOfPawn);
        }
    }

    public IEnumerator GetUserDefenderGroups()
    {
        GameManager.Instance.userDefenderGroups = new();
        // Executing script to get all Defenders Id from account
        Task<FlowScriptResponse> getDefendersIds = FLOW_ACCOUNT.ExecuteScript(GetDefenderGroupsTxn.text,
            new CadenceAddress(userFlowAddress));

        yield return new WaitUntil(() => getDefendersIds.IsCompleted);

        if (getDefendersIds.Result.Error != null)
        {
            Debug.LogError($"Error:  {getDefendersIds.Result.Error.Message}");
            yield break;
        }

        CadenceBase[] allDefendersIds = (getDefendersIds.Result.Value as CadenceArray).Value;

        if (allDefendersIds.Length > 0)
        {
            foreach (CadenceComposite defenderGroup in allDefendersIds)
            {
                List<string> _defenderGroup = new();
                string name = defenderGroup.CompositeFieldAs<CadenceString>("name").Value;
                CadenceBase[] beastIDs = defenderGroup.CompositeFieldAs<CadenceArray>("beastIDs").Value;

                // Executing script to get all defenders from account
                Task<FlowScriptResponse> getPawns = FLOW_ACCOUNT.ExecuteScript(GetPawnsTxn.text,
                    new CadenceAddress(userFlowAddress), new CadenceArray(beastIDs));

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
                    string nameOfPawn = null;
                    string idOfPawn = null;
                    string hpOfPawn = null;
                    hpOfPawn = pawn.CompositeFieldAs<CadenceNumber>("hp").Value;
                    CadenceComposite nft = pawn.CompositeFieldAs<CadenceComposite>("nft");
                    idOfPawn = nft.CompositeFieldAs<CadenceNumber>("id").Value;
                    CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
                    nameOfPawn = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
                    nameOfPawn += "_" + beastTemplate.CompositeFieldAs<CadenceString>("skin").Value;

                    nameOfPawn += "_" + hpOfPawn + "_" + idOfPawn;

                    _defenderGroup.Add(nameOfPawn);
                    Debug.Log(nameOfPawn);
                }

                Debug.Log(name);
                if (GameManager.Instance.userDefenderGroups.ContainsKey(name))
                {
                    GameManager.Instance.userDefenderGroups[name] = _defenderGroup;
                    
                }
                else
                {
                    GameManager.Instance.userDefenderGroups.Add(name, _defenderGroup);
                }   
            }
        }
        else
        {
            Debug.Log("User don't have Defenders now");
        }
    }

    public IEnumerator GetFightsRecords()
    {
        Task<FlowScriptResponse> getChallengeRecords = FLOW_ACCOUNT.ExecuteScript(GetChallengeRecordsTxn.text,
            new CadenceAddress(userFlowAddress), new CadenceAddress(GameManager.Instance.lastDefenderAddress), 
            new CadenceNumber(CadenceNumberType.UInt64, GameManager.Instance.lastFightRecord));

        yield return new WaitUntil(() => getChallengeRecords.IsCompleted);

        if (getChallengeRecords.Result.Error != null)
        {
            Debug.LogError($"Error:  {getChallengeRecords.Result.Error.Message}");
            yield break;
        }

        //challengeRecords = (getChallengeRecords.Result.Value as CadenceComposite);
        challengeRecords = (getChallengeRecords.Result.Value as CadenceOptional);
    }
}