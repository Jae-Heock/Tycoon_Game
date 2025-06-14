using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public Image [] tutoiralImages;

    private int currentIndex = 0;

    public void Start()
    {
        Time.timeScale = 0;
        SoundManager.instance.StopBGM();
        tutoiralImages[currentIndex].gameObject.SetActive(true);
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

        if (currentIndex == tutoiralImages.Length - 1)
        {
            SoundManager.instance.PlayGameBGM();
            Time.timeScale = 1;
            gameObject.SetActive(false);
        }
    }

    public void Previous()
    {
        SoundManager.instance.ButtonClick();
        tutoiralImages[currentIndex].gameObject.SetActive(false);
        currentIndex--;
        if (currentIndex >= 0)
        {
            tutoiralImages[currentIndex].gameObject.SetActive(true);
        }
    }

}
