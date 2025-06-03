using UnityEngine;

public class SettingPanelController : MonoBehaviour
{
    private Vector3 hiddenScale = Vector3.zero;
    private Vector3 shownScale = Vector3.one;
    private bool isOpen = false;

    void Start()
    {
        transform.localScale = hiddenScale;
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            HidePanel();
        }
    }

    public void ShowPanel()
    {
        transform.localScale = shownScale;
        isOpen = true;
    }

    public void HidePanel()
    {
        SoundManager.instance.ButtonClick();
        transform.localScale = hiddenScale;
        isOpen = false;
    }
} 