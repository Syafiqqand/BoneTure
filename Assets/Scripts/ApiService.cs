using Newtonsoft.Json;
using System; 
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

// (Tambahkan 'using GameApi.Models;' jika kamu pakai namespace di PlayerProgress.cs)

public class ApiService : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static ApiService Instance { get; private set; }

    // GANTI URL INI dengan URL 'http' dari API-mu
    private const string BASE_URL = "http://localhost:5164";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Endpoint untuk Menyimpan/Register Player (POST /api/progress) ---
    public IEnumerator SaveProgress(PlayerProgress data,
                                    Action<PlayerProgress> onSuccess,
                                    Action<string> onError)
    {
        string url = $"{BASE_URL}/api/progress";

        // 1. Ubah C# class -> JSON string
        string jsonData = JsonConvert.SerializeObject(data);

        // 2. Ubah JSON string -> bytes
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // 3. Buat Request
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 4. Set Header (PENTING!)
            request.SetRequestHeader("Content-Type", "application/json");

            // 5. Kirim dan tunggu
            yield return request.SendWebRequest();

            // 6. Cek hasil
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Sukses (Code 200 atau 201)
                Debug.Log("Save Progress Sukses: " + request.downloadHandler.text);
                PlayerProgress resultData = JsonConvert.DeserializeObject<PlayerProgress>(request.downloadHandler.text);
                onSuccess?.Invoke(resultData); // Panggil callback sukses
            }
            else
            {
                // Gagal
                Debug.LogError($"Error Save Progress: {request.error} | {request.downloadHandler.text}");
                onError?.Invoke(request.error); // Panggil callback error
            }
        }
    }

    /// <summary>
    /// Mengambil daftar semua nama player dari server.
    /// Memanggil GET /api/progress/all
    /// </summary>
    public IEnumerator GetAllNames(Action<List<string>> onSuccess, Action<string> onError)
    {
        string url = $"{BASE_URL}/api/progress/all";

        // Ini adalah request GET, jadi lebih simpel
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Sukses
                string jsonResponse = request.downloadHandler.text;

                // Ubah JSON ["nama1", "nama2"] -> List<string> C#
                List<string> names = JsonConvert.DeserializeObject<List<string>>(jsonResponse);
                onSuccess?.Invoke(names);
            }
            else
            {
                // Gagal
                Debug.LogError($"Error Get All Names: {request.error} | {request.downloadHandler.text}");
                onError?.Invoke(request.error);
            }
        }
    }
}