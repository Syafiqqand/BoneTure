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
    [SerializeField] private GameObject leaderboardPanel; // <-- BARU

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button leaderboardButton; // <-- BARU
    [SerializeField] private Button exitButton;

    [Header("Username Panel")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private Button saveUsernameButton;
    [SerializeField] private Button backFromUserButton;

    [Header("Load Panel")]
    [SerializeField] private TMP_Dropdown loadDropdown;
    [SerializeField] private Button loadProfileButton;
    [SerializeField] private Button backFromLoadButton;

    [Header("Leaderboard Panel")] // <-- SECTION BARU
    [SerializeField] private Button selectSummitButton; // Tombol tab "Summit"
    [SerializeField] private Button selectTimeButton;   // Tombol tab "Time"
    [SerializeField] private Button backFromLeaderboardButton;
    [SerializeField] private GameObject summitLeaderboardObject; // Panel/Object yg berisi tabel Summit
    [SerializeField] private GameObject timeLeaderboardObject; // Panel/Object yg berisi tabel Time

    [Header("Pengaturan")]
    [SerializeField] private string gameSceneName = "Gameplay"; // (Pastikan nama ini benar)

    // private const string PROFILES_KEY = "GameProfiles"; // (Kita tidak pakai ini lagi)
    private const string CURRENT_USER_KEY = "CurrentPlayer";

    void Awake()
    {
        // Paksa kursor agar terlihat dan tidak terkunci
        // dan reset TimeScale SEBELUM scene-nya mulai.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        // --- Setup Listeners ---
        playButton.onClick.AddListener(OnPlayClicked);
        loadButton.onClick.AddListener(OnLoadClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        // Username Panel
        saveUsernameButton.onClick.AddListener(OnSaveUsernameClicked);
        backFromUserButton.onClick.AddListener(ShowMainMenu);

        // Load Panel
        loadProfileButton.onClick.AddListener(OnLoadProfileClicked);
        backFromLoadButton.onClick.AddListener(ShowMainMenu);

        // --- Listeners BARU ---
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        backFromLeaderboardButton.onClick.AddListener(ShowMainMenu);
        selectSummitButton.onClick.AddListener(ShowSummitLeaderboard);
        selectTimeButton.onClick.AddListener(ShowTimeLeaderboard);
        // --------------------

        // Set Panel Awal
        ShowMainMenu();
    }

    // --- FUNGSI NAVIGASI PANEL ---

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        usernamePanel.SetActive(false);
        loadPanel.SetActive(false);
        leaderboardPanel.SetActive(false); // <-- BARU
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
        PopulateLoadDropdown();
    }

    private void OnExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- FUNGSI BARU UNTUK LEADERBOARD ---

    private void OnLeaderboardClicked()
    {
        mainMenuPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
        // Default, tunjukkan leaderboard Summit
        ShowSummitLeaderboard();
    }

    private void ShowSummitLeaderboard()
    {
        summitLeaderboardObject.SetActive(true);
        timeLeaderboardObject.SetActive(false);

        // Refresh data saat tab diklik (jika perlu)
        if (summitLeaderboardObject.GetComponent<LeaderboardManager>() != null)
        {
            summitLeaderboardObject.GetComponent<LeaderboardManager>().RefreshLeaderboard();
        }
    }

    private void ShowTimeLeaderboard()
    {
        summitLeaderboardObject.SetActive(false);
        timeLeaderboardObject.SetActive(true);

        // Refresh data saat tab diklik (jika perlu)
        if (timeLeaderboardObject.GetComponent<LeaderboardTimeManager>() != null)
        {
            timeLeaderboardObject.GetComponent<LeaderboardTimeManager>().RefreshLeaderboard();
        }
    }
    // ------------------------------------


    // --- FUNGSI LOGIKA PROFIL (Username & Load) ---

    private void OnSaveUsernameClicked()
    {
        string newName = usernameInputField.text.Trim();
        if (string.IsNullOrWhiteSpace(newName)) return;

        PlayerProgress newPlayerData = new PlayerProgress
        {
            PlayerName = newName,
            CheckpointIndex = -1,
            CurrentElapsedTime = 0,
            SummitCount = 0,
            BestTimeInSeconds = 999999
        };

        saveUsernameButton.interactable = false;

        StartCoroutine(ApiService.Instance.SaveProgress(
            newPlayerData,
            (result) => {
                PlayerPrefs.SetString(CURRENT_USER_KEY, result.PlayerName);
                PlayerPrefs.Save();
                SceneManager.LoadScene(gameSceneName);
            },
            (errorMsg) => {
                Debug.LogError($"Gagal menyimpan username: {errorMsg}");
                saveUsernameButton.interactable = true;
            }
        ));
    }

    private void PopulateLoadDropdown()
    {
        loadDropdown.ClearOptions();
        loadDropdown.AddOptions(new List<string> { "Loading..." });
        loadProfileButton.interactable = false;

        StartCoroutine(ApiService.Instance.GetAllNames(
            (namesFromApi) => {
                loadDropdown.ClearOptions();
                if (namesFromApi == null || namesFromApi.Count == 0)
                {
                    loadDropdown.AddOptions(new List<string> { "Belum ada profil..." });
                    loadProfileButton.interactable = false;
                }
                else
                {
                    loadDropdown.AddOptions(namesFromApi);
                    loadProfileButton.interactable = true;
                }
            },
            (error) => {
                loadDropdown.ClearOptions();
                loadDropdown.AddOptions(new List<string> { "Gagal memuat profil..." });
                loadProfileButton.interactable = false;
            }
        ));
    }

    private void OnLoadProfileClicked()
    {
        if (loadDropdown.options[0].text.StartsWith("Belum ada") ||
            loadDropdown.options[0].text.StartsWith("Gagal"))
        {
            return;
        }

        string selectedName = loadDropdown.options[loadDropdown.value].text;
        PlayerPrefs.SetString(CURRENT_USER_KEY, selectedName);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }
}