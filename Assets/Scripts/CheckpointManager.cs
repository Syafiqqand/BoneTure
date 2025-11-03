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

    // --- STATE BARU ---
    // 'true' HANYA jika player sudah menginjak checkpoint terakhir
    public bool IsReadyForSummit { get; private set; } = false;

    private int currentCheckpointIndex = -1;

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
        foreach (var cp in allCheckpointsInOrder)
        {
            cp.SetActivated(false);
        }

        CurrentUsername = PlayerPrefs.GetString(CURRENT_USER_KEY);
        if (string.IsNullOrEmpty(CurrentUsername))
        {
            UpdateUiText();
            return;
        }

        StartCoroutine(ApiService.Instance.GetProgress(CurrentUsername,
            (loadedProgress) => {
                if (loadedProgress != null)
                {
                    currentCheckpointIndex = loadedProgress.CheckpointIndex;
                    TeleportPlayerToCheckpoint(currentCheckpointIndex);

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.InitializeSummitCount(loadedProgress.SummitCount);
                    }

                    // Cek apakah player terakhir save di checkpoint terakhir
                    if (currentCheckpointIndex == allCheckpointsInOrder.Length - 1)
                    {
                        IsReadyForSummit = true;
                    }
                }
                else
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.InitializeSummitCount(0);
                    }
                }
                UpdateUiText();
            },
            (error) => {
                Debug.LogError($"Gagal load data: {error}.");
                UpdateUiText();
            }
        ));
    }

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
            SaveProgressToApi();

            // --- LOGIKA BARU ---
            // Cek apakah ini adalah checkpoint TERAKHIR
            if (currentCheckpointIndex == allCheckpointsInOrder.Length - 1)
            {
                Debug.Log("Player telah mencapai checkpoint terakhir! Siap untuk Summit.");
                IsReadyForSummit = true;
            }
            // ------------------
        }
    }

    private void SaveProgressToApi()
    {
        if (string.IsNullOrEmpty(CurrentUsername)) return;

        PlayerProgress progressData = new PlayerProgress
        {
            PlayerName = CurrentUsername,
            CheckpointIndex = currentCheckpointIndex
        };

        StartCoroutine(ApiService.Instance.SaveProgress(progressData,
            (result) => { Debug.Log($"Progres (CP: {result.CheckpointIndex}) berhasil disimpan ke API!"); },
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

    // --- FUNGSI BARU ---
    // Dipanggil oleh GameManager untuk mereset status 'siap summit'
    public void AcknowledgeSummit()
    {
        IsReadyForSummit = false;
    }

    // Dipanggil oleh GameManager saat reset
    public void ResetCheckpointsToStart()
    {
        IsReadyForSummit = false; // Matikan status siap summit

        foreach (var cp in allCheckpointsInOrder)
        {
            cp.SetActivated(false);
        }

        currentCheckpointIndex = -1;
        UpdateUiText();
        SaveProgressToApi();
    }
}