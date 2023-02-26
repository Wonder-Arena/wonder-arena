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
    private bool firstDamage = false;
    [SerializeField] float newHp;

    public float healthTime = 0.1f;

    private void Start()
    {
        currentHp = maxHp;
    }

    public void DoDamage(CadenceOptional damage) 
    {
        firstDamage = true;
        float damageFloat = float.Parse((damage.Value as CadenceNumber).Value); // fetch damage
        hp -= damageFloat;
        hp = Mathf.Clamp(hp, 0f, maxHp);
    }

    private void Update()
    {
        if (firstDamage)
        {
            float targetHp = hp / maxHp;
            newHp = Mathf.SmoothDamp(HpBar.fillAmount, targetHp, ref healthVelocity, healthTime);
            HpBar.fillAmount = newHp;
        } 
    }
    
}


