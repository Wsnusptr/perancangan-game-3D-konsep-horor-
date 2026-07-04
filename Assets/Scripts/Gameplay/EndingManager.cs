using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [Header("Referensi Cutscene")]
    [Tooltip("Kamera sinematik yang menyorot mobil")]
    public GameObject endingCamera;
    [Tooltip("Karakter Player yang akan dimatikan")]
    public GameObject playerObject;
    [Tooltip("UI Game Utama (GameUIManager) yang akan disembunyikan")]
    public GameObject mainCanvasUI;

    [Header("Pengaturan Mobil")]
    [Tooltip("Tarik model mobilmu ke sini")]
    public Transform carTransform;
    public float carSpeed = 5f;
    public float acceleration = 2.5f;

    [Header("Pengaturan Efek")]
    public float delayBeforeFade = 2.5f;
    public float fadeDuration = 3f;

    [Header("UI Teks")]
    [Tooltip("Tarik font untuk tulisan YOU ESCAPED")]
    public Font fontEscape;
    [Tooltip("Tarik font untuk tombol MAIN MENU")]
    public Font fontMainMenu;
    
    private bool isFading = false;
    private float fadeAlpha = 0f;
    private bool showEndUI = false;
    private bool isEndingActive = false;

    private Texture2D blackTexture;
    private Texture2D buttonTexture;
    private Texture2D hoverTexture;

    void Start()
    {
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, new Color(0, 0, 0, 1f));
        blackTexture.Apply();

        buttonTexture = new Texture2D(1, 1);
        buttonTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.9f));
        buttonTexture.Apply();

        hoverTexture = new Texture2D(1, 1);
        hoverTexture.SetPixel(0, 0, new Color(0.4f, 0.0f, 0.0f, 1f));
        hoverTexture.Apply();

        // Pastikan kamera ending mati saat game baru mulai
        if (endingCamera != null) endingCamera.SetActive(false);
    }

    // Fungsi ini dipanggil oleh EscapeZone.cs
    public void MulaiEnding()
    {
        isEndingActive = true;

        // Matikan efek denyut nadi/darah secara paksa di manapun scriptnya menempel
        FearEffect fearEffect = FindObjectOfType<FearEffect>();
        if (fearEffect != null) fearEffect.enabled = false;

        // 1. Matikan player
        if (playerObject != null) playerObject.SetActive(false);
        
        // 2. Sembunyikan UI
        if (mainCanvasUI != null) mainCanvasUI.SetActive(false);

        // 3. Nyalakan Kamera
        if (endingCamera != null) endingCamera.SetActive(true);

        // 4. Bebaskan mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(EndingSequence());
    }

    void Update()
    {
        if (isEndingActive && carTransform != null)
        {
            carSpeed += acceleration * Time.deltaTime;
            carTransform.Translate(Vector3.forward * carSpeed * Time.deltaTime);
        }
    }

    IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        isFading = true;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        fadeAlpha = 1f;
        showEndUI = true;
    }

    void OnGUI()
    {
        if (isFading)
        {
            Color original = GUI.color;
            GUI.color = new Color(1, 1, 1, fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture, ScaleMode.StretchToFill);
            GUI.color = original;
        }

        if (showEndUI)
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            if (fontEscape != null) titleStyle.font = fontEscape;
            titleStyle.fontSize = Screen.width / 15;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);

            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 100f), "YOU ESCAPED", titleStyle);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            if (fontMainMenu != null) btnStyle.font = fontMainMenu;
            btnStyle.fontSize = Screen.width / 45;
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.normal.textColor = Color.white;
            btnStyle.normal.background = buttonTexture;
            btnStyle.hover.textColor = Color.yellow;
            btnStyle.hover.background = hoverTexture;

            float btnW = Screen.width * 0.15f;
            float btnH = Screen.height * 0.08f;
            // Tombol diturunkan lebih ke bawah (Screen.height * 0.7f)
            if (GUI.Button(new Rect((Screen.width - btnW) / 2f, Screen.height * 0.7f, btnW, btnH), "MAIN MENU", btnStyle))
            {
                PlayerPrefs.DeleteKey("Checkpoint");
                PlayerPrefs.Save();
                SceneManager.LoadScene("main-intro");
            }
        }
    }
}
