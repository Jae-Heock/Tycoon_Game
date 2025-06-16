using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OrderListManager : MonoBehaviour
{
    public static OrderListManager Instance { get; private set; }
    public Transform orderListPanel; // Vertical Layout Group이 붙은 Panel
    public GameObject orderListSlotPrefab; // 슬롯 프리팹

    // 손님 이름과 아이콘 매핑 (프리팹 이름과 동일하게)
    public Sprite[] customerIcons; // 12개 손님 아이콘 (순서 주의)
    public string[] customerNames = { "Bad_개", "Bad_닭", "Bad_쥐", "돼지", "말", "뱀", "소", "양", "용", "원숭이", "토끼", "호랑이" };

    public Sprite dalgonaIcon, hottukIcon, hotdogIcon, boungIcon;
    
    // 나쁜 손님 프리팹
    public GameObject badDalgonaPrefab, badHotdogPrefab, badStunPrefab;

    // 슬라이더 색상
    public Color greenColor = Color.green;
    public Color yellowColor = Color.yellow;
    public Color redColor = Color.red;

    public List<Custom> customerList = new List<Custom>(); // 생성 순서 보장

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
    }

    void Update()
    {
        UpdateOrderList();
    }

    public void UpdateOrderList()
    {
        // 기존 슬롯 삭제 (역순 for문 사용)
        for (int i = orderListPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(orderListPanel.GetChild(i).gameObject);
        }

        foreach (var custom in customerList)
        {
            GameObject slot = Instantiate(orderListSlotPrefab, orderListPanel);
            // 손님 아이콘 세팅
            string pureName = custom.gameObject.name.Replace("(Clone)", "").Trim();
            int idx = System.Array.IndexOf(customerNames, pureName);
            if (idx >= 0 && idx < customerIcons.Length)
                slot.transform.Find("CustomerIcon").GetComponent<Image>().sprite = customerIcons[idx];

            // 나쁜 손님 표시 (이름이 Bad_로 시작하거나 isBadCustomer가 true인 경우)
            bool isBadCustomer = pureName.StartsWith("Bad_") || custom.isBadCustomer;
            
            if (isBadCustomer)
            {
                // 나쁜 손님 UI 표시
                Transform foodIconTransform = slot.transform.Find("FoodIcon");
                if (foodIconTransform != null)
                {
                    // 기존 음식 아이콘 비활성화
                    foodIconTransform.gameObject.SetActive(false);
                    
                    // 나쁜 손님 타입에 따라 프리팹 인스턴스화
                    GameObject badIconPrefab = null;
                    switch (custom.badType)
                    {
                        case Custom.BadType.Dalgona:
                            badIconPrefab = badDalgonaPrefab;
                            break;
                        case Custom.BadType.Hotdog:
                            badIconPrefab = badHotdogPrefab;
                            break;
                        case Custom.BadType.Stun:
                            badIconPrefab = badStunPrefab;
                            break;
                    }
                    
                    if (badIconPrefab != null)
                    {
                        // 나쁜 손님 아이콘 프리팹 생성
                        GameObject badIcon = Instantiate(badIconPrefab, foodIconTransform.position, Quaternion.identity);
                        badIcon.transform.SetParent(slot.transform);
                        badIcon.transform.localScale = Vector3.one * 1.5f;
                        
                        // 위치 조정 (필요시)
                        RectTransform rectTransform = badIcon.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            Vector2 targetPos = foodIconTransform.GetComponent<RectTransform>().anchoredPosition;
                            targetPos.x = 230f; // X좌표만 고정
                            targetPos.y = 50f;
                            rectTransform.anchoredPosition = targetPos;
                        }
                    }
                }
                
                // 나쁜 손님 슬롯 강조 (예: 색상 변경)
                Image slotBackground = slot.GetComponent<Image>();
                if (slotBackground != null)
                {
                    slotBackground.color = new Color(1f, 0.7f, 0.7f); // 빨간색 계열로 강조
                }

                // slot의 자식 중 Bad_로 시작하는 오브젝트 찾기
                Transform badObj = null;
                foreach (Transform child in slot.transform)
                {
                    if (child.name.StartsWith("Bad_"))
                    {
                        badObj = child;
                        break;
                    }
                }

                if (badObj != null)
                {
                    Transform floor10Transform = badObj.Find("Floor10");
                    if (floor10Transform != null)
                    {
                        Text floor10Text = floor10Transform.GetComponent<Text>();
                        if (floor10Text != null)
                        {
                            int playerCount = 0;
                            Player player = FindFirstObjectByType<Player>();
                            switch (custom.badType)
                            {
                                case Custom.BadType.Dalgona: // 닭
                                    playerCount = player != null ? player.sugarCount : 0;
                                    break;
                                case Custom.BadType.Hotdog: // 개
                                    playerCount = player != null ? player.sosageCount : 0;
                                    break;
                                case Custom.BadType.Stun: // 쥐
                                    playerCount = player != null ? player.flourCount : 0;
                                    break;
                            }
                            playerCount = Mathf.Min(playerCount, 10);
                            floor10Text.text = $"{playerCount}/10";
                        }
                    }
                }
            }
            else
            {
                // 일반 손님 음식 아이콘
                Sprite foodIcon = null;
                switch (custom.RequestedFood)
                {
                    case "dalgona": foodIcon = dalgonaIcon; break;
                    case "hottuk": foodIcon = hottukIcon; break;
                    case "hotdog": foodIcon = hotdogIcon; break;
                    case "boung": foodIcon = boungIcon; break;
                }
                slot.transform.Find("FoodIcon").GetComponent<Image>().sprite = foodIcon;
            }

            // 남은 시간 텍스트 표시
            Text waitText = slot.transform.Find("WaitText")?.GetComponent<Text>();
            if (waitText != null)
            {
                float remain = Mathf.Max(0, custom.maxWaitTime - custom.waitTimer);
                waitText.text = $"{remain:F0}s";
                
                // 나쁜 손님인 경우 텍스트 색상 변경
                if (isBadCustomer)
                {
                    waitText.color = Color.red;
                }
            }

            // 슬라이더 값 설정 및 색상 변경
            Slider waitSlider = slot.transform.Find("WaitSlider")?.GetComponent<Slider>();
            if (waitSlider != null)
            {
                float normalizedTime = custom.waitTimer / custom.maxWaitTime;
                waitSlider.value = normalizedTime;
                
                // 슬라이더 색상 변경
                Image fillImage = waitSlider.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (normalizedTime < 0.33f)
                        fillImage.color = greenColor;
                    else if (normalizedTime < 0.66f)
                        fillImage.color = yellowColor;
                    else
                        fillImage.color = redColor;
                }
            }
        }

        
    }

    public void RegisterCustomer(Custom custom)
    {
        customerList.Add(custom);
        UpdateOrderList();
    }

    public void UnregisterCustomer(Custom custom)
    {
        customerList.Remove(custom);
        UpdateOrderList();
    }

    private int GetPlayerIngredientCount(Custom.BadType type)
    {
        Player player = FindFirstObjectByType<Player>();
        if (player == null) return 0;

        switch (type)
        {
            case Custom.BadType.Dalgona: return player.sosageCount;
            case Custom.BadType.Hotdog: return player.sugarCount;
            case Custom.BadType.Stun: return player.flourCount;
            default: return 0;
        }
    }

    
}