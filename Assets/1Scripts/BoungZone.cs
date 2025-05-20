using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/// <summary>
/// 붕어빵을 만드는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 붕어빵을 만들 수 있음
/// </summary>
public class BoungZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조
    private bool isMaking = false;          // 현재 붕어빵을 만들고 있는지 여부
    DishZone dishZone;
    public Slider cookSlider;           // 연결된 슬라이더

    [Header("제작 설정")]
    [SerializeField] private float makeTime = 5f;     // 붕어빵 제작 시간
    [SerializeField] private int requiredFlour = 2;   // 필요 밀가루 개수
    [SerializeField] private int requiredPot = 1;    // 필요 팥 개수

    void Start()
    {
        cookSlider.gameObject.SetActive(false);
        dishZone = FindFirstObjectByType<DishZone>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isPlayerInZone = true;
            player.currentZone = this;
            Debug.Log("붕어빵 제작 구역에 들어왔습니다. E키를 눌러 붕어빵을 만드세요.");
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
                Debug.Log("붕어빵 제작 구역을 나갔습니다.");
            }
        }
    }

    private void Update()
    {
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E) && !isMaking)
        {
            // 재료 확인 후 요리 시작
            if (player.flourCount >= requiredFlour && player.potCount >= requiredPot)
            {
                if(!player.TryStartCooking()) return;
                StartCoroutine(CookProcess());
                TryMakeBoung();
            }
            else
            {
                Debug.Log("재료가 부족합니다! (필요: 밀가루 2개, 팥 1개)");
            }
        }
    }

    private void TryMakeBoung()
    {
        // 재료 소모
        player.flourCount -= requiredFlour;
        player.potCount -= requiredPot;

        // 제작 시작
        StartCoroutine(MakeBoungCoroutine());
    }

    private IEnumerator MakeBoungCoroutine()
    {
        isMaking = true;
        player.isMove = false;
        Debug.Log("붕어빵 제작 시작...");

        yield return new WaitForSeconds(makeTime);

        player.boungCount++;
        player.HoldItem("boung");
        dishZone.AddDish();
        Debug.Log($"붕어빵 제작 완료! (현재 보유: {player.boungCount}개)");

        isMaking = false;
        player.isMove = true;
        player.EndCooking(); 
        player.currentZone = null;
    }

    IEnumerator CookProcess()
    {
        isMaking = true;
        cookSlider.gameObject.SetActive(true); // 게이지 보이기
        cookSlider.value = 0f;

        float timer = 0f;
        while (timer < makeTime)
        {
            timer += Time.deltaTime;
            cookSlider.value = timer / makeTime;
            yield return null;
        }

        cookSlider.gameObject.SetActive(false); // 완료 후 숨기기
        Debug.Log("요리 완료!");
        isMaking = false;
        player.EndCooking();  // 요리 완료 시 EndCooking 호출
    }
}
