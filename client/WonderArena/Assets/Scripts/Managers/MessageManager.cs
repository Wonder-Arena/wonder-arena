using UnityEngine;
using TMPro;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    public float fadeTime = 1.0f;
    public float messageDuration = 2.0f;

    [SerializeField]
    private TMP_FontAsset messageFont;
    [SerializeField]
    private CanvasGroup messageCanvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowMessage(string message, float duration = 0.0f)
    {
        StartCoroutine(FadeIn());
        float messageTime = duration > 0.0f ? duration : messageDuration;
        Invoke(nameof(HideMessage), messageTime);

        foreach (TextMeshProUGUI textMeshPro in messageCanvasGroup.GetComponentsInChildren<TextMeshProUGUI>())
        {
            textMeshPro.text = message;
            textMeshPro.font = messageFont;
        }
    }

    private void HideMessage()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0.0f;
        Color startColor = messageCanvasGroup.alpha * Color.white;
        Color endColor = Color.white;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeTime);
            messageCanvasGroup.alpha = Mathf.Lerp(startColor.a, endColor.a, t);
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0.0f;
        Color startColor = messageCanvasGroup.alpha * Color.white;
        Color endColor = Color.clear;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeTime);
            messageCanvasGroup.alpha = Mathf.Lerp(startColor.a, endColor.a, t);
            yield return null;
        }
    }
}