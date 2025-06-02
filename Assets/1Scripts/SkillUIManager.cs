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

            // 현재 레벨 정보
            var descText = slot.transform.Find("Text Desc").GetComponent<Text>();
            var levelText = slot.transform.Find("Text Level").GetComponent<Text>();
            int level = skill.level;
            int currentLevelIndex = Mathf.Clamp(level - 1, 0, data.values.Length - 1);
            descText.text = string.Format(data.skillDesc, data.values[currentLevelIndex]);
            levelText.text = "Lv." + level;

            // 다음 레벨 정보 (업그레이드 미리보기)
            var nextDescText = slot.transform.Find("Text NextDesc")?.GetComponent<Text>();
            var nextLevelText = slot.transform.Find("Text NextLevel")?.GetComponent<Text>();
            if (nextDescText != null && nextLevelText != null)
            {
                if (level < data.values.Length)
                {
                    nextDescText.text = string.Format(data.skillDesc, data.values[level]);
                    nextLevelText.text = "Lv." + (level + 1);
                }
                else
                {
                    nextDescText.text = "최대 레벨";
                    nextLevelText.text = "";
                }
            }
        }
    }
}

}