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

            // 음식 아이콘
            Sprite foodIcon = null;
            switch (custom.RequestedFood)
            {
                case "dalgona": foodIcon = dalgonaIcon; break;
                case "hottuk": foodIcon = hottukIcon; break;
                case "hotdog": foodIcon = hotdogIcon; break;
                case "boung": foodIcon = boungIcon; break;
            }
            slot.transform.Find("FoodIcon").GetComponent<Image>().sprite = foodIcon;

            // 남은 시간 텍스트 표시
            Text waitText = slot.transform.Find("WaitText")?.GetComponent<Text>();
            if (waitText != null)
            {
                float remain = Mathf.Max(0, custom.maxWaitTime - custom.waitTimer);
                waitText.text = $"{remain:F0}s";
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
}