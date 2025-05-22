using UnityEngine;

/// <summary>
/// 플레이어가 들고 있는 요리를 테이블 위에 놓거나 다시 가져갈 수 있는 클래스
/// </summary>
public class Table : MonoBehaviour
{
    private bool isPlayerInZone = false;
    private Player player;

    [Header("음식 생성 위치")]
    public Transform foodSpawnPoint;

    private GameObject currentFoodObject;   // 테이블 위의 음식 오브젝트
    private string currentFoodName = null;  // 음식 이름 (hotdog, dalgona 등)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            player = null;
        }
    }

    private void Update()
    {
        if (!isPlayerInZone || player == null) return;

        // E키: 플레이어가 음식 테이블에 놓기
        if (Input.GetKeyDown(KeyCode.E))
        {
            player.PlayDownAnimation();
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

        if (currentFoodObject != null)
        {
            Debug.Log("테이블에 이미 음식이 있습니다.");
            return;
        }

        string foodName = player.currentFood;
        GameObject prefab = player.GetFoodPrefab(foodName);
        Transform point = foodSpawnPoint;

        if (prefab != null && point != null)
        {
            currentFoodObject = Instantiate(prefab, point);
            currentFoodObject.transform.localPosition = Vector3.zero;
            currentFoodObject.transform.localRotation = Quaternion.identity;
            currentFoodObject.transform.localScale = Vector3.one * 3f;

            currentFoodName = foodName;
            // 손에 든 음식 제거!
            player.ClearHeldFood();

            Debug.Log($"{foodName}을(를) 테이블에 올렸습니다.");
        }
        else
        {
            Debug.LogError($"프리팹 또는 위치를 찾을 수 없습니다: {foodName}");
        }
    }

    private void TakeFoodToPlayer()
    {
        if (currentFoodObject == null)
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
        switch (currentFoodName)
        {
            case "hotdog":
                player.hotdogCount++;
                break;
            case "dalgona":
                player.dalgonaCount++;
                break;
            case "hottuk":
                player.hottukCount++;
                break;
            case "boung":
                player.boungCount++;
                break;
        }

        player.HoldItem(currentFoodName);
        Destroy(currentFoodObject);

        Debug.Log($"{currentFoodName}을(를) 플레이어가 가져갔습니다.");

        currentFoodObject = null;
        currentFoodName = null;
    }
}
