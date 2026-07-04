using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SynopsisManager : MonoBehaviour
{
    [Header("Pengaturan Tampilan")]
    [Tooltip("Masukkan font kustom dari komputermu ke sini (opsional)")]
    public Font customFont;
    [Tooltip("Ukuran teks cerita (Makin kecil angkanya, makin kecil teksnya)")]
    [Range(0.01f, 0.1f)]
    public float textScale = 0.035f; // Diperkecil
    [Tooltip("Ukuran teks pada tombol Next/Skip")]
    [Range(0.01f, 0.1f)]
    public float buttonTextScale = 0.025f; // Diperkecil

    [Header("Pengaturan Cerita")]
    [TextArea(5, 10)]
    public string[] storyParagraphs = new string[] {
        "Tahun 20XX. Tragedi Gempa Bumi menghancurkan gedung STIE & STMIK Jayakarta...",
        "Banyak jiwa yang tidak tenang tertinggal di lorong-lorong gelap ini.",
        "Terutama seorang siswi...",
        "Namanya Shofy."
    };
    
    public float typingSpeed = 0.05f;
    public string exploreSceneName = "explore-game";

    private int currentParagraph = 0;
    private bool isTyping = false;
    private string currentText = "";
    private Coroutine typingCoroutine;

    void Start()
    {
        // Secara otomatis mengubah background Kamera menjadi Hitam Pekat (Menghilangkan Skybox)
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;
        }

        // Pastikan kursor muncul agar bisa nge-klik tombol
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (storyParagraphs.Length > 0)
        {
            typingCoroutine = StartCoroutine(TypeStory(storyParagraphs[currentParagraph]));
        }
    }

    IEnumerator TypeStory(string paragraph)
    {
        isTyping = true;
        currentText = "";
        
        foreach (char c in paragraph)
        {
            currentText += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // Menggambar layar hitam, teks, dan tombol secara otomatis lewat kode
    void OnGUI()
    {
        // 1. Buat background hitam menutupi seluruh layar
        GUI.backgroundColor = Color.black;
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), ""); // Dikalikan agar benar-benar pekat

        // 2. Buat style teks
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        if (customFont != null) textStyle.font = customFont;
        textStyle.normal.textColor = Color.white;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontSize = Mathf.RoundToInt(Screen.height * textScale); // Ukuran lebih kecil
        textStyle.wordWrap = true;

        // 3. Buat style tombol
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        if (customFont != null) buttonStyle.font = customFont;
        buttonStyle.fontSize = Mathf.RoundToInt(Screen.height * buttonTextScale);

        // 4. Tampilkan teks di tengah layar
        Rect textRect = new Rect(Screen.width * 0.1f, Screen.height * 0.2f, Screen.width * 0.8f, Screen.height * 0.6f);
        GUI.Label(textRect, currentText, textStyle);

        // 5. Hitung ukuran tombol agar responsif (mengembang sesuai layar)
        float btnWidth = Screen.width * 0.15f; // Lebar 15% dari layar
        float btnHeight = Screen.height * 0.08f; // Tinggi 8% dari layar
        float paddingBottom = Screen.height * 0.05f; // Jarak 5% dari bawah layar
        
        // Tombol SKIP di pojok kiri bawah
        Rect skipRect = new Rect(Screen.width * 0.05f, Screen.height - btnHeight - paddingBottom, btnWidth, btnHeight);
        if (GUI.Button(skipRect, "SKIP", buttonStyle))
        {
            SceneManager.LoadScene(exploreSceneName);
        }

        // 6. Tombol NEXT di pojok kanan bawah
        Rect nextRect = new Rect(Screen.width - btnWidth - (Screen.width * 0.05f), Screen.height - btnHeight - paddingBottom, btnWidth, btnHeight);
        if (isTyping)
        {
            if (GUI.Button(nextRect, "FAST FORWARD", buttonStyle))
            {
                StopCoroutine(typingCoroutine);
                currentText = storyParagraphs[currentParagraph];
                isTyping = false;
            }
        }
        else
        {
            if (GUI.Button(nextRect, "NEXT", buttonStyle))
            {
                currentParagraph++;
                if (currentParagraph < storyParagraphs.Length)
                {
                    typingCoroutine = StartCoroutine(TypeStory(storyParagraphs[currentParagraph]));
                }
                else
                {
                    SceneManager.LoadScene(exploreSceneName);
                }
            }
        }
    }
}
