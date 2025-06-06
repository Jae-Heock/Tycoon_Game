using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    public Slider cookSlider;               // 연결된 슬라이더
    DishZone dishZone;                      // 디쉬존 참조

    [Header("제작 설정")]
    [SerializeField] private float makeTime = 5f;               // 붕어빵 제작 시간
    [SerializeField] private int requiredFlour = 2;             // 필요 밀가루
    [SerializeField] private int requiredPot = 1;               // 필요 팥
    [SerializeField] private Transform boungSpawnPoint;         // 붕어빵이 생성될 위치
    [SerializeField] private GameObject boungPrefab;            // 붕어빵 프리팹
    [Header("붕어빵 기계 애니메이션")]
    public Animator boungMachineAnimator;
    public ParticleSystem boungParticle;
    
    public List<GameObject> boungList = new List<GameObject>(); // 생성된 붕어빵들

    void Start()
    {
        cookSlider.gameObject.SetActive(false);
        dishZone = FindFirstObjectByType<DishZone>();
        if (boungParticle != null)
        {
            boungParticle.Stop();
        }
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

            if (player != null && player.currentZone == this)
            {
                player.currentZone = null;
                Debug.Log("붕어빵 제작 구역을 나갔습니다.");
            }
        }
    }

    private void Update()
    {
        // E키로 제작 시작
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E) && !isMaking)
        {
            if (player.flourCount >= requiredFlour && player.potCount >= requiredPot)
            {
                StartCoroutine(CookProcess());
                TryMakeBoung();
            }
            else
            {
                Debug.Log("재료가 부족합니다! (필요: 밀가루 2개, 팥 1개)");
            }
        }

        // Q키로 붕어빵 수집
        if (isPlayerInZone && boungList.Count > 0 && Input.GetKeyDown(KeyCode.Q))
        {
            if (!string.IsNullOrEmpty(player.currentFood))
            {
                Debug.Log("이미 음식을 들고 있습니다.");
                return;
            }

            CollectBoung();
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

        // 애니메이션 시작
        if (boungMachineAnimator != null)
        {
            boungMachineAnimator.SetBool("Boung", true);
        }

        Debug.Log("붕어빵 제작 시작...");
        yield return new WaitForSeconds(makeTime);

        // 붕어빵 프리팹 생성 위치 계산
        int index = boungList.Count;
        Vector3 spawnPos = boungSpawnPoint.position + Vector3.up * (index * 0.5f);

        GameObject newBoung = Instantiate(boungPrefab, spawnPos, Quaternion.identity);
        boungList.Add(newBoung);

        dishZone.AddDish();
        Debug.Log("붕어빵 제작 완료!");

        // 애니메이션 종료
        if (boungMachineAnimator != null)
        {
            boungMachineAnimator.SetBool("Boung", false);
        }

        // 제작 완료 후 파티클 재생
        if (boungParticle != null)
        {
            boungParticle.Play();
            yield return new WaitForSeconds(2f); // 2초 동안 파티클 표시
            boungParticle.Stop();
        }

        isMaking = false;
        player.currentZone = null;
    }

    public void CollectBoung()
    {
        if (boungList.Count > 0 && player != null)
        {
            GameObject topBoung = boungList[boungList.Count - 1];

            player.boungCount++;
            player.HoldItem("boung");
            Debug.Log($"붕어빵 획득! (현재 보유: {player.boungCount}개)");

            Destroy(topBoung);
            boungList.RemoveAt(boungList.Count - 1);
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
