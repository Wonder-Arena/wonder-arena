using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beast : MonoBehaviour
{
    public BeastStats beastStats = new();

    [Serializable]
    public class BeastStats
    {
        public string nameOfBeast;
        public string id;
        public string hp;
        public string manaRequired;
        public string skin;
    }

    public void CopyFrom(Beast.BeastStats other)
    {
        this.beastStats.nameOfBeast = other.nameOfBeast;
        this.beastStats.id = other.id;
        this.beastStats.hp = other.hp;
        this.beastStats.manaRequired = other.manaRequired;
        this.beastStats.skin = other.skin;
    }
}