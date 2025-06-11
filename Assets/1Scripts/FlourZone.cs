using UnityEngine;

/// <summary>
/// 밀가루를 획득할 수 있는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 밀가루를 획득할 수 있음
/// </summary>
public class FlourZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.currentZone = this;
            Debug.Log("밀가루 구역에 들어왔습니다. E키를 눌러 밀가루를 획득하세요.");
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
                Debug.Log("밀가루 구역을 나갔습니다.");
            }
        }
    }

    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            SoundManager.instance.PlayGetItem();
            player.flourCount++;
            player.HoldItem("flour");
            Debug.Log($"밀가루 +1 (현재: {player.flourCount})");
        }
    }
}
