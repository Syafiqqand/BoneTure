using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointObject : MonoBehaviour
{
    [Header("Pengaturan")]
    [Tooltip("Titik aktual di mana player akan respawn. " +
             "Buat Empty GameObject sebagai child dan posisikan di atas plane ini.")]
    [SerializeField] private Transform respawnMarker;

    [Header("Feedback Visual (Opsional)")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;

    // Referensi ke manager
    private CheckpointManager manager;
    private Renderer objRenderer;
    private bool hasBeenTriggered = false; // Mencegah spam panggil manager

    private void Awake()
    {
        // Cari Manager secara otomatis saat game dimulai
        manager = FindFirstObjectByType<CheckpointManager>(); 
        if (manager == null)
        {
            Debug.LogError("Error: Tidak ada CheckpointManager di scene!");
        }

        // Setup untuk feedback visual (opsional)
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null && inactiveMaterial != null)
        {
            objRenderer.material = inactiveMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hanya panggil manager JIKA belum ter-trigger DAN yang menyentuh adalah Player
        if (!hasBeenTriggered && other.CompareTag("Player"))
        {
            if (manager != null)
            {
                // Laporkan diri ke manager
                manager.ActivateCheckpoint(this);

                // Tandai sebagai "sudah pernah disentuh"
                // Logika "apakah ini checkpoint baru?" diurus oleh manager
                hasBeenTriggered = true;
            }
        }
    }

    /// <Fungsi helper untuk memberitahu manager di mana titik respawn-nya>
    public Vector3 GetRespawnPosition()
    {
        if (respawnMarker == null)
        {
            Debug.LogError("Respawn Marker di checkpoint " + name + " belum di-set! " +
                           "Player akan respawn di tengah checkpoint.");
            return transform.position;
        }
        return respawnMarker.position;
    }

    /// <Fungsi ini dipanggil oleh Manager untuk mengubah visual>
    public void SetActivated(bool isActive)
    {
        if (objRenderer == null) return;

        if (isActive && activeMaterial != null)
        {
            objRenderer.material = activeMaterial;
        }
        else if (!isActive && inactiveMaterial != null)
        {
            objRenderer.material = inactiveMaterial;
        }
    }
}