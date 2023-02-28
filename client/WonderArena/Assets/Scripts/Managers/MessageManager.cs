using UnityEngine;
using TMPro;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    public TextMeshProUGUI messageText;
    public float fadeTime = 1.0f;
    public float messageDuration = 2.0f;

    [SerializeField]
    private TMP_FontAsset messageFont;
    [SerializeField]
    private GameObject messageCanvas;

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
        messageText.text = message;
        messageText.font = messageFont;
        StartCoroutine(FadeIn());
        float messageTime = duration > 0.0f ? duration : messageDuration;
        messageCanvas.SetActive(true);
        Invoke(nameof(HideMessage), messageTime);
    }

    private void HideMessage()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0.0f;
        Color startColor = messageText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1.0f);
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeTime);
            messageText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0.0f;
        Color startColor = messageText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0.0f);
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeTime);
            messageText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
    }
}