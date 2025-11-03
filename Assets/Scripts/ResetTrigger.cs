using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResetTrigger : MonoBehaviour
{
    private bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerInside)
        {
            isPlayerInside = true;
            // Panggil fungsi yang memulai Coroutine
            GameManager.Instance.TriggerResetSequence();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
}