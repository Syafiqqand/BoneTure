using UnityEngine;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Referensi UI")]
    [SerializeField] private GameObject rowPrefab; // Drag RowPrefab-mu ke sini
    [SerializeField] private Transform contentContainer; // Drag 'Content' (dari ScrollView) ke sini

    // Dipanggil oleh MainMenuManager saat tab ini diklik
    public void RefreshLeaderboard()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(ApiService.Instance.GetLeaderboard(
            (leaderboardData) =>
            {
                if (leaderboardData.Count == 0)
                {
                    // Tampilkan pesan "kosong" (opsional)
                }

                for (int i = 0; i < leaderboardData.Count; i++)
                {
                    PlayerProgress data = leaderboardData[i];
                    GameObject rowGO = Instantiate(rowPrefab, contentContainer);

                    LeaderboardRow rowUI = rowGO.GetComponent<LeaderboardRow>();
                    rowUI.rankText.text = (i + 1).ToString();
                    rowUI.usernameText.text = data.PlayerName;
                    rowUI.valueText.text = data.SummitCount.ToString(); // <-- SUMMIT
                }
            },
            (error) => { Debug.LogError("Gagal memuat leaderboard: " + error); }
        ));
    }
}