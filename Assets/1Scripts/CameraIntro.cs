using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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

    Player player;
    public Text countdownText;

    void OnEnable()
    {
        
        StartCoroutine(WaitAndStartIntro());
    }

    private IEnumerator WaitAndStartIntro()
    {
        yield return null;
        Debug.Log("✅ CameraIntro Init 시작됨");

        // Player와 GameManager가 생성될 때까지 대기
        while (FindFirstObjectByType<Player>() == null || FindFirstObjectByType<GameManager>() == null)
            yield return null;

        player = FindFirstObjectByType<Player>();
        if (player != null)
            player.isMove = false;
        transform.LookAt(centerPoint);

        // 기존 인트로 로직 실행
        timer = 0f;
        isTouring = true;
    }

    void Update()
    {
        if (!isTouring) return;
        Debug.Log($"[CameraIntro] 회전 중, 시간: {timer}");

        timer += Time.deltaTime;

        if (timer >= duration)
        {
            isTouring = false;
            StartCoroutine(ShowCountdownAndStartGame());
            return;
        }

        float angle = rotationSpeed * timer;
        Vector3 offset = new Vector3(
            Mathf.Sin(angle * Mathf.Deg2Rad),
            0.5f,
            Mathf.Cos(angle * Mathf.Deg2Rad)
        ) * rotationRadius;

        transform.position = centerPoint.position + offset;
        transform.LookAt(centerPoint);

        if (player != null)
            player.isMove = false;
    }

    private IEnumerator ShowCountdownAndStartGame()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        if (player != null)
            player.isMove = false;

        string[] texts = { "3", "2", "1", "Start!" };
        for (int i = 0; i < texts.Length; i++)
        {
            if (countdownText != null)
                countdownText.text = texts[i];
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (player != null)
            player.isMove = true;

        FindFirstObjectByType<GameManager>().StartGame();
    }
}
