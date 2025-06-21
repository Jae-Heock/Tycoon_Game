using UnityEngine;

public class CursorCollisionTracker : MonoBehaviour
{
    // 패널티는 머무르는 내내 감지해야 하므로 OnTriggerStay 사용
    private void OnTriggerStay(Collider other)
    {
        if (DalgonaGameManager.Instance != null && DalgonaGameManager.Instance.isGameActive)
        {
            if (other.CompareTag("InnerPenalty") || other.CompareTag("OuterPenalty"))
            {
                // 지속적으로 깎이는 패널티 호출
                DalgonaGameManager.Instance.ApplyContinuousPenalty();
            }
        }
    }

    // 체크포인트와 '최초 진입' 패널티는 진입하는 순간 한 번만 감지
    void OnTriggerEnter(Collider other)
    {
        if (DalgonaGameManager.Instance != null && DalgonaGameManager.Instance.isGameActive)
        {
            if (other.CompareTag("InnerPenalty") || other.CompareTag("OuterPenalty"))
            {
                // 한번만 깎이는 '최초 진입' 패널티 호출
                DalgonaGameManager.Instance.ApplyInitialPenalty();
            }
            else if (other.CompareTag("Checkpoint"))
            {
                DalgonaGameManager.Instance.CheckpointHit(other.gameObject);
            }
        }
    }
}
