using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Custom : MonoBehaviour
{
    public enum BadType { None, Dalgona, Hotdog, Stun }
    public BadType badType = BadType.None;

    // ====== 상태 ======
    public float waitTimer = 0f;
    public float maxWaitTime = 40f; // 손님 대기 시간 (초)
    private bool isRequesting = false;
    private bool isBeingDelivered = false;
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    
    private Player player;
    private string requestedFood = "";

    public bool IsBeingDelivered => isBeingDelivered;
    public string RequestedFood => requestedFood;

    // ====== 외부 연결 ======
    public CustomSpawner spawner;
    public bool isBadCustomer = false;
    public Transform spawnPoint;

    // ====== 쓰레기 관련 ======
    public GameObject trashPrefab;          // 쓰레기 프리팹
    private GameObject currentTrash;        // 현재 생성된 쓰레기
    public float trashSpawnChance = 0.2f;   // 쓰레기 생성 확률 (20%)

    private Coroutine stunCoroutine;
    private Coroutine tableCheckCoroutine;
    public void MarkBeingDelivered() => isBeingDelivered = true;

    // ======= UI 요소=======
    public GameObject orderIconObject; // 현재 떠 있는 아이콘 오브젝트
    public Transform iconSpawnPoint;   // 아이콘을 띄울 위치 (손님 머리 위 Transform)
    public GameObject dalgonaIconPrefab;  
    public GameObject hottukIconPrefab;
    public GameObject hotdogIconPrefab;
    public GameObject boungIconPrefab;

    [Header("Bad Customer Icons")]
    public GameObject badIconDalgonaPrefab;
    public GameObject badIconHotdogPrefab;
    public GameObject badIconStunPrefab;
    
    [Header("Wait UI")]
    public Slider waitSlider;         // 손님 대기시간 슬라이더
    public Canvas waitCanvas;         // 슬라이더가 붙은 World Space 캔버스

    [Header("Icon Rotation")]
    public float iconRotationSpeed = 100f; // 아이콘 회전 속도 (도/초)

    public CustomTable assignedTable; // 손님이 배정받은 테이블

    private bool isProcessed = false;  // 이미 처리된 손님인지 여부
    private bool isLeaving = false;    // 손님이 떠나는 중인지 여부

    private void Start()
    {
        if (!isRequesting && !isBadCustomer)
            RequestRandomFood();

        // 가장 가까운 CustomTable 찾기
        GameObject[] tables = GameObject.FindGameObjectsWithTag("CustomTable");
        float minDist = float.MaxValue;
        CustomTable closestTable = null;
        foreach (var t in tables)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestTable = t.GetComponent<CustomTable>();
            }
        }
        if (closestTable != null)
        {
            assignedTable = closestTable;
            // Y축만 맞춰서 테이블 바라보기
            Vector3 lookPos = assignedTable.transform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        }

        if (isBadCustomer)
        {
            GameManager.instance.hasBadCustomer = true;
            GameManager.instance.badCustomer = this;

            // UI 이미지 출력
            if (GameManager.instance.badCustomerUI != null)
            {
                GameManager.instance.badCustomerUI.ShowBadCustomer((BadType)badType);
            }
            GameObject badIconPrefab = null;

            // 나쁜손님 종류별 효과음 재생
            if (SoundManager.instance != null)
            {
                switch (badType)
                {
                    case BadType.Dalgona:
                        badIconPrefab = badIconDalgonaPrefab;
                        SoundManager.instance.PlayBadSound1();
                        break;
                    case BadType.Hotdog:
                        badIconPrefab = badIconHotdogPrefab;
                        SoundManager.instance.PlayBadSound2();
                        break;
                    case BadType.Stun:
                        badIconPrefab = badIconStunPrefab;
                        SoundManager.instance.PlayBadSound3();
                        break;
                }
                // 5초간 배경음 재생
                StartCoroutine(PlayBadCustomBackGroundForSeconds());
            }

            if (badIconPrefab != null)
            {
                orderIconObject = Instantiate(badIconPrefab, iconSpawnPoint.position, Quaternion.identity, iconSpawnPoint);
            }

            if (badType == BadType.Stun)
            {
                player = FindFirstObjectByType<Player>();
                stunCoroutine = StartCoroutine(StunPlayerRoutine());
            }
        }

        // 슬라이더 초기화
        if (waitCanvas != null)
            waitCanvas.enabled = false;

        if (waitSlider != null)
            waitSlider.value = 0f;

        OrderListManager.Instance?.RegisterCustomer(this);
    }


    private void Update()
    {
        // TestScene();
        if (isProcessed || isLeaving) return;  // 이미 처리되었거나 떠나는 중이면 업데이트 중지

        waitTimer += Time.deltaTime;
        if (waitTimer > maxWaitTime)
        {
            Debug.Log("시간 초과로 손님 제거");
            GameManager.instance.SadCat();
            HandleCustomerFail();
            return;  // HandleCustomerFail 호출 후 즉시 리턴
        }
        
        if (orderIconObject != null && Camera.main != null)
        {
        orderIconObject.transform.rotation = Quaternion.LookRotation(
            orderIconObject.transform.position - Camera.main.transform.position
        );

        if (waitSlider != null && Camera.main != null)
        {
        waitSlider.transform.rotation = Quaternion.LookRotation(
            waitSlider.transform.position - Camera.main.transform.position
        );
    }
        
    }


        // 테이블 위 음식 체크 (매 프레임 → 음식이 새로 올라간 경우에만 1초 후 체크)
        if (assignedTable != null && assignedTable.HasFood())
        {
            if (tableCheckCoroutine == null)
            {
                tableCheckCoroutine = StartCoroutine(CheckTableFoodAfterDelay());
            }
        }
        else
        {
            if (tableCheckCoroutine != null)
            {
                StopCoroutine(tableCheckCoroutine);
                tableCheckCoroutine = null;
            }
        }

        if (waitCanvas != null)
            waitCanvas.enabled = true;

        if (waitSlider != null)
            waitSlider.value = waitTimer / maxWaitTime;

        // 나쁜 손님일 경우, 지정된 테이블에 달고나가 놓였는지 확인
        if (isBadCustomer && assignedTable != null && assignedTable.HasFood())
        {
            if (assignedTable.GetFoodName() == "dalgona")
            {
                Debug.Log("나쁜 손님 테이블에 달고나가 감지되었습니다. 미니게임을 시작합니다.");
                
                // 테이블 위의 달고나를 즉시 제거 (소모 처리)
                assignedTable.ClearTable();

                // 달고나 씬으로 전환
                ChangeScene();

                // 이 프레임에서 더 이상 Update를 실행하지 않도록 리턴
                return;
            }
        }
    }

    private IEnumerator CheckTableFoodAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        if (assignedTable != null && assignedTable.HasFood())
        {
            string tableFood = assignedTable.GetFoodName();
            if (tableFood == requestedFood)
            {
                // 성공 처리: 손님, 음식 모두 제거
                assignedTable.ClearTable();
                SoundManager.instance.PlaySuccess();
                
                // 점수 증가
                Player player = FindFirstObjectByType<Player>();
                if (player != null)
                {
                    player.Point += player.basePoint + player.bonusPoint;
                    // 주문 성공 카운트 증가
                    player.customerSuccessCount++;
                }

                // 접시 추가
                DishZone dishZone = FindFirstObjectByType<DishZone>();
                if (dishZone != null)
                {
                    dishZone.AddDish();
                }

                // 쓰레기 생성
                if (trashPrefab != null && spawnPoint != null && Random.value < trashSpawnChance)
                {
                    currentTrash = Instantiate(trashPrefab, spawnPoint.position, Quaternion.identity);
                }
                
                if (spawner != null)
                {
                    spawner.OnCustomerCleared();
                    spawner.OnCustomerDestroyed(gameObject);
                }
                Destroy(gameObject);
            }
        }
        tableCheckCoroutine = null;
    }

    private void RequestRandomFood()
    {
        isRequesting = true;
        string[] foods = { "dalgona", "hottuk", "hotdog", "boung" };
        requestedFood = foods[Random.Range(0, foods.Length)];
        Debug.Log($"손님이 요청한 음식: {requestedFood}");

        GameObject prefabToSpawn = null;

        // 기존 아이콘이 있으면 삭제
        if (orderIconObject != null)
        {
            switch (requestedFood)
            {
                case "dalgona":
                    prefabToSpawn = dalgonaIconPrefab;
                    break;
                case "hottuk":
                    prefabToSpawn = hottukIconPrefab;
                    break;
                case "hotdog":
                    prefabToSpawn = hotdogIconPrefab;
                    break;
                case "boung":
                    prefabToSpawn = boungIconPrefab;
                    break;
            }
        }
        OrderListManager.Instance?.UpdateOrderList();
        // 생성
        if (prefabToSpawn != null && iconSpawnPoint != null)
        {
            orderIconObject = Instantiate(prefabToSpawn, iconSpawnPoint.position, Quaternion.identity, iconSpawnPoint);
        }
    }

    public void ReceiveAutoDeliveredFood(string foodName)
    {
        isBeingDelivered = false;
        if (requestedFood == foodName)
        {
            Debug.Log("자동 배달 성공!");
            // 쓰레기 생성
            if (trashPrefab != null && spawnPoint != null && Random.value < trashSpawnChance)
            {
                currentTrash = Instantiate(trashPrefab, spawnPoint.position, Quaternion.identity);
            }
            StartCoroutine(DestroyAndRespawn(true));
        }
        else
        {
            Debug.Log("요청과 불일치!");
        }
    }

    private void OnDestroy()
    {
        // 이미 HandleCustomerFail에서 처리된 경우 여기서는 아무것도 하지 않음
        if (isProcessed) return;

        // UI 요소들 제거
        if (orderIconObject != null)
        {
            Destroy(orderIconObject);
            orderIconObject = null;
        }

        if (waitCanvas != null)
        {
            waitCanvas.enabled = false;
        }

        // 테이블 정리
        if (assignedTable != null)
        {
            assignedTable.ClearTable();
        }

        // OrderListManager에서 제거
        if (OrderListManager.Instance != null)
        {
            OrderListManager.Instance.UnregisterCustomer(this);
        }

        // 스포너에 알림
        if (spawner != null)
        {
            spawner.OnCustomerDestroyed(gameObject);
        }
    }

    private void HandleCustomerFail()
    {
        if (isProcessed) return;
        isProcessed = true;
        isLeaving = true;

        // 실패 카운트 증가
        if (GameManager.instance.player != null)
        {
            GameManager.instance.player.IncreaseFailCount();
        }

        // 만약 실패한 손님이 나쁜 손님이었다면, GameManager의 플래그를 리셋
        if (isBadCustomer)
        {
            GameManager.instance.hasBadCustomer = false;
            GameManager.instance.badCustomer = null;
            Debug.Log("타임아웃된 나쁜 손님을 처리하고 플래그를 리셋합니다.");
        }

        // UI 요소들 먼저 제거
        if (orderIconObject != null)
        {
            Destroy(orderIconObject);
            orderIconObject = null;
        }

        if (waitCanvas != null)
        {
            waitCanvas.enabled = false;
        }

        // 테이블 정리
        if (assignedTable != null)
        {
            assignedTable.ClearTable();
        }

        // OrderListManager에서 제거
        if (OrderListManager.Instance != null)
        {
            OrderListManager.Instance.UnregisterCustomer(this);
        }

        // 스포너에 알림
        if (spawner != null)
        {
            spawner.OnCustomerDestroyed(gameObject);
        }

        // 게임오브젝트 제거
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.currentZone = this;
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player") && !isProcessed && !isLeaving)
    //     {
    //         HandleCustomerFail();
    //     }
    // }

    public void RemoveBadCustomer()
    {
        if (isBadCustomer)
        {
            // 스턴 효과가 있다면 중지
            if (badType == BadType.Stun && stunCoroutine != null)
            {
                StopCoroutine(stunCoroutine);
                stunCoroutine = null;
            }

            // GameManager에서 나쁜 손님 상태 제거
            GameManager.instance.hasBadCustomer = false;
            GameManager.instance.badCustomer = null;

            // 쓰레기 생성
            if (trashPrefab != null && spawnPoint != null && Random.value < trashSpawnChance)
            {
                currentTrash = Instantiate(trashPrefab, spawnPoint.position, Quaternion.identity);
            }

            // 나쁜 손님 제거
            StartCoroutine(DestroyAndRespawn(false));
        }
    }

    private IEnumerator StunPlayerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(12f);
            if (player != null)
                player.Stun(2f);
        }
    }

    private IEnumerator PlayBadCustomBackGroundForSeconds()
    {
        SoundManager.instance.PlayBadCustomBackGround();
        yield return new WaitForSeconds(3f);
    }

    private IEnumerator DestroyAndRespawn(bool success)
    {
        if (isProcessed) yield break;
        isProcessed = true;
        isLeaving = true;

        if (success)
        {
            SoundManager.instance.PlaySuccess();
            // 주문 성공 카운트 증가
            if (GameManager.instance.player != null)
            {
                GameManager.instance.player.customerSuccessCount++;
                Debug.Log($"손님 성공! 현재 성공 횟수: {GameManager.instance.player.customerSuccessCount}");
            }
        }

        // UI 요소들 제거
        if (orderIconObject != null)
        {
            Destroy(orderIconObject);
            orderIconObject = null;
        }

        if (waitCanvas != null)
        {
            waitCanvas.enabled = false;
        }

        // 테이블 정리
        if (assignedTable != null)
        {
            assignedTable.ClearTable();
        }

        // OrderListManager에서 제거
        if (OrderListManager.Instance != null)
        {
            OrderListManager.Instance.UnregisterCustomer(this);
        }

        yield return null;

        if (spawner != null)
        {
            if (success)
                spawner.OnCustomerCleared();

            // 쓰레기가 있는 경우에는 새로운 손님을 생성하지 않음
            if (currentTrash == null)
            {
                spawner.RespawnCustomer(this.gameObject);
            }
        }

        // 게임오브젝트 제거
        Destroy(gameObject);
    }

    // void TestScene()
    // {
    //     if(Input.GetKeyDown(KeyCode.H))
    //     {
    //         ChangeScene();
    //     }
    // }


    public void ChangeScene()
    {
        // 현재 씬의 모든 루트 오브젝트 비활성화
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            // EventSystem은 비활성화하지 않음
            if (rootObject.GetComponent<UnityEngine.EventSystems.EventSystem>() == null)
            {
                rootObject.SetActive(false);
            }
        }
        
        // BGM 일시정지하지 않음 (달고나 씬에서도 계속 재생되도록)
        SceneManager.LoadScene("DalgonaScene", LoadSceneMode.Additive);
        
        // 달고나 씬이 로드된 후 초기화
        StartCoroutine(InitializeDalgonaScene());
    }
    
    private IEnumerator InitializeDalgonaScene()
    {
        // 씬 로딩 완료까지 대기
        yield return new WaitForSeconds(0.1f);
        
        // 시간 스케일 복원 (달고나 게임이 정상 작동하도록)
        Time.timeScale = 1f;
        
        // 달고나 게임 매니저 찾아서 초기화
        DalgonaGameManager dalgonaManager = FindFirstObjectByType<DalgonaGameManager>();
        if (dalgonaManager != null)
        {
            dalgonaManager.PrepareGame();
            Debug.Log("달고나 씬 초기화 완료");
        }
        else
        {
            Debug.LogError("DalgonaGameManager를 찾을 수 없습니다!");
        }
    }
}
