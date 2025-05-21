using UnityEngine.SceneManagement;
using UnityEngine;


public class SceneChanger : MonoBehaviour
{
    public void Change()
    {
        SceneManager.LoadScene("GameScene");
    }
}