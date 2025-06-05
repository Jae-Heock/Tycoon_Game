using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BadCustomerUIManager : MonoBehaviour
{
    public GameObject panel;
    public Image uiImage;

    [Header("나쁜손님 이미지")]
    public Sprite stunSprite;
    public Sprite dalgonaSprite;
    public Sprite hotdogSprite;

    public static BadCustomerUIManager instance;

    private void Awake()
    {
        instance = this;
        panel.SetActive(false);
    }

    public void ShowBadCustomer(Custom.BadType type)
    {
        switch (type)
        {
            case Custom.BadType.Stun:
                uiImage.sprite = stunSprite;
                break;
            case Custom.BadType.Dalgona:
                uiImage.sprite = dalgonaSprite;
                break;
            case Custom.BadType.Hotdog:
                uiImage.sprite = hotdogSprite;
                break;
        }

        panel.SetActive(true);
        StartCoroutine(HideAfterDelay(2f));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }
}
