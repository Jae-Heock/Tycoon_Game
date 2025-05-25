using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class FoodDeliveryAI : MonoBehaviour
{
    public Transform homePosition; // AI가 생성된 위치와 대기할 위치 (홈 위치)

    private string foodName;                       // AI가 배달할 음식 이름
    private Custom targetCustomer;                 // 현재 배달할 손님
    private NavMeshAgent agent;                    // Unity의 네비게이션 이동 컴포넌트
    private bool isBusy = false;                   // 현재 배달 중인 상태
    private bool isReturningHome = false;          // 홈으로 복귀 중인 상태
    Player player;                                 // 플레이어 참조

    private void Start()
    {
        player = GameManager.instance.player;      // GameManager에서 플레이어 참조 가져오기
        agent = GetComponent<NavMeshAgent>();      // NavMeshAgent 컴포넌트 할당
        ReturnHome();                              // 초기 상태로 AI를 홈으로 이동

        StartCoroutine(DeliveryCheckLoop());       // 배달 상태 확인용 코루틴 시작
    }

    // AI의 상태를 지속적으로 체크하는 루프
    IEnumerator DeliveryCheckLoop()
    {
        while (true)
        {
            // 배달 중이고 손님에게 도착했는지 체크
            if (isBusy && targetCustomer != null)
            {
                float dist = Vector3.Distance(transform.position, targetCustomer.transform.position);
                if (dist < 1.5f)
                {
                    DeliverFood(); // 충분한 거리 도달하면 배달 시작
                }
            }

            // 홈으로 복귀 중이고 도착했는지 확인
            if (isReturningHome && !agent.pathPending && agent.remainingDistance < 0.1f)
            {
                Debug.Log("AI가 홈에 도착했습니다.");
                isBusy = false;           // 배달 가능 상태로 복귀
                isReturningHome = false; // 복귀 완료
            }

            yield return new WaitForSeconds(0.5f); // 0.5초마다 체크
        }
    }

    // 배달 요청을 외부(음식 카운터 등)에서 받아 처리하는 함수
    public void AssignDelivery(string foodName, Custom customer)
    {
        if (isBusy)
        {
            Debug.LogWarning("AI가 이미 배달 중인 상태입니다");
            return; // 중복 배달 방지
        }

        // 배달 정보 설정
        this.foodName = foodName;
        targetCustomer = customer;
        isBusy = true;

        Debug.Log($"AI가 {foodName} 를 손님에게 배달 중...");
        agent.SetDestination(customer.transform.position); // 손님 위치로 이동 시작
    }

    // 손님에게 음식을 전달하는 함수 (거리 체크에서 호출됨)
    void DeliverFood()
    {
        Debug.Log("배달 완료");

        player.Point += player.basePoint + player.bonusPoint;// 배달 완료 시 플레이어 점수 증가
        targetCustomer.ReceiveAutoDeliveredFood(foodName); // 손님에게 배달 완료 처리

        // 상태 초기화
        foodName = null;
        targetCustomer = null;

        ReturnHome(); // 배달 완료 후 홈으로 이동 시작
    }

    // AI가 홈(homePosition)으로 복귀하는 함수
    void ReturnHome()
    {
        if (homePosition != null)
        {
            Debug.Log("홈으로 복귀 중...");
            isReturningHome = true;
            agent.SetDestination(homePosition.position); // 복귀 시작
        }
    }

    // 외부에서 AI가 현재 배달 중인지 확인할 수 있는 함수
    public bool IsBusy()
    {
        return isBusy;
    }
}
