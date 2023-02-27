using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;
    private readonly Dictionary<string, IEnumerator> _runningCoroutines = new();

    public static CoroutineHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("CoroutineHelper").AddComponent<CoroutineHelper>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    public bool IsCoroutineRunning(string coroutineName)
    {
        return _runningCoroutines.ContainsKey(coroutineName);
    }

    public void RunCoroutine(string coroutineName, IEnumerator coroutine)
    {
        //if (IsCoroutineRunning(coroutineName))
        //{
        //    StopCoroutine(_runningCoroutines[coroutineName]);
        //    _runningCoroutines.Remove(coroutineName);
        //}
        Debug.Log("Coroutine Helper finished " + coroutineName);
        _runningCoroutines.Add(coroutineName, coroutine);
        StartCoroutine(RunCoroutineWrapper(coroutineName, coroutine));
    }

    public bool AreAllCoroutinesFinished()
    {
        return _runningCoroutines.Count == 0;
    }

    public bool AreAllCoroutinesFinishedExept(string coroutineName)
    {
        if (_runningCoroutines.Count == 1 && _runningCoroutines.ContainsKey(coroutineName))
        {
            return true;
        }
        return _runningCoroutines.Count == 0;
    }

    private IEnumerator RunCoroutineWrapper(string coroutineName, IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);
        Debug.Log("Coroutine Helper finished " + coroutineName);
        _runningCoroutines.Remove(coroutineName);
    }
}