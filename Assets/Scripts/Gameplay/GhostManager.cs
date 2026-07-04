using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class GhostManager : MonoBehaviour
{
    [Header("Referensi")]
    public Transform ghostObject;
    public Transform playerObject;
    public Transform[] spawnNodes;

    [Header("Pengaturan Jarak & Waktu")]
    public float disappearDistance = 4f; // Jarak hantu menghilang saat didekati
    public float minSpawnDistance = 8f; // Jarak minimum spawn
    public float maxAggroDistance = 40f; // Jarak maksimum sebelum teleport
    public float respawnTime = 3f; // Jeda muncul
    public float ghostBoredTime = 10f; // Waktu bosan

    [Header("Perbaikan Posisi & Rotasi")]
    public Vector3 rotationOffset = new Vector3(0, 0, 0);

    [Header("Animasi Hantu")]
    public Animator ghostAnimator;

    [Header("Sistem Kejar-kejaran (NavMesh)")]
    public float chaseSpeed = 5.5f;
    private NavMeshAgent navAgent;
    private GameUIManager uiManager;
    private bool chaseStarted = false;

    private bool isGhostActive = false;
    private float activeTimer = 0f;

    void Start()
    {
        // PERBAIKAN BUG INSPECTOR: Paksa nilai minimal agar tidak nge-bug jika diset kerendahan di Unity
        if (maxAggroDistance < 40f) maxAggroDistance = 45f;
        if (minSpawnDistance < 8f) minSpawnDistance = 8f;
        if (disappearDistance < 3f) disappearDistance = 3f;

        // Cari Player
        if (playerObject == null)
        {
            PlayerMovement pm = FindObjectOfType<PlayerMovement>();
            if (pm != null) playerObject = pm.transform;
            else
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerObject = p.transform;
            }
        }

        uiManager = FindObjectOfType<GameUIManager>();

        if (ghostObject != null)
        {
            if (ghostAnimator == null)
            {
                ghostAnimator = ghostObject.GetComponent<Animator>();
                if (ghostAnimator == null) ghostAnimator = ghostObject.GetComponentInChildren<Animator>();
            }

            // Setup NavMeshAgent
            navAgent = ghostObject.GetComponent<NavMeshAgent>();
            if (navAgent == null) navAgent = ghostObject.gameObject.AddComponent<NavMeshAgent>();
            navAgent.speed = chaseSpeed;
            navAgent.stoppingDistance = 1.5f;
            navAgent.enabled = false;

            ghostObject.gameObject.SetActive(false);
            StartCoroutine(GhostRoutine());
        }
    }

    private float stuckTimer = 0f; // Timer untuk mendeteksi apakah hantu nyangkut

    void Update()
    {
        // ==========================================
        // MODE KEJAR-KEJARAN (ESCAPE PHASE)
        // ==========================================
        if (uiManager != null && uiManager.isEscapePhase)
        {
            if (!chaseStarted)
            {
                chaseStarted = true;
                isGhostActive = true;
                ghostObject.gameObject.SetActive(true);
                
                // Munculkan hantu dari NODE terdekat agar aman (tidak spawn di udara)
                Transform bestNode = null;
                float bestScore = float.MaxValue;

                // Gunakan 'spawnNodes', bukan 'allNodes' (perbaikan error CS0103)
                foreach (Transform node in spawnNodes)
                {
                    if (node == null) continue;
                    float distanceToPlayer = Vector3.Distance(node.position, playerObject.position);
                    float yDifference = Mathf.Abs(node.position.y - playerObject.position.y);
                    
                    if (distanceToPlayer < 10f) continue; // Jangan terlalu nempel
                    
                    float penalty = distanceToPlayer + (yDifference * 50f); // Paling penting harus SELANTAI
                    if (penalty < bestScore)
                    {
                        bestScore = penalty;
                        bestNode = node;
                    }
                }
                
                if (bestNode != null)
                {
                    ghostObject.position = bestNode.position;
                }
                else
                {
                    ghostObject.position = playerObject.position - playerObject.forward * 12f;
                }
                
                ghostObject.LookAt(playerObject);
                navAgent.enabled = true; // Aktifkan sistem berjalan NavMesh
                
                if (ghostAnimator != null) ghostAnimator.CrossFade("run-ghost", 0.2f);
            }
            
            if (chaseStarted && navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(playerObject.position);

                // SISTEM ANTI-NYANGKUT (TELEPORT OTOMATIS SAAT DIKEJAR)
                float dist = Vector3.Distance(playerObject.position, ghostObject.position);
                float yDiff = Mathf.Abs(playerObject.position.y - ghostObject.position.y);
                
                // Jika hantu nyangkut tembok (PathInvalid), ketinggalan terlalu jauh (>35m), atau tertinggal di lantai beda
                if (navAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete || dist > 35f || yDiff > 2.5f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 3f) // Beri waktu 3 detik. Kalau masih stuck, TELEPORT!
                    {
                        Debug.Log("Hantu: Jalan buntu / Tertinggal jauh! TELEPORT MENGEJAR!");
                        navAgent.enabled = false;
                        chaseStarted = false; // Memancing sistem spawn ulang di atas
                        stuckTimer = 0f;
                    }
                }
                else
                {
                    stuckTimer = 0f; // Reset jika jalan lancar
                }
            }
            
            return; // Hentikan logika stalking di bawah
        }

        // ==========================================
        // MODE STALKING & TELEPORT (MENCARI KUNCI)
        // ==========================================
        if (isGhostActive && ghostObject != null && playerObject != null)
        {
            float dist = Vector3.Distance(playerObject.position, ghostObject.position);
            activeTimer += Time.deltaTime;
            
            // 1. Jika pemain mendekat -> HILANG (Jumpscare)
            if (dist < disappearDistance)
            {
                Debug.Log("Hantu: Player terlalu dekat (" + dist + "m). MENGHILANG!");
                DespawnGhost();
            }
            // 2. Jika pemain lari terlalu jauh -> TELEPORT
            // Beri jeda 3 detik setelah spawn agar tidak terjadi teleport beruntun
            else if (dist > maxAggroDistance && activeTimer > 3f)
            {
                Debug.Log("Hantu: Player menjauh (Jarak: " + dist + "m). TELEPORT MENGEJAR!");
                DespawnGhost();
            }
            // 3. Stalking & Bosan
            else
            {
                Vector3 lookDirection = playerObject.position - ghostObject.position;
                lookDirection.y = 0; 

                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    targetRotation *= Quaternion.Euler(rotationOffset); 
                    ghostObject.rotation = Quaternion.Slerp(ghostObject.rotation, targetRotation, Time.deltaTime * 2f);
                }

                if (activeTimer >= ghostBoredTime)
                {
                    Debug.Log("Hantu: Bosan diam 10 detik. PINDAH POSISI!");
                    DespawnGhost();
                }
            }
        }
    }

    void DespawnGhost()
    {
        ghostObject.gameObject.SetActive(false);
        isGhostActive = false;
        activeTimer = 0f;
    }

    IEnumerator GhostRoutine()
    {
        while (true)
        {
            // Jangan jalankan teleporting lagi jika sudah fase melarikan diri
            if (uiManager != null && uiManager.isEscapePhase)
            {
                yield return null;
                continue;
            }

            if (!isGhostActive && playerObject != null && spawnNodes != null && spawnNodes.Length > 0)
            {
                yield return new WaitForSeconds(respawnTime);
                
                Transform bestNode = FindBestSpawnNode();
                if (bestNode != null)
                {
                    // PERBAIKAN BUG RAYCAST: Batasi jarak raycast hanya 2.5 meter ke bawah.
                    // Jika terlalu panjang, hantu bisa tembus ke lantai bawahnya!
                    Vector3 rayStart = bestNode.position + Vector3.up * 1f; 
                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2.5f))
                    {
                        ghostObject.position = hit.point;
                    }
                    else
                    {
                        ghostObject.position = bestNode.position;
                    }
                    
                    Vector3 lookPos = new Vector3(playerObject.position.x, ghostObject.position.y, playerObject.position.z);
                    ghostObject.LookAt(lookPos);
                    ghostObject.Rotate(rotationOffset, Space.Self);
                    
                    ghostObject.gameObject.SetActive(true);
                    isGhostActive = true;
                    activeTimer = 0f;

                    // PERBAIKAN BUG ANIMASI: 
                    // Kita harus menunggu 1 frame agar Animator selesai loading saat hantu dimunculkan
                    // Jika tidak, perintah CrossFade akan dibatalkan oleh Unity.
                    yield return null;

                    Debug.Log("Hantu: MUNCUL di " + bestNode.name + " (Lantai Player Y: " + playerObject.position.y.ToString("F1") + ", Lantai Hantu Y: " + ghostObject.position.y.ToString("F1") + ")");

                    if (ghostAnimator != null)
                    {
                        // Tingkatkan peluang melambai menjadi 70% agar lebih sering terlihat!
                        if (Random.value > 0.3f) ghostAnimator.CrossFade("melambai", 0.1f);
                        else ghostAnimator.CrossFade("bernafas-engap", 0.1f);
                    }
                }
            }
            yield return null;
        }
    }

    Transform FindBestSpawnNode()
    {
        List<Transform> goodNodes = new List<Transform>();

        foreach (Transform node in spawnNodes)
        {
            if (node == null) continue;
            
            float dist = Vector3.Distance(playerObject.position, node.position);
            float yDiff = Mathf.Abs(playerObject.position.y - node.position.y);
            
            // SYARAT MUTLAK NODE IDEAL:
            if (dist <= minSpawnDistance) continue; // Jangan terlalu dekat
            if (dist >= maxAggroDistance) continue; // Jangan terlalu jauh
            if (yDiff > 2.5f) continue; // HARUS DI LANTAI YANG SAMA (Toleransi tinggi 2.5m)
            
            goodNodes.Add(node);
        }

        // Jika ada node yang memenuhi semua syarat, pilih acak
        if (goodNodes.Count > 0)
        {
            return goodNodes[Random.Range(0, goodNodes.Count)];
        }

        // CADANGAN CERDAS JIKA TIDAK ADA NODE IDEAL (Sistem Scoring)
        // Kita akan menilai semua node. Semakin besar skornya, semakin buruk node tersebut.
        Transform bestBackupNode = null;
        float lowestPenaltyScore = float.MaxValue;
        
        foreach (Transform node in spawnNodes)
        {
            if (node == null) continue;
            
            float dist = Vector3.Distance(playerObject.position, node.position);
            float yDiff = Mathf.Abs(playerObject.position.y - node.position.y);
            
            // JANGAN PERNAH pilih node yang bikin hantu langsung numbur player
            if (dist <= disappearDistance + 1f) continue;
            
            // PENILAIAN PENALTI:
            // Beda lantai sangat dibenci (Dikali 50)
            // Jarak yang jauh sedikit dibenci (Dikali 1)
            float score = dist + (yDiff * 50f);
            
            if (score < lowestPenaltyScore)
            {
                lowestPenaltyScore = score;
                bestBackupNode = node;
            }
        }

        return bestBackupNode;
    }
}
