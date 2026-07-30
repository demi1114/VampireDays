using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Slider hpSlider;
    public TextMeshProUGUI timeText;

    private PlayerStatus player;
    public TextMeshProUGUI visionText;

    private VisionController[] visions;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerStatus>();
        visions = FindObjectsByType<VisionController>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        if (player == null)
            return;

        hpSlider.maxValue = player.maxHP;
        hpSlider.value = player.currentHP;

        float time = GameManager.Instance.CurrentTime;

        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);

        timeText.text = $"{min:00}:{sec:00}";

        bool detected = false;

        foreach (VisionController vision in visions)
        {
            if (vision.IsPlayerVisible)
            {
                detected = true;
                break;
            }
        }

        visionText.text = detected ? "Detected" : "Hidden";
    }
}