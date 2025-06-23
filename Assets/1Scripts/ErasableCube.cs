using UnityEngine;

/// <summary>
/// 이 스크립트가 붙은 오브젝트는 'Player' 태그를 가진 오브젝트와 충돌 시 사라집니다.
/// </summary>
public class ErasableCube : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 마우스 커서 역할을 하는 오브젝트의 태그가 "Player"인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            // 충돌 시, 이 큐브를 비활성화시켜서 화면에서 보이지 않게 합니다.
            gameObject.SetActive(false);
            
            // 여기에 추가로 사운드 재생 등의 효과를 넣을 수도 있습니다.
            // 예: SoundManager.instance.ButtonClick(); 
        }
    }
} 