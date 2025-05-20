using UnityEngine;

public class ReceiptToggle : MonoBehaviour
{
    public RectTransform targetPanel; // 조작법 UI

    private bool isExpanded = true;   // 현재 상태
    private Vector3 originalScale;
    private Vector3 collapsedScale = new Vector3(0f, 0f, 0f); // 축소 시

    private void Start()
    {
        if (targetPanel != null)
            originalScale = targetPanel.localScale;
    }

    public void TogglePanel()
    {
        isExpanded = !isExpanded;

        if (targetPanel != null)
        {
            targetPanel.localScale = isExpanded ? originalScale : collapsedScale;
        }
    }
}
