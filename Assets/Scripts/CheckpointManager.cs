using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Drag Player-mu (yang punya script PlayerMovement) ke sini")]
    [SerializeField] private PlayerMovement player;

    [Header("Daftar Checkpoint")]
    [Tooltip("Drag SEMUA object Checkpoint-mu ke sini, HARUS URUT (CP1, CP2, CP3, ...).")]
    [SerializeField] private CheckpointObject[] allCheckpointsInOrder;

    // Menyimpan index dari checkpoint TERAKHIR yang diaktifkan
    // -1 berarti belum ada checkpoint yang aktif (player akan respawn di posisi 'Awake' awal)
    private int currentCheckpointIndex = -1;

    /// <Fungsi ini dipanggil oleh CheckpointObject saat disentuh>
    public void ActivateCheckpoint(CheckpointObject checkpoint)
    {
        // 1. Cari tahu index checkpoint yang baru disentuh ini
        int foundIndex = -1;
        for (int i = 0; i < allCheckpointsInOrder.Length; i++)
        {
            if (allCheckpointsInOrder[i] == checkpoint)
            {
                foundIndex = i;
                break;
            }
        }

        // 2. Cek apakah ini checkpoint baru (bukan yang lama)
        //    (foundIndex > currentCheckpointIndex)
        if (foundIndex > currentCheckpointIndex)
        {
            Debug.Log($"Checkpoint {foundIndex} diaktifkan!");

            // 3. Update index terakhir yang aktif
            currentCheckpointIndex = foundIndex;

            // 4. Update spawn point di Player
            if (player != null)
            {
                player.UpdateSpawnPoint(checkpoint.GetRespawnPosition());
            }

            // 5. (Opsional) Nonaktifkan checkpoint sebelumnya secara visual
            if (currentCheckpointIndex > 0)
            {
                allCheckpointsInOrder[currentCheckpointIndex - 1].SetActivated(false);
            }

            // 6. (Opsional) Aktifkan checkpoint ini secara visual
            checkpoint.SetActivated(true);
        }
    }
}