using UnityEngine;

public class HotdogCollectZone : MonoBehaviour
{
    private bool isPlayerInZone = false;
    private Player player;
    private HotdogZone hotdogZone;

    private void Start()
    {
        hotdogZone = FindFirstObjectByType<HotdogZone>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isPlayerInZone)
            {
                player = other.GetComponent<Player>();
                player.EnterZone(this);
                isPlayerInZone = true;
            }

            Debug.Log("핫도그 수집 구역에 들어왔습니다. E키를 눌러 핫도그를 집으세요.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            if (player != null)
            {
                player.ExitZone(this);
            }
            Debug.Log("핫도그 수집 구역을 나갔습니다.");
        }
    }

    private void Update()
    {
        if (isPlayerInZone && player != null && player.currentZone == this && Input.GetKeyDown(KeyCode.E))
        {
            if (hotdogZone != null && hotdogZone.hotdogList.Count > 0)
            {
                if (!string.IsNullOrEmpty(player.currentFood))
                {
                    Debug.Log("이미 음식을 들고 있습니다!");
                    return;
                }
                SoundManager.instance.ButtonClick();
                hotdogZone.CollectHotdog();
            }
        }
    }
} 