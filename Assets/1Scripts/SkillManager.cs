using UnityEngine;
using System.Collections;
using static SkillData;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// 스킬 시스템의 핵심 관리자 클래스
/// 스킬의 활성화, 레벨 관리, 효과 적용을 담당
/// </summary>
public class SkillManager : MonoBehaviour
{
    public Dictionary<SkillData, int> skillLevels = new();

    public List<SkillData> ownedSkills = new();

    [SerializeField]
    public int skillLevel = 0;            // 현재 스킬 레벨 (0부터 시작)

    public SkillData selectedSkill;        // 현재 선택된 스킬
    private Player player;                 // 플레이어 참조
    private DalgonaZone dalgonaZone;       // 달고나 제작 구역 참조
    private CustomSpawner customSpawner;    // 나쁜손님 스폰 확률 조절용
    private Dictionary<SkillType, Coroutine> activeCoroutines = new();  // 활성화된 코루틴 관리

    [Header("Auto Delivery AI")]
    [SerializeField] private GameObject deliveryAIPrefab;  // AI 프리팹
    [SerializeField] private Transform[] spawnPoints;      // AI 스폰 위치들

    private NavMeshAgent agent;

    /// <summary>
    /// 초기화: 필요한 컴포넌트 참조 설정
    /// </summary>
    private void Awake()
    {
        player = Object.FindFirstObjectByType<Player>();
        dalgonaZone = Object.FindFirstObjectByType<DalgonaZone>();
        customSpawner = Object.FindFirstObjectByType<CustomSpawner>();
    }
    private void Start()
    {
        player = GameManager.instance.player;
        Debug.Log("SkillManager가 참조한 Player: " + player?.name);
        agent = GetComponent<NavMeshAgent>();
    }


    
    /// <summary>
    /// 스킬 레벨 설정
    /// </summary>
public void SetSkillLevel(SkillData skill, int level)
{
    if (skillLevels.ContainsKey(skill))
        skillLevels[skill] = level;
    else
        skillLevels.Add(skill, level);
}


    /// <summary>
    /// 스킬 활성화 및 효과 적용
    /// </summary>
    public void ActivateSkill(SkillData skillData)
{
    selectedSkill = skillData;

    if (!ownedSkills.Contains(skillData))
        ownedSkills.Add(skillData);

    // 👉 Dictionary에서 개별 스킬 레벨 가져오기
    int level = skillLevels.ContainsKey(skillData) ? skillLevels[skillData] : 0;
    if (level <= 0)
        return;

    float value = skillData.values[Mathf.Clamp(level - 1, 0, skillData.values.Length - 1)];
    int count = skillData.counts[Mathf.Clamp(level - 1, 0, skillData.counts.Length - 1)];

    // 기존 스킬 코루틴 종료
    if (activeCoroutines.TryGetValue(skillData.skillType, out Coroutine running))
        StopCoroutine(running);

    Coroutine newRoutine = null;

    // 스킬 실행
    switch (skillData.skillType)
    {
        case SkillType.AutoSugar:
            newRoutine = StartCoroutine(AutoGenerateSugar(value, count));
            break;
        case SkillType.AutoSosage:
            newRoutine = StartCoroutine(AutoGenerateSosage(value, count));
            break;
        case SkillType.AutoCleanTrash:
            newRoutine = StartCoroutine(AutoCleanTrash(value));
            break;
        case SkillType.PlayerSpeed:
            player.SetSkillSpeed(value);
            break;
        case SkillType.CookSpeed:
            dalgonaZone.makeTime = value;
            break;
        case SkillType.AutoDelivery:
            SpawnAutoDeliveryAI(value);
            break;
        case SkillType.PlusPoint:
            player.bonusPoint = Mathf.RoundToInt(value);
            break;
        case SkillType.ReduceBadCustomerChance:
            if (customSpawner != null)
                customSpawner.badCustomerChance = value;
            break;
        case SkillType.AutoCleanDish:
            newRoutine = StartCoroutine(AutoCleanDish(value));
            break;
        case SkillType.AutoFlour:
            newRoutine = StartCoroutine(AutoGenerateFlour(value, count));
            break;
    }

    if (newRoutine != null)
        activeCoroutines[skillData.skillType] = newRoutine;
}


    /// <summary>
    /// 자동 설탕 생성 코루틴
    /// </summary>
    IEnumerator AutoGenerateSugar(float interval, int count)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < count; i++)
            {
                player.sugarCount++;
            }
        }
    }

    /// <summary>
    /// 자동 달고나 생성 코루틴
    /// </summary>
    IEnumerator AutoGenerateSosage(float interval, int count)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < count; i++)
            {
                player.sosageCount++;
            }
        }
    }

    /// <summary>
    /// 자동 쓰레기 처리 코루틴
    /// </summary>
    IEnumerator AutoCleanTrash(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // 씬에 있는 모든 CustomTrash 찾기 (정렬 없이 더 빠른 방식)
            CustomTrash[] trashObjects = Object.FindObjectsByType<CustomTrash>(FindObjectsSortMode.None);
            
            if (trashObjects.Length > 0)
            {
                // 가장 오래된 쓰레기부터 제거
                CustomTrash oldestTrash = trashObjects[0];
                CustomSpawner spawner = Object.FindFirstObjectByType<CustomSpawner>();
                
                if (spawner != null)
                {
                    // CustomSpawner에 쓰레기 제거 알림
                    spawner.OnTrashCleaned(oldestTrash.transform.position);
                }
                
                // 쓰레기 오브젝트 제거
                Destroy(oldestTrash.gameObject);
                Debug.Log("자동으로 쓰레기가 제거되었습니다.");
            }
        }
    }

    IEnumerator AutoCleanDish(float interval)
    {
        DishZone dishZone = Object.FindFirstObjectByType<DishZone>();

        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (dishZone != null && dishZone.currentDishCount > 0)
            {
                dishZone.CleanOneDish(); // ✅ 이걸 호출해야 실제 오브젝트도 파괴됨
            }
        }
    }

    private void SpawnAutoDeliveryAI(float value)
    {
        if (deliveryAIPrefab == null)
        {
            Debug.LogError("Auto Delivery AI Prefab이 설정되지 않았습니다!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("AI 스폰 포인트가 설정되지 않았습니다!");
            return;
        }

        // value를 정수로 변환하여 AI 개수로 사용
        int aiCount = Mathf.RoundToInt(value);
        
        for (int i = 0; i < aiCount; i++)
        {
            // 랜덤한 스폰 포인트 선택
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (spawnPoint == null)
            {
                Debug.LogError("선택된 스폰 포인트가 null입니다!");
                continue;
            }

            // AI 생성
            GameObject aiObject = Instantiate(deliveryAIPrefab, spawnPoint.position, spawnPoint.rotation);
            if (aiObject == null)
            {
                Debug.LogError("AI 생성에 실패했습니다!");
                continue;
            }

            FoodDeliveryAI ai = aiObject.GetComponent<FoodDeliveryAI>();
            if (ai == null)
            {
                Debug.LogError("생성된 AI에 FoodDeliveryAI 컴포넌트가 없습니다!");
                Destroy(aiObject);
                continue;
            }

            // 홈 포지션 설정
            ai.homePosition = spawnPoint;
        }

        Debug.Log($"AutoDelivery 스킬 활성화: AI {aiCount}개 생성");
    }

    // 자동 밀가루 
    IEnumerator AutoGenerateFlour(float interval, int count)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < count; i++)
            {
                player.flourCount++;
            }
        }
    }

    // private void Update()
    // {
    //     // 테스트용: G키를 누르면 AI 생성
    //     if (Input.GetKeyDown(KeyCode.G))
    //     {
    //         SpawnAutoDeliveryAI(1); // 1개의 AI 생성
    //     }
    // }
}
