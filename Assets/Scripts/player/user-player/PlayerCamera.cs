using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;
    public Light flashlight;
    
    [Header("Camera Settings")]
    public float mouseSensitivity = 200f;
    
    [Header("Camera Modes")]
    public Vector3 firstPersonPos = new Vector3(0f, 2.5f, 0f); 
    public Vector3 thirdPersonPos = new Vector3(0.8f, 2.5f, -4.5f);
    public float crouchYDrop = 1.0f; // Berapa meter kamera turun saat jongkok
    public float transitionSpeed = 10f;
    
    private bool isFirstPerson = true;
    private float xRotation = 0f;
    private bool isFlashlightOn = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.localPosition = isFirstPerson ? firstPersonPos : thirdPersonPos;
    }

    [Header("Game State")]
    public bool canMove = true;

    void Update()
    {
        // Jika boleh bergerak, proses putaran mouse dan input
        if (canMove)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f); 
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            if (playerBody != null)
            {
                PlayerMovement parentMovement = playerBody.GetComponentInParent<PlayerMovement>();
                if (parentMovement != null)
                {
                    parentMovement.transform.Rotate(Vector3.up * mouseX);
                }
                else
                {
                    playerBody.Rotate(Vector3.up * mouseX);
                }
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                isFirstPerson = !isFirstPerson;
                Debug.Log("Tombol V ditekan! Mode Mata (FirstPerson) aktif: " + isFirstPerson);
            }
            
            if (Input.GetKeyDown(KeyCode.F) && flashlight != null)
            {
                isFlashlightOn = !isFlashlightOn;
                flashlight.enabled = isFlashlightOn;
            }
        }

        // --- Logika Posisi Kamera HARUS selalu berjalan ---
        Vector3 targetPos = isFirstPerson ? firstPersonPos : thirdPersonPos;
        
        // Cek apakah pemain sedang jongkok
        if (playerBody != null)
        {
            PlayerMovement pm = playerBody.GetComponentInParent<PlayerMovement>();
            if (pm != null && pm.IsCrouching)
            {
                targetPos.y -= crouchYDrop; // Turunkan posisi kamera
            }
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * transitionSpeed);
    }
}
