using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 접시를 관리하는 구역
/// 플레이어가 이 구역에 들어와서 E키를 누르면 접시를 씻을 수 있음
/// </summary>
public class DishZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조
    private bool isReducingPoints = false;  // 포인트 감소 중인지 여부

    public Transform dishPoint;         // 접시 생성 위치치
    private List<GameObject> dishList = new List<GameObject>();  // 생성된 접시시 오브젝트 목록
    public GameObject dishPrefab;       // 접시시 프리팹

    [Header("접시 설정")]
    public int maxDishes = 5;              // 최대 접시 개수
    public float pointReductionInterval = 2f; // 포인트 감소 간격 (초)
    public int pointReductionAmount = 1;    // 감소할 포인트 양

    [Header("현재 접시 개수")]
    [SerializeField] public int currentDishCount;    // 현재 접시 개수

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
    }

    /// <summary>
    /// 플레이어가 구역에 들어왔을 때 호출
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            Debug.Log("설거지 구역에 들어왔습니다. E키를 눌러 설거지를 하세요.");
        }
    }

    /// <summary>
    /// 플레이어가 구역을 나갔을 때 호출
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            Debug.Log("설거지 구역을 나갔습니다.");
        }
    }

    private void Update()
    {
        // E키를 눌러 설거지
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            CleanDishes();
        }

        // 접시 개수에 따른 포인트 감소 처리
        if (currentDishCount >= maxDishes && !isReducingPoints)
        {
            StartCoroutine(ReducePoints());
        }
        else if (currentDishCount < maxDishes && isReducingPoints)
        {
            isReducingPoints = false;
        }
    }

    // 요리를 만들 때 호출할 함수
    public void AddDish()
    {
        currentDishCount++;
        HoldItem("dish"); 
        Debug.Log($"접시 추가! 현재 접시 개수: {currentDishCount}");
    }

    public void HoldItem(string itemName)
    {
        switch (itemName)
        {
            case "dish":
                GameObject dish = Instantiate(dishPrefab, dishPoint);
                dish.transform.localPosition = Vector3.zero + Vector3.up * dishList.Count * 0.5f;
                dish.transform.localRotation = Quaternion.identity;
                dish.transform.localScale = Vector3.one * 1f;
                dishList.Add(dish);
                break;
        }
    }

    private void CleanDishes()
    {
        // 플레이어가 다른 존을 사용 중인지 확인
        if (player.currentZone != null && player.currentZone != this)
        {
            Debug.Log("다른 존을 사용 중입니다.");
            return;
        }

        // 모든 접시 초기화
        currentDishCount = 0;
        foreach (GameObject dish in dishList)
        {
            Destroy(dish);
        }
        dishList.Clear();
        // 포인트 감소 중지
        isReducingPoints = false;

        Debug.Log("설거지 완료! 모든 접시가 정리되었습니다.");
    }

    public void CleanOneDish()
    {
        if (dishList.Count > 0)
        {
            Destroy(dishList[dishList.Count - 1]);
            dishList.RemoveAt(dishList.Count - 1);
            currentDishCount--;

            Debug.Log($"자동으로 접시 1개 제거됨 (남은: {currentDishCount})");
        }
    }

    private IEnumerator ReducePoints()
    {
        isReducingPoints = true;
        while (isReducingPoints)
        {
            yield return new WaitForSeconds(pointReductionInterval);
            if (player.Point > 0)
            {
                player.Point -= pointReductionAmount;
                Debug.Log($"접시가 너무 많습니다! -{pointReductionAmount}점 (현재 점수: {player.Point})");
            }
        }
    }
} 