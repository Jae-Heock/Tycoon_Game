using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 이동, 아이템 관리, 상호작용을 담당하는 클래스
/// </summary>
public class Player : MonoBehaviour
{
    // 이동 관련 변수
    public float baseMoveSpeed = 3f;     // 기본 이동 속도
    public float moveSpeed = 3f;         // 현재 이동 속도
    public float skillSpeed = -1f;       // 스킬로 인한 속도 변화 (기본값 -1)

    float hAxis;                         // 수평 입력값
    float vAxis;                         // 수직 입력값

    public bool isMove;                  // 이동 가능 여부 (true: 이동 가능 / false: 이동 불가)
    public float cookTime = 5f;          // 기본 조리 시간 5초, 스킬로 감소 가능
    public bool isCooking = false;       // 조리 중인지 여부

    [Header("# 점수")]
    public int Point = 0;                    // 현재 점수

    [Header("# 재료아이템")]
    public int sugarCount = 0;        // 보유한 설탕 개수
    public int flourCount = 0;        // 보유한 밀가루 개수
    public int sosageCount = 0;       // 보유한 소시지 개수
    public int potCount = 0;          // 보유한 팥 개수

    [Header("# 음식아이템")]
    public int dalgonaCount = 0;      // 보유한 달고나 개수
    public int hottukCount = 0;       // 보유한 호떡 개수
    public int hotdogCount = 0;       // 보유한 핫도그 개수
    public int boungCount = 0;        // 보유한 붕어빵 개수
    

    [Header("# 성공 횟수")]
    public int customerSuccessCount = 0;  // 성공한 손님 수

    public int basePoint = 4;       // 기본 점수
    public int bonusPoint = 0;      // 스킬로 증가되는 점수
    

    [Header("# 요리 생성위치")]
    public Transform hotdogPoint;         // 핫도그 생성 위치
    public Transform dalgonaPoint;        // 달고나 생성 위치
    public Transform hottukPoint;          // 호떡 생성 위치
    public Transform boungPoint;          // 붕어빵 생성 위치

    private List<GameObject> hotdogList = new List<GameObject>();   // 생성된 핫도그 오브젝트 리스트
    private List<GameObject> dalgonaList = new List<GameObject>();  // 생성된 달고나 오브젝트 리스트
    private List<GameObject> hottukList = new List<GameObject>();  // 생성된 호떡 오브젝트 리스트
    private List<GameObject> boungList = new List<GameObject>();    // 생성된 붕어빵 오브젝트 리스트


    [Header("# 요리 프리팹")]
    public GameObject hotdogPrefab;        // 핫도그 프리팹
    public GameObject dalgonaPrefab;       // 달고나 프리팹
    public GameObject hottukPrefab;        // 호떡 프리팹
    public GameObject boungPrefab;         // 붕어빵 프리팹

    Vector3 moveVec;                      // 이동 방향 벡터
    Rigidbody rigid;                      // 물리 컴포넌트
    Animator anim;

    [Header("# 기절")]
    public bool isStunned = false; // 기절 상태

    public MonoBehaviour currentZone;  // 현재 사용 중인 존

    public string currentFood;  // 현재 들고 있는 음식 타입

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        // 물리 컴포넌트 초기화 및 회전 제한 설정
        rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotation;
        isMove = true;
    }

    private void Update()
    {
        if (isStunned) return; // 기절 중이면 아무것도 못함
        Move();                  // 이동 처리
        UpdateItemVisibility();  // 아이템 표시 업데이트
    }

    /// <summary>
    /// 플레이어 이동 처리
    /// </summary>
    void Move()
    {
        if (!isMove || isCooking) return;  // 이동 불가 상태거나 요리 중이면 리턴

        // 입력값 받기
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        anim.SetBool("isWalk", moveVec != Vector3.zero);
        // 이동 적용
        transform.position += moveVec * moveSpeed * Time.deltaTime;
        if (moveVec != Vector3.zero)
        {
            transform.LookAt(transform.position + moveVec); // 회전
        }
    }

    public void HoldItem(string itemName)
    {
        currentFood = itemName; // ← 여기 추가!

        switch (itemName)
        {
            case "hotdog":
                GameObject hotdog = Instantiate(hotdogPrefab, hotdogPoint);
                hotdog.transform.localPosition = Vector3.zero;
                hotdog.transform.localRotation = Quaternion.identity;
                hotdog.transform.localScale = Vector3.one * 0.1f;
                hotdogList.Add(hotdog);
                break;

            case "dalgona":
                GameObject dalgona = Instantiate(dalgonaPrefab, dalgonaPoint);
                dalgona.transform.localPosition = Vector3.zero;
                // dalgona.transform.localPosition = Vector3.zero + Vector3.up * dalgonaList.Count * 0.5f;
                dalgona.transform.localRotation = Quaternion.identity;
                dalgona.transform.localScale = Vector3.one * 0.1f;
                dalgonaList.Add(dalgona);
                break;

            case "hottuk":
                GameObject hottuk = Instantiate(hottukPrefab, hottukPoint);
                hottuk.transform.localPosition = Vector3.zero + Vector3.up * hottukList.Count * 0.5f;
                hottuk.transform.localRotation = Quaternion.identity;
                hottuk.transform.localScale = Vector3.one * 0.1f;
                hottukList.Add(hottuk);
                break;

            case "boung":
                GameObject boung = Instantiate(boungPrefab, boungPoint);
                boung.transform.localPosition = Vector3.zero + Vector3.up * boungList.Count * 0.5f;
                boung.transform.localRotation = Quaternion.identity;
                boung.transform.localScale = Vector3.one * 0.1f;
                boungList.Add(boung);
                break;
        }
    }

    void UpdateItemVisibility()
    {
        // 핫도그 개수 제한
        while (hotdogList.Count > hotdogCount)
        {
            GameObject lastHotdog = hotdogList[hotdogList.Count - 1];
            hotdogList.RemoveAt(hotdogList.Count - 1);
            Destroy(lastHotdog);
        }

        // 달고나 개수 제한
        while (dalgonaList.Count > dalgonaCount)
        {
            GameObject lastDalgona = dalgonaList[dalgonaList.Count - 1];
            dalgonaList.RemoveAt(dalgonaList.Count - 1);
            Destroy(lastDalgona);
        }

        // 호떡 개수 제한
        while (hottukList.Count > hottukCount)
        {
            GameObject lastHottuk = hottukList[hottukList.Count - 1];
            hottukList.RemoveAt(hottukList.Count - 1);
            Destroy(lastHottuk);
        }

        // 붕어빵 개수 제한
        while (boungList.Count > boungCount)
        {
            GameObject lastBoung = boungList[boungList.Count - 1];
            boungList.RemoveAt(boungList.Count - 1);
            Destroy(lastBoung);
        }
    }


    public void ApplySpeedPenalty()
    {
        moveSpeed = 2f;
    }

    public void RemoveSpeedPenalty()
    {
        // 스킬로 인한 속도 변화가 적용되는 경우, 기본 속도로 복귀
        moveSpeed = (skillSpeed > 0) ? skillSpeed : baseMoveSpeed;
    }

    public void SetSkillSpeed(float value)
    {
        skillSpeed = value;
        moveSpeed = value; // 스킬 적용
    }

    public void PlusPoint()
    {
        Point += basePoint + bonusPoint;  // 기본 점수 + 보너스 점수
        Debug.Log($"점수 획득! +{basePoint + bonusPoint}점 (현재 점수: {Point})");
    }

    public void Stun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        isMove = false;
        Debug.Log($"플레이어가 {duration}초간 기절!");
        yield return new WaitForSeconds(duration);
        isStunned = false;
        isMove = true;
        Debug.Log("플레이어 기절 해제");
    }

    public GameObject GetFoodPrefab(string itemName)
    {
    switch (itemName)
    {
        case "hotdog": return hotdogPrefab;
        case "dalgona": return dalgonaPrefab;
        case "hottuk": return hottukPrefab;
        case "boung": return boungPrefab;
        default: return null;
        }
    }

    public void ClearHeldFood()
    {
        string foodToRemove = currentFood;  // 먼저 저장
        currentFood = null;

        // 해당 음식 오브젝트도 제거
        switch (foodToRemove)  // currentFood 대신 foodToRemove 사용
        {
            case "hotdog":
                if (hotdogList.Count > 0)
                {
                    Destroy(hotdogList[hotdogList.Count - 1]);
                    hotdogList.RemoveAt(hotdogList.Count - 1);
                    hotdogCount--;  // 카운트 감소
                }
                break;
            case "dalgona":
                if (dalgonaList.Count > 0)
                {
                    Destroy(dalgonaList[dalgonaList.Count - 1]);
                    dalgonaList.RemoveAt(dalgonaList.Count - 1);
                    dalgonaCount--;  // 카운트 감소
                }
                break;
            case "hottuk":
                if (hottukList.Count > 0)
                {
                    Destroy(hottukList[hottukList.Count - 1]);
                    hottukList.RemoveAt(hottukList.Count - 1);
                    hottukCount--;  // 카운트 감소
                }
                break;
            case "boung":
                if (boungList.Count > 0)
                {
                    Destroy(boungList[boungList.Count - 1]);
                    boungList.RemoveAt(boungList.Count - 1);
                    boungCount--;  // 카운트 감소
                }
                break;
        }
    }
    public bool TryStartCooking()
    {
        if (isCooking) return false;

        isCooking = true;
        isMove = false;  // 요리 시작할 때 이동 불가
        return true;
    }

    public void EndCooking()
    {
        isCooking = false;
        isMove = true;   // 요리 끝날 때 이동 가능
    }
    public void PlayHoldAnimation()
    {
        anim.SetTrigger("doHold");
    }

    public void PlayPickAnimation()
    {
        anim.SetTrigger("doPick");
    }
}

