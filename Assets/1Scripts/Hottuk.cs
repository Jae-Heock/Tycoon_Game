using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 호떡을 만드는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 호떡을 만들 수 있음
/// </summary>
public class Hottuk : MonoBehaviour
{
    Animator anim;
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조
    private bool isMaking = false;          // 현재 호떡을 만들고 있는지 여부
    private DishZone dishZone;              // 접시 관리 구역 참조

    [Header("제작 설정")]
    [SerializeField] private float makeTime = 5f;     // 호떡 제작 시간
    [SerializeField] private int requiredFlour = 1;   // 필요 밀가루 개수
    [SerializeField] private int requiredSugar = 1;   // 필요 설탕 개수

    public Slider cookSlider;           // 연결된 슬라이더
    
    [Header("달고나 보이기")]
    public GameObject hottukPrefab;
    public Transform hottukPoint;
    public GameObject hottukInHand;

    public ParticleSystem hottukParticle;
    private void Start()
    {
        cookSlider.gameObject.SetActive(false);
        dishZone = FindFirstObjectByType<DishZone>();
        anim = GetComponent<Animator>();
        hottukParticle.Stop();
    }

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
            Debug.Log("호떡 제작 구역에 들어왔습니다. E키를 눌러 호떡을 만드세요.");
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
            if (player != null)
            {
                player.ExitZone(this);
            }
            Debug.Log("호떡 제작 구역을 나갔습니다.");
        }
    }

    /// <summary>
    /// 매 프레임마다 호출되는 업데이트 함수
    /// E키 입력 시 호떡 제작 시작
    /// </summary>
    private void Update()
    {
        if (isPlayerInZone && player != null && player.currentZone == this && Input.GetKeyDown(KeyCode.E) && !isMaking)
        {
            if (!string.IsNullOrEmpty(player.currentFood))
            {
                Debug.Log("이미 음식을 들고 있어 요리를 시작할 수 없습니다!");
                return;
            }
            // 재료 확인 후 요리 시작
            if (player.flourCount >= requiredFlour && player.sugarCount >= requiredSugar)
            {
                if(!player.TryStartCooking()) return;
                StartCoroutine(CookProcess());
                TryMakeHottuk();
            }
            else
            {
                SoundManager.instance.PlayFail();
                Debug.Log("재료가 부족합니다! (필요: 밀가루 1개, 설탕 1개)");
            }
        }
    }

    /// <summary>
    /// 호떡 제작 시도
    /// </summary>
    private void TryMakeHottuk()
    {
        // 재료 소모
        player.flourCount -= requiredFlour;
        player.sugarCount -= requiredSugar;
        // 제작 시작
        StartCoroutine(MakeHottukCoroutine());
    }

    /// <summary>
    /// 호떡 제작 코루틴
    /// </summary>
    private IEnumerator MakeHottukCoroutine()
    {
        isMaking = true;
        Debug.Log("호떡 제작 시작...");
        SoundManager.instance.PlayHottuk();
        player.anim.SetBool("isDal", true);
        hottukParticle.Play();
        // 호떡 프리팹 붙이기
        if (hottukInHand == null && hottukPrefab != null && hottukPoint != null)
        {
            hottukInHand = Instantiate(hottukPrefab, hottukPoint);
            hottukInHand.transform.localPosition = Vector3.zero;
            hottukInHand.transform.localRotation = Quaternion.identity;
            hottukInHand.transform.localScale = Vector3.one * 200f;
        }
        yield return new WaitForSeconds(makeTime);

        // 호떡 생성
        player.hottukCount++;
        player.anim.SetBool("isDal", false);
        player.HoldItem("hottuk");
        Debug.Log($"호떡 제작 완료! (현재 보유: {player.hottukCount}개)");
        // 상태 초기화

        if(hottukInHand != null)
        {
            Destroy(hottukInHand);
            hottukInHand = null;
        }
        hottukParticle.Stop();
        isMaking = false;
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
