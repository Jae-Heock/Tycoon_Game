using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DalgonaGameManager : MonoBehaviour
{
    public static DalgonaGameManager Instance;

    [Header("정확도 설정")]
    public float accuracy = 100f;
    public float initialPenaltyAmount = 1f;   // 진입 시 한 번만 깎이는 점수
    public float continuousPenaltyAmount = 0.1f; // 머무는 동안 '초당' 깎이는 점수
    public float successThreshold = 80f;

    [Header("오브젝트 연결")]
    public GameObject cursorObject;
    public GameObject startFinishObject;
    public Transform checkpointsParent;

    private TrailRenderer trail;
    private HashSet<GameObject> passedCheckpoints = new HashSet<GameObject>();
    private int totalCheckpoints = 0;
    public bool isGameActive = false;

    [Header("UI 연결")]
    public Text accuracyText;
    public GameObject guideTextObject; // 시작 안내 문구 UI

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
        if (accuracyText != null)
        {
            accuracyText.text = $"정확도 : {accuracy:F0}%";
        }
    }

    // 게임 시작을 준비하고, 커서를 시작 위치로 이동시킵니다.
    void PrepareGame()
    {
        isGameActive = false;
        accuracy = 100f;
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

        // 커서를 시작 위치로 이동시켜 대기
        if (cursorObject != null && startFinishObject != null)
        {
            cursorObject.transform.position = startFinishObject.transform.position;
        }

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
        }
        else
        {
            Debug.Log("실패! 다시 도전하세요.");
            PrepareGame();
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
