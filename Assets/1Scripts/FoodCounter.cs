using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어가 만든 달고나를 저장하고, 배달 AI가 이를 손님에게 전달하는 시스템.
/// </summary>
public class FoodCounter : MonoBehaviour
{
    [Header("AI 설정")]
    public GameObject aiPrefab;                         // 생성할 배달 AI 프리팹
    public Transform[] aiSpawnPoints;                   // AI가 생성될 위치들
    public List<FoodDeliveryAI> deliveryAIs = new();    // 현재 생성된 AI 목록

    [Header("음식 큐 관리")]
    private Queue<string> foodQueue = new();            // 대기 중인 음식 종류 이름 큐
    [SerializeField] private List<string> debugFoodList = new(); // 인스펙터에서 큐 내용 확인용 (디버깅용)

    private bool playerInZone = false;                  // 플레이어가 이 카운터 안에 있는지 확인
    private Player player;                             // 현재 플레이어 참조

    // ========== [1] 초기화 ==========

    private void Start()
    {
        // 주기적으로 배달 가능 여부 확인
        InvokeRepeating(nameof(CheckForDelivery), 0f, 1f);
    }

    // ========== [2] 매 프레임 처리 ==========
    private void Update()
    {
        // 플레이어가 카운터 안에 있고 E키 입력 시 음식 저장 시도
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            TryStoreFoodFromPlayer();
        }

        // Space 키 입력 시 AI 한 명 추가 생성 (디버깅용)
        if (Input.GetKeyDown(KeyCode.G))
        {
            SpawnNewAI();
        }
    }

    // ========== [3] 플레이어 구역 처리 ==========

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            player = other.GetComponent<Player>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            player = null;
        }
    }

    // ========== [4] 플레이어 음식 저장 ==========

    /// <summary>
    /// 플레이어가 가지고 있는 음식을 큐에 저장한다.
    /// </summary>
    void TryStoreFoodFromPlayer()
    {
        int storedCount = 0;

        // 달고나 저장
        while (player.dalgonaCount > 0)
        {
            foodQueue.Enqueue("dalgona");
            player.dalgonaCount--;
            storedCount++;
        }

        // 호떡 저장
        while (player.hottukCount > 0)
        {
            foodQueue.Enqueue("hottuk");
            player.hottukCount--;
            storedCount++;
        }
        // 핫도그 저장
        while (player.hotdogCount > 0)
        {
            foodQueue.Enqueue("hotdog");
            player.hotdogCount--;
            storedCount++;
        }

        // 붕어빵 저장
        while (player.boungCount > 0)
        {
            foodQueue.Enqueue("boung");
            player.boungCount--;
            storedCount++;
        }

        
        if (storedCount > 0)
        {
            Debug.Log($"플레이어로부터 {storedCount}개 음식 저장 완료 (현재 대기: {foodQueue.Count})");
            UpdateDebugList();
        }
        else
        {
            Debug.Log("플레이어가 보유한 음식 없음");
        }
    }

    // ========== [5] 배달 가능 여부 체크 및 배정 ==========

    /// <summary>
    /// AI 중에서 배달 가능한 자에게 음식을 할당 시도.
    /// </summary>
    void CheckForDelivery()
    {
        foreach (var ai in deliveryAIs)
        {
            if (!ai.IsBusy() && foodQueue.Count > 0)
            {
                TryAssignDeliveryToCustomer(ai);
            }
        }
    }

    /// <summary>
    /// 음식과 요청이 일치하는 손님을 찾아 AI에게 배달을 맡긴다.
    /// </summary>
    void TryAssignDeliveryToCustomer(FoodDeliveryAI ai)
    {
        string[] foodArray = foodQueue.ToArray(); // 큐 복사 (인덱스 접근을 위해)

        for (int i = 0; i < foodArray.Length; i++)
        {
            string currentFood = foodArray[i];

            // 현재 모든 손님 객체 검색
            Custom[] customers = Object.FindObjectsByType<Custom>(FindObjectsSortMode.None);

            foreach (var customer in customers)
            {
                // 배달 중이 아니고 요청과 일치하는 손님 찾기
                if (!customer.isBadCustomer && customer.RequestedFood == currentFood && !customer.IsBeingDelivered)
                {
                    Debug.Log($"요청 일치! AI에게 {currentFood} 배달 시작");

                    RemoveFoodAtIndex(i);             // 대기 큐에서 해당 음식 제거
                    customer.MarkBeingDelivered();    // 손님을 '배달중' 상태로 표시
                    ai.AssignDelivery(currentFood, customer); // AI에게 배달 할당
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 대기 큐에서 특정 인덱스의 음식을 제거하는 함수
    /// </summary>
    void RemoveFoodAtIndex(int index)
    {
        var newQueue = new Queue<string>();
        int i = 0;
        foreach (var item in foodQueue)
        {
            if (i != index) newQueue.Enqueue(item);
            i++;
        }
        foodQueue = newQueue;
        UpdateDebugList();
    }

    /// <summary>
    /// Inspector 창의 리스트 업데이트
    /// </summary>
    void UpdateDebugList()
    {
        debugFoodList = new List<string>(foodQueue);
    }

    // ========== [6] AI 생성 ==========

    /// <summary>
    /// AI를 프리팹으로 하나씩 생성하고 리스트에 추가.
    /// </summary>
    public void SpawnNewAI()
    {
        // 생성 위치 선택
        int spawnIndex = deliveryAIs.Count % aiSpawnPoints.Length;
        Transform spawnPoint = aiSpawnPoints[spawnIndex];

        // AI 생성
        GameObject newAI = Instantiate(aiPrefab, spawnPoint.position, Quaternion.identity);
        FoodDeliveryAI aiScript = newAI.GetComponent<FoodDeliveryAI>();

        if (aiScript != null)
        {
            aiScript.homePosition = spawnPoint; // 생성 위치 저장
            deliveryAIs.Add(aiScript);          // 리스트 추가
        }
        else
        {
            Debug.LogError("생성된 AI에 FoodDeliveryAI 컴포넌트가 없음");
        }
    }
}
