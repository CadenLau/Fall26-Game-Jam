using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    [SerializeField] private Image timerFill;

    private UIControllerScript controller;
    private float duration;
    private float timeRemaining;
    private bool running;

    public float TimeRemaining => timeRemaining;

    private void Awake()
    {
        controller = GetComponentInParent<UIControllerScript>();
    }

    public void StartTimer(float seconds)
    {
        duration = seconds;
        timeRemaining = seconds;
        running = true;
        timerFill.fillAmount = 1f;
    }

    public void StopTimer()
    {
        running = false;
    }

    public void AddTime(float seconds)
    {
        if (running)
            SetTimeRemaining(timeRemaining + seconds);
    }

    public void RemoveTime(float seconds)
    {
        if (running)
            SetTimeRemaining(timeRemaining - seconds);
    }

    private void Update()
    {
        if (running)
            SetTimeRemaining(timeRemaining - Time.deltaTime);
    }

    private void SetTimeRemaining(float seconds)
    {
        timeRemaining = Mathf.Max(0f, seconds);
        timerFill.fillAmount = timeRemaining / duration;

        if (timeRemaining <= 0f)
        {
            running = false;
            controller.OnTimerEnd();
        }
    }
}
