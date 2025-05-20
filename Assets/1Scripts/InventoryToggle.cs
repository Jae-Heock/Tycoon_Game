using UnityEngine;
using System.Collections;
public class InventoryToggle : MonoBehaviour
{
    public RectTransform inventoryPanel;
    private bool isVisible = true;

    public float animationSpeed = 5f;

    private Vector3 visibleScale = Vector3.one * 1.35f;
    private Vector3 hiddenScale = Vector3.zero;

    public void ToggleInventory()
    {
        isVisible = !isVisible;
        StopAllCoroutines();
        StartCoroutine(AnimateToggle(isVisible));
    }

    private IEnumerator AnimateToggle(bool show)
    {
        Vector3 start = inventoryPanel.localScale;
        Vector3 target = show ? visibleScale : hiddenScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            inventoryPanel.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        inventoryPanel.localScale = target;
    }
}
