using UnityEngine;
using UnityEngine.UI;

public class CustomTrash : MonoBehaviour
{
    Player player;
    private bool isPlayerNearby = false;
    private bool isCleaning = false;
    private float holdTime = 0f;
    private float requiredHoldTime = 1f;

    [Header("UI")]
    public Slider slider;          // World Space 슬라이더 (자식 오브젝트)
    public Canvas sliderCanvas;    // 슬라이더 캔버스

    private void Start()
    {
        if (slider != null)
            slider.value = 0f;

        if (sliderCanvas != null)
            sliderCanvas.enabled = false; // 처음엔 안 보이게
    }

    private void Update()
    {
        if (isPlayerNearby && player != null)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isCleaning)
                {
                    isCleaning = true;
                    if (sliderCanvas != null) sliderCanvas.enabled = true;
                }

                holdTime += Time.deltaTime;
                if (slider != null)
                    slider.value = holdTime / requiredHoldTime;

                if (holdTime >= requiredHoldTime)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                ResetCleaning();
            }
        }
        else
        {
            ResetCleaning();
        }
    }

    private void ResetCleaning()
    {
        if (isCleaning)
        {
            isCleaning = false;
            holdTime = 0f;

            if (slider != null)
                slider.value = 0f;

            if (sliderCanvas != null)
                sliderCanvas.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = other.GetComponent<Player>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            player = null;
        }
    }
}
