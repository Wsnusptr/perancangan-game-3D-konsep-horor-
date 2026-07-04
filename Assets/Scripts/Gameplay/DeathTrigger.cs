using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathTrigger : MonoBehaviour
{
    [Tooltip("Centang ini jika ditempel di hantu. Hilangkan centang jika ditempel di kotak bawah tanah (jurang).")]
    public bool isGhost = true; 

    [Tooltip("Semakin tinggi angkanya, semakin lama jatuhnya (contoh: -15f = jatuh sekitar 1.5 detik)")]
    public float batasKecepatanJatuh = -15f;

    private bool isFading = false;
    private float fadeAlpha = 0f;
    private Texture2D blackTexture;

    private PlayerMovement pm;

    void Start()
    {
        // Buat tekstur hitam pekat untuk efek fade
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, new Color(0, 0, 0, 1f));
        blackTexture.Apply();

        // Cari script PlayerMovement secara otomatis
        pm = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        // SISTEM DETEKSI JATUH BERDASARKAN KECEPATAN! (Lebih cerdas, tidak peduli lantai berapa)
        if (pm != null && pm.VerticalVelocity <= batasKecepatanJatuh)
        {
            if (!isFading)
            {
                StartCoroutine(FadeDanGameOver());
            }
        }
    }

    // Jika menggunakan sistem Trigger (IsTrigger dicentang)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            ProsesKematian();
        }
    }

    // Jika menggunakan sistem Collision padat
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            ProsesKematian();
        }
    }

    void ProsesKematian()
    {
        if (isGhost)
        {
            // Jika mati karena hantu, langsung instan pindah scene untuk Jumpscare
            TriggerGameOverInstan("Ghost");
        }
        else
        {
            // Jika mati karena jatuh, jalankan efek memudar perlahan (Fade to Black)
            if (!isFading)
            {
                StartCoroutine(FadeDanGameOver());
            }
        }
    }

    IEnumerator FadeDanGameOver()
    {
        isFading = true;
        Debug.Log("PLAYER JATUH! Memulai efek redup...");

        float fadeDuration = 2.5f; // Durasi layar menjadi hitam (2.5 detik)
        float timer = 0f;

        // Loop untuk menambah kegelapan layar secara perlahan
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeAlpha = Mathf.Clamp01(timer / fadeDuration); // Naik dari 0 ke 1
            yield return null;
        }

        fadeAlpha = 1f; // Pastikan benar-benar hitam pekat

        // Setelah hitam pekat, baru pindah ke Game Over
        TriggerGameOverInstan("Fall");
    }

    void TriggerGameOverInstan(string cause)
    {
        Debug.Log("KEMATIAN: " + cause);
        
        // Simpan penyebab kematian agar bisa dibaca oleh layar Game Over
        PlayerPrefs.SetString("CauseOfDeath", cause);
        PlayerPrefs.Save();

        // Bebaskan kursor mouse agar bisa klik tombol di Game Over
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Pindah ke scene game-over
        SceneManager.LoadScene("game-over");
    }

    void OnGUI()
    {
        // Gambar efek redup di layar jika isFading = true
        if (isFading)
        {
            Color originalColor = GUI.color;
            GUI.color = new Color(1, 1, 1, fadeAlpha); // Alpha bertambah gelap
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture, ScaleMode.StretchToFill);
            GUI.color = originalColor; // Kembalikan warna awal
        }
    }
}
