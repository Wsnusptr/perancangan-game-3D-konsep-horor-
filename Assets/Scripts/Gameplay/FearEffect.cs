using UnityEngine;

public class FearEffect : MonoBehaviour
{
    private GameUIManager uiManager;
    private GhostManager ghostManager;
    private Texture2D fearTexture;
    
    [Header("Pengaturan Ketakutan")]
    public float maxFearDistance = 25f; // Jarak maksimum layar mulai memerah
    public float minFearDistance = 4f;  // Jarak terdekat (layar paling gelap & panik)

    void Start()
    {
        uiManager = FindObjectOfType<GameUIManager>();
        ghostManager = FindObjectOfType<GhostManager>();

        // Membuat tekstur warna darah (merah tua gelap) secara otomatis dari script
        fearTexture = new Texture2D(1, 1);
        fearTexture.SetPixel(0, 0, new Color(0.3f, 0f, 0f, 1f)); 
        fearTexture.Apply();
    }

    void OnGUI()
    {
        if (uiManager == null || ghostManager == null) return;
        
        // HANYA AKTIF SAAT FASE KEJAR-KEJARAN (ESCAPE) DAN HANTU MUNCUL
        if (uiManager.isEscapePhase && ghostManager.ghostObject != null && ghostManager.ghostObject.gameObject.activeInHierarchy)
        {
            float dist = Vector3.Distance(ghostManager.playerObject.position, ghostManager.ghostObject.position);
            
            // Jika hantu mulai mendekat di bawah 25 meter
            if (dist < maxFearDistance)
            {
                // Hitung seberapa panik dari 0.0 (aman) sampai 1.0 (sangat dekat)
                float fearFactor = 1f - Mathf.Clamp01((dist - minFearDistance) / (maxFearDistance - minFearDistance));
                
                // Opacity maksimal dikurangi drastis menjadi 35% agar player bebas melihat jalan!
                float alpha = fearFactor * 0.35f;
                
                // Efek Denyut Jantung (Pulsing)
                // Semakin dekat hantunya, detak jantung semakin cepat (2x lipat sampai 12x lipat per detik)
                float heartbeatSpeed = Mathf.Lerp(2f, 12f, fearFactor); 
                float pulse = (Mathf.Sin(Time.time * heartbeatSpeed) * 0.5f + 0.5f) * 0.25f; 
                
                alpha += pulse; // Tambahkan denyutan ke opacity
                alpha = Mathf.Clamp01(alpha); // Cegah nilai lebih dari 100%
                
                // Menggambar efek merah darah di seluruh layar
                Color originalColor = GUI.color;
                GUI.color = new Color(1, 1, 1, alpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fearTexture, ScaleMode.StretchToFill);
                GUI.color = originalColor; 
            }
        }
    }
}
