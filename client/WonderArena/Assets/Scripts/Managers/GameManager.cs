using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string userFlowAddress = null;
    public string userAccessToken = null;
    public string lastFightRecord = null;
    public string lastDefenderAddress = null;

    public List<string> attackerComp = new();
    public List<string> lastDefenderNamesOfPawns = new();
    public List<string> allUserDefendersGroup = new();

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
