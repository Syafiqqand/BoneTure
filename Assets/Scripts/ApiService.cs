using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
// (Pastikan kamu punya 'using GameApi.Models;' jika kamu pakai namespace)

public class ApiService : MonoBehaviour
{
    public static ApiService Instance { get; private set; }
    private const string BASE_URL = "http://localhost:5164"; // (Pastikan port-mu benar)

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- FUNGSI LAMA (Tidak Berubah) ---
    public IEnumerator SaveProgress(PlayerProgress data, Action<PlayerProgress> onSuccess, Action<string> onError)
    {
        string url = $"{BASE_URL}/api/progress";
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                PlayerProgress resultData = JsonConvert.DeserializeObject<PlayerProgress>(request.downloadHandler.text);
                onSuccess?.Invoke(resultData);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    // --- FUNGSI LAMA (Tidak Berubah) ---
    public IEnumerator GetProgress(string playerName, Action<PlayerProgress> onSuccess, Action<string> onError)
    {
        string encodedPlayerName = System.Uri.EscapeDataString(playerName);
        string url = $"{BASE_URL}/api/progress/{encodedPlayerName}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                PlayerProgress progress = JsonConvert.DeserializeObject<PlayerProgress>(request.downloadHandler.text);
                onSuccess?.Invoke(progress);
            }
            else if (request.responseCode == 404)
            {
                onSuccess?.Invoke(null);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    // --- FUNGSI LAMA (Tidak Berubah) ---
    public IEnumerator GetAllNames(Action<List<string>> onSuccess, Action<string> onError)
    {
        string url = $"{BASE_URL}/api/progress/all";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                List<string> names = JsonConvert.DeserializeObject<List<string>>(request.downloadHandler.text);
                onSuccess?.Invoke(names);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }
    }

    // --- FUNGSI INI DIMODIFIKASI ---
    // Sekarang memanggil POST /api/progress/summit
    // dan mengirim 'finalTime'
    public IEnumerator IncrementSummit(string playerName, float finalTime,
                                     Action<PlayerProgress> onSuccess,
                                     Action<string> onError)
    {
        string url = $"{BASE_URL}/api/progress/summit"; // URL berubah

        // Buat payload (data JSON) baru
        var payload = new { PlayerName = playerName, FinalTime = finalTime };
        string jsonData = JsonConvert.SerializeObject(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                PlayerProgress updatedProgress = JsonConvert.DeserializeObject<PlayerProgress>(json);
                onSuccess?.Invoke(updatedProgress);
            }
            else
            {
                Debug.LogError($"Error Increment Summit: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }

    // --- FUNGSI BARU ---
    // Mengambil leaderboard SUMMIT
    public IEnumerator GetLeaderboard(Action<List<PlayerProgress>> onSuccess,
                                     Action<string> onError, int topN = 10)
    {
        string url = $"{BASE_URL}/api/progress/leaderboard?topN={topN}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                List<PlayerProgress> data = JsonConvert.DeserializeObject<List<PlayerProgress>>(json);
                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogError($"Error Get Leaderboard: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }

    // --- FUNGSI BARU ---
    // Mengambil leaderboard TIME
    public IEnumerator GetLeaderboardTime(Action<List<PlayerProgress>> onSuccess,
                                         Action<string> onError, int topN = 10)
    {
        string url = $"{BASE_URL}/api/progress/leaderboard-time?topN={topN}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                List<PlayerProgress> data = JsonConvert.DeserializeObject<List<PlayerProgress>>(json);
                onSuccess?.Invoke(data);
            }
            else
            {
                Debug.LogError($"Error Get Time Leaderboard: {request.error}");
                onError?.Invoke(request.error);
            }
        }
    }
}