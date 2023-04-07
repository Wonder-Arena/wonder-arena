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
    public List<Beast.BeastStats> _attackerComp = new();
    public List<CadenceComposite> allPlayerPawns = new();
    public List<CadenceComposite> allAttackerPawns = new();
    public CadenceComposite record;

    public string winner;

    [SerializeField]
    List<GameObject> allPawnsPrefabs;
    [SerializeField]
    List<GameObject> attackersList;
    [SerializeField]
    List<GameObject> defendersList;

    [SerializeField]
    GameObject winScreen;
    [SerializeField]
    GameObject resultBeasts;
    string attackerScoreChange;
    [SerializeField]
    GameObject defeatScreen;
    string defenderScoreChange;
    [SerializeField]
    float secondsBetweenEvents = 0.5f;

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

    private IEnumerator Start()
    {   
        winScreen.SetActive(false);
        defeatScreen.SetActive(false);
        resultBeasts.SetActive(false);
        
        while (!CoroutineHelper.Instance.AreAllCoroutinesFinished())
        {
            yield return null;
        }

        if (FlowInterfaceBB.Instance.challengeRecords.Value != null)
        {
            record = (FlowInterfaceBB.Instance.challengeRecords.Value as CadenceComposite);
        }
        else
        {
            record = null;
            Debug.Log("There's no such Battle Record");
        }
        _attackerComp = new(NetworkManager.Instance.attackerComp);
        StartCoroutine(SimulateFight());
    }

    private void SetAllPawns()
    {
        List<Beast.BeastStats> defenderComp = new(NetworkManager.Instance.lastDefenderBeasts);
        for (int i = 0; i < _attackerComp.Count; i++)
        {
            GameObject newAttackerObject = null;
            string attackerNameAndSkin = _attackerComp[i].nameOfBeast + "_" + _attackerComp[i].skin;
            string attackerHp = _attackerComp[i].hp;
            string attackerId = _attackerComp[i].id;
            string attackerManaRequired = _attackerComp[i].manaRequired;
            for (int j = 0; j < allPawnsPrefabs.Count; j++)
            {
                if (allPawnsPrefabs[j].name == attackerNameAndSkin)
                {
                    newAttackerObject = allPawnsPrefabs[j];
                    break;
                }
            }
            GameObject attackerObjectOnField = Instantiate(newAttackerObject, attackersList[i].transform);
            attackerObjectOnField.name = attackerNameAndSkin;
            attackerObjectOnField.transform.GetComponent<BeastOnField>().maxHp = float.Parse(attackerHp);
            attackerObjectOnField.transform.GetComponent<BeastOnField>().hp = float.Parse(attackerHp);
            attackerObjectOnField.transform.GetComponent<BeastOnField>().id = int.Parse(attackerId);
            attackerObjectOnField.transform.GetComponent<BeastOnField>().manaRequired = int.Parse(attackerManaRequired);

            // Defenders:

            GameObject newDefenderObject = null;
            string defenderNameAndSkin = defenderComp[i].nameOfBeast + "_" + defenderComp[i].skin;
            string defenderHp = defenderComp[i].hp;
            string defenderId = defenderComp[i].id;
            string defenderManaRequired = defenderComp[i].manaRequired;
            for (int j = 0; j < allPawnsPrefabs.Count; j++)
            {
                if (allPawnsPrefabs[j].name == defenderNameAndSkin)
                {
                    newDefenderObject = allPawnsPrefabs[j];
                    break;
                }
            }
            GameObject defenderObjectOnField = Instantiate(newDefenderObject, defendersList[i].transform);
            defenderObjectOnField.name = defenderNameAndSkin;
            defenderObjectOnField.transform.GetComponent<BeastOnField>().maxHp = float.Parse(defenderHp);
            defenderObjectOnField.transform.GetComponent<BeastOnField>().hp = float.Parse(defenderHp);
            defenderObjectOnField.transform.GetComponent<BeastOnField>().id = int.Parse(defenderId);
            defenderObjectOnField.transform.GetComponent<BeastOnField>().manaRequired = int.Parse(defenderManaRequired);
        }
    }

    private IEnumerator SimulateFight()
    {
        string id = (record.CompositeFieldAs<CadenceNumber>("id").Value);
        winner = (record.CompositeFieldAs<CadenceAddress>("winner").Value);
        CadenceBase[] defenders = (record.CompositeFieldAs<CadenceArray>("defenderBeasts").Value);
        attackerScoreChange = (record.CompositeFieldAs<CadenceNumber>("attackerScoreChange").Value);
        defenderScoreChange = (record.CompositeFieldAs<CadenceNumber>("defenderScoreChange").Value);

        yield return StartCoroutine(FlowInterfaceBB.Instance.GetDefenderPawnsNames(defenders));

        SetAllPawns();

        for (int i = 0; i < attackersList.Count; i++)
        {
            attackersList[i].transform.GetChild(0).GetComponent<BeastOnField>().HpBar = healthBars.transform.Find("Attackers")
            .GetChild(i).Find("Hp").Find("HpBar").GetComponent<Image>();

            attackersList[i].transform.GetChild(0).GetComponent<BeastOnField>().ManaBar = healthBars.transform.Find("Attackers")
            .GetChild(i).Find("Mana").Find("ManaBar").GetComponent<Image>();

            attackersList[i].transform.GetChild(0).GetComponent<BeastOnField>().hpText = healthBars.transform.Find("Attackers")
            .GetChild(i).Find("Hp").Find("HpNumber").GetComponent<TextMeshProUGUI>();

            attackersList[i].transform.GetChild(0).GetComponent<BeastOnField>().manaText = healthBars.transform.Find("Attackers")
            .GetChild(i).Find("Mana").Find("ManaNumber").GetComponent<TextMeshProUGUI>();

            healthBars.transform.Find("Attackers").GetChild(i).Find("Name").GetComponent<TextMeshProUGUI>().text = 
            attackersList[i].transform.GetChild(0).name.Split("_")[0];



            defendersList[i].transform.GetChild(0).GetComponent<BeastOnField>().HpBar = healthBars.transform.Find("Defenders")
            .GetChild(i).Find("Hp").Find("HpBar").GetComponent<Image>();

            defendersList[i].transform.GetChild(0).GetComponent<BeastOnField>().ManaBar = healthBars.transform.Find("Defenders")
            .GetChild(i).Find("Mana").Find("ManaBar").GetComponent<Image>();

            defendersList[i].transform.GetChild(0).GetComponent<BeastOnField>().hpText = healthBars.transform.Find("Defenders")
            .GetChild(i).Find("Hp").Find("HpNumber").GetComponent<TextMeshProUGUI>();

            defendersList[i].transform.GetChild(0).GetComponent<BeastOnField>().manaText = healthBars.transform.Find("Defenders")
            .GetChild(i).Find("Mana").Find("ManaNumber").GetComponent<TextMeshProUGUI>();

            healthBars.transform.Find("Defenders").GetChild(i).Find("Name").GetComponent<TextMeshProUGUI>().text =
            defendersList[i].transform.GetChild(0).name.Split("_")[0];
        }

        CadenceBase[] events = (record.CompositeFieldAs<CadenceArray>("events").Value);


        int index = 0;
        yield return new WaitForSeconds(0.5f);
        foreach (CadenceComposite _event in events)
        {
            index += 1;
            CadenceOptional byBeastId = _event.CompositeFieldAs<CadenceOptional>("byBeastID");
            CadenceOptional withSkill = _event.CompositeFieldAs<CadenceOptional>("withSkill");
            CadenceOptional byStatus = _event.CompositeFieldAs<CadenceOptional>("byStatus");
            CadenceBase[] targetBeastIDs = _event.CompositeFieldAs<CadenceArray>("targetBeastIDs").Value;
            bool hitTheTarget = _event.CompositeFieldAs<CadenceBool>("hitTheTarget").Value;
            CadenceOptional effect = _event.CompositeFieldAs<CadenceOptional>("effect");
            CadenceOptional damage = _event.CompositeFieldAs<CadenceOptional>("damage");
            bool targetSkipped = _event.CompositeFieldAs<CadenceBool>("targetSkipped").Value;
            bool targetDefeated = _event.CompositeFieldAs<CadenceBool>("targetDefeated").Value;

            textLog.text = "Event: " + index.ToString() + ":\n";
            yield return StartCoroutine(SimulateEvent(byBeastId, withSkill, byStatus, targetBeastIDs, 
                hitTheTarget, effect, damage, targetSkipped, targetDefeated));
            yield return new WaitForSeconds(secondsBetweenEvents);
        }

        SetResultScreen();
        PlatformSetter.Instance.SetAllBeast(_attackerComp);
    }


    // To Suurikat: How do we get all beast ids used in an event?

    // foreach (Beast beast in allBeasts)
    // {
            // newBeast.id = 
            // newBeast.hp = 100;
            // gameState.beasts.Add(beast)
    // }


    private IEnumerator SimulateEvent(CadenceOptional byBeastId, CadenceOptional withSkill, CadenceOptional byStatus,
        CadenceBase[] targetBeastIDs, bool hitTheTarget, CadenceOptional effect, CadenceOptional damage,
        bool targetSkipped, bool targetDefeated)
    {
        // [0] - name, [1] - skin, [2] - hp, [3] - id

        string target = (targetBeastIDs[0] as CadenceNumber).Value;
        GameObject targetObject = GetObjectById(target);
        BeastOnField targetStats = targetObject.GetComponent<BeastOnField>();
        bool isSideEffect = !IsOptionalNull(effect);
        
        string targetName = targetObject.name.Split("_")[0];

        //// case 1: if attacker exists
        //
        //
        if (!IsOptionalNull(byBeastId))
        {
            GameObject byBeastObject = GetObjectById((byBeastId.Value as CadenceNumber).Value);
            BeastOnField byBeastStats = byBeastObject.GetComponent<BeastOnField>();

            string byBeastName = byBeastObject.name.Split("_")[0];

            // L214: let isSideEffect = e.effect != nil
            
            // L217: case 1.1: if skill exist
            bool isSkillUsed = !IsOptionalNull(withSkill);
            if (isSkillUsed) // 1.1.1 log("NAME used")
            {
                byBeastObject.GetComponent<BeastOnField>().currentMana = 0;
                string skillName = (withSkill.Value as CadenceString).Value;
                if (!isSideEffect)
                {
                    textLog.text += $"{byBeastName} used \"{skillName}\"";
                    //yield return StartCoroutine(ChangeAndWaitAnimationStateTime("Skill", byBeastObject));
                    //TODO: Change the state of the beast GetObjectById(beastId).SetAnimation(skill)
                }
            }
            else // 1.1.2 log("NAME attacked TARGET")
            { 
                if (!isSideEffect) //L223:
                {
                    textLog.text += $"{byBeastName} attacked {targetName}";
                    yield return StartCoroutine(ChangeAndWaitAnimationStateTime("StandardAttack", byBeastObject));
                }   
            }
            // TODO: add delay here
            if (!hitTheTarget && !isSideEffect)
            {
                textLog.text += "Miss!";
            }
            else
            {
                bool didDamage = !IsOptionalNull(damage); // if let damage = e.damage 
                if (didDamage) // log("that's a lot of damage")
                {
                    // textLog.text += $"\n {targetName} suffered {(damage.Value as CadenceNumber).Value} damage";
                    yield return StartCoroutine(ChangeAndWaitAnimationStateTime("GetHurt", targetObject));
                    targetStats.TakeDamage(damage);
                    targetStats.GetMana(damage);
                    
                    // Log that hp changed (temp)
                    // textLog.text += $" and now has this much hp left: {targetStats.hp}";
                } 
                
                // pub let effect: PawnEffect?
                else if (isSideEffect) // L230_old: else if let effect = e.effect 
                {
                    textLog.text += $" and now {targetName} going {PawnEffect(effect.Value as CadenceComposite)}!";
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
                    targetStats.TakeDamage(damage);
                    
                    string damageFloat = (damage.Value as CadenceNumber).Value;
                    
                    textLog.text += 
                    $"{targetName} suffers {PawnStatus(byStatus.Value as CadenceComposite)} and got {damageFloat} damage";
                    StartCoroutine(ChangeAndWaitAnimationStateTime("GetHurt", targetObject));
                }

                else if (targetSkipped)
                {
                    textLog.text += 
                    $"{targetName} is skipping next turn due to {PawnStatus(byStatus.Value as CadenceComposite)}!"; 
                }

                else if (isSideEffect) //TODO: this is wrong only happens when isSideEffect == ToNormal
                {
                    textLog.text += $"{targetName} now is in Normal state!";
                }
            }
        } 

        //// case 3: check targetDefeated
        //
        //

        else if (targetDefeated)
        {
            textLog.text += $"{targetName} is defeated!";
            yield return StartCoroutine(ChangeAndWaitAnimationStateTime("Dead", targetObject));
            targetObject.SetActive(false);
        }
    }

    private GameObject GetObjectById(string id)
    {
        foreach (Transform team in groupsParentObject.transform)
        {
            foreach (Transform place in team)
            {
                GameObject beastObject = place.GetChild(0).gameObject;
                string beastId = beastObject.transform.GetComponent<BeastOnField>().id.ToString();
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
            default:
                break;
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
            default:
                break;
        }
        return null;
    }

    private IEnumerator ChangeAndWaitAnimationStateTime(string newState, GameObject beast)
    {
        Animator animator = beast.GetComponent<Animator>();

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(newState))
        {
            yield return null;
        }

        animator.Play(newState);

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.Play("Idle");
    }

    private void SetResultScreen()
    {
        resultBeasts.SetActive(true);
        if (winner == NetworkManager.Instance.userFlowAddress)
        {
            SetScreen(winScreen);
        }
        else
        {
            SetScreen(defeatScreen);
        }
    }

    private void SetScreen(GameObject screen)
    {
        screen.SetActive(true);
        if (int.Parse(attackerScoreChange) > 0)
        {
            attackerScoreChange = $"+{attackerScoreChange}";
        }
        if (int.Parse(defenderScoreChange) > 0)
        {
            defenderScoreChange = $"+{defenderScoreChange}";
        }
        screen.transform.Find("Score").Find("Value").GetComponent<TextMeshProUGUI>().text = attackerScoreChange;
        screen.transform.Find("EnemyScore").Find("Value").GetComponent<TextMeshProUGUI>().text = defenderScoreChange;
    }


    /*


    */

    /*
    Event: 1:
    Moon used default attack without side effects -> Moon attacked Shen
     Shen_Normal suffered 5 damage and now has this much hp left: 75 -> 

     Event: 2:
    Moon used default attack without side effects
     Azazel_Normal suffered 20 damage and now has this much hp left: 40

     Event: 3:
    Azazel used default attack without side effects
     Shen_Normal suffered 80 damage and now has this much hp left: 0

     Event: 4:
    Shen_Normal is defeated!

    Event: 5:
    Saber used default attack without side effects
     Saber_Normal suffered 0 damage and now has this much hp left: 70

     Event: 0:
    Saber used default attack without side effects
     Azazel_Normal suffered 50 damage and now has this much hp left: 0

     Event: 0:
    Azazel_Normal is defeated!

    Event: 0:
    Moon used default attack without side effects
     Saber_Normal suffered 45 damage and now has this much hp left: 25

     Event: 0:
    Moon used default attack without side effects
     Moon_Normal suffered 10 damage and now has this much hp left: 60

     Event: 0:
    Saber used default attack without side effects
     Saber_Normal suffered 0 damage and now has this much hp left: 25

     Event: 0:
    Saber used default attack without side effects
     Moon_Normal suffered 25 damage and now has this much hp left: 35

     Event: 0:
    Moon used default attack without side effects
     Saber_Normal suffered 45 damage and now has this much hp left: 0

     Event: 0:
    Saber_Normal is defeated!Event: 0:
    Moon used default attack without side effects
     Moon_Normal suffered 10 damage and now has this much hp left: 25

     Event: 0:
    Saber used default attack without side effects
     Moon_Normal suffered 25 damage and now has this much hp left: 45

     Event: 0:
    Moon used default attack without side effects
     Moon_Normal suffered 10 damage and now has this much hp left: 35

     Event: 0:
    Moon used default attack without side effects
     Moon_Normal suffered 10 damage and now has this much hp left: 15

     Event: 0:
    Saber used default attack without side effects
     Moon_Normal suffered 25 damage and now has this much hp left: 10

     Event: 0:
    Moon used "Mega Volt Crash" without side effects
     Moon_Normal suffered 40 damage and now has this much hp left: 0

     Event: 0:
    Moon_Normal is defeated!

    */




}


