using UnityEngine;

public class CustomTable : MonoBehaviour
{
    [Header("음식 생성 위치 (빈 오브젝트로 할당)")]
    public Transform foodSpawnPoint;

    private GameObject placedFood; // 테이블 위에 올려진 음식 오브젝트
    private string foodName;       // 테이블 위 음식 이름
    private bool isPlayerInZone = false;
    private Player player;

    // 음식 올리기 시도 (성공 시 true)
    public bool PlaceFood(string newFoodName, GameObject foodPrefab)
    {
        if (placedFood != null || foodPrefab == null) return false;
        placedFood = Instantiate(foodPrefab, foodSpawnPoint.position, Quaternion.identity, foodSpawnPoint);
        placedFood.transform.localPosition = Vector3.zero;
        placedFood.transform.localRotation = Quaternion.identity;
        placedFood.transform.localScale = Vector3.one * 1f;
        foodName = newFoodName;
        return true;
    }

    // 음식 제거
    public void ClearTable()
    {
        if (placedFood != null)
        {
            Destroy(placedFood);
            placedFood = null;
            foodName = null;
        }
    }

    // 음식 유무
    public bool HasFood() => placedFood != null;

    // 음식 이름 반환
    public string GetFoodName() => foodName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            if (player != null)
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

    private void Update()
    {
        if (!isPlayerInZone || player == null) return;

        // E키: 플레이어가 음식 테이블에 놓기
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlaceFoodFromPlayer();
        }

        // F키: 플레이어가 테이블 음식 가져가기
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeFoodToPlayer();
        }
    }

    private void PlaceFoodFromPlayer()
    {
        if (string.IsNullOrEmpty(player.currentFood))
        {
            Debug.Log("플레이어가 들고 있는 음식이 없습니다.");
            return;
        }

        if (placedFood != null)
        {
            Debug.Log("테이블에 이미 음식이 있습니다.");
            return;
        }

        string food = player.currentFood;
        GameObject prefab = player.GetFoodPrefab(food);
        Transform point = foodSpawnPoint;

        if (prefab != null && point != null)
        {
            placedFood = Instantiate(prefab, point);
            placedFood.transform.localPosition = Vector3.zero;
            placedFood.transform.localRotation = Quaternion.identity;
            placedFood.transform.localScale = Vector3.one * 1f;
            foodName = food;
            player.ClearHeldFood();
            Debug.Log($"{food}을(를) 테이블에 올렸습니다.");
        }
        else
        {
            Debug.LogError($"프리팹 또는 위치를 찾을 수 없습니다: {food}");
        }
    }

    private void TakeFoodToPlayer()
    {
        if (placedFood == null)
        {
            Debug.Log("테이블에 음식이 없습니다.");
            return;
        }

        if (!string.IsNullOrEmpty(player.currentFood))
        {
            Debug.Log("플레이어가 이미 음식을 들고 있습니다.");
            return;
        }

        // 음식 타입에 따라 카운트 증가
        switch (foodName)
        {
            case "hotdog": player.hotdogCount++; break;
            case "dalgona": player.dalgonaCount++; break;
            case "hottuk": player.hottukCount++; break;
            case "boung": player.boungCount++; break;
        }

        player.HoldItem(foodName);
        Destroy(placedFood);
        placedFood = null;
        foodName = null;
        Debug.Log($"{foodName}을(를) 플레이어가 가져갔습니다.");
    }
} 