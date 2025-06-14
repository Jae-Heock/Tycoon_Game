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
    public GameObject pause;
    public GameObject tutorial;

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

    [Header("클리어 UI")]
    public GameObject clearPanel;
    public Text successText;
    public Text failText;
    public Text pointText;

    [Header("실패 UI")]
    public GameObject failPanel;
    public Text failSuccessText;
    public Text failFailText;
    public Text failPointText;

    private bool isGameCleared = false;

    private void Awake()
    {
        instance = this;
                // 자동으로 Player 찾아서 연결 (안 되어 있으면)
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    private void Start()
    {
        tutorial.SetActive(true);
        // 게임 설정 초기화
        gameTime = 0;
        
        // UI 업데이트
        UpdateUI();
        
        // StartGame(); // ← 이 줄을 주석 처리 또는 삭제
    }

    private void Update()
    {
        gameTime += Time.deltaTime;
        if (gameTime > maxGameTime)
            gameTime = maxGameTime;

            // 테스트용: X 키를 누르면 즉시 5분 경과 처리
    if (Input.GetKeyDown(KeyCode.X))
    {
        gameTime = maxGameTime;
        Debug.Log("⚡ X키로 게임 시간 강제 종료!");
    }
        // UI 업데이트
        UpdateUI();

        // 일시정지 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundManager.instance.ButtonClick();

            if(settingPanel.isOpen)
            {
                settingPanel.HidePanel();
                return;
            }

            if(settingPanel.returnToTitleUi.activeSelf)
            {
                settingPanel.HideReturnTiTitleUi();
                return;
            }

            if(settingPanel.quitGameUi.activeSelf)
            {
                settingPanel.HideQuitGameUi();
                return;
            }

            bool isPause = pause.activeSelf;

            if (!isPause)
            {
                SoundManager.instance.PauseBGM();
                pause.SetActive(true);
                Time.timeScale = 0;
                // settingPanel.ShowPanel();
            }
            else
            {
                pause.SetActive(false);
                Time.timeScale = 1;
                // settingPanel.HidePanel();
                SoundManager.instance.ResumeBGM();
            }
        }

        // 5분이 지나면 점수에 따라 클리어/실패 판정
        if (gameTime >= maxGameTime && !isGameCleared)
        {
            isGameCleared = true;
            if (player.Point >= 10)
            {
                ShowClearPanel();
            }
            else
            {
                ShowFailPanel();
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

        // 플레이어 이동 멈춤
        if (player != null)
            player.isMove = false;
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

    void ShowClearPanel()
    {
        clearPanel.SetActive(true);
        successText.text = player.customerSuccessCount.ToString();
        failText.text = player.customerFailCount.ToString();
        pointText.text = player.Point.ToString();

        // 1. 플레이어 이동 완전 차단
        if (player != null)
            player.isMove = false;

        // 2. 사운드 완전 정지
        SoundManager.instance.StopBGM();
        SoundManager.instance.PlayClearBGM();
        // 3. 게임 전체 멈춤 (선택)
        Time.timeScale = 0;
    }

    void ShowFailPanel()
    {
        failPanel.SetActive(true);
        failSuccessText.text = player.customerSuccessCount.ToString();
        failFailText.text = player.customerFailCount.ToString();
        failPointText.text = player.Point.ToString();

        // 1. 플레이어 이동 완전 차단
        if (player != null)
            player.isMove = false;

        // 2. 사운드 완전 정지
        SoundManager.instance.StopBGM();
        SoundManager.instance.PlayClearBGM();
        // 3. 게임 전체 멈춤 (선택)
        Time.timeScale = 0;
    }

    public void ExitGame()
    {
        SoundManager.instance.ButtonClick();
        Time.timeScale = 1;
        SceneManager.LoadScene("TitleScene");
    }

}
