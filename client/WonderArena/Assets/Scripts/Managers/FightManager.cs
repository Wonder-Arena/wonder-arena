using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.DevWallet;
using DapperLabs.Flow.Sdk.Unity;
using UnityEngine;

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

    private void Start()
    {
        record = FlowInterfaceBB.Instance.challengeRecords;
        attackerCompNames = new(GameManager.Instance.attackerComp);
        SetAllPawns();
        SimulateFight();
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

    private void SimulateFight()
    {
        string id = (record.CompositeFieldAs<CadenceNumber>("id").Value);
        string winner = (record.CompositeFieldAs<CadenceAddress>("winner").Value);
        CadenceBase[] attackers = (record.CompositeFieldAs<CadenceArray>("attackerBeasts").Value);
        CadenceBase[] defenders = (record.CompositeFieldAs<CadenceArray>("defenderBeasts").Value);
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

    private void SimulateEvent(CadenceOptional byBeastId, CadenceOptional withSkill, CadenceOptional byStatus,
        CadenceBase[] targetBeastIDs, bool hitTheTarget, CadenceOptional effect, CadenceOptional damage,
        bool targetSkipped, bool targetDefeated)
    {
        GameObject byBeastObject = null;
        string skill = null;
        if (!IsOptionalNull(byBeastId))
        {
            byBeastObject = GetObjectById((byBeastId.Value as CadenceNumber).Value);
        }
        if (!IsOptionalNull(withSkill))
        {

        }
        else
        {
            skill = "Default";
        }
    }

    private GameObject GetObjectById(string id)
    {
        foreach (GameObject child in attackersList)
        {
            GameObject attacker = child.transform.GetChild(0).gameObject;
            string attackerId = attacker.name.Split("_")[3];
            if (attackerId == id)
            {
                return attacker;
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
