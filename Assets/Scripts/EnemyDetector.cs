using UnityEngine;

/// <summary>
/// State sederhana milik Enemy.
/// Untuk tantangan tambahan, state Suspicious bisa disisipkan di antara keduanya.
/// </summary>
public enum EnemyState
{
    Idle,
    Alert
}

/// <summary>
/// AI sederhana: Perception - Decision - Action.
///
/// Perception : menghitung jarak Enemy ke Player.
/// Decision   : membandingkan jarak dengan detectionRadius.
/// Action     : mengubah warna Point Light sesuai state.
/// </summary>
public class EnemyDetector : MonoBehaviour
{
    [Header("Perception")]
    [SerializeField]
    [Tooltip("Referensi Transform Player. Drag object Player dari Hierarchy.")]
    private Transform player;

    [Header("Parameter AI")]
    [SerializeField]
    [Min(0f)]
    [Tooltip("Jarak maksimum Enemy masih dapat mendeteksi Player.")]
    private float detectionRadius = 5f;

    [Header("Action")]
    [SerializeField]
    [Tooltip("Point Light anak dari Enemy. Warnanya berubah mengikuti state.")]
    private Light enemyLight;

    [SerializeField]
    [Tooltip("Warna lampu saat Player berada di luar detection radius.")]
    private Color idleColor = Color.blue;

    [SerializeField]
    [Tooltip("Warna lampu saat Player berada di dalam detection radius.")]
    private Color alertColor = Color.red;

    [Header("Debug (read only)")]
    [SerializeField]
    [Tooltip("Jarak Enemy ke Player pada frame ini.")]
    private float currentDistance;

    [SerializeField]
    [Tooltip("State Enemy pada frame ini.")]
    private EnemyState currentState = EnemyState.Idle;

    [Header("Gizmos")]
    [SerializeField]
    [Tooltip("Tampilkan radius deteksi walau Enemy tidak sedang dipilih.")]
    private bool alwaysDrawGizmos = true;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "[EnemyDetector] Field 'player' belum diisi. " +
                "Drag object Player ke Inspector Enemy.", this);
        }

        if (enemyLight == null)
        {
            Debug.LogError(
                "[EnemyDetector] Field 'enemyLight' belum diisi. " +
                "Drag Point Light anak Enemy ke Inspector.", this);
        }

        // Terapkan warna awal supaya tampilan konsisten sejak frame pertama.
        ApplyStateColor(currentState);
    }

    private void Update()
    {
        // Tanpa referensi Player, perception tidak dapat dijalankan.
        if (player == null)
        {
            return;
        }

        Perceive();
        Decide();
    }

    /// <summary>
    /// PERCEPTION - membaca informasi dari environment.
    /// Sensor yang dipakai di praktikum ini adalah jarak.
    /// </summary>
    private void Perceive()
    {
        currentDistance = Vector3.Distance(transform.position, player.position);
    }

    /// <summary>
    /// DECISION - membandingkan hasil perception dengan parameter AI.
    /// </summary>
    private void Decide()
    {
        EnemyState newState = currentDistance <= detectionRadius
            ? EnemyState.Alert
            : EnemyState.Idle;

        // Hanya bereaksi bila state benar-benar berubah, agar Console
        // tidak dibanjiri log setiap frame.
        if (newState != currentState)
        {
            SetState(newState);
        }
    }

    /// <summary>
    /// Mencatat state baru lalu menjalankan aksinya.
    /// </summary>
    private void SetState(EnemyState newState)
    {
        currentState = newState;

        Debug.Log(
            $"Enemy State -> {currentState} (distance: {currentDistance:F2} m)",
            this);

        ApplyStateColor(currentState);
    }

    /// <summary>
    /// ACTION - respons yang terlihat oleh pemain.
    /// </summary>
    private void ApplyStateColor(EnemyState state)
    {
        if (enemyLight == null)
        {
            return;
        }

        enemyLight.color = state == EnemyState.Alert ? alertColor : idleColor;
    }

    /// <summary>
    /// Visualisasi radius deteksi di Scene View.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!alwaysDrawGizmos)
        {
            return;
        }

        DrawDetectionGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (alwaysDrawGizmos)
        {
            // Sudah digambar oleh OnDrawGizmos, hindari menggambar dua kali.
            return;
        }

        DrawDetectionGizmos();
    }

    private void DrawDetectionGizmos()
    {
        // Lingkaran radius deteksi, ikut berganti warna mengikuti state.
        Gizmos.color = currentState == EnemyState.Alert ? alertColor : idleColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (player == null)
        {
            return;
        }

        // Garis bantu Enemy ke Player untuk membaca jarak secara visual.
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
