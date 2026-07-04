using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeZone : MonoBehaviour
{
    private GameUIManager uiManager;
    
    [Header("Hubungkan ke Ending System")]
    [Tooltip("Tarik objek EndingSystem ke kotak ini")]
    public EndingManager endingManager;

    void Start()
    {
        uiManager = FindObjectOfType<GameUIManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Deteksi jika yang masuk ke area ini adalah Player
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            // Pastikan misi melarikan diri sudah aktif (9 kunci terkumpul)
            if (uiManager != null && uiManager.isEscapePhase)
            {
                Debug.Log("PLAYER BERHASIL KABUR! Memulai Cutscene Ending...");
                
                // Panggil sistem cutscene dari EndingManager (langsung di scene ini)
                if (endingManager != null)
                {
                    endingManager.MulaiEnding();
                }
                else
                {
                    Debug.LogError("KAMU LUPA MEMASUKKAN ENDING MANAGER KE DALAM KOTAK ESCAPE ZONE!");
                }
            }
            else
            {
                Debug.Log("Player mencoba kabur, tapi kunci belum lengkap! (Hanya muncul di Log)");
            }
        }
    }

    // Menggambar kotak/bola kuning di Unity Editor agar gampang dicari
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); // Kuning semi-transparan
        
        BoxCollider box = GetComponent<BoxCollider>();
        SphereCollider sphere = GetComponent<SphereCollider>();

        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (sphere != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else
        {
            // Jika user lupa pasang collider, tetap gambar bola kecil sebagai penanda
            Gizmos.DrawSphere(transform.position, 1f);
        }
    }
}
