using UnityEngine;

public class BoungCollectZone : MonoBehaviour
{
    private bool isPlayerInZone = false;
    private Player player;
    private BoungZone boungZone;

    private void Start()
    {
        boungZone = FindFirstObjectByType<BoungZone>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.EnterZone(this);
            Debug.Log("붕어빵 수집 구역에 들어왔습니다. Q키를 눌러 붕어빵을 집으세요.");
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
            Debug.Log("붕어빵 수집 구역을 나갔습니다.");
        }
    }

    private void Update()
    {
        if (isPlayerInZone && player != null && player.currentZone == this && Input.GetKeyDown(KeyCode.E))
        {
            if (boungZone != null && boungZone.boungList.Count > 0)
            {
                if (!string.IsNullOrEmpty(player.currentFood))
                {
                    Debug.Log("이미 음식을 들고 있습니다!");
                    return;
                }
                SoundManager.instance.ButtonClick();
                boungZone.CollectBoung();
            }
        }
    }
} 