using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    [Tooltip("Nama scene sinopsis yang akan dimuat saat tombol Play ditekan")]
    public string synopsisSceneName = "SynopsisScene";
    [Tooltip("Nama scene game utama (untuk opsi Lanjutkan)")]
    public string exploreSceneName = "explore-game";

    [Header("Tampilan UI Popup")]
    public Font customFont;

    // Flag untuk menampilkan menu OnGUI
    private bool showOptions = false;
    private bool showPlayPopup = false;

    // Variabel setting sementara
    private float masterVolume = 1.0f;
    private float mouseSensitivity = 200f;

    public void OnPlayButtonClicked()
    {
        // Mengecek apakah pemain punya data Checkpoint
        if (PlayerPrefs.HasKey("Checkpoint"))
        {
            // Munculkan popup pilihan
            showPlayPopup = true;
            showOptions = false; // Tutup menu option jika sedang terbuka
        }
        else
        {
            // Mulai dari awal secara normal
            Debug.Log("Belum ada save data. Mulai dari awal...");
            SceneManager.LoadScene(synopsisSceneName);
        }
    }

    public void OnOptionsButtonClicked()
    {
        Debug.Log("Membuka Options via OnGUI");
        showOptions = true;
        showPlayPopup = false;
    }

    public void OnQuitButtonClicked()
    {
        Debug.Log("Keluar dari Game");
        Application.Quit();
    }

    // Menggambar UI via kode agar responsif
    void OnGUI()
    {
        // Atur font global jika ada
        if (customFont != null) GUI.skin.font = customFont;

        // ===============================================
        // 1. POPUP MULAI BERMAIN (NEW GAME VS CONTINUE)
        // ===============================================
        if (showPlayPopup)
        {
            float boxWidth = Screen.width * 0.5f; // Lebar 50% dari layar
            float boxHeight = Screen.height * 0.4f; // Tinggi 40% dari layar
            float posX = (Screen.width - boxWidth) / 2f; // Tengah horizontal
            float posY = (Screen.height - boxHeight) / 2f; // Tengah vertikal

            Rect windowRect = new Rect(posX, posY, boxWidth, boxHeight);

            // Kotak background hitam agak transparan
            GUI.color = new Color(0, 0, 0, 0.95f);
            GUI.DrawTexture(windowRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Box(windowRect, ""); // Garis pinggir kotak

            GUILayout.BeginArea(new Rect(windowRect.x + 20, windowRect.y + 20, windowRect.width - 40, windowRect.height - 40));
            
            // Judul Popup
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            if (customFont != null) titleStyle.font = customFont;
            titleStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.035f);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.yellow;
            
            GUILayout.Label("DATA PERMAINAN DITEMUKAN!", titleStyle);
            GUILayout.Space(20);

            // Teks Deskripsi
            GUIStyle descStyle = new GUIStyle(GUI.skin.label);
            if (customFont != null) descStyle.font = customFont;
            descStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.025f);
            descStyle.alignment = TextAnchor.MiddleCenter;
            descStyle.wordWrap = true;
            descStyle.normal.textColor = Color.white;
            
            GUILayout.Label("Kamu sudah mengumpulkan ke-9 kunci. Apakah kamu ingin melanjutkan langsung ke adegan kejar-kejaran, atau mengulang semuanya dari awal?", descStyle);

            GUILayout.FlexibleSpace();

            // Deretan Tombol Utama
            GUILayout.BeginHorizontal();
            
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            if (customFont != null) btnStyle.font = customFont;
            btnStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.022f);

            if (GUILayout.Button("MULAI DARI AWAL", btnStyle, GUILayout.Height(Screen.height * 0.08f)))
            {
                PlayerPrefs.DeleteKey("Checkpoint");
                PlayerPrefs.Save();
                SceneManager.LoadScene(synopsisSceneName);
            }
            
            GUILayout.Space(20);

            if (GUILayout.Button("LANJUTKAN (KEJAR-KEJARAN)", btnStyle, GUILayout.Height(Screen.height * 0.08f)))
            {
                // Jika pilih lanjut, langsung masuk explore-game. Sistem sana akan otomatis load Checkpoint-nya!
                SceneManager.LoadScene(exploreSceneName);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(15);
            
            // Tombol Batal
            if (GUILayout.Button("BATAL", btnStyle, GUILayout.Height(Screen.height * 0.05f)))
            {
                showPlayPopup = false;
            }

            GUILayout.EndArea();
        }

        // ===============================================
        // 2. MENU PENGATURAN (OPTIONS)
        // ===============================================
        if (showOptions && !showPlayPopup)
        {
            float boxWidth = Screen.width * 0.35f; 
            float boxHeight = Screen.height * 0.5f; 
            float paddingLeft = 30f; 
            float paddingTop = (Screen.height - boxHeight) / 2f; 

            Rect windowRect = new Rect(paddingLeft, paddingTop, boxWidth, boxHeight);

            // Kotak background hitam agak transparan
            GUI.color = new Color(0, 0, 0, 0.9f);
            GUI.DrawTexture(windowRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Box(windowRect, "M E N U   P E N G A T U R A N");

            GUILayout.BeginArea(new Rect(windowRect.x + 20, windowRect.y + 40, windowRect.width - 40, windowRect.height - 60));
            
            GUILayout.Label("Master Volume: " + Mathf.RoundToInt(masterVolume * 100) + "%");
            masterVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f);
            
            GUILayout.Space(25);
            
            GUILayout.Label("Mouse Sensitivity: " + Mathf.RoundToInt(mouseSensitivity));
            mouseSensitivity = GUILayout.HorizontalSlider(mouseSensitivity, 50f, 500f);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("TUTUP (CLOSE)", GUILayout.Height(40)))
            {
                showOptions = false;
            }
            
            GUILayout.EndArea();
        }
    }
}
