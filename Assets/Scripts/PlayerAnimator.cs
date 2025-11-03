using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Referensi")]
    // 1. Tarik object 'PlayerModel' (yang punya Animator) ke slot ini
    [SerializeField] private Animator animator;

    // 2. Kita butuh CharacterController untuk cek 'isGrounded'
    private CharacterController characterController;

    // StringToHash lebih efisien daripada string di Update()
    private int isRunningHash;
    private int doJumpHash;

    private void Awake()
    {
        // 3. Ambil referensi CharacterController dari object ini
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            Debug.LogError("Error: Animator belum di-set di PlayerAnimator script!");
        }

        // 4. Inisialisasi parameter hash
        isRunningHash = Animator.StringToHash("isRunning");
        doJumpHash = Animator.StringToHash("doJump");
    }

    private void Update()
    {
        HandleRunningAnimation();
        HandleJumpAnimation();
    }

    private void HandleRunningAnimation()
    {
        // Ambil input gerak dari GameInput
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        // Jika magnitudo (panjang) vektor lebih dari 0.1f, berarti player bergerak
        bool isRunning = inputVector.magnitude > 0.1f;

        // Set parameter "isRunning" di Animator
        animator.SetBool(isRunningHash, isRunning);
    }

    private void HandleJumpAnimation()
    {
        // Kita hanya mau trigger animasi JIKA:
        // 1. Player menekan tombol Jump, DAN
        // 2. Player sedang di tanah (isGrounded)
        if (GameInput.Instance.IsJumpPressed() && characterController.isGrounded)
        {
            // Set parameter "doJump" di Animator
            // Parameter 'doJump' kamu kelihatannya adalah Trigger
            animator.SetTrigger(doJumpHash);
        }
    }
}