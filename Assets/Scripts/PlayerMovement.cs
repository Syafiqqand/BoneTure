using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private Transform cameraTransform; // Tetap diisi Main Camera

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7.0f;
    [SerializeField] private float rotationSpeed = 10.0f;

    [Header("Jump & Gravity Settings")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravityValue = -9.81f;

    private CharacterController characterController;
    private Vector3 playerVelocity;
    private Vector3 spawnPoint;
    private CameraFollow cameraFollowScript;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // --- BERUBAH ---
        if (cameraTransform == null)
        {
            Debug.LogError("Error: 'Camera Transform' belum di-set!");
        }
        else
        {
            // Ambil script CameraFollow dari referensi cameraTransform
            cameraFollowScript = cameraTransform.GetComponent<CameraFollow>();
            if (cameraFollowScript == null)
            {
                Debug.LogError("Error: Object di 'Camera Transform' (Main Camera) " +
                               "tidak memiliki script CameraFollow!");
            }
        }

        spawnPoint = transform.position;
    }

    private void Update()
    {
        HandleGravity();
        HandleJump();
        HandleMovementAndRotation();
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
    }

    private void HandleJump()
    {
        if (characterController.isGrounded && GameInput.Instance.IsJumpPressed())
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    private void HandleMovementAndRotation()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = Vector3.zero;

        if (inputVector.magnitude >= 0.1f)
        {
            // --- INI PERBAIKAN UTAMA ---

            // 1. Ambil rotasi Yaw (horizontal) YANG BERSIH
            //    langsung dari script kamera, BUKAN dari transform.
            float cameraYaw = cameraFollowScript.GetCurrentYaw();

            // 2. Buat rotasi HANYA berdasarkan Yaw
            Quaternion camHorizontalRotation = Quaternion.Euler(0, cameraYaw, 0);

            // 3. Hitung 'forward' dan 'right' berdasarkan rotasi bersih itu
            Vector3 camForward = camHorizontalRotation * Vector3.forward;
            Vector3 camRight = camHorizontalRotation * Vector3.right;

            // 4. Kombinasikan input
            moveDir = (camForward * inputVector.y + camRight * inputVector.x).normalized;

            // --- AKHIR PERBAIKAN ---
        }

        // Gerakkan Player
        Vector3 horizontalMove = moveDir * moveSpeed;
        Vector3 finalMove = new Vector3(horizontalMove.x, playerVelocity.y, horizontalMove.z);
        characterController.Move(finalMove * Time.deltaTime);

        // Rotasi Player
        if (moveDir != Vector3.zero)
        {
            HandleRotation(moveDir);
        }
    }

    private void HandleRotation(Vector3 moveDir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void Respawn()
    {
        // Trik untuk memindahkan CharacterController
        characterController.enabled = false;
        transform.position = spawnPoint;
        characterController.enabled = true;

        // Reset kecepatan jatuh
        playerVelocity.y = -2f;
    }

    public void UpdateSpawnPoint(Vector3 newSpawnPosition)
    {
        spawnPoint = newSpawnPosition;
        Debug.Log("Spawn Point telah di-update ke: " + newSpawnPosition);
    }
}