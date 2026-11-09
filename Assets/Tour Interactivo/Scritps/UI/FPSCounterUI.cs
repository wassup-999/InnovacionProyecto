using UnityEngine;
using TMPro;
public class FPSCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;

    void Start()
    {
        timeLeft = updateInterval;
    }

    
    void Update()
    {
        UpdateFPS();
    }

    private void UpdateFPS()
    {
        timeLeft -= Time.unscaledDeltaTime;
        accum += Time.unscaledDeltaTime;
        frames++;

        if(timeLeft <= 0f)
        {
            float fps = frames / accum;
            fpsText.text = $"{fps:0.} FPS";
            fpsText.color = fps >= 30 ? Color.green : (fps >= 10 ? Color.yellow : Color.red);
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
}
