using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class FixedAspectRatioUI : MonoBehaviour
{
    public float targetAspectRatio = 16f / 9f;
    private CanvasScaler canvasScaler;

    private void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        UpdateAspectRatio();
    }

    private void UpdateAspectRatio()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        float scaleRatio = targetAspectRatio / currentAspectRatio;
        canvasScaler.matchWidthOrHeight = scaleRatio > 1 ? 1 : scaleRatio;
    }

    private void Update()
    {
        UpdateAspectRatio();
    }
}
