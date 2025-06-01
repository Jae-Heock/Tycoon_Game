using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillUIManager : MonoBehaviour
{
    public GameObject skillPanel;             // "Skill Panel" 전체
    public GameObject skillSlotPrefab;        // 프리팹
    public Transform slotParent;              // "Skill Group" 오브젝트
    public SkillManager skillManager;          // SkillManager 연결

    void Start()
    {
        skillPanel.SetActive(false);

        foreach (var skill in skillManager.ownedSkills)
        {
            if (!skillManager.ownedSkills.Contains(skill))
                skillManager.ownedSkills.Add(skill);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            skillPanel.SetActive(true);
            RefreshSkillSlots();
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            skillPanel.SetActive(false);
        }
    }

void RefreshSkillSlots()
{
    foreach (Transform child in slotParent)
        Destroy(child.gameObject);

    foreach (SkillData data in skillManager.ownedSkills)
    {
        if (data == null) continue;

        GameObject slot = Instantiate(skillSlotPrefab, slotParent);
        Skill skill = slot.GetComponent<Skill>();
        if (skill != null)
        {
            skill.data = data;
            skill.level = skillManager.skillLevels.ContainsKey(data) ? skillManager.skillLevels[data] : 0;
            skill.skillManager = skillManager;
            skill.UpdateUI();
        }
    }
}

}