using UnityEngine.SceneManagement;
using UnityEngine;


public class SceneChanger : MonoBehaviour
{
    public void Change()
    {
        SoundManager.instance.StopBGM();
        SceneManager.LoadScene("StoryScene");
    }
}