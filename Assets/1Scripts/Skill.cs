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

    Image icon;                             // 스킬 아이콘 이미지
    Text textLevel;                         // 레벨 표시 텍스트
    Text textName;                          // 스킬 이름 텍스트
    [TextArea]
    Text textDesc;                          // 스킬 설명 텍스트

    public Sprite[] frameSprites;           // 프레임 스프라이트 배열 [0]: 흰색, [1]: 초록, [2]: 파랑, [3]: 보라
    public Image frameImage;               // 프레임 이미지 컴포넌트

    /// <summary>
    /// 초기화: UI 컴포넌트 참조 및 기본 설정
    /// </summary>
    private void Awake()
    {
        frameImage = transform.Find("Frame").GetComponent<Image>();  // 프레임 이미지 컴포넌트 가져오기

        icon = GetComponentsInChildren<Image>()[1];  // 아이콘 이미지 컴포넌트 가져오기
        icon.sprite = data.skillIcon;                // 스킬 아이콘 설정

        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];                        // 레벨 텍스트
        textName = texts[1];                         // 이름 텍스트
        textDesc = texts[2];                         // 설명 텍스트

        textName.text = data.skillName;              // 스킬 이름 설정

        UpdateFrame();                               // 프레임 업데이트
    }

    /// <summary>
    /// 스킬 레벨에 따른 프레임 이미지 업데이트
    /// </summary>
    private void UpdateFrame()
    {
        if (frameImage == null || frameSprites.Length < 3) return;

        if (level == 0)
            frameImage.sprite = frameSprites[0];     // 동색 프레임
        else if (level == 1)
            frameImage.sprite = frameSprites[1];     // 은색 프레임
        else if (level == 2 || level == 3)
            frameImage.sprite = frameSprites[2];     // 금색 프레임
    }

    /// <summary>
    /// 오브젝트가 활성화될 때 UI 업데이트
    /// </summary>
    private void OnEnable()
    {
        UpdateUI();
        UpdateFrame();
    }

    /// <summary>
    /// 스킬 UI 정보 업데이트 (레벨, 설명 등)
    /// </summary>
    private void UpdateUI()
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
        textLevel.text = "Lv." + (level + 1);
    }

    /// <summary>
    /// 스킬 선택 시 호출되는 함수
    /// </summary>
    public void OnClick()
    {
        level++;                                     // 레벨 증가

        if (level >= data.values.Length +1)             // 최대 레벨 체크
            return;

        UpdateFrame();                               // 프레임 업데이트

        skillManager.SetSkillLevel(level);           // 스킬 매니저에 레벨 설정
        skillManager.ActivateSkill(data);            // 스킬 활성화

        if (level == data.values.Length + 1)             // 최대 레벨 도달 시
        {
            GetComponent<Button>().interactable = false;  // 버튼 비활성화
        }
        Time.timeScale = 1f; // 게임 재개
    }
}
