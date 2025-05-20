using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
    public GameObject targetObject;  // 이건 큐브 오브젝트를 할당
    private Outline outline;

    void Start()
    {
        if (targetObject != null)
        {
            outline = targetObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = targetObject.AddComponent<Outline>();
            }

            outline.OutlineColor = Color.red;
            outline.OutlineWidth = 7f;
            outline.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && outline != null)
        {
            outline.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && outline != null)
        {
            outline.enabled = false;
        }
    }
}
