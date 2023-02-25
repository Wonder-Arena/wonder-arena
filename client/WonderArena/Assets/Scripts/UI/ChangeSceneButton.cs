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
        StartCoroutine(waitForDefenders());
    }

    private IEnumerator waitForDefenders()
    {
        yield return StartCoroutine(FlowInterfaceBB.Instance.GetUserDefenderGroups());
        LevelManager.Instance.LoadScene("DefendTeam");
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
        if (GameManager.Instance.HaveAttackerComp())
        {
            Debug.Log("Have Attacker Comp");
            sceneName = "TeamMakingAttacking";
        }
        else
        {
            Debug.Log("Don't Have Attacker Comp");
            sceneName = "";
        }
        LevelManager.Instance.LoadScene(sceneName);
    }
}
