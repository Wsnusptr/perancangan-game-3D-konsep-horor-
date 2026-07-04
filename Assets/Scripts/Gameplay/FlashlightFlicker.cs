using UnityEngine;
using System.Collections;

public class FlashlightFlicker : MonoBehaviour
{
    public Light flashlight;

    private float normalIntensity = 2.5f;

    void Start()
    {
        // Cari komponen Light secara otomatis jika belum diisi di Inspector
        if (flashlight == null)
        {
            flashlight = GetComponent<Light>();
        }

        // Jika tidak ada di objek ini, cari di anak-anaknya (child)
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        // Jika MASIH TIDAK KETEMU, cari Spotlight apapun yang ada di game sebagai cadangan ekstrem
        if (flashlight == null)
        {
            Light[] allLights = FindObjectsOfType<Light>();
            foreach (Light l in allLights)
            {
                if (l.type == LightType.Spot)
                {
                    flashlight = l;
                    break;
                }
            }
        }

        if (flashlight != null)
        {
            normalIntensity = flashlight.intensity; // Menyimpan seberapa terang senter aslinya
            Debug.Log("Senter Berhasil Ditemukan! Memulai efek rusak...");
            StartCoroutine(FlashlightLogic());
        }
        else
        {
            Debug.LogError("FlashlightFlicker: SENTER TIDAK DITEMUKAN! Pastikan ada komponen Light (Spotlight) di kameramu!");
        }
    }

    // Menggunakan satu alur logika agar efek horornya dramatis dan tidak tumpang tindih
    IEnumerator FlashlightLogic()
    {
        while (true)
        {
            // =====================================
            // FASE 1: NORMAL
            // =====================================
            // Senter menyala terang dan stabil
            // WAKTU DIPERCEPAT agar pemain langsung kaget (1 sampai 4 detik saja)
            flashlight.intensity = normalIntensity;
            yield return new WaitForSeconds(Random.Range(1f, 4f));


            // =====================================
            // FASE 2: KEDAP-KEDIP RUSAK (MATI-NYALA)
            // =====================================
            // Kedap-kedip secara brutal sebanyak 6 sampai 15 kali
            int flickerCount = Random.Range(6, 15);
            for (int i = 0; i < flickerCount; i++)
            {
                // Mati / Sangat Redup
                flashlight.intensity = Random.Range(0f, 0.2f);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                
                // Nyala Normal
                flashlight.intensity = normalIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }

            // =====================================
            // FASE 3: MATI TOTAL (BLACKOUT)
            // =====================================
            // Ada peluang 60% setelah kedap-kedip, senternya malah mati total!
            if (Random.value > 0.4f) 
            {
                flashlight.intensity = 0f; // Gelap gulita
                
                // Menunggu dalam kegelapan selama 1.5 sampai 3 detik
                yield return new WaitForSeconds(Random.Range(1.5f, 3.5f));
                
                // Sedikit sentuhan horor: Nyala 1 frame lalu mati lagi sebelum benar-benar normal
                flashlight.intensity = normalIntensity;
                yield return new WaitForSeconds(0.1f);
                flashlight.intensity = 0f;
                yield return new WaitForSeconds(0.1f);
            }
            
            // Loop akan kembali ke atas (Senter kembali menyala normal)
        }
    }
}
