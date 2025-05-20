using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 게임의 UI 요소들을 관리하는 HUD(Heads-Up Display) 클래스
/// </summary>
public class Hud : MonoBehaviour
{
    /// <summary>
    /// HUD에 표시할 정보의 종류를 정의하는 열거형
    /// </summary>
    public enum InfoType { Customer, Level, ClearCustom, Time, Point, Health, 
    Sugar, Flour, Sosage, Pot, 
    Dalgona, Hotdog, Hottuk, Boung}

    public InfoType type;  // 현재 HUD가 표시할 정보의 종류

    Text myText;      // 텍스트를 표시할 UI 컴포넌트
    Slider mySlider;  // 슬라이더를 표시할 UI 컴포넌트
    void Awake()
    {
        // 필요한 UI 컴포넌트들을 가져옴
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    private void LateUpdate()
    {
        // HUD 타입에 따라 다른 정보를 표시
        switch (type)
        {
            case InfoType.Customer:
                // 현재 클리어한 손님 수와 다음 레벨업까지 필요한 손님 수를 계산하여 슬라이더 값 설정
                float curCustomer = GameManager.instance.clearedCustomerCount;
                float maxCustomer = GameManager.instance.nextCustomer[Mathf.Min(GameManager.instance.level, GameManager.instance.nextCustomer.Length - 1)];
                mySlider.value = curCustomer / maxCustomer;
                break;

            case InfoType.Level:
                // 현재 플레이어의 레벨을 표시
                myText.text = $"Lv. {GameManager.instance.level}";
                break;

            case InfoType.ClearCustom:
                // 현재 클리어한 손님 수와 다음 레벨업까지 필요한 손님 수를 표시
                int cur = GameManager.instance.clearedCustomerCount;
                int max = GameManager.instance.nextCustomer[Mathf.Min(GameManager.instance.level, GameManager.instance.nextCustomer.Length - 1)];
                myText.text = $"{cur} / {max}";
                break;

            case InfoType.Time:
                // 남은 게임 시간을 분:초 형식으로 표시
                float remainTime = GameManager.instance.maxGameTime - GameManager.instance.gameTime;    // 남은 시간
                int min = Mathf.FloorToInt(remainTime / 60);        // 분 계산
                int sec = Mathf.FloorToInt(remainTime % 60);        // 초 계산
                myText.text = string.Format("{0:D2}:{1:D2}", min, sec);         // D2는 두 자리 숫자로 표시
                break;

            case InfoType.Point:
                // 현재 플레이어의 포인트를 표시
                myText.text = $"{GameManager.instance.player.Point:N0}";  // N0는 천 단위 구분자 표시
                break;

            case InfoType.Health:
                // 플레이어의 체력을 표시 (필요한 경우)
                break;
                
            case InfoType.Sugar:
                myText.text = GameManager.instance.player.sugarCount.ToString();
                break;

            case InfoType.Flour:
                myText.text = GameManager.instance.player.flourCount.ToString();
                break;

            case InfoType.Sosage:
                myText.text = GameManager.instance.player.sosageCount.ToString();
                break;

            case InfoType.Pot:
                myText.text = GameManager.instance.player.potCount.ToString();
                break;
            // =========================== 음식 =======================
            case InfoType.Dalgona:
                myText.text = GameManager.instance.player.dalgonaCount.ToString();
                break;

            case InfoType.Hotdog:
                myText.text = GameManager.instance.player.hotdogCount.ToString();
                break;

            case InfoType.Hottuk:
                myText.text = GameManager.instance.player.hottukCount.ToString();
                break;

            case InfoType.Boung:
                myText.text = GameManager.instance.player.boungCount.ToString();
                break;
        }
    }
}
