using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;
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
            attackerObjectOnField.name = $"{attackerNameAndSkinWithoutId}_{attackerHp}_{attackerId}";

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
            defenderObjectOnField.name = $"{defenderNameAndSkinWithoutId}_{defenderHp}_{defenderId}";

        }
    }

    private IEnumerator SimulateFight()
    {
        string id = (record.CompositeFieldAs<CadenceNumber>("id").Value);
        string winner = (record.CompositeFieldAs<CadenceAddress>("winner").Value);
        CadenceBase[] defenders = (record.CompositeFieldAs<CadenceArray>("defenderBeasts").Value);

        yield return StartCoroutine(FlowInterfaceBB.Instance.GetDefenderPawnsNames(defenders));

        SetDefendersPawns();

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
            }
    }

    public class Beast
    {
        public int hp;
        public int id;
    }

    public class GameState //bz: create a state to update HP after each event and have it shown on the health bars
    {
        public List<Beast> beasts;
    }

    GameState gameState = new GameState(); //Create a new game state

    // To Suurikat: How do we get all beast ids used in an event?

    // foreach (Beast beast in allBeasts)
    // {
            Beast newBeast = new Beast();
            // newBeast.id = 
            // newBeast.hp = 100;
            // gameState.beasts.Add(beast)
    // }


    private void SimulateEvent(CadenceOptional byBeastId, CadenceOptional withSkill, CadenceOptional byStatus,
        CadenceBase[] targetBeastIDs, bool hitTheTarget, CadenceOptional effect, CadenceOptional damage,
        bool targetSkipped, bool targetDefeated)
    {
        // [0] - name, [1] - skin, [2] - hp, [3] - id

        // bz stuff start

        // -Get the IDs
        string beastId = null; // Beast ID
        
        // int targetBeastId = int.Parse(targetBeastIDs[0].ToString()); //Target Beast
        

        // case 1: if attacker exists
        
        if (!IsOptionalNull(byBeastId)) 
        {
            beastId = (byBeastId.Value as CadenceNumber).Value;
            // L210: let target = e.targetBeastIDs[0]
            string target = (targetBeastIDs[0] as CadenceNumber).Value;

            // L214: let isSideEffect = e.effect != nil
            bool isSideEffect = !IsOptionalNull(effect);
            
            // L217: case 1.1: if skill exist
            bool isSkillUsed = !IsOptionalNull(withSkill);
            if (isSkillUsed)
            {
                if (!isSideEffect)
                {
                    string log = "Skill used without side effects";
                    textLog.text += log + "\n";
                    //TODO: Change the state of the beast GetObjectById(beastId).SetAnimation(skill)
                }
            } 
            else
            { //L223:
                if (!isSideEffect)
                {
                    string log = "Used default attack without side effects";
                    textLog.text += log + "\n";
                }   
            }  

            


        } 
        else if (false)
        {

        } //case 2: check if pawnStatus exist

        else if (false)
        {
            
        } //case 3: check targetDefeated

        // bz stuff end

        GameObject byBeastObject = null;
        string actualSkill = null;

        List<GameObject> targetBeastObjects = new();


        string actualDamage = null;
        
        if (!IsOptionalNull(byBeastId))
        {
            byBeastObject = GetObjectById((byBeastId.Value as CadenceNumber).Value);
            Debug.Log("By: " + byBeastObject.name);
        }

        Beast byBeast = new();

        byBeast.hp = int.Parse(byBeastObject.name.Split("_")[2]);
        byBeast.id = int.Parse(byBeastObject.name.Split("_")[3]);

        if (!IsOptionalNull(damage))
        {
            actualDamage = (damage.Value as CadenceNumber).Value;
        }
        if (targetBeastIDs.Length > 0)
        {
            foreach (CadenceNumber targetId in targetBeastIDs)
            {
                targetBeastObjects.Add(GetObjectById(targetId.Value));
            }
            foreach (var x in targetBeastObjects)
            {
                Debug.Log("To: " + x.name);
            }
            foreach (GameObject target in targetBeastObjects)
            {
                string hpOfTarget = target.name.Split("_")[2];
                int.TryParse(hpOfTarget, out int hpInt);
                int.TryParse(actualDamage, out int actualDamageInt);
                int newHpOfTarget = hpInt - actualDamageInt;
                string newtargetName = target.name.Split("_")[0] + "_"
                    + target.name.Split("_")[1] + "_" + newHpOfTarget.ToString() + "_" + target.name.Split("_")[2] + "_" + target.name.Split("_")[3];
                target.name = newtargetName;
                Debug.Log(target.name + " Current hp: " + hpOfTarget);
            }
        }
        if (!IsOptionalNull(withSkill))
        {
            actualSkill = (withSkill.Value as CadenceString).Value;
        }
        else
        {
            actualSkill = "Default";
        }
 
        // Damaging all the targets by damage value
    }

    private GameObject GetObjectById(string id)
    {
        foreach (Transform team in groupsParentObject.transform)
        {
            foreach (Transform place in team)
            {
                GameObject beastObject = place.GetChild(0).gameObject;
                string beastId = beastObject.name.Split("_")[3];
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

    private class Pawns
    {
        private class Effects
        {

        }
    }
}
