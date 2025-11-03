using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    // Singleton pattern
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Error: Ada lebih dari satu instance GameInput!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    /// <summary>
    /// Mengambil Vector2 input gerakan (WASD) dan menormalisasinya.
    /// </summary>
    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    // --- FUNGSI BARU DI BAWAH INI ---

    /// <summary>
    /// Mengambil pergerakan delta (perubahan) mouse.
    /// </summary>
    public Vector2 GetMouseDelta()
    {
        return playerInputActions.Player.Look.ReadValue<Vector2>();
    }

    /// <summary>
    /// Cek apakah tombol kanan mouse DITAHAN saat ini.
    /// </summary>
    public bool IsCameraToggleHeld()
    {
        return playerInputActions.Player.CameraToggle.IsPressed();
    }

    /// <summary>
    /// Cek apakah tombol kanan mouse BARU DITEKAN frame ini.
    /// </summary>
    public bool IsCameraToggleButtonDown()
    {
        return playerInputActions.Player.CameraToggle.WasPressedThisFrame();
    }

    /// <summary>
    /// Cek apakah tombol kanan mouse BARU DILEPAS frame ini.
    /// </summary>
    public bool IsCameraToggleButtonUp()
    {
        return playerInputActions.Player.CameraToggle.WasReleasedThisFrame();
    }

    public bool IsJumpPressed()
    {
        // Kita pakai WasPressedThisFrame agar player tidak lompat terus-menerus
        // jika menahan spasi.
        return playerInputActions.Player.Jump.WasPressedThisFrame();
    }
}