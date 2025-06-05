using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Custom : MonoBehaviour
{
    public enum BadType { None, Dalgona, Hotdog, Stun }
    public BadType badType = BadType.None;

    // ====== 상태 ======
    private bool isRequesting = false;
    private bool isBeingDelivered = false;
    private float waitTimer = 0f;
    private float maxWaitTime = 40f; // 손님 대기 시간 (초)
    
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
    [Header("Wait UI")]
    public Slider waitSlider;         // 손님 대기시간 슬라이더
    public Canvas waitCanvas;         // 슬라이더가 붙은 World Space 캔버스

    [Header("Icon Rotation")]
    public float iconRotationSpeed = 100f; // 아이콘 회전 속도 (도/초)

    private CustomTable assignedTable; // 손님이 배정받은 테이블

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
    }

    private void Update()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer > maxWaitTime)
        {
            Debug.Log("시간 초과로 손님 제거");
            GameManager.instance.SadCat();
            StartCoroutine(DestroyAndRespawn(false));
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

        // 아이콘 회전
        if (orderIconObject != null)
        {
            orderIconObject.transform.Rotate(Vector3.up * iconRotationSpeed * Time.deltaTime);
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
                
                // 점수 증가
                Player player = FindFirstObjectByType<Player>();
                if (player != null)
                {
                    player.Point += player.basePoint + player.bonusPoint;
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
    }

    private IEnumerator DestroyAndRespawn(bool success)
    {
        isRequesting = false;
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
    }

    public void RemoveBadCustomer()
    {
        if (isBadCustomer)
        {
            GameManager.instance.hasBadCustomer = false;
            GameManager.instance.badCustomer = null;
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
}
