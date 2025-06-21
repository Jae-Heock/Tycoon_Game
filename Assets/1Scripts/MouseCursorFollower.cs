using UnityEngine;

public class MouseCursorFollower : MonoBehaviour
{
    public float distanceFromCamera = 10f;

    void Update()
    {
        if (DalgonaGameManager.Instance != null && DalgonaGameManager.Instance.isGameActive)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = distanceFromCamera;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = worldPos;
        }
    }
}
