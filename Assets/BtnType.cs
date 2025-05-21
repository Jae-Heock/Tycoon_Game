using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BtnType : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BTNType currentType;
    public Transform buttonScale;
    Vector3 defaultScale;

    private void Start()
    {
        defaultScale = buttonScale.localScale;
    }

    public void OnBtnClick()
  {
     switch(currentType)
     {      
        case BTNType.New:
           Debug.Log("새게임");
           break;
        case BTNType.Continue:
           Debug.Log("이어하기");
           break;
        case BTNType.Option:
            Debug.Log("설정");
            break;
        case BTNType.Sound:
            Debug.Log("소리");
            break;
        case BTNType.Quit:
            Debug.Log("게임종료");
            break;
        } 
  }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonScale.localScale = defaultScale * 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonScale.localScale = defaultScale;
    }
}
