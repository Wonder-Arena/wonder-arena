using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSetter : MonoBehaviour
{
    [SerializeField]
    GameObject platformsParent;
    [SerializeField]
    List<GameObject> beastsPrefabs = new();

    public static PlatformSetter Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        //// Make our Instance only for and for all scenes
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //}
        //else
        //{
        //    Instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
    }

    public void SetAllBeast(List<Beast.BeastStats> beastsStats)
    {
        foreach (Transform child in platformsParent.transform)
        {
            foreach (Transform childInChild in child)
            {
                if (childInChild.name != "Platform")
                {
                    Destroy(childInChild.gameObject);
                }
            }
        }

        foreach (var beastPrefab in beastsPrefabs)
        {
            for (int i = 0; i < beastsStats.Count; i++)
            {
                if (beastsStats[i] != null)
                {
                    if (beastPrefab.name == beastsStats[i].nameOfBeast + "_" + beastsStats[i].skin)
                    {
                        GameObject newBeastObject = Instantiate(beastPrefab, platformsParent.transform.GetChild(i));
                        newBeastObject.name = beastPrefab.name;
                    }
                }
            }
        }
        
    }
}
