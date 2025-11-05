using UnityEngine;
using TMPro;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Referensi UI")]
    [SerializeField] private TMP_Text checkpointUiText;

    [Header("Referensi Player")]
    [SerializeField] private PlayerMovement player;

    [Header("Daftar Checkpoint")]
    [SerializeField] private CheckpointObject[] allCheckpointsInOrder;

    private const string CURRENT_USER_KEY = "CurrentPlayer";

    public string CurrentUsername { get; private set; }
    private int currentCheckpointIndex = -1;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        foreach (var cp in allCheckpointsInOrder)
        {
            cp.SetActivated(false);
        }

        CurrentUsername = PlayerPrefs.GetString(CURRENT_USER_KEY);
        if (string.IsNullOrEmpty(CurrentUsername))
        {
            Debug.Log("Tidak ada username! Memulai game tanpa progress.");
            UpdateUiText();

            // --- PERBAIKAN BUG ---
            // Player baru, paksa timer ke "WaitingToStart"
            GameManager.Instance.ResetTimerAndStartWaiting();
            // -------------------
            return;
        }

        StartCoroutine(ApiService.Instance.GetProgress(CurrentUsername,
            (loadedProgress) => {
                if (loadedProgress != null)
                {
                    // Player ditemukan di DB
                    currentCheckpointIndex = loadedProgress.CheckpointIndex;
                    TeleportPlayerToCheckpoint(currentCheckpointIndex);
                    GameManager.Instance.InitializeSummitCount(loadedProgress.SummitCount);

                    // Panggil 'InitializeTimer' (ini sudah benar)
                    GameManager.Instance.InitializeTimer(loadedProgress.CurrentElapsedTime);

                    if (currentCheckpointIndex == allCheckpointsInOrder.Length - 1)
                    {
                        GameManager.Instance.SetReadyForSummit(true);
                    }
                }
                else
                {
                    // Player TIDAK ditemukan di DB (player baru)
                    GameManager.Instance.InitializeSummitCount(0);

                    // --- PERBAIKAN BUG ---
                    // Player baru, paksa timer ke "WaitingToStart"
                    GameManager.Instance.ResetTimerAndStartWaiting();
                    // -------------------
                }
                UpdateUiText();
            },
            (error) => {
                // Terjadi error API
                Debug.LogError($"Gagal load data: {error}. Memulai game tanpa progress.");
                UpdateUiText();

                // --- PERBAIKAN BUG ---
                // Gagal load, paksa timer ke "WaitingToStart"
                GameManager.Instance.ResetTimerAndStartWaiting();
                // -------------------
            }
        ));
    }

    // ... (Sisa script CheckpointManager.cs TIDAK BERUBAH) ...
    // ... (ActivateCheckpoint, SaveProgressToApi, UpdateUiText, dll...) ...

    public void ActivateCheckpoint(CheckpointObject checkpoint)
    {
        int foundIndex = -1;
        for (int i = 0; i < allCheckpointsInOrder.Length; i++)
        {
            if (allCheckpointsInOrder[i] == checkpoint)
            {
                foundIndex = i;
                break;
            }
        }

        bool isNextCheckpoint = (foundIndex == currentCheckpointIndex + 1);

        if (isNextCheckpoint)
        {
            if (currentCheckpointIndex >= 0 && currentCheckpointIndex < allCheckpointsInOrder.Length)
            {
                allCheckpointsInOrder[currentCheckpointIndex].SetActivated(false);
            }

            currentCheckpointIndex = foundIndex;
            player.UpdateSpawnPoint(checkpoint.GetRespawnPosition());
            checkpoint.SetActivated(true);
            UpdateUiText();

            if (currentCheckpointIndex == allCheckpointsInOrder.Length - 1)
            {
                GameManager.Instance.SetReadyForSummit(true);
            }

            SaveProgressToApi();
        }
    }

    public void SaveProgressToApi()
    {
        if (string.IsNullOrEmpty(CurrentUsername)) return;

        // Ambil waktu saat ini dari GameManager (yang mengambil dari TimeManager)
        float currentTime = GameManager.Instance.GetCurrentTime();

        PlayerProgress progressData = new PlayerProgress
        {
            PlayerName = CurrentUsername,
            CheckpointIndex = currentCheckpointIndex,
            CurrentElapsedTime = (float)currentTime
        };

        StartCoroutine(ApiService.Instance.SaveProgress(progressData,
            (result) => { Debug.Log($"Progres (CP: {result.CheckpointIndex}, Time: {result.CurrentElapsedTime}) berhasil disimpan!"); },
            (error) => { Debug.LogError($"Gagal menyimpan progres ke API: {error}"); }
        ));
    }

    private void UpdateUiText()
    {
        if (checkpointUiText != null)
        {
            checkpointUiText.text = $"Checkpoint: {currentCheckpointIndex + 1}";
        }
    }

    private void TeleportPlayerToCheckpoint(int index)
    {
        if (index >= 0 && index < allCheckpointsInOrder.Length)
        {
            Vector3 spawnPos = allCheckpointsInOrder[index].GetRespawnPosition();
            player.UpdateSpawnPoint(spawnPos);
            player.Respawn();
            allCheckpointsInOrder[index].SetActivated(true);
        }
    }

    public void ResetCheckpointsToStart()
    {
        GameManager.Instance.SetReadyForSummit(false);

        foreach (var cp in allCheckpointsInOrder)
        {
            cp.SetActivated(false);
        }

        currentCheckpointIndex = -1;
        UpdateUiText();

        SaveProgressToApi();
    }
}