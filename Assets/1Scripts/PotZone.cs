using UnityEngine;

/// <summary>
/// 냄비(재료)를 획득하는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 냄비를 획득할 수 있음
/// </summary>
public class PotZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            if (player != null)
                player.EnterZone(this);
            Debug.Log("팥 구역에 들어왔습니다. E키를 눌러 팥을 획득하세요.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (player != null)
                player.ExitZone(this);
            Debug.Log("팥 구역을 나갔습니다.");
        }
    }

    private void Update()
    {
        if (isPlayerInZone && player != null && player.currentZone == this && Input.GetKeyDown(KeyCode.E))
        {
            SoundManager.instance.PlayGetItem();
            player.potCount++;
            Debug.Log($"팥 +1 (현재: {player.potCount})");
        }
    }
} 