using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SummitTrigger : MonoBehaviour
{
    // Cooldown agar tidak spam saat player berdiri diam
    private bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        // Jika player masuk DAN trigger-nya belum aktif
        if (other.CompareTag("Player") && !isPlayerInside)
        {
            isPlayerInside = true; // Tandai player ada di dalam
            GameManager.Instance.PlayerReachedSummit();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Saat player keluar, reset triggernya
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
}