using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 게임의 전반적인 상태와 진행을 관리하는 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SettingPanelController settingPanel;

    [Header("# Game Control")]
    public float gameTime;
    public float maxGameTime = 300f;  // 5분(300초)으로 설정

    [Header("# Player Info")]
    public int level;
    public int clearedCustomerCount; // 손님 클리어 시 경험치로 전환되는 수치

    // 레벨업에 필요한 클리어 수
    public int[] nextCustomer = { 1, 3, 5, 7, 9, 11, 13, 15, 17, 20 };

    [Header("# Game Object")]
    public Player player;
    public LevelUp uiLevelUp;

    public bool hasBadCustomer = false;
    public Custom badCustomer = null;

    [Header("UI 요소")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public Text scoreText;
    public Text timerText;

    [Header("고양이 표정")]
    public GameObject happyCat;
    public GameObject sadCat;
    [Header("나쁜 손님 UI")]
    public BadCustomerUIManager badCustomerUI;


    private void Awake()
    {
        instance = this;
                // 자동으로 Player 찾아서 연결 (안 되어 있으면)
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    private void Start()
    {
        SoundManager.instance.PlayGameBGM();
        // 게임 설정 초기화
        gameTime = 0;
        
        // UI 업데이트
        UpdateUI();
        
        // 게임 시작
        StartGame();
    }

    private void Update()
    {
        gameTime += Time.deltaTime;
        if (gameTime > maxGameTime)
            gameTime = maxGameTime;

        // UI 업데이트
        UpdateUI();

        // 일시정지 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundManager.instance.ButtonClick();
            if (!settingPanel.isOpen)
            {
                SoundManager.instance.PauseBGM();
                settingPanel.ShowPanel();
            }
            else
            {
                settingPanel.HidePanel();
                SoundManager.instance.ResumeBGM();
            }
        }
    }

    private void UpdateUI()
    {
        // 점수 표시
        if (scoreText != null)
            scoreText.text = $"Score: {clearedCustomerCount}";

        // 시간 표시
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartGame()
    {
        clearedCustomerCount = 0;
        gameTime = 0;
        UpdateUI();
    }

    public void GameOver()
    {
        // 게임 오버 UI 표시
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        
        if (pausePanel != null)
            pausePanel.SetActive(Time.timeScale == 0);
    }

    public void IncreaseCustomerCount()
    {
        clearedCustomerCount++;

        GetExp();
    }

    void GetExp()
    {
        if (clearedCustomerCount == nextCustomer[Mathf.Min(level, nextCustomer.Length-1)])
        {
            level++;
            clearedCustomerCount = 0;
            // 레벨업 효과가 있다면 여기에 추가
            Debug.Log($"레벨업! 현재 레벨: {level}");
            uiLevelUp.Show();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 고양이 얼굴 변하기 
    public void HappyCat()
    {
        StartCoroutine(PlayHappyCat());
    }
    private IEnumerator PlayHappyCat()
    {
        happyCat.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        happyCat.SetActive(false);
    }

    public void SadCat()
    {
        StartCoroutine(PlaySadCat());
    }
    private IEnumerator PlaySadCat()
    {
        sadCat.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        sadCat.SetActive(false);
    }
}
