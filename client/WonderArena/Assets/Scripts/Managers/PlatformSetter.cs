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

    public void SetAllBeast(List<string> namesOfBeasts)
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

        foreach (var beast in beastsPrefabs)
        {
            for (int i = 0; i < namesOfBeasts.Count; i++)
            {
                if (namesOfBeasts[i] != null)
                {
                    if (beast.name == namesOfBeasts[i].Split("_")[0] + "_" + namesOfBeasts[i].Split("_")[1])
                    {
                        GameObject newBeastObject = Instantiate(beast, 
                            platformsParent.transform.GetChild(i));
                        newBeastObject.name = namesOfBeasts[i];
                    }
                }
            }
        }
        
    }
}
