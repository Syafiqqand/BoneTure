using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
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

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }

    public Vector2 GetMouseDelta()
    {
        return playerInputActions.Player.Look.ReadValue<Vector2>();
    }

    public bool IsCameraToggleHeld()
    {
        return playerInputActions.Player.CameraToggle.IsPressed();
    }

    public bool IsCameraToggleButtonDown()
    {
        return playerInputActions.Player.CameraToggle.WasPressedThisFrame();
    }

    public bool IsCameraToggleButtonUp()
    {
        return playerInputActions.Player.CameraToggle.WasReleasedThisFrame();
    }

    public bool IsJumpPressed()
    {
        return playerInputActions.Player.Jump.WasPressedThisFrame();
    }

    // Fungsi baru ditambahkan di sini
    public bool IsPausePressed()
    {
        return playerInputActions.Player.Pause.WasPressedThisFrame();
    }
}