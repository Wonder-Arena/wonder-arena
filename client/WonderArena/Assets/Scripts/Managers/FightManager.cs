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
using TMPro;

public class FightManager : MonoBehaviour
{
    public List<string> attackerCompNames = new();
    public List<CadenceComposite> allPlayerPawns = new();
    public List<CadenceComposite> allAttackerPawns = new();
    public CadenceComposite record;

    [SerializeField]
    List<GameObject> allPawnsPrefabs;
    [SerializeField]
    List<GameObject> attackersList;
    [SerializeField]
    List<GameObject> defendersList;

    [SerializeField]
    TextMeshProUGUI textLog;
    [SerializeField]
    GameObject healthBars;

    [SerializeField]
    GameObject groupsParentObject;

    private void Awake()
    {
        textLog = textLog.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (FlowInterfaceBB.Instance.challengeRecords.Value != null)
        {
            record = (FlowInterfaceBB.Instance.challengeRecords.Value as CadenceComposite);
        }
        else
        {
            record = null;
            Debug.Log("There's no such Battle Record");
        }
        attackerCompNames = new(GameManager.Instance.attackerComp);
        SetAllPawns();
        StartCoroutine(SimulateFight());
    }

    private void SetAllPawns()
    {
        for (int i = 0; i < attackerCompNames.Count; i++)
        {
            GameObject newAttackerObject = null;
            string attackerNameAndSkinWithoutId = attackerCompNames[i].Split("_")[0] + "_" + attackerCompNames[i].Split("_")[1];
            string attackerHp = attackerCompNames[i].Split("_")[2];
            string attackerId = attackerCompNames[i].Split("_")[3];
            for (int j = 0; j < allPawnsPrefabs.Count; j++)
            {
                if (allPawnsPrefabs[j].name == attackerNameAndSkinWithoutId)
                {
                    newAttackerObject = allPawnsPrefabs[j];
                    break;
                }
            }
            GameObject attackerObjectOnField = Instantiate(newAttackerObject, attackersList[i].transform);
            attackerObjectOnField.name = $"{attackerNameAndSkinWithoutId}";
            attackerObjectOnField.transform.GetComponent<BeastStats>().maxHp = float.Parse(attackerHp);
            attackerObjectOnField.transform.GetComponent<BeastStats>().hp = float.Parse(attackerHp);
            attackerObjectOnField.transform.GetComponent<BeastStats>().id = int.Parse(attackerId);
        }
    }

    private void SetDefendersPawns()
    {
        Debug.Log("Start Setting defenders");
        List<string> defenderCompNames = new(GameManager.Instance.lastDefenderNamesOfPawns);
        for (int i = 0; i < defenderCompNames.Count; i++)
        {
            GameObject newDefenderObject = null;
            string defenderNameAndSkinWithoutId = defenderCompNames[i].Split("_")[0] + "_" + defenderCompNames[i].Split("_")[1];
            string defenderHp = defenderCompNames[i].Split("_")[2];
            string defenderId = defenderCompNames[i].Split("_")[3];
            for (int j = 0; j < allPawnsPrefabs.Count; j++)
            {
                if (allPawnsPrefabs[j].name == defenderNameAndSkinWithoutId)
                {
                    newDefenderObject = allPawnsPrefabs[j];
                    break;
                }
            }
            GameObject defenderObjectOnField = Instantiate(newDefenderObject, defendersList[i].transform);
            defenderObjectOnField.name = $"{defenderNameAndSkinWithoutId}";
            defenderObjectOnField.transform.GetComponent<BeastStats>().maxHp = float.Parse(defenderHp);
            defenderObjectOnField.transform.GetComponent<BeastStats>().hp = float.Parse(defenderHp);
            defenderObjectOnField.transform.GetComponent<BeastStats>().id = int.Parse(defenderId);

        }
    }

    private IEnumerator SimulateFight()
    {
        string id = (record.CompositeFieldAs<CadenceNumber>("id").Value);
        string winner = (record.CompositeFieldAs<CadenceAddress>("winner").Value);
        CadenceBase[] defenders = (record.CompositeFieldAs<CadenceArray>("defenderBeasts").Value);

        yield return StartCoroutine(FlowInterfaceBB.Instance.GetDefenderPawnsNames(defenders));

        SetDefendersPawns();

        for (int i = 0; i < attackersList.Count; i++)
        {
            attackersList[i].transform.GetChild(0).GetComponent<BeastStats>().HpBar = healthBars.transform.Find("Defenders")
            .GetChild(i).Find("HpBar").GetComponent<Image>();

            healthBars.transform.Find("Defenders").GetChild(i).Find("Name").GetComponent<TextMeshProUGUI>().text = 
            attackersList[i].transform.GetChild(0).name.Split("_")[0];
        }

        for (int i = 0; i < defendersList.Count; i++)
        {
            defendersList[i].transform.GetChild(0).GetComponent<BeastStats>().HpBar = healthBars.transform.Find("Attackers")
            .GetChild(i).Find("HpBar").GetComponent<Image>();

            healthBars.transform.Find("Attackers").GetChild(i).Find("Name").GetComponent<TextMeshProUGUI>().text = 
            defendersList[i].transform.GetChild(0).name.Split("_")[0];
        }

        CadenceBase[] events = (record.CompositeFieldAs<CadenceArray>("events").Value);

        foreach (CadenceComposite _event in events)
            {
                CadenceOptional byBeastId = _event.CompositeFieldAs<CadenceOptional>("byBeastID");
                CadenceOptional withSkill = _event.CompositeFieldAs<CadenceOptional>("withSkill");
                CadenceOptional byStatus = _event.CompositeFieldAs<CadenceOptional>("byStatus");
                CadenceBase[] targetBeastIDs = _event.CompositeFieldAs<CadenceArray>("targetBeastIDs").Value;
                bool hitTheTarget = _event.CompositeFieldAs<CadenceBool>("hitTheTarget").Value;
                CadenceOptional effect = _event.CompositeFieldAs<CadenceOptional>("effect");
                CadenceOptional damage = _event.CompositeFieldAs<CadenceOptional>("damage");
                bool targetSkipped = _event.CompositeFieldAs<CadenceBool>("targetSkipped").Value;
                bool targetDefeated = _event.CompositeFieldAs<CadenceBool>("targetDefeated").Value;

                SimulateEvent(byBeastId, withSkill, byStatus, targetBeastIDs, hitTheTarget, effect, damage, targetSkipped, targetDefeated);
                yield return new WaitForSeconds(5);
            }
    }


    // To Suurikat: How do we get all beast ids used in an event?

    // foreach (Beast beast in allBeasts)
    // {
            // newBeast.id = 
            // newBeast.hp = 100;
            // gameState.beasts.Add(beast)
    // }


    private void SimulateEvent(CadenceOptional byBeastId, CadenceOptional withSkill, CadenceOptional byStatus,
        CadenceBase[] targetBeastIDs, bool hitTheTarget, CadenceOptional effect, CadenceOptional damage,
        bool targetSkipped, bool targetDefeated)
    {
        // [0] - name, [1] - skin, [2] - hp, [3] - id

        string target = (targetBeastIDs[0] as CadenceNumber).Value;
        GameObject targetObject = GetObjectById(target);
        BeastStats targetStats = targetObject.GetComponent<BeastStats>();
        bool isSideEffect = !IsOptionalNull(effect);
        

        //// case 1: if attacker exists
        //
        //
        if (!IsOptionalNull(byBeastId)) 
        {
            GameObject byBeastObject = GetObjectById((byBeastId.Value as CadenceNumber).Value);
            BeastStats byBeastStats = byBeastObject.GetComponent<BeastStats>();
            
            // L214: let isSideEffect = e.effect != nil
            
            // L217: case 1.1: if skill exist
            bool isSkillUsed = !IsOptionalNull(withSkill);
            if (isSkillUsed)
            {
                if (!isSideEffect)
                {
                    string log = $"{byBeastObject.name} used skill without side effects";
                    textLog.text += log + "\n";
                    //TODO: Change the state of the beast GetObjectById(beastId).SetAnimation(skill)
                }
            }

            else
            { 
                if (!isSideEffect) //L223:
                {
                    string log = $"{byBeastObject.name} used default attack without side effects";
                    textLog.text += log + "\n";
                }   
            }

            if (!hitTheTarget && !isSideEffect)
            {
                string log = "Miss!";
                textLog.text += log + "\n";
            }
            
            else
            {
                bool didDamage = !IsOptionalNull(damage); // if let damage = e.damage 
                if (didDamage) 
                {
                    string log = $"{targetObject.name} suffered {(damage.Value as CadenceNumber).Value} damage";
                    textLog.text += log + "\n";
                    
                    targetStats.DoDamage(damage);
                    
                    // Log that hp changed (temp)
                    string log2 = $"{targetObject.name} has this much hp left: {targetStats.hp}";
                    textLog.text += log2 + "\n";
                } 
                
                // pub let effect: PawnEffect?
                else if (isSideEffect) // L230_old: else if let effect = e.effect 
                {
                    string log = $"{targetObject.name} going {PawnEffect(effect.Value as CadenceComposite)}";
                    textLog.text += log + "\n";
                    //TODO: change pawnStatus whether it's poisened, sleeping or has returned to normal, etc
                } 
            }
        } 

        //// case 2: check if pawnStatus exist
        //
        //

        else if (!IsOptionalNull(byStatus)) //L243_old: else if let status = e.byStatus
        {
            if (hitTheTarget)
            {
                bool didDamage = !IsOptionalNull(damage);
                
                if (didDamage)
                {
                    // float damageInt = float.Parse((damage.Value as CadenceNumber).Value); // fetch damage
                    // targetStats.hp -= damageInt; // subtract damage from beast class
                    targetStats.DoDamage(damage);
                    
                    string damageFloat = (damage.Value as CadenceNumber).Value;
                    
                    string log = $"{targetObject.name} suffers {PawnStatus(byStatus.Value as CadenceComposite)}" + 
                        $" and got {damageFloat} damage";
                    textLog.text += log + "\n";
                }

                else if (targetSkipped)
                {
                    string log = $"{targetObject.name} is skipping next turn due to {PawnStatus(byStatus.Value as CadenceComposite)}";
                    textLog.text += log + "\n";    
                }

                else if (isSideEffect)
                {
                    string log = $"{targetObject.name} now is Normal!";
                    textLog.text += log + "\n";
                }
            }
        } 

        //// case 3: check targetDefeated
        //
        //

        else if (targetDefeated)
        {
            string log = $"{targetObject.name} is defeated!";
            textLog.text += log + "\n";
        }
    }

    // private void DoDamage(CadenceOptional damage, float hp, BeastStats beastStats)
    // {
    //     beastStats.currentHp = beastStats.hp;
    //     float damageFloat = float.Parse((damage.Value as CadenceNumber).Value); // fetch damage
    //     float newHp = hp - damageFloat;
    //     beastStats.hp = newHp;
    //     beastStats.hp = Mathf.Clamp(beastStats.hp, 0f, beastStats.maxHp);
    //     //ChangeAnimationState("GetHurt", beastStats.gameObject);
    // }

    private GameObject GetObjectById(string id)
    {
        foreach (Transform team in groupsParentObject.transform)
        {
            foreach (Transform place in team)
            {
                GameObject beastObject = place.GetChild(0).gameObject;
                string beastId = beastObject.transform.GetComponent<BeastStats>().id.ToString();
                if (beastId == id)
                {
                    return beastObject;
                }
            }  
        }
        return null;
    }

    private bool IsOptionalNull(CadenceOptional cadenceOptional)
    {
        if (cadenceOptional.Value == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private string PawnEffect(CadenceComposite effect)
    {
        string effectNumber = effect.CompositeFieldAs<CadenceNumber>("rawValue").Value;
        switch (effectNumber)
        {
            case "0":
                return "ToNormal";
            case "1":
                return "ToParalysis";
            case "2":
                return "ToPoison";
            case "3":
                return "ToSleep";            
        }
        return null;
    }

    private string PawnStatus(CadenceComposite status)
    {
        string statusNumber = status.CompositeFieldAs<CadenceNumber>("rawValue").Value;
        switch (statusNumber)
        {
            case "0":
                return "Normal";
            case "1":
                return "Paralysis";
            case "2":
                return "Poison";
            case "3":
                return "Sleep";    
            case "4":
                return "Defeated";            
        }
        return null;
    }

    private void ChangeAnimationState(string newState, GameObject beast)
    {
        Animator animator = beast.GetComponent<Animator>();

        animator.Play(newState);
    }

}


