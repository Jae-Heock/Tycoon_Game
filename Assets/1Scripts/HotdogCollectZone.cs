using UnityEngine;

public class HotdogCollectZone : MonoBehaviour
{
    private bool isPlayerInZone = false;
    private Player player;
    private Hotdog hotdogZone;

    private void Start()
    {
        hotdogZone = FindFirstObjectByType<Hotdog>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.currentZone = this;
            Debug.Log("핫도그 수집 구역에 들어왔습니다. Q키를 눌러 핫도그를 집으세요.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (player != null && player.currentZone == this)
            {
                player.currentZone = null;
                Debug.Log("핫도그 수집 구역을 나갔습니다.");
            }
        }
    }

    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.Q))
        {
            if (hotdogZone != null && hotdogZone.currentHotdog != null)
            {
                if (!string.IsNullOrEmpty(player.currentFood))
                {
                    Debug.Log("이미 음식을 들고 있습니다!");
                    return;
                }
                hotdogZone.CollectHotdog();
            }
        }
    }
} 