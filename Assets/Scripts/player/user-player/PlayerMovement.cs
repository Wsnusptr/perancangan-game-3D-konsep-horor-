using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 5.0f;
    public float crouchSpeed = 1.5f;
    public float gravity = -9.81f;

    [Header("Animation")]
    public Animator animator; 
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching = false;
    public bool IsCrouching { get { return isCrouching; } }
    
    private float originalHeight;
    private float crouchHeight;
    private Vector3 originalCenter;
    private Vector3 crouchCenter;

    // Akses publik untuk membaca kecepatan jatuh
    public float VerticalVelocity { get { return velocity.y; } }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
            
        originalHeight = controller.height;
        originalCenter = controller.center;
        
        // Menghitung tinggi saat jongkok
        crouchHeight = originalHeight / 2f;
        
        // Logika matematika penting: Saat jongkok, kita ingin batas bawah kapsul (kaki) tetap di tempat yang sama,
        // yang memendek hanya bagian atasnya.
        float capsuleBottom = originalCenter.y - (originalHeight / 2f);
        crouchCenter = new Vector3(originalCenter.x, capsuleBottom + (crouchHeight / 2f), originalCenter.z);
    }

    [Header("Game State")]
    public bool canMove = true;

    void Update()
    {
        // Jika tidak boleh bergerak (misal saat dialog), hentikan semua input pergerakan
        if (!canMove)
        {
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isCrouching", false);
            }
            return;
        }

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            if (isCrouching)
            {
                controller.height = crouchHeight;
                controller.center = crouchCenter;
            }
            else
            {
                controller.height = originalHeight;
                controller.center = originalCenter;
            }
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && z > 0;
        float currentSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        if (animator != null)
        {
            bool isMoving = move.magnitude > 0.1f;
            animator.SetBool("isWalking", isMoving && !isRunning && !isCrouching);
            animator.SetBool("isRunning", isMoving && isRunning);
            animator.SetBool("isCrouching", isCrouching);
        }
    }
}
