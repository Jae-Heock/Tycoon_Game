using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingPanelController : MonoBehaviour
{
    private Vector3 hiddenScale = Vector3.zero;
    private Vector3 shownScale = Vector3.one;
    public bool isOpen = false;

    void Start()
    {
        transform.localScale = hiddenScale;
    }

    public void ShowPanel()
    {
        transform.localScale = shownScale;
        isOpen = true;
        Time.timeScale = 0;
    }

    public void HidePanel()
    {
        SoundManager.instance.ButtonClick();
        transform.localScale = hiddenScale;
        isOpen = false;
        Time.timeScale = 1;
    }

    public void ReturnToTitle()
    {
        SoundManager.instance.ButtonClick();
        Time.timeScale = 1; // 시간 정상화
        SceneManager.LoadScene("TitleScene"); // 타이틀 씬으로 이동
    }
} 