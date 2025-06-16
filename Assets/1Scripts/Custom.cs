using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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

    // 나쁜 손님 관련 변수 추가
    private int requiredSugar = 10;    // 달고나 나쁜손님에게 필요한 설탕
    private int requiredSosage = 10;   // 핫도그 나쁜손님에게 필요한 소시지
    private int requiredFlour = 10;    // 스턴 나쁜손님에게 필요한 밀가루

    public bool IsBeingDelivered => isBeingDelivered;
    public string RequestedFood => requestedFood;

    // ====== 외부 연결 ======
    public CustomSpawner spawner;
    public bool isBadCustomer = false;
    public Transform spawnPoint;

    // ====== 쓰레기 관련 ======
    public GameObject trashPrefab;          // 쓰레기 프리팹
    private GameObject currentTrash;        // 현재 생성된 쓰레기

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
        waitTimer += Time.deltaTime;
        if (waitTimer > maxWaitTime)
        {
            Debug.Log("시간 초과로 손님 제거");
            GameManager.instance.SadCat();
            // 주문 실패 카운트 증가
            if (GameManager.instance.player != null)
                GameManager.instance.player.customerFailCount++;
            StartCoroutine(DestroyAndRespawn(false));
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

        // // 아이콘 회전
        // if (orderIconObject != null)
        // {
        //     orderIconObject.transform.Rotate(Vector3.up * iconRotationSpeed * Time.deltaTime);
        // }

        // E키를 눌렀을 때 나쁜 손님에게 자원 전달
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E) && isBadCustomer && player != null)
        {
            switch (badType)
            {
                case BadType.Dalgona:
                    if (player.sugarCount >= requiredSugar)
                    {
                        player.sugarCount -= requiredSugar;
                        RemoveBadCustomer();
                    }
                    else
                    {
                        Debug.Log($"설탕이 부족합니다! (필요: {requiredSugar}개)");
                    }
                    break;
                case BadType.Hotdog:
                    if (player.sosageCount >= requiredSosage)
                    {
                        player.sosageCount -= requiredSosage;
                        RemoveBadCustomer();
                    }
                    else
                    {
                        Debug.Log($"소시지가 부족합니다! (필요: {requiredSosage}개)");
                    }
                    break;
                case BadType.Stun:
                    if (player.flourCount >= requiredFlour)
                    {
                        player.flourCount -= requiredFlour;
                        RemoveBadCustomer();
                    }
                    else
                    {
                        Debug.Log($"밀가루가 부족합니다! (필요: {requiredFlour}개)");
                    }
                    break;
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
                if (trashPrefab != null && spawnPoint != null)
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
            StartCoroutine(DestroyAndRespawn(true));
        }
        else
        {
            Debug.Log("요청과 불일치!");
        }
    }

    private void OnDestroy()
    {
        // 아이콘 제거
        if (orderIconObject != null)
        {
            Destroy(orderIconObject);
            orderIconObject = null;
        }

        // 스포너에 알림
        if (spawner != null)
        {
            spawner.OnCustomerDestroyed(gameObject);
        }

        OrderListManager.Instance?.UnregisterCustomer(this);
    }

    private IEnumerator DestroyAndRespawn(bool success)
    {
        isRequesting = false;
        if (success)
        {
            SoundManager.instance.PlaySuccess();
            // 주문 성공 카운트 증가
            if (GameManager.instance.player != null)
                GameManager.instance.player.customerSuccessCount++;
        }
        if (orderIconObject != null)
        {
            Destroy(orderIconObject);
            orderIconObject = null;
        }

        // 손님이 사라질 때 테이블 음식도 제거
        if (assignedTable != null)
        {
            assignedTable.ClearTable();
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

        // 주문 목록 갱신
        if (OrderListManager.Instance != null)
            OrderListManager.Instance.UpdateOrderList();
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (player != null && player.currentZone == this)
            {
                player.currentZone = null;
            }
        }
    }

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
}
