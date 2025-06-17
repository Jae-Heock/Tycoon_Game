using UnityEngine;

/// <summary>
/// 소시지를 획득할 수 있는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 소시지를 획득할 수 있음
/// </summary>
public class SosageZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조

    /// <summary>
    /// 플레이어가 구역에 들어왔을 때 호출
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.EnterZone(this);
            Debug.Log("소시지 구역에 들어왔습니다. E키를 눌러 소시지를 획득하세요.");
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
            player.ExitZone(this);
            Debug.Log("소시지 구역을 나갔습니다.");
        }
    }

    /// <summary>
    /// 매 프레임마다 호출되는 업데이트 함수
    /// E키 입력 시 소시지 획득 처리
    /// </summary>
    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E) && player.currentZone == this)
        {
            SoundManager.instance.PlayGetItem();
            player.sosageCount++;
            player.HoldItem("sosage");
            Debug.Log($"소시지 +1 (현재: {player.sosageCount})");
        }
    }
}
