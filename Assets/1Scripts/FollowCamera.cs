using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;     // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0f, 10f, -10f);  // 카메라 위치 오프셋
    public float followSpeed = 5f; // 따라가는 속도
    public float sensitivity = 5f;
    public float minDistance = 3f; // 최소 줌 거리
    public float maxDistance = 20f; // 최대 줌 거리
    public bool allowSpaceLock = false;     // 인트로중엔 움직임 금지지
    private float xRotation = 0f;
    private float yRotation = 45f; // 위에서 내려다보는 각도
    private bool isRotating = false;

    private Vector3 initialOffset;
    private float initialXRotation;
    private float initialYRotation;
    private bool isLocked = false;
    private CameraIntro cameraIntro;

    private void Start()
    {
        initialOffset = new Vector3(0f, 7f, -8f);  // 오프셋셋
        initialXRotation = 45f;  //X 축
        initialYRotation = 45f;  // y축
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (cameraIntro != null && cameraIntro.isTouring) return;

        // 스페이스 키로 고정/해제
        if (allowSpaceLock &&Input.GetKeyDown(KeyCode.Space))
        {
            isLocked = !isLocked;
            if (isLocked)
            {
                offset = initialOffset;
                xRotation = initialXRotation;
                yRotation = initialYRotation;
            }
        }

        Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0);
        Vector3 desiredOffset = rotation * new Vector3(0, 0, -offset.magnitude);
        Vector3 targetPosition = target.position + desiredOffset;

        if (isLocked)
        {
            // 고정 모드: 즉시 위치/회전 적용
            transform.position = targetPosition;
            transform.LookAt(target.position);
            return;
        }

        // 마우스 휠로 줌 인/아웃
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float distance = offset.magnitude - scroll * 10f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        offset = offset.normalized * distance;

        // 마우스 좌클릭 누르고 있을 때만 회전
        if (Input.GetMouseButtonDown(0)) isRotating = true;
        if (Input.GetMouseButtonUp(0)) isRotating = false;

        if (isRotating)
        {
            xRotation += Input.GetAxis("Mouse X") * sensitivity;
            yRotation -= Input.GetAxis("Mouse Y") * sensitivity;
            yRotation = Mathf.Clamp(yRotation, 10, 80);
        }

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position);
    }
}