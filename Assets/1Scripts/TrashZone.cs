using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 쓰레기 구역을 관리하는 클래스
/// 플레이어가 들고있는 음식을 버릴 수 있고, E키를 눌러 정리할 수 있음
/// </summary>
public class TrashZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 쓰레기 구역 근처에 있는지 확인
    public int trashCount = 0;           // 현재 쓰레기 개수
    public int maxTrash = 5;             // 최대 쓰레기 개수
    private Player player;               // 플레이어 참조

    public Transform trashPoint;         // 쓰레기 생성 위치
    private List<GameObject> trashList = new List<GameObject>();  // 생성된 쓰레기 오브젝트 목록

    private void Awake()
    {
        player = Object.FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPlayerInZone)
        {
            // 플레이어가 음식을 들고 있고, 쓰레기통이 가득 차지 않았다면
            if (!string.IsNullOrEmpty(player.currentFood) && trashCount < maxTrash)
            {
                // 플레이어가 들고있는 음식을 쓰레기통에 버림
                string foodType = player.currentFood;
                GameObject foodToTrash = player.GetFoodPrefab(foodType);
                if (foodToTrash != null)
                {
                    foodToTrash = Instantiate(foodToTrash, trashPoint);
                    player.ClearHeldFood(); // 플레이어의 음식 제거
                    
                    // 쓰레기통에 음식 추가
                    trashCount++;
                    AddTrash(foodToTrash);
                    Debug.Log($"음식을 버렸습니다. 현재 쓰레기: {trashCount}");

                    if (trashCount == maxTrash)
                    {
                        Debug.Log("쓰레기통이 가득 찼습니다! E키를 눌러 비워주세요.");
                    }
                }
            }
            // 쓰레기통이 가득 찼다면 정리
            else if (trashCount >= maxTrash)
            {
                CleanTrash();
            }
            else if (string.IsNullOrEmpty(player.currentFood))
            {
                Debug.Log("버릴 음식이 없습니다.");
            }
        }
    }

    private void AddTrash(GameObject food)
    {
        if (food == null) return;

        // 원형 반경 내 무작위 위치에 생성 (X,Z) + Y축으로 쌓기
        Vector2 circle = Random.insideUnitCircle * 0.7f;
        Vector3 offset = new Vector3(circle.x, trashList.Count * 0.3f, circle.y);

        food.transform.SetParent(trashPoint);
        food.transform.localPosition = offset;
        food.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        food.transform.localScale = Vector3.one * 3f;

        trashList.Add(food);
    }

    public void CleanTrash()
    {
        trashCount = 0;
        Debug.Log("Cleaning Complete! 쓰레기 정리 완료!");

        foreach (GameObject trash in trashList)
        {
            if (trash != null)
                Destroy(trash);
        }

        trashList.Clear();
    }

    public void CleanOneTrash()
    {
        if (trashList.Count > 0)
        {
            Destroy(trashList[^1]);
            trashList.RemoveAt(trashList.Count - 1);
            trashCount--;

            Debug.Log("쓰레기 1개 제거!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.EnterZone(this);
            Debug.Log("쓰레기통에 접근했습니다. E키를 눌러 음식을 버리거나 쓰레기를 정리하세요.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            player.ExitZone(this);
            Debug.Log("쓰레기통에서 벗어났습니다.");
        }
    }
}
