using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneButton : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        LevelManager.Instance.LoadScene(sceneName);
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
        Debug.Log(CharacterManager.Instance.listOfAllSelectedUnits[0]);
        if (GameManager.Instance.HaveAttackerComp())
        {
            Debug.Log("Have Attacker Comp");
            sceneName = "";
        }
        else
        {
            Debug.Log("Don't Have Attacker Comp");
            sceneName = "";
        }
        LevelManager.Instance.LoadScene(sceneName);
    }
}
