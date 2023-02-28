using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DapperLabs.Flow.Sdk.Cadence;

public class BeastStats : MonoBehaviour
{
    [SerializeField]
    public Image HpBar;
    [SerializeField]
    public Image ManaBar;

    public float maxHp;
    public float manaRequired;
    public float currentMana;
    public float currentHp;
    public float hp;
    public int id;
    public float healthVelocity;
    public string manaText;
    public string hpText;


    private bool firstDamage = false;
    private bool firstMana = false;

    
    public float healthTime = 0.1f;

    private void Start()
    {
        currentHp = maxHp;
        currentMana = 0;
    }

    public void TakeDamage(CadenceOptional damage) 
    {
        firstDamage = true;
        float damageFloat = float.Parse((damage.Value as CadenceNumber).Value); // fetch damage
        hp -= damageFloat;
        hp = Mathf.Clamp(hp, 0f, maxHp);
    }

    public void GetMana(CadenceOptional damage) 
    {
        firstMana = true;
        float damageFloat = float.Parse((damage.Value as CadenceNumber).Value); // fetch damage
        currentMana += damageFloat;
        currentMana = Mathf.Clamp(currentMana, 0f, manaRequired);
        if (currentMana == manaRequired)
        {
            currentMana = 0;
        }
    }

    private void Update()
    {
        if (firstDamage)
        {
            float targetHp = hp / maxHp;
            float newHp = Mathf.SmoothDamp(HpBar.fillAmount, targetHp, ref healthVelocity, healthTime);
            HpBar.fillAmount = newHp;
        }
        if (firstMana)
        {
            float targetMana = currentMana / manaRequired;
            float newMana = Mathf.SmoothDamp(ManaBar.fillAmount, targetMana, ref healthVelocity, healthTime);
            ManaBar.fillAmount = newMana;
        }

        hpText = $"{currentHp}/{maxHp}";
        manaText = $"{currentMana}/{manaRequired}";
    }
    
}


