using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OrderListToggle : MonoBehaviour
{
    public RectTransform backGroundPanel; // BackGround의 RectTransform
    public GameObject openButton;         // Button (Legacy)
    public float animDuration = 0.5f;
    private float originalHeight;

    void Start()
    {
        originalHeight = backGroundPanel.sizeDelta.y;
        backGroundPanel.sizeDelta = new Vector2(backGroundPanel.sizeDelta.x, 0f);
        backGroundPanel.gameObject.SetActive(true);
        openButton.SetActive(true);
    }

    public void OnOpenButtonClick()
    {
        openButton.SetActive(false);
        backGroundPanel.gameObject.SetActive(true);
        StartCoroutine(AnimatePanel(0f, originalHeight, animDuration, true));
    }

    public void OnCloseButtonClick()
    {
        StartCoroutine(AnimatePanel(originalHeight, 0f, 0f, false));
    }

    IEnumerator AnimatePanel(float from, float to, float duration, bool opening)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float h = Mathf.Lerp(from, to, t / duration);
            Vector2 size = backGroundPanel.sizeDelta;
            size.y = h;
            backGroundPanel.sizeDelta = size;
            yield return null;
        }
        Vector2 finalSize = backGroundPanel.sizeDelta;
        finalSize.y = to;
        backGroundPanel.sizeDelta = finalSize;

        if (!opening)
        {
            backGroundPanel.gameObject.SetActive(false);
            openButton.SetActive(true);
        }
    }
}