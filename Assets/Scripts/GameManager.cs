using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // <-- WAJIB: Untuk Coroutine (delay)

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

    // --- STATE BARU ---
    [SerializeField] private float resetDelay = 2.0f; // Delay 2 detik
    private bool isResetting = false; // Mencegah spam reset
    // ------------------

    private bool isPaused = false;
    private int currentSummitCount = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void InitializeSummitCount(int countFromApi)
    {
        currentSummitCount = countFromApi;
        UpdateSummitUi();
    }

    // --- LOGIKA SUMMIT DIPERBARUI ---
    public void PlayerReachedSummit()
    {
        // Cek ke CheckpointManager apakah player sudah siap
        if (!CheckpointManager.Instance.IsReadyForSummit)
        {
            Debug.Log("Player menyentuh summit, tapi belum menyelesaikan semua checkpoint.");
            return; // Tidak melakukan apa-apa
        }

        string username = CheckpointManager.Instance.CurrentUsername;
        if (string.IsNullOrEmpty(username)) return;

        Debug.Log("Player reached summit! Menyimpan ke API...");

        // Matikan status 'siap' agar tidak di-spam
        CheckpointManager.Instance.AcknowledgeSummit();

        StartCoroutine(ApiService.Instance.IncrementSummit(username,
            (updatedProgress) => {
                // Sukses! Update UI
                currentSummitCount = updatedProgress.SummitCount;
                UpdateSummitUi();
            },
            (error) => {
                Debug.LogError("Gagal menyimpan summit ke API: " + error);
                // (Harus bagaimana? Kembalikan status 'isReadyForSummit'?)
                // Untuk saat ini, biarkan.
            }
        ));
    }

    // --- LOGIKA RESET DIPERBARUI ---
    // Fungsi ini dipanggil oleh ResetTrigger
    public void TriggerResetSequence()
    {
        // Jika sedang dalam proses reset, jangan lakukan apa-apa
        if (isResetting) return;

        StartCoroutine(ResetSequenceCoroutine());
    }

    private IEnumerator ResetSequenceCoroutine()
    {
        isResetting = true;
        Debug.Log("Player menyentuh area reset. Teleport dalam 2 detik...");

        // Tampilkan UI "Loading..." atau "Resetting..." di sini (opsional)

        // Tunggu 2 detik
        yield return new WaitForSeconds(resetDelay);

        Debug.Log("Resetting now!");

        // 1. Reset Checkpoint (panggil CheckpointManager)
        CheckpointManager.Instance.ResetCheckpointsToStart();

        // 2. Teleport Player ke Basecamp
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null && basecampSpawnPoint != null)
        {
            player.UpdateSpawnPoint(basecampSpawnPoint.position);
            player.Respawn();
        }

        isResetting = false; // Siap untuk di-reset lagi
    }
    // ---------------------------------

    private void UpdateSummitUi()
    {
        if (summitUiText != null)
        {
            summitUiText.text = $"Summit: {currentSummitCount}";
        }
    }
}