using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;     // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0f, 10f, -10f);  // 카메라 위치 오프셋

    public float followSpeed = 5f; // 따라가는 속도

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
