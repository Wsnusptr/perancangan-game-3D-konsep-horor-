using UnityEngine;
using System.Collections;

public class KeyItem : MonoBehaviour
{
    private GameUIManager uiManager;
    private Transform player;
    private Animator playerAnimator;

    [Header("Pengaturan Jarak & Interaksi")]
    public float interactDistance = 2.5f;
    private bool isPlayerNear = false;
    private bool isPickingUp = false;

    private Material outlineMaterial;

    void Start()
    {
        // Cari GameUIManager
        uiManager = FindObjectOfType<GameUIManager>();
        
        // Cari Player secara otomatis yang paling aman (lewat script PlayerMovement)
        PlayerMovement pm = FindObjectOfType<PlayerMovement>();
        if (pm != null)
        {
            player = pm.transform;
        }
        else
        {
            // Cadangan jika tidak ada PlayerMovement, cari lewat tag
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator == null) playerAnimator = player.GetComponentInChildren<Animator>();
        }
        else
        {
            Debug.LogError("KeyItem: GAGAL MENEMUKAN PLAYER! Pastikan Player memiliki script PlayerMovement atau Tag 'Player'.");
        }

        // ==========================================
        // SISTEM OUTLINE 3D (GARIS PINGGIR OTOMATIS)
        // ==========================================
        Shader outlineShader = Shader.Find("Custom/Outline");
        if (outlineShader != null)
        {
            outlineMaterial = new Material(outlineShader);
            outlineMaterial.SetColor("_OutlineColor", Color.white); // Warna outline putih elegan
            
            // Cari MeshRenderer pada objek ini atau anak-anaknya
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null) renderer = GetComponentInChildren<MeshRenderer>();

            if (renderer != null)
            {
                // Sisipkan material outline ke urutan paling akhir tanpa merusak material asli
                Material[] oldMats = renderer.materials;
                Material[] newMats = new Material[oldMats.Length + 1];
                for (int i = 0; i < oldMats.Length; i++) newMats[i] = oldMats[i];
                newMats[oldMats.Length] = outlineMaterial;
                renderer.materials = newMats;
            }
        }
    }

    void Update()
    {
        if (isPickingUp || player == null) return;

        // Cek jarak antara Player dan Kunci ini
        float dist = Vector3.Distance(player.position, transform.position);
        isPlayerNear = (dist <= interactDistance);

        // Jika pemain dekat dan menekan tombol E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(PickupRoutine());
        }
    }

    IEnumerator PickupRoutine()
    {
        isPickingUp = true;
        isPlayerNear = false; // Matikan tulisan UI "Tekan E"

        // 1. Jalankan Animasi Mengambil pada Player
        if (playerAnimator != null)
        {
            // PENTING: Kamu harus membuat parameter Trigger bernama "AmbilKunci" di Animator Controller milik Player!
            playerAnimator.SetTrigger("AmbilKunci");
        }
        else
        {
            Debug.LogWarning("KeyItem: Animator pada Player tidak ditemukan! Animasi mengambil dilewati.");
        }

        // 2. Sembunyikan kunci sementara animasinya berjalan agar terkesan sudah di tangan
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;

        // 3. Tunggu animasi selesai (Sesuaikan waktunya dengan durasi animasi mengambilmu, misal 1.5 detik)
        yield return new WaitForSeconds(1.5f);

        // 4. Tambahkan kunci ke sistem UI
        if (uiManager != null)
        {
            uiManager.AddKey();
        }
        
        // 5. Hancurkan objek ini sepenuhnya
        Destroy(gameObject);
    }

    void OnGUI()
    {
        // Jika pemain dekat dan kunci belum diambil, munculkan instruksi di tengah layar
        if (isPlayerNear && !isPickingUp)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 26;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;

            GUIStyle shadow = new GUIStyle(style);
            shadow.normal.textColor = Color.black;

            string prompt = "Tekan <b>[ E ]</b> untuk mengambil Kunci";
            
            Rect rectShadow = new Rect(Screen.width / 2f - 200f + 2f, Screen.height / 2f + 60f + 2f, 400f, 50f);
            Rect rect = new Rect(Screen.width / 2f - 200f, Screen.height / 2f + 60f, 400f, 50f);

            GUI.Label(rectShadow, prompt, shadow);
            GUI.Label(rect, "<color=#00FF00>" + prompt + "</color>", style);
        }
    }
}
