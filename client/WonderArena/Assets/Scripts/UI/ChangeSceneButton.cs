using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneButton : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        LevelManager.Instance.LoadScene(sceneName);
    }

    public void ToDefendGroup()
    {       
        StartCoroutine(FlowInterfaceBB.Instance.GetUserDefenderGroups());
        LevelManager.Instance.LoadScene("DefendTeam");
    }

    private void Update()
    {

    }

    public void MakeATeamAndChangeScene(string sceneName)
    {
        if (GameManager.Instance.HaveAttackerComp())
        {
            LevelManager.Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Select your Units");
        }
    }

    public void ChangeSceneToChallenge()
    {
        string sceneName;
        if (GameManager.Instance.userDefenderGroups.Count > 0)
        {
            Debug.Log("Have Defender Comp");
            sceneName = "Leaderboard";
        }
        else
        {
            Debug.Log("Don't Have Defender Comp");
            sceneName = "RegisterPlayer";
        }
        LevelManager.Instance.LoadScene(sceneName);
    }
}
