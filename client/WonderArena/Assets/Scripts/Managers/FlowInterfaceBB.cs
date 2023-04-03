using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowInterfaceBB : MonoBehaviour
{
    // Test addresses - Alice and Bob
    public string alicesAddress = "0x1801c3f618a511e6";
    public string bobsAddress = "0x771519260bbe1ee6";

    // Actual address;
    public string userFlowAddress = null;

    CoroutineHelper coroutineHelper;

    public CadenceOptional challengeRecords;
    public List<CadenceDictionaryItem> allPlayers_ListDictionaryItems = new();
    public CadenceBase[] playerAllBeastsIDs_CadenceBaseArray;
    public List<CadenceComposite> playerAllPawns_ListCadenceComposite = new();
    public bool isScriptsCompleted = false;
    public List<Beast> beastsForListingList = new();
    public bool hasParentAddress = false;

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
    [SerializeField] TextAsset GetListingBeastsTxn;
    [SerializeField] TextAsset HasParentAccountTxn;

    private static FlowInterfaceBB m_instance = null;
    public static FlowInterfaceBB Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<FlowInterfaceBB>();
                if (SceneManager.GetActiveScene().name != "ConnectingWallet")
                {
                    DontDestroyOnLoad(m_instance);
                }  
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

        //// Register DevWallet
        //FlowSDK.RegisterWalletProvider(ScriptableObject.CreateInstance<DevWalletProvider>());
        coroutineHelper = CoroutineHelper.Instance;
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "ConnectingWallet")
        {
            Destroy(gameObject);
        }
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

        StartCoroutine(NetworkManager.Instance.GetPlayerUpdate(5f));

        
        while (NetworkManager.Instance.userFlowAddress == null)
        {
            int dots = ((int)(Time.time * 2.0f) % 4);
            Debug.Log("Registrating Flow Account" + new string('.', dots));
            yield return null;
        }

        userFlowAddress = NetworkManager.Instance.userFlowAddress;

        if (PlayerPrefs.HasKey("Username"))
        {
            if (allPlayers_ListDictionaryItems.Count == 0 || allPlayers_ListDictionaryItems == null)
            {
                coroutineHelper.RunCoroutine("GetAllPlayers", GetAllPlayers());
            }
            coroutineHelper.RunCoroutine("GetAllBeasts", GetAllBeastsIDs());
            if (!hasParentAddress)
            {
                coroutineHelper.RunCoroutine("HasParentAccount", HasParentAccount());
            }            
        }
        else
        {
            Debug.LogError("User is not registred!");
        }
    }

    // Getting all players
    public IEnumerator GetAllPlayers()
    {
        allPlayers_ListDictionaryItems = new();
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

    public IEnumerator HasParentAccount()
    {
        Task<FlowScriptResponse> getIsParentAccount = FLOW_ACCOUNT.ExecuteScript(HasParentAccountTxn.text, new CadenceAddress(userFlowAddress));
        
        yield return new WaitUntil(() => getIsParentAccount.IsCompleted);

        if (getIsParentAccount.Result.Error != null)
        {
            Debug.LogError($"Error:  {getIsParentAccount.Result.Error.Message}");
            yield break;
        }

        hasParentAddress = (getIsParentAccount.Result.Value as CadenceBool).Value;
        Debug.Log(hasParentAddress);
    }

    private IEnumerator GetAllPlayerPawns()
    {
        playerAllPawns_ListCadenceComposite = new();
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
        NetworkManager.Instance.lastDefenderBeasts = new();
        // Executing script to get all Pawns from account
        Task<FlowScriptResponse> getPawns = FLOW_ACCOUNT.ExecuteScript(GetPawnsTxn.text,
            new CadenceAddress(NetworkManager.Instance.lastDefenderAddress), new CadenceArray(beastIds));

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
            //string nameOfPawn = null;
            //string idOfPawn = null;
            //string hpOfPawn = null;
            //string manaRequired = null;
            //hpOfPawn = pawn.CompositeFieldAs<CadenceNumber>("hp").Value;
            //CadenceComposite skill = pawn.CompositeFieldAs<CadenceComposite>("skill");
            //manaRequired = skill.CompositeFieldAs<CadenceNumber>("manaRequired").Value;
            //CadenceComposite nft = pawn.CompositeFieldAs<CadenceComposite>("nft");
            //idOfPawn = nft.CompositeFieldAs<CadenceNumber>("id").Value;
            //CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
            //nameOfPawn = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
            //nameOfPawn += "_" + beastTemplate.CompositeFieldAs<CadenceString>("skin").Value;

            //nameOfPawn += "_" + hpOfPawn + "_" + idOfPawn + "_" + manaRequired;
            //Debug.Log(nameOfPawn);

            Beast beast = new();

            beast.hp = pawn.CompositeFieldAs<CadenceNumber>("hp").Value;

            CadenceComposite skill = pawn.CompositeFieldAs<CadenceComposite>("skill");
            beast.manaRequired = skill.CompositeFieldAs<CadenceNumber>("manaRequired").Value;

            CadenceComposite nft = pawn.CompositeFieldAs<CadenceComposite>("nft");
            beast.id = nft.CompositeFieldAs<CadenceNumber>("id").Value;

            CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
            beast.name = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
            beast.skin = beastTemplate.CompositeFieldAs<CadenceString>("skin").Value;

            NetworkManager.Instance.lastDefenderBeasts.Add(beast);
        }
    }

    public IEnumerator GetListingBeasts()
    {
        beastsForListingList = new();
        // Executing script to get all Pawns from account
        Task<FlowScriptResponse> getListingBeasts = FLOW_ACCOUNT.ExecuteScript(GetListingBeastsTxn.text);

        yield return new WaitUntil(() => getListingBeasts.IsCompleted);

        if (getListingBeasts.Result.Error != null)
        {
            Debug.LogError($"Error:  {getListingBeasts.Result.Error.Message}");
            yield break;
        }

        CadenceDictionary allListingBeasts = getListingBeasts.Result.Value as CadenceDictionary;

        // Adding all pawns to List of all pawns that user has
        foreach (CadenceDictionaryItem listingBeast in allListingBeasts.Value)
        {
            Beast beast = new();

            beast.id = (listingBeast.Key as CadenceNumber).Value;
            CadenceComposite beastNft = listingBeast.Value as CadenceComposite;
            CadenceComposite beastTemplate = beastNft.CompositeFieldAs<CadenceComposite>("beastTemplate");
            beast.name = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
            beast.skin = beastTemplate.CompositeFieldAs<CadenceString>("skin").Value;

            beastsForListingList.Add(beast);
        }
    }

    public IEnumerator GetUserDefenderGroups()
    {
        NetworkManager.Instance.userDefenderGroups = new();
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
                List<Beast> _defenderGroup = new();
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
                    Beast beast = new();

                    beast.hp = pawn.CompositeFieldAs<CadenceNumber>("hp").Value;
                    CadenceComposite nft = pawn.CompositeFieldAs<CadenceComposite>("nft");
                    beast.id = nft.CompositeFieldAs<CadenceNumber>("id").Value;
                    CadenceComposite beastTemplate = nft.CompositeFieldAs<CadenceComposite>("beastTemplate");
                    beast.name = beastTemplate.CompositeFieldAs<CadenceString>("name").Value;
                    beast.skin = beastTemplate.CompositeFieldAs<CadenceString>("skin").Value;

                    _defenderGroup.Add(beast);
                }

                if (NetworkManager.Instance.userDefenderGroups.ContainsKey(name))
                {
                    NetworkManager.Instance.userDefenderGroups[name] = _defenderGroup;       
                }
                else
                {
                    NetworkManager.Instance.userDefenderGroups.Add(name, _defenderGroup);
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
            new CadenceAddress(userFlowAddress), new CadenceAddress(NetworkManager.Instance.lastDefenderAddress), 
            new CadenceNumber(CadenceNumberType.UInt64, NetworkManager.Instance.lastFightRecord));

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