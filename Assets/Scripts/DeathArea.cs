using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeathArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. Cek apakah yang menyentuh adalah Player
        //    (Kita akan set Tag "Player" di langkah setup)
        if (other.CompareTag("Player"))
        {
            // 2. Jika benar Player, cari script PlayerMovement di object itu
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            // 3. Jika script-nya ada, panggil fungsi Respawn()
            if (player != null)
            {
                player.Respawn(); // Kita akan buat fungsi ini di PlayerMovement
            }
        }
    }
}