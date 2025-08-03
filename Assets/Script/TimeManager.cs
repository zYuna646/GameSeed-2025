using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Start is called before the first frame update
    private float _elapsedTime = 0f;
    private bool _isRunning = false;
    public TextMeshProUGUI _textMeshPro;

    public float CurrentTime => _elapsedTime;
    public bool IsRunning => _isRunning;

    private void Start()
    {
        StartTimer();
    }
    void Update()
    {
        if (_isRunning)
        {
            _elapsedTime += Time.deltaTime;
        }
        _textMeshPro.text = GetFormattedTime();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsRunning)
                PauseTimer();
            else
                StartTimer();
        }
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
        _elapsedTime = 0f;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
