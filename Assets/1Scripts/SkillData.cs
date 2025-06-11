using UnityEngine;

/// <summary>
/// 스킬 데이터를 정의하는 ScriptableObject 클래스
/// </summary>
[CreateAssetMenu(fileName = "SkillManager", menuName = "Scriptable Object/SkillData")]
public class SkillData : ScriptableObject
{
    /// <summary>
    /// 스킬의 종류를 정의하는 열거형
    /// </summary>
    public enum SkillType
    {
        AutoSugar,           // 자동 설탕 생성
        AutoSosage,         // 자동 달고나 생성
        PlayerSpeed,         // 플레이어 이동 속도
        CookSpeed,           // 조리 시간 감소 - 이거는뺏음 
        AutoCleanTrash,      // 쓰레기 자동 처리
        AutoDelivery,        // 자동 배달
        PlusPoint,           // 점수 증가
        ReduceBadCustomerChance, // 나쁜손님 등장 확률 감소
        AutoCleanDish,            // 자동 설거지
        AutoFlour,               // 자동 밀가루 생성
    }

    [Header("# 기본 정보")]
    public SkillType skillType;    // 스킬 종류
    public int skillId;            // 스킬 고유 ID
    public string skillName;       // 스킬 이름
    public string skillDesc;       // 스킬 설명
    public Sprite skillIcon;       // 스킬 아이콘

    [Header("# 레벨별 데이터")]
    public float[] values;         // 레벨별 수치 (예: 쿨타임/효과값/AI수)
    public int[] counts;           // 레벨별 횟수
}
