using UnityEngine;
using System.Collections;

public class CameraIntro : MonoBehaviour
{
    [Header("🎯 회전 중심 및 거리 설정")]
    public Transform centerPoint;      // 📌 카메라가 바라볼 중심 위치 (맵 중앙이나 중요한 오브젝트)
    public float rotationRadius = 10f; // 📏 중심으로부터의 거리 (멀리서 볼수록 커짐)

    [Header("⚙️ 회전 설정")]
    public float rotationSpeed = 70f;  // 🔄 카메라 회전 속도 (값이 크면 빠르게 돎)
    public float duration = 4f;        // ⏱ 회전 지속 시간 (초 단위, 이 시간이 지나면 멈춤)

    private float timer = 0f;          // ⏲ 경과 시간
    private bool isTouring = true;     // 🎬 인트로 회전 중 여부 (false가 되면 멈춤)

    void Start()
    {
        // 🎯 시작 시 중심점을 바라보도록 방향 고정
        transform.LookAt(centerPoint);
    }

    void Update()
    {
        if (!isTouring) return;

        // ⏱ 시간 누적
        timer += Time.deltaTime;

        // ⏹ 설정된 시간이 지나면 회전 종료
        if (timer >= duration)
        {
            isTouring = false;

            // 🔔 회전 종료 시 GameManager에 알림 → 게임 시작
            FindFirstObjectByType<GameManager>().StartGame();
            return;
        }

        // 🌀 카메라를 중심을 기준으로 회전시키는 로직
        float angle = rotationSpeed * timer; // ⬅ 시간에 따라 회전 각도 증가
        Vector3 offset = new Vector3(
            Mathf.Sin(angle * Mathf.Deg2Rad), // X축 방향 (좌우 원형 회전)
            0.5f,                             // Y축 높이 (고정값, 적당히 떠 있게)
            Mathf.Cos(angle * Mathf.Deg2Rad)  // Z축 방향 (앞뒤 원형 회전)
        ) * rotationRadius;

        // 📍 카메라 위치 = 중심 위치 + 회전 오프셋
        transform.position = centerPoint.position + offset;

        // 👁 중심 바라보기 유지
        transform.LookAt(centerPoint);
    }
}
