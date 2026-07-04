using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Pengaturan Karakter")]
    public PlayerMovement playerMovement;
    public PlayerCamera playerCamera;
    
    [Header("Pengaturan Titik Awal (Spawn)")]
    public Transform playerStartPoint; // titikplayer
    public Transform npcStartPoint;    // titiknpc
    public Transform playerUtama;      // objek Player_Utama
    public Transform npcObj;           // objek player2

    [Header("Pengaturan Dialog (Kanan Atas)")]
    [Range(0.01f, 0.05f)]
    public float fontSizeRatio = 0.02f; // Font sangat kecil (2% dari layar)
    public Font customFont;

    [TextArea(2, 5)]
    public string[] dialogues = new string[] {
        "Pemain: Tempat apa ini? Terasa sangat dingin...",
        "Teman: Ssst... Jangan berisik. Kudengar rumor buruk tentang tempat ini.",
        "Teman: Misi kita hanya satu. Cari 9 kunci pintu keluar yang tersebar di gedung ini.",
        "Pemain: Kunci? Memangnya siapa yang mengunci kita dari dalam?",
        "Teman: Bukan siapa... tapi apa. Cepatlah, kita harus berpencar sebelum 'dia' menyadari kita di sini!"
    };

    public int currentDialogueIndex = 0;
    public bool isDialogueActive = true;

    [Header("Pengaturan Kamera Dialog")]
    public GameObject camDialogue; // Kamera khusus dialog
    public float dialogueMouseSensitivity = 150f;
    private float camDialogueXRotation = 0f;
    private float camDialogueYRotation = 0f;

    private Quaternion initialCamRotation;

    void Start()
    {
        // JIKA PEMAIN LOAD DARI CHECKPOINT, LEWATI SEMUA DIALOG!
        if (PlayerPrefs.GetInt("Checkpoint", 0) == 1)
        {
            isDialogueActive = false;
            
            // Sembunyikan NPC temanmu karena kita sudah di fase kejar-kejaran
            if (npcObj != null) npcObj.gameObject.SetActive(false);
            
            // Pastikan kamera utama player yang menyala
            if (camDialogue != null) camDialogue.SetActive(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(true);

            // Buka kunci pergerakan
            if (playerMovement != null) playerMovement.canMove = true;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            return; // Berhenti di sini, abaikan setup dialog di bawah
        }

        // 1. Pindahkan Karakter ke Titik Berhadapan (Spawn)
        Transform realPlayerRoot = null;
        if (playerMovement != null && playerStartPoint != null)
        {
            realPlayerRoot = playerMovement.transform;
            CharacterController cc = realPlayerRoot.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Kembalikan teleport murni ke titik awal tanpa ditambah tinggi
            realPlayerRoot.position = playerStartPoint.position;
            
            if (cc != null) cc.enabled = true;
        }

        if (npcObj != null && npcStartPoint != null)
        {
            npcObj.position = npcStartPoint.position;
        }

        // PAKSA MEREKA SALING BERHADAPAN
        if (realPlayerRoot != null && npcObj != null)
        {
            Vector3 lookAtNpc = new Vector3(npcObj.position.x, realPlayerRoot.position.y, npcObj.position.z);
            Vector3 lookAtPlayer = new Vector3(realPlayerRoot.position.x, npcObj.position.y, realPlayerRoot.position.z);
            
            realPlayerRoot.LookAt(lookAtNpc);
            npcObj.LookAt(lookAtPlayer);
        }

        // 2. Kunci Pergerakan (WASD)
        if (playerMovement != null) playerMovement.canMove = false;
        
        // 3. Matikan MainCamera, Nyalakan CamDialogue
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (camDialogue != null) camDialogue.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isDialogueActive)
        {
            // --- Fitur Menoleh (Mouse Look) untuk CamDialogue ---
            if (camDialogue != null)
            {
                // Gunakan cara paling primitif dan anti-gagal untuk rotasi FPS
                float mouseX = Input.GetAxisRaw("Mouse X") * 2.5f;
                float mouseY = Input.GetAxisRaw("Mouse Y") * 2.5f;

                camDialogue.transform.Rotate(Vector3.up * mouseX, Space.World);
                camDialogue.transform.Rotate(Vector3.right * -mouseY, Space.Self);
                
                // Kunci kemiringan (Roll) agar leher tidak bengkok
                Vector3 currentRot = camDialogue.transform.eulerAngles;
                camDialogue.transform.eulerAngles = new Vector3(currentRot.x, currentRot.y, 0f);
            }

            // Klik kiri atau spasi untuk lanjut dialog
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                currentDialogueIndex++;
                if (currentDialogueIndex >= dialogues.Length)
                {
                    EndDialogue();
                }
            }
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;

        // Buka Kunci Pemain, game Eksplorasi dimulai!
        if (playerMovement != null) playerMovement.canMove = true;
        
        // Matikan CamDialogue, Nyalakan MainCamera
        if (camDialogue != null) camDialogue.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        // Kursor tetap terkunci untuk mode eksplorasi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if (isDialogueActive && currentDialogueIndex < dialogues.Length)
        {
            // Responsif: Teks di Tengah Bawah Layar (Standar Game RPG/Horor)
            float boxWidth = Screen.width * 0.7f;     // Lebar 70% layar agar lebih luas
            float boxHeight = Screen.height * 0.15f;  // Tinggi 15% layar
            
            // Posisikan persis di tengah horizontal, dan sedikit di atas dasar layar
            float posX = (Screen.width - boxWidth) / 2f; 
            float posY = Screen.height - boxHeight - (Screen.height * 0.05f); 

            Rect dialogueRect = new Rect(posX, posY, boxWidth, boxHeight);

            // Kotak background semi transparan (Opsional, agar teks terbaca)
            GUI.color = new Color(0, 0, 0, 0.7f); // Hitam transparan 70%
            GUI.DrawTexture(dialogueRect, Texture2D.whiteTexture);
            GUI.color = Color.white; // Kembalikan warna normal

            // Style Teks (Ukuran Kecil)
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            if (customFont != null) textStyle.font = customFont;
            textStyle.normal.textColor = Color.white;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.wordWrap = true;
            textStyle.fontSize = Mathf.RoundToInt(Screen.height * fontSizeRatio);
            
            // Beri sedikit margin ke dalam kotak
            Rect textInsideRect = new Rect(dialogueRect.x + 10, dialogueRect.y + 10, dialogueRect.width - 20, dialogueRect.height - 20);
            
            // Tulis Teks Dialog
            GUI.Label(textInsideRect, dialogues[currentDialogueIndex], textStyle);

            // Instruksi klik
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
            if (customFont != null) hintStyle.font = customFont;
            hintStyle.normal.textColor = Color.yellow;
            hintStyle.alignment = TextAnchor.LowerRight;
            hintStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.015f); // Super kecil
            
            GUI.Label(textInsideRect, "[Klik Kiri / Spasi untuk Lanjut]", hintStyle);
        }
    }
}
