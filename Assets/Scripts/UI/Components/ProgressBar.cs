using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Vector2 padding = new (9, 8);
    [SerializeField] private RectTransform mask;
    private RectTransform maskParentRectTransform;

    private void Awake()
    {
        if (mask == null) return;

        maskParentRectTransform = mask.parent.GetComponent<RectTransform>();
    }

    public void SetProgress(float progress)
    {
        Vector2 parentSize = maskParentRectTransform.sizeDelta;
        Vector2 targetSize = parentSize - padding * 2;

        targetSize.x *= Mathf.Clamp01(progress);

        mask.offsetMin = padding;
        mask.offsetMax = new(padding.x + targetSize.x - parentSize.x, -padding.y);
    }
}