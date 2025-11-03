using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 
using System.Collections.Generic; 
using System.Linq; 

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel Referensi")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject usernamePanel;
    [SerializeField] private GameObject loadPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button exitButton;

    [Header("Username Panel")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private Button saveUsernameButton; 
    [SerializeField] private Button backFromUserButton; 

    [Header("Load Panel")]
    [SerializeField] private TMP_Dropdown loadDropdown;
    [SerializeField] private Button loadProfileButton; 
    [SerializeField] private Button backFromLoadButton; 

    [Header("Pengaturan")]
    [SerializeField] private string gameSceneName = "Level_1"; 

    private const string PROFILES_KEY = "GameProfiles"; 
    private const string CURRENT_USER_KEY = "CurrentPlayer"; 

    void Start()
    {
        // 1. Setup Button Listeners
        // Main Menu
        playButton.onClick.AddListener(OnPlayClicked);
        loadButton.onClick.AddListener(OnLoadClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Username Panel
        saveUsernameButton.onClick.AddListener(OnSaveUsernameClicked);
        backFromUserButton.onClick.AddListener(ShowMainMenu);

        // Load Panel
        loadProfileButton.onClick.AddListener(OnLoadProfileClicked);
        backFromLoadButton.onClick.AddListener(ShowMainMenu);

        // 2. Set Panel Awal
        ShowMainMenu();
    }

    // --- FUNGSI NAVIGASI PANEL ---

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        usernamePanel.SetActive(false);
        loadPanel.SetActive(false);
    }

    private void OnPlayClicked()
    {
        mainMenuPanel.SetActive(false);
        usernamePanel.SetActive(true);
    }

    private void OnLoadClicked()
    {
        mainMenuPanel.SetActive(false);
        loadPanel.SetActive(true);

        // Panggil fungsi untuk mengisi dropdown saat panel dibuka
        PopulateLoadDropdown();
    }

    private void OnExitClicked()
    {
        Debug.Log("Keluar dari game...");
        // Ini hanya berfungsi di build, tidak di Editor
        Application.Quit();

        // Untuk tes di Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- FUNGSI LOGIKA PROFIL (Username & Load) ---

    private void OnSaveUsernameClicked()
    {
        string newName = usernameInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("Nama tidak boleh kosong!");
            return;
        }

        // 1. Buat data baru untuk dikirim
        PlayerProgress newPlayerData = new PlayerProgress
        {
            PlayerName = newName,
            CheckpointIndex = 0 // Player baru selalu mulai dari checkpoint 0
        };

        // 2. Nonaktifkan tombol agar tidak di-spam
        saveUsernameButton.interactable = false;

        // 3. Panggil API Service (ini akan berjalan di background)
        StartCoroutine(ApiService.Instance.SaveProgress(
            newPlayerData,

            // Ini adalah 'onSuccess' (jika berhasil)
            (resultFromServer) => {

                Debug.Log($"Player {resultFromServer.PlayerName} berhasil disimpan/login!");

                // Simpan nama player yg aktif SEKARANG ke PlayerPrefs
                // agar scene game-mu (Level_1) tahu siapa yg main
                PlayerPrefs.SetString(CURRENT_USER_KEY, resultFromServer.PlayerName);
                PlayerPrefs.Save();

                // Pindah ke Scene Game
                SceneManager.LoadScene(gameSceneName);
            },

            // Ini adalah 'onError' (jika gagal)
            (errorMsg) => {
                Debug.LogError($"Gagal menyimpan username: {errorMsg}");
                // Tampilkan pesan error ke player (opsional)

                // Nyalakan tombol lagi agar bisa dicoba ulang
                saveUsernameButton.interactable = true;
            }
        ));
    }

    private void PopulateLoadDropdown()
    {
        loadDropdown.ClearOptions(); // Kosongkan dropdown
        loadDropdown.AddOptions(new List<string> { "Loading..." }); // Tampilkan status
        loadProfileButton.interactable = false; // Matikan tombol selagi loading

        // Panggil coroutine baru untuk mengambil data dari API
        StartCoroutine(ApiService.Instance.GetAllNames(

            // Ini adalah callback 'onSuccess' (jika berhasil)
            (namesFromApi) => {
                loadDropdown.ClearOptions(); // Hapus "Loading..."

                if (namesFromApi.Count == 0)
                {
                    loadDropdown.AddOptions(new List<string> { "Belum ada profil..." });
                    loadProfileButton.interactable = false;
                }
                else
                {
                    // Masukkan semua nama dari API ke dropdown
                    loadDropdown.AddOptions(namesFromApi);
                    loadProfileButton.interactable = true;
                }
            },

            // Ini adalah callback 'onError' (jika gagal)
            (error) => {
                loadDropdown.ClearOptions();
                loadDropdown.AddOptions(new List<string> { "Gagal memuat profil..." });
                loadProfileButton.interactable = false;
            }
        ));
    }

    private void OnLoadProfileClicked()
    {
        // Cek apakah ada profil valid
        if (loadDropdown.options[0].text == "Belum ada profil...")
        {
            return;
        }

        // 1. Ambil nama yang dipilih dari dropdown
        string selectedName = loadDropdown.options[loadDropdown.value].text;

        // 2. Set nama ini sebagai "Current User"
        PlayerPrefs.SetString(CURRENT_USER_KEY, selectedName);
        PlayerPrefs.Save();

        // 3. Pindah ke Scene Game
        Debug.Log($"Melanjutkan game sebagai: {selectedName}");
        SceneManager.LoadScene(gameSceneName);
    }

    // --- FUNGSI HELPER (BANTUAN) ---

    private List<string> GetSavedProfiles()
    {
        // Ambil string panjang dari PlayerPrefs
        string profilesString = PlayerPrefs.GetString(PROFILES_KEY, string.Empty);

        if (string.IsNullOrEmpty(profilesString))
        {
            return new List<string>(); // Kembalikan list kosong
        }

        // Pecah string-nya berdasarkan ';'
        return profilesString.Split(';').ToList();
    }
}