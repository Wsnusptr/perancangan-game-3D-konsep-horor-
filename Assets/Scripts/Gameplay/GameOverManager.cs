using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // WAJIB untuk menggunakan sistem Video MP4

public class GameOverManager : MonoBehaviour
{
    [Header("Pengaturan Tampilan")]
    public Font titleCustomFont; // Font khusus judul (YOU DIED)
    public Font buttonCustomFont; // Font khusus tombol di bawah

    [Header("Pengaturan Video Jumpscare")]
    [Tooltip("Centang kotak ini jika ingin UI 'YOU DIED' disembunyikan dan baru muncul SETELAH video MP4 tamat.")]
    public bool tungguVideoSelesai = true; 

    private Texture2D blackTexture;
    private Texture2D buttonTexture;
    private Texture2D hoverTexture;
    
    private VideoPlayer videoPlayer;
    private bool uiBolehMuncul = false;

    // UI Responsive
    private Vector2 baseResolution = new Vector2(1920, 1080);

    void Start()
    {
        // Pastikan kursor mouse muncul agar pemain bisa menekan tombol UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Coba cari komponen VideoPlayer (bisa di objek ini, atau objek kamera)
        videoPlayer = FindObjectOfType<VideoPlayer>();
        
        // BACA PENYEBAB KEMATIAN
        string causeOfDeath = PlayerPrefs.GetString("CauseOfDeath", "Ghost");

        // Cek apakah video MP4 sudah terpasang DAN matinya karena diterkam hantu
        if (videoPlayer != null && videoPlayer.clip != null && causeOfDeath == "Ghost")
        {
            if (tungguVideoSelesai)
            {
                uiBolehMuncul = false; // Sembunyikan UI sementara
                videoPlayer.loopPointReached += SaatVideoTamat; // Akan dipanggil pas video beres
            }
            else
            {
                uiBolehMuncul = true; // Munculkan UI bersamaan dengan video
            }
        }
        else
        {
            // Jika matinya karena jatuh (atau video tidak ada), matikan video player!
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                videoPlayer.enabled = false; 
            }
            uiBolehMuncul = true; // Langsung munculkan UI layar hitam
        }

        // Buat tekstur background hitam pekat
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, new Color(0, 0, 0, 1f));
        blackTexture.Apply();

        // Buat tekstur tombol (merah gelap)
        buttonTexture = new Texture2D(1, 1);
        buttonTexture.SetPixel(0, 0, new Color(0.2f, 0, 0, 1f));
        buttonTexture.Apply();

        // Buat tekstur hover tombol (merah terang saat disentuh mouse)
        hoverTexture = new Texture2D(1, 1);
        hoverTexture.SetPixel(0, 0, new Color(0.6f, 0, 0, 1f));
        hoverTexture.Apply();
    }

    // Fungsi ini dipanggil otomatis oleh Unity saat durasi video MP4 sudah habis
    void SaatVideoTamat(VideoPlayer vp)
    {
        uiBolehMuncul = true;
    }

    void OnGUI()
    {
        // === SISTEM UI RESPONSIVE (Auto-Scale) ===
        Vector3 scale = new Vector3(Screen.width / baseResolution.x, Screen.height / baseResolution.y, 1.0f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        // Hanya gambar background hitam JIKA TIDAK ADA VIDEO atau video sudah berhenti
        if (videoPlayer == null || (!videoPlayer.isPlaying && !videoPlayer.isPrepared))
        {
            GUI.DrawTexture(new Rect(0, 0, 1920, 1080), blackTexture, ScaleMode.StretchToFill);
        }

        // Jika settingan 'tungguVideoSelesai' di-centang dan video belum beres, JANGAN GAMBAR TULISAN & TOMBOL
        if (!uiBolehMuncul) return;

        // 2. Teks Judul Kematian
        GUIStyle titleStyle = new GUIStyle();
        if (titleCustomFont != null) titleStyle.font = titleCustomFont;
        titleStyle.fontSize = 150;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.red;
        
        // Efek bayangan untuk teks
        GUIStyle titleShadow = new GUIStyle(titleStyle);
        if (titleCustomFont != null) titleShadow.font = titleCustomFont;
        titleShadow.normal.textColor = new Color(0.3f, 0, 0);
        
        GUI.Label(new Rect(0, 155, 1920, 300), "YOU DIED", titleShadow);
        GUI.Label(new Rect(0, 150, 1920, 300), "YOU DIED", titleStyle);

        // 3. Pengaturan Tombol
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        if (buttonCustomFont != null) buttonStyle.font = buttonCustomFont;
        buttonStyle.fontSize = 28; // Diperkecil sesuai permintaan
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.yellow; 
        buttonStyle.normal.background = buttonTexture;
        buttonStyle.hover.background = hoverTexture;
        buttonStyle.active.background = hoverTexture;

        float buttonWidth = 350; // Lebar tombol diperkecil
        float buttonHeight = 70; // Tinggi tombol diperkecil
        float gap = 40; // Jarak antar tombol
        
        // Hitung posisi awal agar kedua tombol sejajar di tengah secara horizontal
        float totalWidth = (buttonWidth * 2) + gap;
        float startX = (1920 / 2) - (totalWidth / 2);
        float buttonY = 600;

        // 4. Tombol Retry (Kiri)
        if (GUI.Button(new Rect(startX, buttonY, buttonWidth, buttonHeight), "MAIN LAGI", buttonStyle))
        {
            RetryGame();
        }

        // 5. Tombol Quit (Kanan)
        if (GUI.Button(new Rect(startX + buttonWidth + gap, buttonY, buttonWidth, buttonHeight), "KEMBALI KE MENU", buttonStyle))
        {
            QuitToMenu();
        }
    }

    private void RetryGame()
    {
        Debug.Log("RETRY DITEKAN! Memuat ulang game...");
        // Jangan hapus Checkpoint!
        SceneManager.LoadScene("explore-game");
    }

    private void QuitToMenu()
    {
        // Hapus checkpoint karena pemain menyerah
        PlayerPrefs.DeleteKey("Checkpoint");
        PlayerPrefs.Save();
        
        Debug.Log("QUIT DITEKAN! Checkpoint dihapus, kembali ke Intro...");
        SceneManager.LoadScene("main-intro");
    }
}
