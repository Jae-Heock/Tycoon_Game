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

    private Coroutine stunCoroutine;
    public void MarkBeingDelivered() => isBeingDelivered = true;

    // ======= UI 요소=======
    public GameObject orderIconObject; // 현재 떠 있는 아이콘 오브젝트
    public Transform iconSpawnPoint;   // 아이콘을 띄울 위치 (손님 머리 위 Transform)
    public GameObject dalgonaIconPrefab;
    public GameObject hottukIconPrefab;
    public GameObject hotdogIconPrefab;
    public GameObject boungIconPrefab;

    private void Start()
    {
        if (!isRequesting && !isBadCustomer)
            RequestRandomFood();

        if (isBadCustomer)
        {
            GameManager.instance.hasBadCustomer = true;
            GameManager.instance.badCustomer = this;

            if (badType == BadType.Stun)
                stunCoroutine = StartCoroutine(StunPlayerRoutine());
        }
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

        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E)) TryDeliver();
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
            isRequesting = false;
        }
    }

    private void RequestRandomFood()
    {
        isRequesting = true;
        string[] foods = { "dalgona", "hottuk", "hotdog", "boung" };
        requestedFood = foods[Random.Range(0, foods.Length)];
        Debug.Log($"손님이 요청한 음식: {requestedFood}");

        GameObject prefabToSpawn = null;

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
                    if (player.boungCount >= 2)
                    {
                        player.boungCount -= 2;
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
            StartCoroutine(DestroyAndRespawn(true));
    }

    private bool TryGive(ref int itemCount, string itemName)
    {
        if (itemCount > 0)
        {
            itemCount--;
            player.Point += player.basePoint + player.bonusPoint;
            player.customerSuccessCount++;
            GameManager.instance.HappyCat();
            Debug.Log($"{itemName} 전달 성공!");
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

            spawner.RespawnCustomer(this.gameObject);
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
            yield return new WaitForSeconds(10f);
            if (player != null)
                player.Stun(0.5f);
        }
    }
}
