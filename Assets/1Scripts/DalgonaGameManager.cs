using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class DalgonaGameManager : MonoBehaviour
{
    public static DalgonaGameManager Instance;

    [Header("정확도 설정")]
    public float accuracy = 100f;
    public float initialPenaltyAmount = 1f;   // 진입 시 한 번만 깎이는 점수
    public float continuousPenaltyAmount = 0.1f; // 머무는 동안 '초당' 깎이는 점수
    public float successThreshold = 80f;
    public float gameTimeLimit = 30f; // 게임 제한시간 30초

    [Header("오브젝트 연결")]
    public GameObject cursorObject;
    public GameObject startFinishObject;
    public Transform checkpointsParent;

    private TrailRenderer trail;
    private HashSet<GameObject> passedCheckpoints = new HashSet<GameObject>();
    private int totalCheckpoints = 0;
    public bool isGameActive = false;
    private float currentGameTime = 0f; // 현재 게임 시간

    [Header("UI 연결")]
    public Text accuracyText;
    public GameObject guideTextObject; // 시작 안내 문구 UI
    public Text timeText; // 남은 시간 표시용

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 스크립트에서 필요한 컴포넌트와 오브젝트를 찾아 초기화합니다.
        if (cursorObject == null) cursorObject = GameObject.FindWithTag("Player");
        if (startFinishObject == null) startFinishObject = GameObject.FindWithTag("StartFinish");
        if (cursorObject != null) trail = cursorObject.GetComponent<TrailRenderer>();
        if (checkpointsParent != null) totalCheckpoints = checkpointsParent.childCount;

        PrepareGame();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isGameActive)
        {
            StartGame();
        }
        else if (Input.GetMouseButtonUp(0) && isGameActive)
        {
            EndGame(IsCursorOverStartFinish());
        }
        
        // 게임 진행 중일 때 타이머 업데이트
        if (isGameActive)
        {
            currentGameTime += Time.deltaTime;
            if (currentGameTime >= gameTimeLimit)
            {
                // 시간 초과로 실패
                EndGame(false);
            }
        }
        
        if (accuracyText != null)
        {
            accuracyText.text = $"정확도 : {accuracy:F0}%";
        }

        if (timeText != null)
        {
            float remainingTime = gameTimeLimit - currentGameTime;
            if (remainingTime < 0) remainingTime = 0;
            
            // 남은 시간을 "초:밀리초" 형태로 표시
            int seconds = Mathf.FloorToInt(remainingTime);
            int milliseconds = Mathf.FloorToInt((remainingTime * 100) % 100);
            timeText.text = $"남은시간 : {seconds:D2}:{milliseconds:D2}";
        }
    }

    // 게임 시작을 준비하고, 커서를 시작 위치로 이동시킵니다.
    public void PrepareGame()
    {
        // BGM 정지하지 않음 (게임 BGM이 계속 재생되도록)
        // if (SoundManager.instance != null)
        // {
        //     SoundManager.instance.StopBGM();
        // }

        accuracy = 100f;
        isGameActive = false;
        currentGameTime = 0f; // 타이머 초기화
        passedCheckpoints.Clear();

        // [추가] 안내 문구 보이기
        if (guideTextObject != null)
        {
            guideTextObject.SetActive(true);
        }

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        // // 커서를 시작 위치로 이동시켜 대기
        // if (cursorObject != null && startFinishObject != null)
        // {
        //     cursorObject.transform.position = startFinishObject.transform.position;
        // }

        if (checkpointsParent != null)
        {
            foreach (Transform cp in checkpointsParent)
            {
                cp.gameObject.SetActive(true);
            }
        }

        Debug.Log($"게임 준비 완료. 커서를 시작점으로 이동했습니다. 총 체크포인트: {totalCheckpoints}");
    }

    public void StartGame()
    {
        isGameActive = true;
        accuracy = 100f;
        currentGameTime = 0f; // 타이머 초기화
        passedCheckpoints.Clear();
        
        // [추가] 안내 문구 숨기기
        if (guideTextObject != null)
        {
            guideTextObject.SetActive(false);
        }

        if (trail != null)
        {
            trail.Clear(); // 혹시 모를 흔적을 지우고 시작
            trail.emitting = true;
        }

        Debug.Log("게임 시작!");
    }

    public void EndGame(bool isValidEnd)
    {
        isGameActive = false;
        bool allPassed = passedCheckpoints.Count >= totalCheckpoints;

        if (!isValidEnd || !allPassed)
        {
            accuracy = 0f;
            if (!isValidEnd) Debug.Log("게임 실패: Finish 지점에서 끝나지 않았습니다!");
            if (!allPassed) Debug.Log($"게임 실패: 모든 체크포인트를 통과하지 못했습니다. ({passedCheckpoints.Count}/{totalCheckpoints})");
        }

        Debug.Log($"게임 종료 - 최종 정확도: {accuracy:F1}%");

        if (accuracy >= successThreshold)
        {
            Debug.Log("성공! 달고나 획득!");
            // 성공 시 나쁜 손님 제거하고 게임 씬으로 돌아가기
            StartCoroutine(ReturnToGameSceneWithSuccess());
        }
        else
        {
            Debug.Log("실패! 다시 도전하세요.");
            // 실패 시 나쁜 손님은 그대로 두고 게임 씬으로 돌아가기
            StartCoroutine(ReturnToGameSceneWithFailure());
        }
    }

    private IEnumerator ReturnToGameSceneWithSuccess()
    {
        // 성공 효과음 재생
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySuccess();
        }
        
        // 잠시 대기 (성공 메시지를 볼 수 있도록)
        yield return new WaitForSeconds(2f);
        
        // 달고나 씬 언로드
        SceneManager.UnloadSceneAsync("DalgonaScene");
        
        // 게임 씬의 모든 오브젝트 다시 활성화
        Scene gameScene = SceneManager.GetSceneByName("GameScene");
        if (gameScene.isLoaded)
        {
            GameObject[] rootObjects = gameScene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                rootObject.SetActive(true);
            }
            
            // 게임 씬을 활성 씬으로 설정
            SceneManager.SetActiveScene(gameScene);
            
            // 게임 시간 정상화
            Time.timeScale = 1f;
            
            // 나쁜 손님 제거
            if (GameManager.instance != null && GameManager.instance.badCustomer != null)
            {
                GameManager.instance.badCustomer.RemoveBadCustomer();
            }
            
            // 플레이어 상태 리셋
            if (GameManager.instance != null && GameManager.instance.player != null)
            {
                GameManager.instance.player.ResetState();
            }

            // 모든 파티클 끄기
            if (GameManager.instance != null)
            {
                GameManager.instance.StopAllCookingParticles();
            }
            
            // CustomSpawner 재시작
            CustomSpawner spawner = FindFirstObjectByType<CustomSpawner>();
            if (spawner != null)
            {
                // 스폰 루프를 다시 시작
                spawner.RestartSpawning();
                Debug.Log("CustomSpawner 스폰 루프를 재시작했습니다.");
            }
            
            Debug.Log("게임 씬으로 돌아왔습니다! 나쁜 손님이 제거되었습니다.");
        }
    }
    
    private IEnumerator ReturnToGameSceneWithFailure()
    {
        // 실패 효과음 재생
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayFail();
        }
        
        // 잠시 대기 (실패 메시지를 볼 수 있도록)
        yield return new WaitForSeconds(2f);
        
        // 달고나 씬 언로드
        SceneManager.UnloadSceneAsync("DalgonaScene");
        
        // 게임 씬의 모든 오브젝트 다시 활성화
        Scene gameScene = SceneManager.GetSceneByName("GameScene");
        if (gameScene.isLoaded)
        {
            GameObject[] rootObjects = gameScene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                rootObject.SetActive(true);
            }
            
            // 게임 씬을 활성 씬으로 설정
            SceneManager.SetActiveScene(gameScene);
            
            // 게임 시간 정상화
            Time.timeScale = 1f;
            
            // 플레이어 상태 리셋
            if (GameManager.instance != null && GameManager.instance.player != null)
            {
                GameManager.instance.player.ResetState();
            }

            // 모든 파티클 끄기
            if (GameManager.instance != null)
            {
                GameManager.instance.StopAllCookingParticles();
            }
            
            // CustomSpawner 재시작
            CustomSpawner spawner = FindFirstObjectByType<CustomSpawner>();
            if (spawner != null)
            {
                // 스폰 루프를 다시 시작
                spawner.RestartSpawning();
                Debug.Log("CustomSpawner 스폰 루프를 재시작했습니다.");
            }
            
            Debug.Log("게임 씬으로 돌아왔습니다! 다시 도전하세요.");
        }
    }

    // 진입하는 순간 한 번만 호출되는 패널티
    public void ApplyInitialPenalty()
    {
        if (!isGameActive) return;
        accuracy -= initialPenaltyAmount;
        accuracy = Mathf.Max(0f, accuracy);
        Debug.Log($"최초 진입 패널티! 정확도 -{initialPenaltyAmount}점. 현재 정확도: {accuracy:F1}%");
    }

    // 머무는 동안 계속 호출되는 패널티
    public void ApplyContinuousPenalty()
    {
        if (!isGameActive) return;
        accuracy -= continuousPenaltyAmount * Time.deltaTime;
        accuracy = Mathf.Max(0f, accuracy);
    }

    public void CheckpointHit(GameObject checkpoint)
    {
        if (passedCheckpoints.Add(checkpoint))
        {
            SoundManager.instance.ButtonClick();
            Debug.Log($"체크포인트 통과! ({passedCheckpoints.Count}/{totalCheckpoints})");
            checkpoint.SetActive(false);
        }
    }

    private bool IsCursorOverStartFinish()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider.CompareTag("StartFinish");
        }
        return false;
    }
}
