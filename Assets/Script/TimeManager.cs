using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [Header("Timer Settings")]
    [SerializeField] private float _startingTime = 60f; // Set your desired countdown time
    private float _remainingTime;
    private bool _isRunning = false;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI _timerText;

    public float RemainingTime => _remainingTime;
    public bool IsRunning => _isRunning;

    private void Start()
    {
        ResetTimer();
        StartTimer();
    }

    void Update()
    {
        if (_isRunning)
        {
            _remainingTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                TimerComplete();
            }
        }

        if (tileManager.isGoing)
        {
            _isRunning = false;
        }
        else
        {
            _isRunning = true;
        }
    }

    private void UpdateTimerDisplay()
    {
        _timerText.text = GetFormattedTime();
    }

    private void TimerComplete()
    {
        _isRunning = false;
        Debug.Log("Timer finished!");
        // Add your timer completion logic here
    }

    public void ToggleTimer()
    {
        if (_isRunning) PauseTimer();
        else StartTimer();
    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void PauseTimer()
    {
        _isRunning = false;
    }

    public void StopTimer()
    {
        _isRunning = false;
        _remainingTime = 0f;
        UpdateTimerDisplay();
    }

    public void ResetTimer()
    {
        _remainingTime = _startingTime;
        UpdateTimerDisplay();
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(_remainingTime / 60f);
        int seconds = Mathf.FloorToInt(_remainingTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
