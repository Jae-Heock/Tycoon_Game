using System.Collections.Generic;
using UnityEngine;
// using static UnityEditor.Progress;

/// <summary>
/// 레벨업 시 스킬 선택 UI를 관리하는 클래스
/// </summary>
public class LevelUp : MonoBehaviour
{
    RectTransform rect;      // UI의 위치와 크기를 조절하기 위한 RectTransform
    Skill[] skills;          // 모든 스킬 오브젝트들의 배열

    private void Awake()
    {
        // 필요한 컴포넌트들을 가져옴
        rect = GetComponent<RectTransform>();
        skills = GetComponentsInChildren<Skill>();
    }

    /// <summary>
    /// 레벨업 UI를 표시하고 다음 스킬 선택을 준비
    /// </summary>
    public void Show()
    {
        // 1. 선택 가능한 스킬 확인
        List<Skill> candidates = new List<Skill>();
        foreach (Skill skill in skills)
        {
        if (skill.level < skill.data.values.Length)
        {
            candidates.Add(skill);
        }
    }

    // 2. 선택 가능한 스킬이 하나도 없으면 UI를 띄우지 않음
    if (candidates.Count == 0)
    {
        Debug.Log("모든 스킬이 만렙입니다. 레벨업 UI를 띄우지 않습니다.");
        return;
    }

    // 3. 스킬 선택 로직 실행
    Next(candidates);
    rect.localScale = Vector3.one;  // UI 표시
    Time.timeScale = 0f;            // 게임 일시정지
    }


    /// <summary>
    /// 레벨업 UI를 숨김
    /// </summary>
    public void Hide()
    {
        rect.localScale = Vector3.zero;  // UI를 숨김
    }

    /// <summary>
    /// 다음 레벨업에서 선택할 수 있는 스킬들을 준비
    /// </summary>
    void Next(List<Skill> candidates)
    {
    // 1. 모든 스킬 비활성화
    foreach (Skill skill in skills)
        skill.gameObject.SetActive(false);

    // 2. 랜덤하게 최대 3개 선택
    int count = Mathf.Min(3, candidates.Count);
    List<Skill> selected = new List<Skill>();

    while (selected.Count < count)
    {
        Skill pick = candidates[Random.Range(0, candidates.Count)];
        if (!selected.Contains(pick))
            selected.Add(pick);
    }

    // 3. 선택된 스킬만 활성화
    foreach (Skill skill in selected)
        skill.gameObject.SetActive(true);
    }
}
