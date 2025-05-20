using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    public RectTransform handTransform;         // 시곗바늘 이미지
    public float totalGameTime = 300f;          // 총 게임 시간 (초)

    private void Update()
    {
        float elapsed = GameManager.instance.gameTime;
        float ratio = Mathf.Clamp01(elapsed / totalGameTime); // 0~1 비율
        float angle = -360f * ratio;                          // 시계 방향 회전 (반시계면 +360f)

        handTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
