using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class FoodDeliveryAI : MonoBehaviour
{
    public Transform homePosition; // AI가 생성된 위치와 대기할 위치 (홈 위치)
    public Transform foodHoldPoint; // Inspector에서 지정 (AI가 음식을 들 위치)
    private GameObject heldFoodObject;
    private Table targetTable;
    private Custom targetCustomer;
    private NavMeshAgent agent;
    private Player player;

    private enum State { Idle, MovingToTable, PickingUp, MovingToCustomer, Delivering }
    private State aiState = State.Idle;

    private void Start()
    {
        player = GameManager.instance.player;
        agent = GetComponent<NavMeshAgent>();
        aiState = State.Idle;
    }

    private void Update()
    {
        switch (aiState)
        {
            case State.Idle:
                if (IsAtHome())
                    LookAtNearestTable();
                if (heldFoodObject == null)
                    FindTableAndCustomer();
                else
                    FindCustomerForHeldFood();
                break;

            case State.MovingToTable:
                if (targetTable != null && agent.remainingDistance < 0.2f && !agent.pathPending)
                {
                    aiState = State.PickingUp;
                }
                break;

            case State.PickingUp:
                PickUpFood();
                break;
                
            case State.MovingToCustomer:
                if (targetCustomer == null || !targetCustomer.gameObject.activeSelf)
                {
                    HandleMissingCustomer(); // 👈 추가된 함수 호출
                    break;
                }
                if (agent.remainingDistance < 0.2f && !agent.pathPending)
                {
                    aiState = State.Delivering;
                }
                break;

            case State.Delivering:
                DeliverFood();
                break;
        }
    }

    void FindTableAndCustomer()
    {
        Table[] tables = Object.FindObjectsByType<Table>(FindObjectsSortMode.None);
        foreach (var table in tables)
        {
            if (table.isLockedByAI) continue;
            string foodName = table.GetCurrentFoodName();
            if (!string.IsNullOrEmpty(foodName))
            {
                Custom[] customers = Object.FindObjectsByType<Custom>(FindObjectsSortMode.None);
                foreach (var customer in customers)
                {
                    if (customer.RequestedFood == foodName && !customer.IsBeingDelivered)
                    {
                        targetTable = table;
                        targetCustomer = customer;
                        customer.MarkBeingDelivered();
                        table.LockTable();
                        agent.SetDestination(table.pickupPoint != null ? table.pickupPoint.position : table.transform.position);
                        aiState = State.MovingToTable;
                        return;
                    }
                }
            }
        }
    }

    void PickUpFood()
    {
        if (targetTable == null) { aiState = State.Idle; return; }
        heldFoodObject = targetTable.PickupFood();
        if (heldFoodObject != null && foodHoldPoint != null)
        {
            heldFoodObject.transform.SetParent(foodHoldPoint);
            heldFoodObject.transform.localPosition = Vector3.zero;
            heldFoodObject.transform.localRotation = Quaternion.identity;
            heldFoodObject.transform.localScale = Vector3.one * 0.1f;
        }
        // 손님에게 이동
        if (targetCustomer != null)
        {
            agent.SetDestination(targetCustomer.transform.position);
            aiState = State.MovingToCustomer;
        }
        else
        {
            aiState = State.Idle;
        }
    }

    void DeliverFood()
    {
        if (targetCustomer == null || heldFoodObject == null)
        {
            if (targetTable != null) targetTable.UnlockTable();
            aiState = State.Idle;
            return;
        }
        string deliveredName = targetCustomer.RequestedFood;
        Destroy(heldFoodObject);
        heldFoodObject = null;
        player.Point += player.basePoint + player.bonusPoint;

        // 접시 추가
        DishZone dishZone = FindFirstObjectByType<DishZone>();
        if (dishZone != null)
        {
            dishZone.AddDish();
        }

        // 쓰레기 생성
        if (targetCustomer.trashPrefab != null && targetCustomer.spawnPoint != null && Random.value < targetCustomer.trashSpawnChance)
        {
            Instantiate(targetCustomer.trashPrefab, targetCustomer.spawnPoint.position, Quaternion.identity);
        }

        targetCustomer.ReceiveAutoDeliveredFood(deliveredName);
        targetCustomer = null;
        if (targetTable != null) targetTable.UnlockTable();
        targetTable = null;
        if (homePosition != null)
        {
            agent.SetDestination(homePosition.position);
        }
        aiState = State.Idle;
    }

    private void HandleMissingCustomer()
    {
        Debug.Log("❗ 배달 도중 손님이 사라졌습니다. 대기 위치로 복귀합니다.");

        if (targetTable != null)
        {
            targetTable.UnlockTable(); // 테이블 잠금 해제
            targetTable = null;
        }

        targetCustomer = null;

        // 음식을 들고 있는 상태를 유지한 채로 홈 포지션으로 이동
        if (homePosition != null)
        {
            agent.SetDestination(homePosition.position);
        }

        aiState = State.Idle;
    }

    void FindCustomerForHeldFood()
    {
        if (heldFoodObject == null) return;

        // Dish 프리팹 이름을 정규화된 음식 이름으로 변환
        string prefabName = heldFoodObject.name.Replace("(Clone)", "").Trim();  // 예: "Dish_핫도그"
        string foodName = ConvertDishPrefabNameToFoodName(prefabName);          // 결과: "hotdog"

        Debug.Log($"🍱 들고 있는 음식: {prefabName} → 비교용 이름: {foodName}");

        if (string.IsNullOrEmpty(foodName)) return;

        Custom[] customers = Object.FindObjectsByType<Custom>(FindObjectsSortMode.None);
        foreach (var customer in customers)
        {
            if (customer.RequestedFood.ToLower() == foodName && !customer.IsBeingDelivered)
            {
                targetCustomer = customer;
                customer.MarkBeingDelivered();
                agent.SetDestination(customer.transform.position);
                aiState = State.MovingToCustomer;
                Debug.Log($"💡 기존에 들고 있던 {foodName}을 새로운 손님에게 배달 시작!");
                return;
            }
        }

        // 아직 배달할 손님이 없다면 홈 포지션으로 이동
        if (homePosition != null)
        {
            agent.SetDestination(homePosition.position);
        }
    }

    string ConvertDishPrefabNameToFoodName(string prefabName)
    {
        if (prefabName.StartsWith("Dish_"))
        {
            string localName = prefabName.Substring(5); // "핫도그", "붕어빵" 등
            switch (localName)
            {
                case "핫도그": return "hotdog";
                case "달고나": return "dalgona";
                case "호떡": return "hottuk";
                case "붕어빵": return "boung";
            }
        }
        return null;
    }

    private void LookAtNearestTable()
    {
        Table[] tables = Object.FindObjectsByType<Table>(FindObjectsSortMode.None);
        if (tables.Length == 0) return;

        Table nearest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (var table in tables)
        {
            float dist = Vector3.Distance(myPos, table.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = table;
            }
        }

        if (nearest != null)
        {
            Vector3 lookPos = nearest.transform.position;
            lookPos.y = transform.position.y; // y축 고정(수평만 회전)
            transform.LookAt(lookPos);
        }
    }

    private bool IsAtHome()
    {
        if (homePosition == null) return false;
        return Vector3.Distance(transform.position, homePosition.position) < 0.2f;
    }
}
