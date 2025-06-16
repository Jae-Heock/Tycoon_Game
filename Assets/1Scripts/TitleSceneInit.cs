using UnityEngine;

public class TitleSceneInit : MonoBehaviour
{
    void Start()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayTitleBGM();
        }
    }
} 