using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField]
    private GameObject _loaderCanvas;
    [SerializeField]
    private Image _progressBar;
    [SerializeField]
    private TextMeshProUGUI _progressText;
    private float _target;
    private CanvasGroup canvasGroup;
    public bool isLoadedScene;
    private float _targedAplpha;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        canvasGroup = _loaderCanvas.GetComponent<CanvasGroup>();
    }

    public async void LoadScene(string sceneName)
    {
        _target = 0;
        _targedAplpha = 0;
        canvasGroup.alpha = 1;
        _progressBar.fillAmount = 0;

        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        _loaderCanvas.SetActive(true);

        do
        {
            await Task.Delay(100);
            _target = scene.progress;

        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;

        await Task.Delay(500);

        StartCoroutine(WaitForEnumerators(scene));
    }

    private IEnumerator WaitForEnumerators(AsyncOperation scene)
    {
        while (!CoroutineHelper.Instance.AreAllCoroutinesFinished())
        {
            if (_target < 0.99f)
            {
                _target += 0.0008f;
            }
            yield return null;
        }

        _target = 1;

        isLoadedScene = true;
    }

    //public void LoadScene(string nameScene)
    //{
    //    StartCoroutine(LoadSceneEnum(nameScene));
    //}

    //public IEnumerator LoadSceneEnum(string sceneName)
    //{
    //    AsyncOperation gameLevel = SceneManager.LoadSceneAsync(sceneName);

    //    _loaderCanvas.SetActive(true);

    //    while (!gameLevel.isDone && !CoroutineHelper.Instance.AreAllCoroutinesFinished())
    //    {
    //        float progress = Mathf.Clamp01(gameLevel.progress / 0.9f);
    //        _progressBar.fillAmount = progress;
    //        //_progressText.text = "Loading " + (progress * 100f).ToString("F0") + "%";

    //        yield return null;
    //    }

    //    _loaderCanvas.SetActive(false);
    //}

    private void Update()
    {
        _progressBar.fillAmount = Mathf.MoveTowards(_progressBar.fillAmount, _target, 1f * Time.deltaTime);
        if (isLoadedScene)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targedAplpha, 1.5f * Time.deltaTime);
            if (canvasGroup.alpha == 0)
            {
                _loaderCanvas.SetActive(false);
                canvasGroup.alpha = 1;
                isLoadedScene = false;
            }
        }
    }
}
