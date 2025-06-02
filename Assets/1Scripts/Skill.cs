using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 UI와 기능을 관리하는 클래스
/// </summary>
public class Skill : MonoBehaviour
{
    public SkillData data;                  // 스킬 데이터 (이름, 설명, 아이콘 등)
    public int level;                       // 현재 스킬 레벨
    public SkillManager skillManager;       // 스킬 매니저 참조

    // UI 연결용
    private Image icon;
    private Text textLevel;
    private Text textName;
    private Text textDesc;
    public Image frameImage;
    public Sprite[] frameSprites;

    private void Awake()
    {
        // 명시적 경로로 안전하게 컴포넌트 참조
        icon = transform.Find("Icon")?.GetComponent<Image>();
        textName = transform.Find("Text Name")?.GetComponent<Text>();
        textDesc = transform.Find("Text Desc")?.GetComponent<Text>();
        textLevel = transform.Find("Text Level")?.GetComponent<Text>();
        frameImage = transform.Find("Frame")?.GetComponent<Image>();

        UpdateUI(); // 시작 시 초기화
    }

    private void OnEnable()
    {
        UpdateUI();
        UpdateFrame();
    }

    /// <summary>
    /// UI 내용 갱신
    /// </summary>
    public void UpdateUI()
    {
        if (data == null) return;

        if (icon != null)
            icon.sprite = data.skillIcon;

        if (textName != null)
            textName.text = data.skillName;

        if (textDesc != null)
        {
            if (data.skillType == SkillData.SkillType.ReduceBadCustomerChance)
            {
                int percent = Mathf.RoundToInt(data.values[Mathf.Min(level, data.values.Length - 1)] * 100f);
                textDesc.text = string.Format(data.skillDesc, percent);
            }
            else
            {
                textDesc.text = string.Format(data.skillDesc, data.values[Mathf.Min(level, data.values.Length - 1)]);
            }
        }

        if (textLevel != null)
            textLevel.text = "Lv." + (level + 1);
    }

    /// <summary>
    /// 프레임 테두리 변경
    /// </summary>
    private void UpdateFrame()
    {
        if (frameImage == null || frameSprites.Length < 3) return;

        if (level == 0)
            frameImage.sprite = frameSprites[0];
        else if (level == 1)
            frameImage.sprite = frameSprites[1];
        else
            frameImage.sprite = frameSprites[2];
    }

    /// <summary>
    /// 클릭 시 스킬 적용
    /// </summary>
    public void OnClick()
    {
        level++;
         Debug.Log($"[Skill] {data.skillName} 클릭됨");
        // 먼저 현재 레벨로 효과 적용
        skillManager.SetSkillLevel(data, level);
        skillManager.ActivateSkill(data);
    
        UpdateFrame();
        UpdateUI();

        // 최대 레벨 도달했으면 버튼 비활성화
        if (level >= data.values.Length - 1)
            GetComponent<Button>().interactable = false;

        // 마지막에 레벨 증가
        

        Time.timeScale = 1f;
    }


}
