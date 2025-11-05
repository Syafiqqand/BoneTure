using UnityEngine;
using TMPro;

[RequireComponent(typeof(GameManager))]
public class TimeManager : MonoBehaviour
{
    [Header("Referensi UI")]
    [SerializeField] private TMP_Text timerUiText;

    private float elapsedTime = 0f;

    private enum TimerState { Stopped, WaitingToStart, Running }
    private TimerState currentState = TimerState.Stopped;

    void Start()
    {
        // Jangan mulai otomatis, tunggu perintah dari GameManager/CheckpointManager
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (currentState == TimerState.WaitingToStart)
        {
            if (GameInput.Instance.GetMovementVectorNormalized().magnitude > 0.1f ||
                GameInput.Instance.IsJumpPressed())
            {
                currentState = TimerState.Running; // Mulai timer
                Debug.Log("Timer Dimulai!");
            }
        }

        if (currentState == TimerState.Running)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    // --- FUNGSI INI DIPERBAIKI ---
    /// <summary>
    /// Dipanggil oleh CheckpointManager saat me-load data
    /// </summary>
    public void InitializeTimer(float loadedTime)
    {
        elapsedTime = loadedTime;

        // --- INI PERBAIKANNYA ---
        // SETIAP KALI game di-load, timer harus 'WaitingToStart',
        // tidak peduli waktunya 0 atau tidak.
        currentState = TimerState.WaitingToStart;
        // -------------------------

        UpdateTimerDisplay();
        Debug.Log($"Timer di-load ke: {elapsedTime}. Status: {currentState}");
    }
    // --- AKHIR PERBAIKAN ---

    /// <summary>
    /// Dipanggil oleh GameManager saat summit
    /// </summary>
    public void StopTimerAndResetDisplay()
    {
        currentState = TimerState.Stopped; // HANYA di sini kita set 'Stopped'
        elapsedTime = 0f;
        UpdateTimerDisplay();
        Debug.Log("Timer Berhenti dan di-reset ke 00:00");
    }

    /// <summary>
    /// Dipanggil oleh GameManager saat reset ke base
    /// </summary>
    public void ResetTimerAndStartWaiting()
    {
        elapsedTime = 0f;
        currentState = TimerState.WaitingToStart; // Siap jalan
        UpdateTimerDisplay();
        Debug.Log("Timer di-reset dan SIAP.");
    }

    /// <summary>
    /// Dipanggil oleh CheckpointManager untuk di-save ke API
    /// </summary>
    public float GetCurrentTime()
    {
        return elapsedTime;
    }

    private void UpdateTimerDisplay()
    {
        if (timerUiText == null) return;

        float minutes = Mathf.FloorToInt(elapsedTime / 60);
        float seconds = Mathf.FloorToInt(elapsedTime % 60);

        timerUiText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }
}