using UnityEngine;

public class Talk : MonoBehaviour
{
    public GameObject[] dialogueTexts; // Dialogue_text_1, Dialogue_text_2 등
    private int currentIndex = 0;
    public GameObject talkPanel;

    private void OnEnable()
    {
        currentIndex = 0;
        SetCameraToFixedView(); // Talk가 켜질 때 카메라 고정
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
                UnlockCamera();
            }
        }
    }

    void ShowCurrentText()
    {
        for (int i = 0; i < dialogueTexts.Length; i++)
            dialogueTexts[i].SetActive(i == currentIndex);
    }

    void SetCameraToFixedView()
    {
        FollowCamera cam = FindFirstObjectByType<FollowCamera>();
        if (cam != null)
        {
            cam.offset = new Vector3(0f, 7f, -8f);
            cam.xRotation = 45f;
            cam.yRotation = 45f;
            cam.isLocked = true;

            // transform 즉시 갱신
            Quaternion rotation = Quaternion.Euler(cam.yRotation, cam.xRotation, 0);
            Vector3 desiredOffset = rotation * new Vector3(0, 0, -cam.offset.magnitude);
            Vector3 targetPosition = cam.target.position + desiredOffset;
            cam.transform.position = targetPosition;
            cam.transform.LookAt(cam.target.position);
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
