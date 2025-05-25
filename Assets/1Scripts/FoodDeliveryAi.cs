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
                FindTableAndCustomer();
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
            heldFoodObject.transform.localScale = Vector3.one * 1f;
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
        Debug.Log("❗ 배달 도중 손님이 사라졌습니다. AI가 음식 반환 후 대기 위치로 복귀합니다.");

        if (heldFoodObject != null)
        {
            Destroy(heldFoodObject);
            heldFoodObject = null;
        }

        if (targetTable != null)
        {
            targetTable.UnlockTable(); // 테이블 잠금 해제
            targetTable = null;
        }

        targetCustomer = null;

        if (homePosition != null)
        {
            agent.SetDestination(homePosition.position);
        }

        aiState = State.Idle;
}

}
