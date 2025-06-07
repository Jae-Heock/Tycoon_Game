using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 접시를 관리하는 구역
/// 플레이어가 이 구역에 들어와서 E키를 누르면 접시를 씻을 수 있음
/// </summary>
public class DishZone : MonoBehaviour
{
    private bool isPlayerInZone = false;    // 플레이어가 구역 안에 있는지 여부
    private Player player;                  // 플레이어 참조
    private bool isReducingPoints = false;  // 포인트 감소 중인지 여부

    public Transform dishPoint;         // 접시 생성 위치치
    private List<GameObject> dishList = new List<GameObject>();  // 생성된 접시시 오브젝트 목록
    public GameObject dishPrefab;       // 접시 프리팹
    private GameObject dishInHandInstance; // 현재 손에 붙은 오브젝트

    [Header("접시 설정")]
    public int maxDishes = 5;              // 최대 접시 개수
    public float pointReductionInterval = 2f; // 포인트 감소 간격 (초)
    public int pointReductionAmount = 1;    // 감소할 포인트 양

    [Header("현재 접시 개수")]
    [SerializeField] public int currentDishCount;    // 현재 접시 개수

    [Header("설거지 파티클")]
    public ParticleSystem cleanParticle;

    public Slider cleanSlider; // 인스펙터에서 할당
    public float cleanDuration = 5f;

    private float cleanTimer = 0f;
    private bool isCleaning = false;
    private GameObject heldDishObject; // 손에 든 그릇

    public Transform handPoint; // 인스펙터에서 손 위치(빈 오브젝트) 할당

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (cleanSlider != null)
            cleanSlider.gameObject.SetActive(false);
            
        if (cleanParticle != null)
        {
            cleanParticle.Stop(true);
        }
    }

    /// <summary>
    /// 플레이어가 구역에 들어왔을 때 호출
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            player = other.GetComponent<Player>();
            player.currentZone = this;  // currentZone 설정
            Debug.Log("설거지 구역에 들어왔습니다. E키를 눌러 설거지를 하세요.");
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
             if (player != null && player.currentZone == this)
            {
                player.currentZone = null;
            }
            Debug.Log("설거지 구역을 나갔습니다.");
        }
    }

    private void Update()
    {
        if (isPlayerInZone && player != null && player.currentZone == this && currentDishCount > 0 && string.IsNullOrEmpty(player.currentFood))
        {
            if (Input.GetKey(KeyCode.E))
            {
                // 설거지 시작
                if (!isCleaning)
                {
                    isCleaning = true;
                    cleanTimer = 0f;
                    if (cleanSlider != null)
                    {
                        cleanSlider.value = 0f;
                        cleanSlider.gameObject.SetActive(true);
                    }
                    if (cleanParticle != null)
                        cleanParticle.Play();
                    player.anim.SetBool("isClean", true);
                    player.isMove = false;

                    // 손에 접시 붙이기
                    if (dishInHandInstance == null && dishPrefab != null && player.handPoint != null)
                    {
                        dishInHandInstance = Instantiate(dishPrefab, player.handPoint);
                        dishInHandInstance.transform.localPosition = Vector3.zero;
                        dishInHandInstance.transform.localRotation = Quaternion.identity;
                        dishInHandInstance.transform.localScale = Vector3.one * 5f;
                    }
                }

                // 진행
                cleanTimer += Time.deltaTime;
                if (cleanSlider != null)
                    cleanSlider.value = cleanTimer / cleanDuration;

                if (cleanTimer >= cleanDuration)
                {
                    // 설거지 완료
                    player.anim.SetBool("isClean", false);
                    player.isMove = true;
                    if (cleanParticle != null)
                        cleanParticle.Stop();
                    if (cleanSlider != null)
                        cleanSlider.gameObject.SetActive(false);

                    CleanOneDish();
                    isCleaning = false;

                    // 접시 제거
                    if (dishInHandInstance != null)
                    {
                        Destroy(dishInHandInstance);
                        dishInHandInstance = null;
                    }
                }
            }
            else
            {
                // E키를 뗐을 때 중단 및 리셋
                if (isCleaning)
                {
                    isCleaning = false;
                    cleanTimer = 0f;
                    if (cleanSlider != null)
                    {
                        cleanSlider.value = 0f;
                        cleanSlider.gameObject.SetActive(false);
                    }
                    if (cleanParticle != null)
                        cleanParticle.Stop();
                    player.anim.SetBool("isClean", false);
                    player.isMove = true;

                    // 접시 제거
                    if (dishInHandInstance != null)
                    {
                        Destroy(dishInHandInstance);
                        dishInHandInstance = null;
                    }
                }
            }
        }
        else
        {
            // 존을 벗어나거나 설거지할 그릇이 없으면 강제 리셋
            if (isCleaning)
            {
                isCleaning = false;
                cleanTimer = 0f;
                if (cleanSlider != null)
                {
                    cleanSlider.value = 0f;
                    cleanSlider.gameObject.SetActive(false);
                }
                if (cleanParticle != null)
                    cleanParticle.Stop();
                player.anim.SetBool("isClean", false);
                player.isMove = true;

                // 접시 제거
                if (dishInHandInstance != null)
                {
                    Destroy(dishInHandInstance);
                    dishInHandInstance = null;
                }
            }
        }

        // 접시 개수에 따른 포인트 감소 처리
        if (currentDishCount >= maxDishes && !isReducingPoints)
        {
            StartCoroutine(ReducePoints());
        }
        else if (currentDishCount < maxDishes && isReducingPoints)
        {
            isReducingPoints = false;
        }
    }

    // 요리를 만들 때 호출할 함수
    public void AddDish()
    {
        currentDishCount++;
        HoldItem("dish"); 
        Debug.Log($"접시 추가! 현재 접시 개수: {currentDishCount}");
    }

    public void HoldItem(string itemName)
    {
        switch (itemName)
        {
            case "dish":
                GameObject dish = Instantiate(dishPrefab, dishPoint);
                dish.transform.localPosition = Vector3.zero + Vector3.up * dishList.Count * 0.3f;
                dish.transform.localRotation = Quaternion.identity;
                dish.transform.localScale = Vector3.one * 100f;
                dishList.Add(dish);
                break;
        }
    }

    private void CleanDishes()
    {
        // 플레이어가 다른 존을 사용 중인지 확인
        if (player.currentZone != null && player.currentZone != this)
        {  
            Debug.Log("다른 존을 사용 중입니다.");
            return;
        }

        // 모든 접시 초기화
        currentDishCount = 0;
        foreach (GameObject dish in dishList)
        {
            Destroy(dish);
        }
        dishList.Clear();

        // 포인트 감소 중지
        isReducingPoints = false;

        Debug.Log("설거지 완료! 모든 접시가 정리되었습니다.");
    }


    public void CleanOneDish()
    {
        if (currentDishCount > 0)
        {
            currentDishCount--;
            Debug.Log($"설거지 완료! 남은 그릇: {currentDishCount}");
        }
    }

    private IEnumerator ReducePoints()
    {
        isReducingPoints = true;
        while (isReducingPoints)
        {
            yield return new WaitForSeconds(pointReductionInterval);
            if (player.Point > 0)
            {
                player.Point -= pointReductionAmount;
                Debug.Log($"접시가 너무 많습니다! -{pointReductionAmount}점 (현재 점수: {player.Point})");
            }
        }
    }
} 