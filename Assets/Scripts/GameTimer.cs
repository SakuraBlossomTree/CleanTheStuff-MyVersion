using UnityEngine;
using UnityEngine.UI;
using System;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public bool useRandomTime = true;
    public float levelTime = 45f;
    public bool isRunning = false;

    private float timer;

    [Header("UI (Optional)")]
    public Text timerText;
    public Action OnTimerEnd;

    // CHANGED: Start() → Awake()
    // Awake() ALWAYS runs before any Start() method
    void Awake()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;

        if (timerText != null)
            timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

        if (timer <= 0f)
        {
            isRunning = false;
            timer = 0f;
            OnTimerEnd?.Invoke();
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        timer = levelTime;
        isRunning = false;
    }
}