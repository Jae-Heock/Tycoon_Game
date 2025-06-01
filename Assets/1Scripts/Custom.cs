using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Custom : MonoBehaviour
{
    public enum BadType { None, Dalgona, Hotdog, Stun }
    public BadType badType = BadType.None;

    // ====== 상태 ======
    private bool isPlayerInZone = false;
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

    private void Start()
    {
        if (!isRequesting && !isBadCustomer)
            RequestRandomFood();

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

        if (isPlayerInZone)
        {
            if (currentTrash == null && Input.GetKeyDown(KeyCode.E))
            {
                TryDeliver();
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



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            player = other.GetComponent<Player>();

            if (!isRequesting && !isBadCustomer)
                RequestRandomFood();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
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

    private void TryDeliver()
    {
        if (isBadCustomer)
        {
            switch (badType)
            {
                case BadType.Dalgona:
                    if (player.sugarCount >= 10)
                    {
                        player.sugarCount -= 10;
                        player.Point += 10;
                        Debug.Log("설탕 10개를 줘서 나쁜 손님 제거!");
                        RemoveBadCustomer();
                    }
                    else
                    {
                        Debug.Log("설탕이 부족합니다!");
                    }
                    return;
                case BadType.Hotdog:
                    if (player.sosageCount >= 10)
                    {
                        player.sosageCount -= 10;
                        player.Point += 10;
                        Debug.Log("소세지 10개를 줘서 나쁜 손님 제거!");
                        RemoveBadCustomer();
                    }
                    else
                    {
                        Debug.Log("소세지가 부족합니다!");
                    }
                    return;
                case BadType.Stun:
                    if (player.boungCount >= 1)
                    {
                        player.boungCount -= 1;
                        player.Point += 10;
                        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
                        Debug.Log("붕어빵 2개를 줘서 나쁜 손님 제거!");
                        RemoveBadCustomer();
                    }
                    else
                    {
                        Debug.Log("붕어빵이 부족합니다!");
                    }
                    return;
            }
        }

        bool delivered = false;

        switch (requestedFood)
        {
            case "dalgona":
                delivered = TryGive(ref player.dalgonaCount, "달고나");
                break;
            case "hottuk":
                delivered = TryGive(ref player.hottukCount, "호떡");
                break;
            case "hotdog":
                delivered = TryGive(ref player.hotdogCount, "핫도그");
                break;
            case "boung":
                delivered = TryGive(ref player.boungCount, "붕어빵");
                break;
        }

        if (delivered)
        {
            // 쓰레기 생성
            if (trashPrefab != null && spawnPoint != null)
            {
                currentTrash = Instantiate(trashPrefab, spawnPoint.position, Quaternion.identity);
            }
            
            // 손님 제거
            if (spawner != null)
            {
                spawner.OnCustomerCleared();
                spawner.OnCustomerDestroyed(gameObject);
            }
            Destroy(gameObject);
        }
    }

    private bool TryGive(ref int itemCount, string itemName)
    {
        if (itemCount > 0)
        {
            player.Point += player.basePoint + player.bonusPoint;
            player.customerSuccessCount++;
            GameManager.instance.HappyCat();
            Debug.Log($"{itemName} 전달 성공!");
            player.ClearHeldFood();
            return true;
        }
        else
        {
            Debug.Log($"{itemName} 부족!");
            return false;
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
