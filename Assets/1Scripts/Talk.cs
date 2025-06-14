using UnityEngine;

public class Talk : MonoBehaviour
{
    public GameObject[] dialogueTexts; // Dialogue_text_1, Dialogue_text_2 등
    private int currentIndex = 0;
    public GameObject talkPanel;

    private void OnEnable()
    {
        currentIndex = 0;
        
        ShowCurrentText();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SoundManager.instance.ButtonClick();
            dialogueTexts[currentIndex].SetActive(false);
            currentIndex++;
            if (currentIndex < dialogueTexts.Length)
            {
                ShowCurrentText();
            }
            else
            {
                Time.timeScale = 1;
                // 대화 끝! BGM, CameraIntro 시작
                talkPanel.SetActive(false);
                SoundManager.instance.PlayGameBGM();
                var cameraIntro = FindFirstObjectByType<CameraIntro>();
                if (cameraIntro != null)
                    cameraIntro.StartIntro();
            }
        }
    }

    void ShowCurrentText()
    {
        for (int i = 0; i < dialogueTexts.Length; i++)
            dialogueTexts[i].SetActive(i == currentIndex);
    }
}
