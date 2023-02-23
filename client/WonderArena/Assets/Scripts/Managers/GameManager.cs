using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string userFlowAddress = null;

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

    public bool HaveAttackerComp()
    {
        return true;
    }
}
