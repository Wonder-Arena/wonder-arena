using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        // Make our Instance only for and for all scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    //public List<GameObject> GetAttackerComp()
    //{
    //    if (HaveAttackerComp)
    //    {

    //    }
    //}

    public bool HaveAttackerComp()
    {
        foreach (GameObject unit in CharacterManager.Instance.listOfAllSelectedUnits)
        {
            if (unit == null)
            {
                return false;
            }
        }

        return true;
    }
}
