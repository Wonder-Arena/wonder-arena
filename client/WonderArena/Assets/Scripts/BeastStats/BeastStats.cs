using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeastStats : MonoBehaviour
{
    [SerializeField]
    public Image HpBar;

    public float maxHp;
    public float hp;
    public int id;

    private void Awake()
    {

    }

    private void Update()
    {
        HpBar.fillAmount = hp / maxHp;
    }
}
