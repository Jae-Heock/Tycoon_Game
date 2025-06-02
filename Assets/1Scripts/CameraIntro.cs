using UnityEngine;
using System.Collections;

public class CameraIntro : MonoBehaviour
{
    public Transform centerPoint;      // 중심 위치 (맵 중앙)
    public float rotationRadius = 10f; // 카메라 거리
    public float rotationSpeed = 70f;  // 회전 속도
    public float duration = 4f;        // 몇 초 동안 회전할지

    private float timer = 0f;
    private bool isTouring = true;

    void Start()
    {
        // 시작 시 회전 위치 설정
        transform.LookAt(centerPoint);
    }

    void Update()
    {
        if (!isTouring) return;

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            isTouring = false;
            // 회전 끝났으니 게임 시작 (예: 플레이어 카메라로 전환)
            FindFirstObjectByType<GameManager>()?.StartGame();
            return;
        }

        // 회전
        float angle = rotationSpeed * timer;
        Vector3 offset = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0.5f, Mathf.Cos(angle * Mathf.Deg2Rad)) * rotationRadius;
        transform.position = centerPoint.position + offset;
        transform.LookAt(centerPoint);
    }
}
