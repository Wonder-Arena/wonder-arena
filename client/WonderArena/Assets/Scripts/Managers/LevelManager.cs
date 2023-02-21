using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField]
    private GameObject _loaderCanvas;
    [SerializeField]
    private Image _progressBar;
    private float _target;
    private CanvasGroup canvasGroup;
    private bool isLoadedScene;
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
        _target = 1;
        await Task.Delay(500);

        isLoadedScene = true;
        scene.allowSceneActivation = true;
    }

    private void Update()
    {
        _progressBar.fillAmount = Mathf.MoveTowards(_progressBar.fillAmount, _target, 3 * Time.deltaTime);
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
