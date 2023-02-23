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
    public CadenceBase[] challengeRecords;

    [SerializeField]
    List<GameObject> allPawnsPrefabs;

    private void Start()
    {
        challengeRecords = FlowInterfaceBB.Instance.challengeRecords;
        attackerCompNames = new(GameManager.Instance.attackerComp);
        allPlayerPawns = new(FlowInterfaceBB.Instance.playerAllPawns_ListCadenceComposite);
        SimulateFight();
    }

    private void SetAllPawns()
    {
        foreach (string attacker in attackerCompNames)
        {
            string attackerId = attacker.Split("_")[1];
            foreach (CadenceComposite pawn in allPlayerPawns)
            {
                foreach (CadenceCompositeField pawnField in pawn.Value.Fields)
                {
                    if (pawnField.Name == "nft")
                    {
                        foreach (CadenceCompositeField nftField in (pawnField.Value as CadenceComposite).Value.Fields)
                        {
                            if (nftField.Name == "id")
                            {
                                if (attackerId == (nftField.Value as CadenceNumber).Value)
                                {
                                    allAttackerPawns.Add(pawn);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void SimulateFight()
    {
        foreach (CadenceComposite record in challengeRecords)
        {
            foreach (CadenceCompositeField recordField in record.Value.Fields)
            {
                if (recordField.Name == "events")
                {
                    foreach (CadenceComposite _event in (recordField.Value as CadenceArray).Value)
                    {
                        foreach (CadenceCompositeField _eventField in _event.Value.Fields)
                        {
                            Debug.Log(_eventField.Name);
                            switch (_eventField.Name)
                            {
                                case "byBeastID":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceNumber).Value);
                                    }
                                    else
                                    {
                                        Debug.Log(_eventField.Name + (_eventField.Value as CadenceNumber).Value);
                                    }
                                    break;
                                case "withSkill":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceString).Value);
                                    }
                                    break;
                                case "byStatus":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceNumber).Value);
                                    }
                                    break;
                                case "targetBeastIDs":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceArray).Value);
                                    }
                                    break;
                                case "hitTheTarget":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceBool).Value);
                                    }
                                    break;
                                case "effect":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceNumber).Value);
                                    }
                                    break;
                                case "damage":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceNumber).Value);
                                    }
                                    break;
                                case "targetSkipped":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceBool).Value);
                                    }
                                    break;
                                case "targetDefeated":
                                    if (_eventField.Value as CadenceVoid != null)
                                    {
                                        Debug.Log((_eventField.Value as CadenceBool).Value);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
