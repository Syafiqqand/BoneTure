using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Pause")]
    [SerializeField] private GameObject resumePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button backToMainMenuButton;

    [Header("Fitur Baru (Summit & Reset)")]
    [SerializeField] private TMP_Text summitUiText;
    [SerializeField] private Transform basecampSpawnPoint;

    [Header("Pengaturan")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [SerializeField] private float resetDelay = 2.0f;
    private bool isResetting = false;
    private bool isPaused = false;
    private int currentSummitCount = 0;

    private TimeManager timeManager;
    private bool isReadyForSummit = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        timeManager = GetComponent<TimeManager>();
    }

    private void Start()
    {
        resumePanel.SetActive(false);
        resumeButton.onClick.AddListener(ResumeGame);
        backToMainMenuButton.onClick.AddListener(GoToMainMenu);
        UpdateSummitUi();
    }

    private void Update()
    {
        if (GameInput.Instance.IsPausePressed())
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // ... (PauseGame, ResumeGame, GoToMainMenu, InitializeSummitCount biarkan sama) ...
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        resumePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        resumePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void GoToMainMenu()
    {
        Debug.Log("Menyimpan progres sebelum kembali ke Main Menu...");
        CheckpointManager.Instance.SaveProgressToApi();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void InitializeSummitCount(int countFromApi)
    {
        currentSummitCount = countFromApi;
        UpdateSummitUi();
    }
    // ... (Akhir dari fungsi yang tidak berubah) ...


    // --- FUNGSI INI DIMODIFIKASI (SANGAT PENTING) ---
    public void PlayerReachedSummit()
    {
        if (!this.isReadyForSummit)
        {
            Debug.Log("Player menyentuh summit, tapi belum menyelesaikan semua checkpoint.");
            return;
        }

        string username = CheckpointManager.Instance.CurrentUsername;
        if (string.IsNullOrEmpty(username)) return;

        Debug.Log("Player reached summit! Menyimpan ke API...");

        this.SetReadyForSummit(false);

        // 1. Ambil waktu final DARI TimeManager
        float finalTime = timeManager.GetCurrentTime();

        // 2. Hentikan timer LOKAL dan reset display ke 00:00
        timeManager.StopTimerAndResetDisplay();

        // 3. Kirim 'finalTime' ke API
        StartCoroutine(ApiService.Instance.IncrementSummit(username, finalTime,
            (updatedProgress) => {
                // (Callback sukses dari API)
                currentSummitCount = updatedProgress.SummitCount;
                UpdateSummitUi();
                // Kita tidak melakukan apa-apa lagi. Timer sudah stop.
            },
            (error) => {
                Debug.LogError("Gagal menyimpan summit ke API: " + error);
            }
        ));
    }

    // --- FUNGSI INI DIMODIFIKASI ---
    public void TriggerResetSequence()
    {
        if (isResetting) return;
        StartCoroutine(ResetSequenceCoroutine());
    }

    private IEnumerator ResetSequenceCoroutine()
    {
        isResetting = true;
        Debug.Log("Player menyentuh area reset. Teleport dalam 2 detik...");

        // 1. Beritahu TimeManager untuk SIAP JALAN (State = WaitingToStart)
        timeManager.ResetTimerAndStartWaiting();

        // 2. Reset checkpoint di API
        CheckpointManager.Instance.ResetCheckpointsToStart();

        yield return new WaitForSeconds(resetDelay);

        Debug.Log("Resetting now!");

        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null && basecampSpawnPoint != null)
        {
            player.UpdateSpawnPoint(basecampSpawnPoint.position);
            player.Respawn();
        }

        isResetting = false;
    }
    // ---------------------------------

    private void UpdateSummitUi()
    {
        if (summitUiText != null)
        {
            summitUiText.text = $"Summit: {currentSummitCount}";
        }
    }

    public void SetReadyForSummit(bool isReady)
    {
        this.isReadyForSummit = isReady;
    }

    // --- FUNGSI 'PASS-THROUGH' INI DIMODIFIKASI ---

    public void InitializeTimer(float time)
    {
        timeManager.InitializeTimer(time);
    }

    public float GetCurrentTime()
    {
        return timeManager.GetCurrentTime();
    }

    // --- TAMBAHKAN FUNGSI BARU INI ---
    /// <summary>
    /// Dipanggil oleh CheckpointManager saat player baru / load gagal
    /// untuk memaksa timer ke state "WaitingToStart".
    /// </summary>
    public void ResetTimerAndStartWaiting()
    {
        if (timeManager != null)
        {
            timeManager.ResetTimerAndStartWaiting();
        }
    }

    // Fungsi ini sekarang digantikan oleh TimeManager
    // public void ResetTimer(float time, bool autoStart)
    // { ... }
}