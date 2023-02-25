using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DapperLabs.Flow.Sdk.Cadence;

public class BeastStats : MonoBehaviour
{
    [SerializeField]
    public Image HpBar;

    public float maxHp;
    public float currentHp;
    public float hp;
    public int id;
    public float healthVelocity;
    public float smoothTime = 100f;

    private bool doingDamage = false;
    [SerializeField] float newHp;

    // void Update()
    // {
    //     newHp = Mathf.SmoothDamp(HpBar.fillAmount, hp, ref healthVelocity, smoothTime);
    //     HpBar.fillAmount = newHp;
    // }


    //chat gpt
    public float healthSpeed = 0.1f;

    private void Start()
    {
        currentHp = maxHp;
    }

    public void DoDamage(CadenceOptional damage) 
    {
        doingDamage = true;
        // currentHp = hp; // Why do we have this?

        float damageFloat = float.Parse((damage.Value as CadenceNumber).Value); // fetch damage

        // float newHp = hp - damageFloat;
        // hp = newHp;
        hp -= damageFloat;
        hp = Mathf.Clamp(hp, 0f, maxHp);
    }

    private void Update()
    {
        if (doingDamage)
        {
            float targetHp = hp / maxHp;
            newHp = Mathf.SmoothDamp(HpBar.fillAmount, targetHp, ref healthVelocity, healthSpeed);
            HpBar.fillAmount = newHp;
        } 
    }

    // public void TakeDamage(float damage)
    // {
    //     currentHp -= damage;
    //     currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
    // }

    /*
    private void DoDamage(CadenceOptional damage, float hp, BeastStats beastStats)
    {
        beastStats.currentHp = beastStats.hp;
        float damageFloat = float.Parse((damage.Value as CadenceNumber).Value); // fetch damage
        float newHp = hp - damageFloat;
        beastStats.hp = newHp;
        beastStats.hp = Mathf.Clamp(beastStats.hp, 0f, beastStats.maxHp);
        //ChangeAnimationState("GetHurt", beastStats.gameObject);
    }
    */

    
}


