using DapperLabs.Flow.Sdk.Cadence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoardManager : MonoBehaviour
{
    [SerializeField]
    GameObject LeaderboardRowPrefab;
    [SerializeField]
    GameObject contentParent;
    List<string> allPlayersScore = new();
    

    FlowInterfaceBB flowInterface;

    private void Awake()
    {
        flowInterface = FlowInterfaceBB.Instance;
    }

    private IEnumerator Start()
    {
        bool completed = false;
        while (!completed)
        {
            completed = true;
            completed = completed && flowInterface.isScriptsCompleted;
            yield return null;
        }

        foreach (CadenceDictionaryItem player in flowInterface.allPlayers_ListDictionaryItems)
        {
            string name = (player.Value as CadenceComposite).CompositeFieldAs<CadenceString>("name").Value;
            string score = (player.Value as CadenceComposite).CompositeFieldAs<CadenceNumber>("score").Value;

            string playerName = name + "_" + score;

            allPlayersScore.Add(playerName);
        }
    }
}
