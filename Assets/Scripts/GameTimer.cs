using UnityEngine;
using UnityEngine.UI;   // Only if you want to show the timer on screen
using System;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public bool useRandomTime = true;   // Toggle random vs fixed
    public float levelTime = 45f;       // Fixed time if random disabled

    public bool isRunning = false;

    private float timer;

    [Header("UI (Optional)")]
    public Text timerText; // Assign in inspector if you want to show countdown

    public Action OnTimerEnd;

    void Start()
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
