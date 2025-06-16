using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public Image [] tutoiralImages;
    public GameObject talkPanel;

    private int currentIndex = 0;

    public void Start()
    {
        Debug.Log("튜토리얼 Start, SoundManager.instance: " + SoundManager.instance);
        SoundManager.instance.PlayTutorialBGM();
        Debug.Log("튜토리얼 BGM 재생 시도, tutorialBGM: " + SoundManager.instance.tutorialBGM);
        tutoiralImages[currentIndex].gameObject.SetActive(true);
        Time.timeScale = 0;
        SetCameraToFixedView();
    }

    public void Next()
    {
        SoundManager.instance.ButtonClick();
        tutoiralImages[currentIndex].gameObject.SetActive(false);
        currentIndex++;
        if (currentIndex < tutoiralImages.Length)
        {
            tutoiralImages[currentIndex].gameObject.SetActive(true);
        }

        if (currentIndex >= tutoiralImages.Length)
        {
            // 마지막 이미지가 보이도록 수정
            currentIndex = tutoiralImages.Length - 1;
            tutoiralImages[currentIndex].gameObject.SetActive(true);
            
            // 다음 버튼을 눌렀을 때만 talkPanel로 전환
            if (currentIndex == tutoiralImages.Length - 1)
            {
                talkPanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }

    public void Previous()
    {
        if (currentIndex == 0)
        {
            SoundManager.instance.ButtonClick();
            return; // 첫 번째 이미지면 아무것도 하지 않음
        }


        SoundManager.instance.ButtonClick();
        tutoiralImages[currentIndex].gameObject.SetActive(false);
        currentIndex--;
        tutoiralImages[currentIndex].gameObject.SetActive(true);
    }

    void SetCameraToFixedView()
    {
        FollowCamera cam = FindFirstObjectByType<FollowCamera>();
        if (cam != null)
        {
            cam.offset = new Vector3(0f, 7f, -8f); // 고정 시점 오프셋
            cam.xRotation = 45f;
            cam.yRotation = 45f;
            cam.isLocked = true; // 고정 모드
        }
    }

    void UnlockCamera()
    {
        FollowCamera cam = FindFirstObjectByType<FollowCamera>();
        if (cam != null)
        {
            cam.isLocked = false;
        }
    }

}
