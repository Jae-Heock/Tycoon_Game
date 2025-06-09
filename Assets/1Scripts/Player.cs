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
    public float baseMoveSpeed = 5f;     // 기본 이동 속도
    public float moveSpeed = 3f;         // 현재 이동 속도
    public float skillSpeed = -1f;       // 스킬로 인한 속도 변화 (기본값 -1)

    float hAxis;                         // 수평 입력값
    float vAxis;                         // 수직 입력값
    bool isBorder;

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
    public Animator anim;

    [Header("# 기절")]
    public bool isStunned = false; // 기절 상태
    [Header("# 이펙트")]
    public GameObject stunEffectObject;  // 머리 위 헤롱헤롱 파티클
    [Header("존")]
    public MonoBehaviour currentZone;  // 현재 사용 중인 존
    public string currentFood;  // 현재 들고 있는 음식 타입
    public Transform handPoint;

    private List<MonoBehaviour> zonesInRange = new List<MonoBehaviour>(); // 플레이어가 감지한 모든 존

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        rigid.constraints = RigidbodyConstraints.FreezeRotation;
        isMove = true;
    }

    // 존 진입 처리
    public void EnterZone(MonoBehaviour zone)
    {
        if (!zonesInRange.Contains(zone))
        {
            zonesInRange.Add(zone);
            UpdateCurrentZone();
        }
    }

    // 존 이탈 처리
    public void ExitZone(MonoBehaviour zone)
    {
        if (zonesInRange.Contains(zone))
        {
            zonesInRange.Remove(zone);
            if (currentZone == zone)
            {
                currentZone = null;
            }
            UpdateCurrentZone();
        }
    }

    // 현재 존 업데이트
    private void UpdateCurrentZone()
    {
        if (zonesInRange.Count > 0)
        {
            // 가장 가까운 존을 현재 존으로 설정
            MonoBehaviour closestZone = zonesInRange[0];
            float closestDistance = float.MaxValue;

            foreach (MonoBehaviour zone in zonesInRange)
            {
                float distance = Vector3.Distance(transform.position, zone.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestZone = zone;
                }
            }

            currentZone = closestZone;
        }
        else
        {
            currentZone = null;
        }
    }

    private void Update()
    {
        if (isStunned) return;
        if (!isMove) return;  // 이동이 불가능하면 입력 처리하지 않음

        StopToWall();
        Move();
        UpdateItemVisibility();
        UpdateCurrentZone(); // 매 프레임마다 현재 존 업데이트

        // CustomTable 상호작용: E키 하나로 음식 올리기/집기 모두 처리
        if (currentZone is CustomTable customTable && Input.GetKeyDown(KeyCode.E))
        {
            if (string.IsNullOrEmpty(currentFood))
            {
                // 손에 음식이 없으면 테이블에서 집기
                SoundManager.instance.ButtonClick();
                customTable.TakeFoodToPlayer();
            }
            else
            {
                // 손에 음식이 있으면 테이블에 올리기
                GameObject prefab = GetFoodPrefab(currentFood);
                if (customTable.PlaceFood(currentFood, prefab))
                {
                    SoundManager.instance.ButtonClick();
                    ClearHeldFood();
                    Debug.Log($"{currentFood}을(를) 테이블에 올렸습니다.");
                }
                else
                {
                    Debug.Log("테이블에 이미 음식이 있습니다.");
                }
            }
        }

        bool isCookedFood = currentFood == "hotdog" || currentFood == "dalgona" || currentFood == "hottuk" || currentFood == "boung";
        bool isDalgonaCooking = isCooking;

        // PICK 애니메이션 Layer 1
        anim.SetBool("isPick", isCookedFood && !isDalgonaCooking);

        // Layer 1 Weight 조정
        if (isDalgonaCooking)
            anim.SetLayerWeight(1, 1f);      // 요리 중엔 1f
        else if (isCookedFood)
            anim.SetLayerWeight(1, 0.65f);   // 음식만 들고 있으면 0.65f
        else if (anim.GetBool("isClean"))
            anim.SetLayerWeight(1, 0f);
        else
            anim.SetLayerWeight(1, 0f);      // 아무것도 없으면 0f

    }

    /// <summary>
    /// 플레이어 이동 처리
    /// </summary>
    void Move()
    {
        if (!isMove || isCooking) return;

        // 입력값 받기
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");

        // 카메라 기준 방향
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 방향 계산
        moveVec = (camForward * vAxis + camRight * hAxis).normalized;

        // 이동 적용
        if (!isBorder)
        {
            transform.position += moveVec * moveSpeed * Time.deltaTime;
        }

        if (moveVec != Vector3.zero)
        {
            transform.LookAt(transform.position + moveVec); // 캐릭터가 이동 방향으로 회전
        }

        anim.SetBool("isWalk", moveVec != Vector3.zero); // 걷기 애니메이션
    }

    void StopToWall()
    {
        if (moveVec == Vector3.zero)
        {
            isBorder = false;
            return;
        }

        Vector3 rayStart = transform.position + Vector3.up * 0.3f + moveVec * 0.1f;
        float radius = 0.2f;
        float castDistance = 0.5f;

        RaycastHit hit;
        isBorder = Physics.SphereCast
        (rayStart, radius, moveVec, out hit, castDistance, LayerMask.GetMask("Wall"), QueryTriggerInteraction.Ignore);
        
        Debug.DrawRay(rayStart, moveVec * castDistance, isBorder ? Color.red : Color.green);
    }

    public void HoldItem(string itemName)
    {
        // 완성된 음식이 아닐 경우 들 수 없음
        if (itemName != "hotdog" && itemName != "dalgona" && itemName != "hottuk" && itemName != "boung")
        {
            Debug.Log($"'{itemName}'은 들 수 없는 재료입니다. currentFood 설정 생략.");
            return;
        }

        currentFood = itemName;

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
        if (hotdogList != null)
        {
            while (hotdogList.Count > hotdogCount && hotdogList.Count > 0)
            {
                GameObject lastHotdog = hotdogList[hotdogList.Count - 1];
                hotdogList.RemoveAt(hotdogList.Count - 1);
                if (lastHotdog != null)
                    Destroy(lastHotdog);
            }
        }

        // 달고나 개수 제한
        if (dalgonaList != null)
        {
            while (dalgonaList.Count > dalgonaCount && dalgonaList.Count > 0)
            {
                GameObject lastDalgona = dalgonaList[dalgonaList.Count - 1];
                dalgonaList.RemoveAt(dalgonaList.Count - 1);
                if (lastDalgona != null)
                    Destroy(lastDalgona);
            }
        }

        // 호떡 개수 제한
        if (hottukList != null)
        {
            while (hottukList.Count > hottukCount && hottukList.Count > 0)
            {
                GameObject lastHottuk = hottukList[hottukList.Count - 1];
                hottukList.RemoveAt(hottukList.Count - 1);
                if (lastHottuk != null)
                    Destroy(lastHottuk);
            }
        }

        // 붕어빵 개수 제한
        if (boungList != null)
        {
            while (boungList.Count > boungCount && boungList.Count > 0)
            {
                GameObject lastBoung = boungList[boungList.Count - 1];
                boungList.RemoveAt(boungList.Count - 1);
                if (lastBoung != null)
                    Destroy(lastBoung);
            }
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

        // 🔥 이펙트 ON
        if (stunEffectObject != null)
            stunEffectObject.SetActive(true);

        Debug.Log($"플레이어가 {duration}초간 기절!");

        yield return new WaitForSeconds(duration);

        isStunned = false;
        isMove = true;

        // 🔥 이펙트 OFF
        if (stunEffectObject != null)
            stunEffectObject.SetActive(false);

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
        //anim.SetTrigger("doDown");      // DOWN 애니메이션 실행
        string foodToRemove = currentFood;  // 먼저 저장
        currentFood = null;
        anim.SetBool("isPick", false);  // PICK 레이어 비활성화
        anim.SetLayerWeight(1, 0f); // 직접 꺼주기

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

    public void PlayDownAnimation()
    {
        anim.SetTrigger("doDown");
    }

    public void PlayDalgonaAnimation()
    {
        Debug.Log("PlayDalgonaAnimation 호출됨");
        anim.SetTrigger("doDal");
    }

    public void StopDalgonaAnimation()
    {
        Debug.Log("StopDalgonaAnimation 호출됨");
        anim.SetTrigger("doDal");
    }

}