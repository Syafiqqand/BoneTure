using UnityEngine;
using System.Collections.Generic;

public class LeaderboardTimeManager : MonoBehaviour
{
    [Header("Referensi UI")]
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform contentContainer;

    public void RefreshLeaderboard()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(ApiService.Instance.GetLeaderboardTime(
            (leaderboardData) =>
            {
                if (leaderboardData.Count == 0)
                {
                    // Tampilkan pesan "kosong" (opsional)
                    Debug.Log("Leaderboard waktu kosong");
                }

                for (int i = 0; i < leaderboardData.Count; i++)
                {
                    PlayerProgress data = leaderboardData[i];
                    GameObject rowGO = Instantiate(rowPrefab, contentContainer);

                    LeaderboardRow rowUI = rowGO.GetComponent<LeaderboardRow>();
                    rowUI.rankText.text = (i + 1).ToString();
                    rowUI.usernameText.text = data.PlayerName;

                    // --- PERBAIKAN DI SINI ---
                    // Pastikan menggunakan BestTimeInSeconds, bukan CurrentElapsedTime
                    rowUI.valueText.text = FormatTime(data.BestTimeInSeconds);
                }
            },
            (error) => { Debug.LogError("Gagal memuat leaderboard waktu: " + error); }
        ));
    }

    private string FormatTime(float timeInSeconds)
    {
        // Handle default value (999999 = belum pernah summit)
        if (timeInSeconds >= 999999)
        {
            return "--:--";
        }

        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}