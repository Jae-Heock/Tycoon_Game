using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
/// <summary>
/// 핫도그를 만드는 구역을 관리하는 클래스
/// 플레이어가 이 구역에 들어와서 E키를 누르면 핫도그를 만들 수 있음
/// </summary>
public class HotdogZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조
    private bool isMaking = false;          // 현재 핫도그를 만들고 있는지 여부
    public Slider cookSlider;               // 연결된 슬라이더
    DishZone dishZone;                      // 디쉬존 참조

    [Header("제작 설정")]
    [SerializeField] private float makeTime = 10f;    // 핫도그 제작 시간
    [SerializeField] private int requiredFlour = 1;   // 필요 밀가루 개수
    [SerializeField] private int requiredSosage = 1;  // 필요 소시지 개수
    [SerializeField] private Transform hotdogSpawnPoint; // 핫도그가 생성될 위치
    [SerializeField] private GameObject hotdogPrefab;    // 핫도그 프리팹

    public List<GameObject> hotdogList = new List<GameObject>(); // 생성된 핫도그들

    private void Start()
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
            Debug.Log("핫도그 제작 구역에 들어왔습니다. E키를 눌러 핫도그를 만드세요.");
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
                Debug.Log("핫도그 제작 구역을 나갔습니다.");
            }
        }
    }

    private void Update()
    {
        // E키로 제작 시작
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E) && !isMaking)
        {
            if (player.flourCount >= requiredFlour && player.sosageCount >= requiredSosage)
            {
                StartCoroutine(CookProcess());
                TryMakeHotdog();
            }
            else
            {
                Debug.Log("재료가 부족합니다! (필요: 밀가루 1개, 소시지 1개)");
            }
        }

        // Q키로 핫도그 수집
        if (isPlayerInZone && hotdogList.Count > 0 && Input.GetKeyDown(KeyCode.Q))
        {
            if (!string.IsNullOrEmpty(player.currentFood))
            {
                Debug.Log("이미 음식을 들고 있습니다.");
                return;
            }

            CollectHotdog();
        }
    }

    private void TryMakeHotdog()
    {
        // 재료 소모
        player.flourCount -= requiredFlour;
        player.sosageCount -= requiredSosage;

        // 제작 시작
        StartCoroutine(MakeHotdogCoroutine());
    }

    private IEnumerator MakeHotdogCoroutine()
    {
        isMaking = true;
        Debug.Log("핫도그 제작 시작...");
        yield return new WaitForSeconds(makeTime);

        // 핫도그 프리팹 생성 위치 계산
        int index = hotdogList.Count;
        Vector3 spawnPos = hotdogSpawnPoint.position + Vector3.up * (index * 0.5f);

        GameObject newHotdog = Instantiate(hotdogPrefab, spawnPos, Quaternion.identity);
        hotdogList.Add(newHotdog);

        dishZone.AddDish();
        Debug.Log("핫도그 제작 완료!");

        isMaking = false;
        player.currentZone = null;
    }

    public void CollectHotdog()
    {
        if (hotdogList.Count > 0 && player != null)
        {
            GameObject topHotdog = hotdogList[hotdogList.Count - 1];

            player.hotdogCount++;
            player.HoldItem("hotdog");
            Debug.Log($"핫도그 획득! (현재 보유: {player.hotdogCount}개)");

            Destroy(topHotdog);
            hotdogList.RemoveAt(hotdogList.Count - 1);
        }
    }

    IEnumerator CookProcess()
    {
        isMaking = true;
        cookSlider.gameObject.SetActive(true);
        cookSlider.value = 0f;

        float timer = 0f;
        while (timer < makeTime)
        {
            timer += Time.deltaTime;
            cookSlider.value = timer / makeTime;
            yield return null;
        }

        cookSlider.gameObject.SetActive(false);
        Debug.Log("요리 완료!");
        isMaking = false;
    }
}
