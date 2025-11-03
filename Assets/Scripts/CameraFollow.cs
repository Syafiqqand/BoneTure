using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private Transform playerTransform; // Target yang diikuti (Player kamu)

    [Header("Pengaturan Kamera")]
    [SerializeField] private float distance = 5.0f; // Jarak kamera dari player
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float minPitch = -30f; // Batas lihat ke bawah (negatif)
    [SerializeField] private float maxPitch = 80f;  // Batas lihat ke atas
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0); // Titik target di player (di atas kaki)

    [Header("Pengaturan Input")]
    [SerializeField] private float tapThreshold = 0.2f; // Batas waktu untuk dianggap "Tap"

    private enum CameraState { Unlocked, Locked }
    private CameraState currentState;

    private float rightClickPressTime = 0f;
    private float currentYaw = 0.0f;    // Rotasi Horizontal (Kiri/Kanan)
    private float currentPitch = 20.0f; // Rotasi Vertikal (Atas/Bawah)

    void Start()
    {
        currentState = CameraState.Unlocked;
        UpdateCursorState();
    }

    void Update()
    {
        if (playerTransform == null) return;

        HandleStateInput(); // Mengurus logika tap vs hold
        HandleMouseLook();  // Mengurus rotasi kamera
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // Hitung target posisi player + offset
        Vector3 targetPosition = playerTransform.position + offset;

        // Hitung rotasi berdasarkan input mouse
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Hitung posisi kamera baru
        // Mundur sejauh 'distance' dari 'targetPosition' berdasarkan 'rotation'
        Vector3 cameraPosition = targetPosition - (rotation * Vector3.forward * distance);

        // Terapkan posisi dan rotasi ke kamera
        transform.position = cameraPosition;
        transform.LookAt(targetPosition); // Kamera selalu melihat ke arah player
    }

    private void HandleStateInput()
    {
        // 1. Cek jika tombol baru ditekan
        if (GameInput.Instance.IsCameraToggleButtonDown())
        {
            rightClickPressTime = Time.time;
        }

        // 2. Cek jika tombol baru dilepas
        if (GameInput.Instance.IsCameraToggleButtonUp())
        {
            float pressDuration = Time.time - rightClickPressTime;

            // 3. Cek apakah ini "Tap" (ditekan dan dilepas cepat)
            if (pressDuration < tapThreshold)
            {
                if (currentState == CameraState.Unlocked)
                    currentState = CameraState.Locked;
                else
                    currentState = CameraState.Unlocked;

                UpdateCursorState();
            }
        }
    }

    private void HandleMouseLook()
    {
        bool isHolding = GameInput.Instance.IsCameraToggleHeld();
        bool canLook = false;

        if (currentState == CameraState.Locked)
        {
            canLook = true;
        }
        else if (isHolding)
        {
            canLook = true;
        }

        if (canLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            UpdateCursorState();
        }

        if (!canLook) return; // Jika tidak bisa lihat, stop

        // --- INI PERUBAHAN UTAMA ---
        // Kita tidak lagi memutar 'playerTransform'
        // Kita hanya mengubah nilai 'currentYaw' dan 'currentPitch'
        Vector2 mouseInput = GameInput.Instance.GetMouseDelta();
        currentYaw += mouseInput.x * mouseSensitivity * Time.deltaTime;
        currentPitch -= mouseInput.y * mouseSensitivity * Time.deltaTime;

        // Batasi rotasi vertikal (pitch)
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    private void UpdateCursorState()
    {
        if (currentState == CameraState.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public float GetCurrentYaw()
    {
        return currentYaw;
    }
}