using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beast : MonoBehaviour
{
    public string nameOfBeast;
    public string id;
    public string hp;
    public string manaRequired;
    public string skin;

    public void CopyFrom(Beast other)
    {
        this.nameOfBeast = other.nameOfBeast;
        this.id = other.id;
        this.hp = other.hp;
        this.manaRequired = other.manaRequired;
        this.skin = other.skin;
    }
}