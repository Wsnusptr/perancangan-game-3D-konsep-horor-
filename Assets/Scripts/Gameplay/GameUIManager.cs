using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("Status Fase")]
    public bool isEscapePhase = false;

    [Header("Referensi Sistem Utama")]
    public DialogueManager dialogueManager; // Untuk mengecek kapan dialog selesai

    [Header("Referensi Kunci")]
    public int totalKeys = 9;
    public int collectedKeys = 0;
    public Texture2D keyIcon;
    public Font customFont; 

    [Header("Referensi Pergerakan (Stance)")]
    public PlayerMovement playerMovement; 
    public Texture2D standingIcon;
    public Texture2D crouchingIcon;

    [Header("Pengaturan Live Chat")]
    public int maxChatLines = 10; // Ditambah agar muat di kotak yang lebih panjang
    public float chatIntervalMin = 2f;
    public float chatIntervalMax = 6f;

    [Header("Pengaturan Walkie-Talkie")]
    public float radioIntervalMin = 5f; // Dipercepat jadi 5 detik
    public float radioIntervalMax = 12f; // Paling lama 12 detik
    public float radioDisplayTime = 5f;

    private string currentRadioMessage = "";
    private string[] radioLines = {
        "Teman: Hei, kamu masih hidup kan? Jangan sampai ketahuan.",
        "Teman: [Kresek...] Sinyalnya buruk... Cepat cari kunci lainnya.",
        "Teman: Hati-hati, aku merasa 'dia' sedang berpatroli.",
        "Teman: Ingat, kalau melihat hantu itu, langsung menjauh!",
        "Teman: Fokus cari kunci! Waktu kita tidak banyak.",
        "Teman: [Kresek... Kresek...] Sesuatu bergerak di dekatmu!",
        "Teman: Usahakan jongkok agar tidak terlalu bising.",
        "Teman: Kalau kau tertangkap, aku tidak bisa menolongmu..."
    };

    [Header("Pengaturan Info Task")]
    private string currentTaskInfo = "Misi Utama: Temukan 9 Kunci untuk keluar dari tempat ini!";
    private float taskHighlightTimer = 0f;

    private List<string> liveChatMessages = new List<string>();
    private string[] fakeUsernames = { "GamerBoi99", "HorrorFan", "KucingOren", "Misterius21", "TukangSpam", "SukaJumpscare", "GhostHunter", "AnonUser", "BocilKematian" };
    private string[] fakeComments = { 
        "Ada suara apa itu?!", 
        "Di belakangmu bro!!", 
        "Sumpah game ini bikin merinding...", 
        "Kok gelap banget", 
        "Lariiii!!", 
        "Itu bayangan siapa?", 
        "Jangan ke sana woy!", 
        "Gue kira bakal ada jumpscare",
        "Atmosfernya gila sih",
        "Awas ada yang ngintip",
        "Kaget banget njir",
        "Bang coba lihat ke atas"
    };

    // Resolusi dasar untuk membuat UI Responsive di semua device
    private Vector2 baseResolution = new Vector2(1920, 1080);
    private Texture2D darkBoxTexture;

    void Start()
    {
        // Cari DialogueManager secara otomatis jika kotak di Inspector kosong
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        // Membuat tekstur semi-transparan untuk latar belakang box UI
        darkBoxTexture = new Texture2D(1, 1);
        darkBoxTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.6f)); // Hitam dengan transparansi 60%
        darkBoxTexture.Apply();

        StartCoroutine(ChatRoutine());
        StartCoroutine(RadioRoutine());

        // Cek Checkpoint (Jika Player mengulang dari GameOver saat fase melarikan diri)
        if (PlayerPrefs.GetInt("Checkpoint", 0) == 1)
        {
            collectedKeys = totalKeys;
            float px = PlayerPrefs.GetFloat("CP_X", 0f);
            float py = PlayerPrefs.GetFloat("CP_Y", 0f);
            float pz = PlayerPrefs.GetFloat("CP_Z", 0f);
            
            if (playerMovement != null)
            {
                // Matikan sementara CharacterController agar posisi bisa diubah secara paksa
                CharacterController cc = playerMovement.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                
                playerMovement.transform.position = new Vector3(px, py, pz);
                
                if (cc != null) cc.enabled = true;
            }

            StartEscapePhase();
        }
    }

    public void AddKey()
    {
        collectedKeys++;
        int sisaKunci = totalKeys - collectedKeys;

        if (sisaKunci > 0)
        {
            currentTaskInfo = "SYSTEM: Kunci ditemukan! Sisa " + sisaKunci + " Kunci lagi yang harus dicari.";
            taskHighlightTimer = 4f; // Info akan berkedip kuning selama 4 detik
        }
        else
        {
            // KUNCI TERAKHIR DIAMBIL! Simpan Checkpoint posisi pemain.
            if (playerMovement != null)
            {
                PlayerPrefs.SetInt("Checkpoint", 1);
                PlayerPrefs.SetFloat("CP_X", playerMovement.transform.position.x);
                PlayerPrefs.SetFloat("CP_Y", playerMovement.transform.position.y);
                PlayerPrefs.SetFloat("CP_Z", playerMovement.transform.position.z);
                PlayerPrefs.Save();
                Debug.Log("CHECKPOINT TERSIMPAN!");
            }

            StartEscapePhase();
        }
    }

    private void StartEscapePhase()
    {
        isEscapePhase = true;
        currentTaskInfo = "SYSTEM: SEMUA KUNCI TERKUMPUL! LARI KE MOBIL SEKARANG!";
    }

    IEnumerator RadioRoutine()
    {
        while (true)
        {
            if (dialogueManager != null && dialogueManager.isDialogueActive)
            {
                yield return null;
                continue;
            }

            // Tunggu beberapa belas detik secara acak
            yield return new WaitForSeconds(Random.Range(radioIntervalMin, radioIntervalMax));
            
            // Munculkan pesan teman dari Walkie Talkie
            currentRadioMessage = radioLines[Random.Range(0, radioLines.Length)];
            
            // Tampilkan di layar selama 5 detik
            yield return new WaitForSeconds(radioDisplayTime);
            
            // Sembunyikan pesan lagi
            currentRadioMessage = "";
        }
    }

    IEnumerator ChatRoutine()
    {
        while (true)
        {
            // Jika dialog masih berjalan, jangan munculkan chat
            if (dialogueManager != null && dialogueManager.isDialogueActive)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Random.Range(chatIntervalMin, chatIntervalMax));
            
            string randomUser = fakeUsernames[Random.Range(0, fakeUsernames.Length)];
            string randomComment = fakeComments[Random.Range(0, fakeComments.Length)];
            
            string newMessage = "<color=#FFD700><b>" + randomUser + "</b></color>: " + randomComment;
            liveChatMessages.Add(newMessage);
            
            if (liveChatMessages.Count > maxChatLines)
            {
                liveChatMessages.RemoveAt(0);
            }
        }
    }

    void OnGUI()
    {
        // JANGAN MUNCUL JIKA DIALOG MASIH AKTIF
        if (dialogueManager != null && dialogueManager.isDialogueActive)
            return;

        // === SISTEM UI RESPONSIVE (Auto-Scale) ===
        Vector3 scale = new Vector3(Screen.width / baseResolution.x, Screen.height / baseResolution.y, 1.0f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        GUIStyle style = new GUIStyle();
        if (customFont != null) style.font = customFont;
        style.normal.textColor = Color.white;
        style.richText = true; 

        // Style untuk Kotak Background
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = darkBoxTexture;

        // ==========================================
        // 1. MENGGAMBAR UI KIRI ATAS (Task & Stance)
        // ==========================================
        // -- Ikon Kunci --
        if (keyIcon != null)
        {
            GUI.DrawTexture(new Rect(30, 30, 50, 50), keyIcon, ScaleMode.ScaleToFit);
        }
        
        GUIStyle keyStyle = new GUIStyle(style);
        keyStyle.fontSize = 28; // Angka lebih besar agar jelas
        keyStyle.normal.textColor = Color.white; 
        
        // HANYA ANGKA 0 / 9 (Dengan efek bayangan agar tetap terbaca meski tanpa box)
        GUIStyle shadowKeyStyle = new GUIStyle(keyStyle);
        shadowKeyStyle.normal.textColor = Color.black;
        shadowKeyStyle.richText = false; // Matikan warna HTML untuk bayangan
        GUI.Label(new Rect(91, 41, 100, 50f), collectedKeys + " / " + totalKeys, shadowKeyStyle);
        GUI.Label(new Rect(90, 40, 100, 50f), "<color=#FF4444><b>" + collectedKeys + " / " + totalKeys + "</b></color>", keyStyle);

        // -- Ikon Jongkok/Berdiri (Hanya Icon) --
        Texture2D currentStance = standingIcon;
        if (playerMovement != null && playerMovement.IsCrouching)
        {
            currentStance = crouchingIcon;
        }

        if (currentStance != null)
        {
            GUI.DrawTexture(new Rect(75, 90, 50, 50), currentStance, ScaleMode.ScaleToFit);
        }

        // ==========================================
        // 1.5 MENGGAMBAR WALKIE-TALKIE (Bawah Jongkok)
        // ==========================================
        if (!string.IsNullOrEmpty(currentRadioMessage))
        {
            float radioY = 160f; // Muncul pas di bawah ikon jongkok
            
            GUIStyle radioStyle = new GUIStyle(style);
            radioStyle.fontSize = 22;
            radioStyle.normal.textColor = new Color(0.6f, 1f, 0.6f); // Warna hijau khas teks radio/walkie-talkie
            radioStyle.wordWrap = true;

            GUIStyle radioShadow = new GUIStyle(radioStyle);
            radioShadow.normal.textColor = Color.black;

            string formattedRadio = "<b>[Walkie-Talkie]</b>\n" + currentRadioMessage;

            // Efek bayangan
            GUI.Label(new Rect(32, radioY + 2, 400, 100), formattedRadio, radioShadow);
            // Teks Asli
            GUI.Label(new Rect(30, radioY, 400, 100), formattedRadio, radioStyle);
        }

        // ==========================================
        // 2. MENGGAMBAR UI LIVE CHAT (Kanan Atas)
        // ==========================================
        // Box Chat diperlebar dan diperpanjang ke bawah
        float chatBoxWidth = 550f;
        float chatBoxHeight = 480f;
        float chatBoxX = 1920f - chatBoxWidth - 20f;
        float chatBoxY = 20f;

        // Gambar Box Chat
        GUI.Box(new Rect(chatBoxX, chatBoxY, chatBoxWidth, chatBoxHeight), "", boxStyle);

        // Judul Chat Box
        GUIStyle chatHeaderStyle = new GUIStyle(style);
        chatHeaderStyle.fontSize = 22; // Header sedikit dibesarkan
        chatHeaderStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(chatBoxX, chatBoxY + 10, chatBoxWidth, 30), "<color=#FF0000>●</color> <b>LIVE CHAT (1,240 Viewers)</b>", chatHeaderStyle);
        
        // Garis Pemisah Chat Header
        GUI.Box(new Rect(chatBoxX + 20, chatBoxY + 45, chatBoxWidth - 40, 2), "", boxStyle);

        // Konten Chat
        GUIStyle chatContentStyle = new GUIStyle(style);
        chatContentStyle.fontSize = 22; // Ukuran teks chat sedikit dibesarkan agar lebih pas dengan box
        chatContentStyle.alignment = TextAnchor.UpperLeft;
        chatContentStyle.wordWrap = true;

        float chatItemY = chatBoxY + 60f;
        foreach (string msg in liveChatMessages)
        {
            // Drop shadow tipis
            GUIStyle shadowStyle = new GUIStyle(chatContentStyle);
            shadowStyle.normal.textColor = Color.black;
            
            float contentHeight = chatContentStyle.CalcHeight(new GUIContent(msg), chatBoxWidth - 40f);

            GUI.Label(new Rect(chatBoxX + 21, chatItemY + 1, chatBoxWidth - 40, contentHeight), msg, shadowStyle);
            GUI.Label(new Rect(chatBoxX + 20, chatItemY, chatBoxWidth - 40, contentHeight), msg, chatContentStyle);
            
            chatItemY += contentHeight + 10f; // Jarak rapi antar chat
        }

        // ==========================================
        // 3. MENGGAMBAR UI INFO TASK (Bawah Live Chat)
        // ==========================================
        float taskBoxY = chatBoxY + chatBoxHeight + 20f;
        float taskBoxHeight = 110f; // Tinggi box info task

        // Box Info Task
        GUI.Box(new Rect(chatBoxX, taskBoxY, chatBoxWidth, taskBoxHeight), "", boxStyle);

        // Judul Box Task
        GUIStyle taskHeader = new GUIStyle(style);
        taskHeader.fontSize = 20;
        taskHeader.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(chatBoxX, taskBoxY + 10, chatBoxWidth, 30), "<b>[ MISSION LOG ]</b>", taskHeader);

        // Garis Pemisah Task
        GUI.Box(new Rect(chatBoxX + 20, taskBoxY + 40, chatBoxWidth - 40, 2), "", boxStyle);

        // Konten Teks Task
        GUIStyle taskInfoStyle = new GUIStyle(style);
        taskInfoStyle.fontSize = 20;
        taskInfoStyle.alignment = TextAnchor.MiddleCenter;
        taskInfoStyle.wordWrap = true;

        // Efek warna untuk Fase Escape
        if (isEscapePhase)
        {
            // Efek kedap-kedip Merah dan Putih dengan cepat untuk panik
            taskInfoStyle.normal.textColor = (Mathf.Sin(Time.time * 8f) > 0) ? Color.red : Color.white; 
        }
        // Efek warna kuning jika baru mendapatkan kunci
        else if (Event.current.type == EventType.Repaint && taskHighlightTimer > 0)
        {
            taskHighlightTimer -= Time.deltaTime;
            taskInfoStyle.normal.textColor = Color.yellow; // Warna menyala
        }
        else
        {
            taskInfoStyle.normal.textColor = Color.white; // Warna biasa
        }

        GUIStyle taskInfoShadow = new GUIStyle(taskInfoStyle);
        taskInfoShadow.normal.textColor = Color.black;
        taskInfoShadow.richText = false;

        GUI.Label(new Rect(chatBoxX + 21, taskBoxY + 51, chatBoxWidth - 40, 50), currentTaskInfo, taskInfoShadow);
        GUI.Label(new Rect(chatBoxX + 20, taskBoxY + 50, chatBoxWidth - 40, 50), currentTaskInfo, taskInfoStyle);
    }
}
