using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 쓰레기 구역을 관리하는 클래스
/// 주기적으로 쓰레기를 생성하고, 플레이어가 E키를 눌러 정리할 수 있음
/// </summary>
public class TrashZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 쓰레기 구역 근처에 있는지 확인
    public int trashCount = 0;           // 현재 쓰레기 개수
    public int maxTrash = 5;             // 최대 쓰레기 개수
    private Player player;               // 플레이어 참조

    public Transform trashPoint;         // 쓰레기 생성 위치
    private List<GameObject> trashList = new List<GameObject>();  // 생성된 쓰레기 오브젝트 목록
    public GameObject[] trashPrefabs;       // 쓰레기 프리팹

    private void Awake()
    {
        player = Object.FindFirstObjectByType<Player>();

        if (player != null)
        {
            StartCoroutine(SpawnTrash());
        }
    }

    private IEnumerator SpawnTrash()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5, 10));

            if (trashCount < maxTrash)
            {
                trashCount++;
                HoldItem("trash");
                Debug.Log($"쓰레기 생성: {trashCount}");

                if (trashCount == maxTrash)
                {
                    Debug.Log("쓰레기가 너무 많습니다! 이동 속도 감소!");
                    player.ApplySpeedPenalty(); // 이동속도 감소
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPlayerInZone)
        {
            if (player.currentZone != this)
            {
                Debug.Log("다른 존을 사용 중입니다.");
                return;
            }

            CleanTrash();
        }
    }

    public void CleanTrash()
    {
        trashCount = 0;
        player.RemoveSpeedPenalty(); // 이동속도 복구
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

            Debug.Log("코루틴으로 쓰레기 1개 제거!");
        }
    }

    public void HoldItem(string itemName)
    {
        if (itemName == "trash")
        {
            // ✅ 프리팹이 비어있지 않다면 랜덤으로 선택
            if (trashPrefabs.Length == 0) return;

            GameObject prefabToUse = trashPrefabs[Random.Range(0, trashPrefabs.Length)];
            GameObject trash = Instantiate(prefabToUse, trashPoint);

            // 원형 반경 내 무작위 위치에 생성 (X,Z) + Y축으로 쌓기
            Vector2 circle = Random.insideUnitCircle * 0.7f;
            Vector3 offset = new Vector3(circle.x, trashList.Count * 0.3f, circle.y); // Y축으로 쌓기

            trash.transform.localPosition = offset;
            trash.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            trash.transform.localScale = Vector3.one * 60f;

            trashList.Add(trash);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.currentZone = this;
            Debug.Log("쓰레기 구역에 들어왔습니다. E키를 눌러 쓰레기를 버리세요.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            
            // 이 존이 현재 존이었다면 null로 설정
            if (player != null && player.currentZone == this)
            {
                player.currentZone = null;
                Debug.Log("쓰레기 구역을 나갔습니다.");
            }
        }
    }
}
