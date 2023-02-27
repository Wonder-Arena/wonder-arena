using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;
    private Dictionary<string, IEnumerator> _runningCoroutines = new Dictionary<string, IEnumerator>();

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
        if (IsCoroutineRunning(coroutineName))
        {
            StopCoroutine(_runningCoroutines[coroutineName]);
            _runningCoroutines.Remove(coroutineName);
        }
        _runningCoroutines.Add(coroutineName, coroutine);
        StartCoroutine(RunCoroutineWrapper(coroutineName, coroutine));
    }

    private IEnumerator RunCoroutineWrapper(string coroutineName, IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);
        _runningCoroutines.Remove(coroutineName);
    }
}
