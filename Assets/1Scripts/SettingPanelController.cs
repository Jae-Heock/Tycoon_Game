using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingPanelController : MonoBehaviour
{
    private Vector3 hiddenScale = Vector3.zero;
    private Vector3 shownScale = Vector3.one;
    public bool isOpen = false;
    public GameObject returnToTitleUi;
    public GameObject quitGameUi;

    void Start()
    {
        transform.localScale = hiddenScale;
    }

    // 설정창 표시
    public void ShowPanel()
    {
        SoundManager.instance.ButtonClick();
        transform.localScale = shownScale;
        isOpen = true;
        GameManager.instance.pause.SetActive(false);
    }

    public void HidePanel()
    {
        SoundManager.instance.ButtonClick();
        transform.localScale = hiddenScale;
        isOpen = false;
        GameManager.instance.pause.SetActive(true);

    }

    // 타이틀로 돌아가기 위한 Yes/No 창 표시시
    public void ShowReturnToTitleUi()
    {
        SoundManager.instance.ButtonClick();
        returnToTitleUi.SetActive(true);
    }


    public void HideReturnTiTitleUi()
    {
        SoundManager.instance.ButtonClick();
        returnToTitleUi.SetActive(false);
    }

    // 타이틀로 돌아가기
    public void ReturnToTitle()
    {
        SoundManager.instance.ButtonClick();
        Time.timeScale = 1; // 시간 정상화
        SceneManager.LoadScene("TitleScene"); // 타이틀 씬으로 이동
    }

    // 게임 종료 위한 Yes/No 창 표시시
    public void ShowQuitGameUi()
    {
        SoundManager.instance.ButtonClick();
        quitGameUi.SetActive(true);
    }

    public void HideQuitGameUi()
    {
        SoundManager.instance.ButtonClick();
        quitGameUi.SetActive(false);
    }

    // 게임 종료
    public void QuitGame()
    {
        SoundManager.instance.ButtonClick();
        Application.Quit();
        Debug.Log("게임 종료");
    }


} 