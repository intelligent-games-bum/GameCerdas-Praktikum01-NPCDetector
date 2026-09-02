using UnityEngine;

/// <summary>
/// State Enemy, diurutkan dari paling tenang ke paling waspada.
/// Tiga state ini membentuk finite state machine paling sederhana:
/// transisinya ditentukan sepenuhnya oleh jarak ke Player.
/// </summary>
public enum EnemyState
{
    Idle,
    Suspicious,
    Alert
}

/// <summary>
/// Seberapa sering perubahan kondisi dicatat ke Console.
/// </summary>
public enum DetectorLogMode
{
    /// <summary>Tidak mencatat apa pun.</summary>
    Off,

    /// <summary>Mencatat hanya saat state berpindah. Pilihan yang dianjurkan.</summary>
    OnStateChange,

    /// <summary>
    /// Mencatat jarak setiap frame. Berguna untuk memeriksa perhitungan
    /// perception, tetapi membanjiri Console dan membebani performa.
    /// Nyalakan hanya sementara saat sedang menelusuri masalah.
    /// </summary>
    EveryFrame
}

/// <summary>
/// AI sederhana: Perception - Decision - Action.
///
/// Perception : menghitung jarak Enemy ke Player.
/// Decision   : membandingkan jarak dengan dua parameter radius.
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
    [Tooltip("Jarak terluar. Player di dalamnya membuat Enemy menjadi Suspicious.")]
    private float suspiciousRadius = 8f;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Jarak terdalam. Player di dalamnya membuat Enemy menjadi Alert.")]
    private float alertRadius = 4f;

    [Header("Action")]
    [SerializeField]
    [Tooltip("Point Light anak dari Enemy. Warnanya berubah mengikuti state.")]
    private Light enemyLight;

    [SerializeField]
    [Tooltip("Warna saat Player berada di luar semua radius.")]
    private Color idleColor = Color.blue;

    [SerializeField]
    [Tooltip("Warna saat Player berada di antara alertRadius dan suspiciousRadius.")]
    private Color suspiciousColor = Color.yellow;

    [SerializeField]
    [Tooltip("Warna saat Player berada di dalam alertRadius.")]
    private Color alertColor = Color.red;

    [Header("Debug (read only)")]
    [SerializeField]
    [Tooltip("Jarak Enemy ke Player pada frame ini.")]
    private float currentDistance;

    [SerializeField]
    [Tooltip("State Enemy pada frame ini.")]
    private EnemyState currentState = EnemyState.Idle;

    [SerializeField]
    [Tooltip("Seberapa sering Console mencatat. EveryFrame hanya untuk penelusuran sementara.")]
    private DetectorLogMode logMode = DetectorLogMode.EveryFrame;

    /// <summary>
    /// State terkini, dibaca oleh komponen lain seperti EnemyPatrol.
    /// </summary>
    public EnemyState CurrentState => currentState;

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

    /// <summary>
    /// Menjaga agar radius Alert tidak pernah melebihi radius Suspicious.
    /// Bila tertukar, state Suspicious tidak akan pernah tercapai.
    /// </summary>
    private void OnValidate()
    {
        if (alertRadius > suspiciousRadius)
        {
            alertRadius = suspiciousRadius;
        }
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

        if (logMode == DetectorLogMode.EveryFrame)
        {
            Debug.Log(
                $"[Frame {Time.frameCount}] distance: {currentDistance:F2} m, " +
                $"state: {currentState}",
                this);
        }
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
    ///
    /// Urutan pemeriksaan penting: radius terkecil diuji lebih dulu,
    /// karena Player yang berada di dalam alertRadius otomatis juga
    /// berada di dalam suspiciousRadius.
    /// </summary>
    private void Decide()
    {
        EnemyState newState;

        if (currentDistance <= alertRadius)
        {
            newState = EnemyState.Alert;
        }
        else if (currentDistance <= suspiciousRadius)
        {
            newState = EnemyState.Suspicious;
        }
        else
        {
            newState = EnemyState.Idle;
        }

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
        EnemyState previousState = currentState;
        currentState = newState;

        if (logMode == DetectorLogMode.OnStateChange)
        {
            Debug.Log(
                $"Enemy State: {previousState} -> {currentState} " +
                $"(distance: {currentDistance:F2} m)",
                this);
        }

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

        enemyLight.color = GetStateColor(state);
    }

    private Color GetStateColor(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Alert:
                return alertColor;

            case EnemyState.Suspicious:
                return suspiciousColor;

            default:
                return idleColor;
        }
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
        // Dua lingkaran batas, masing-masing memakai warna state
        // yang akan aktif bila Player memasukinya.
        Gizmos.color = suspiciousColor;
        Gizmos.DrawWireSphere(transform.position, suspiciousRadius);

        Gizmos.color = alertColor;
        Gizmos.DrawWireSphere(transform.position, alertRadius);

        if (player == null)
        {
            return;
        }

        // Garis bantu Enemy ke Player, diwarnai sesuai state saat ini
        // agar hasil keputusan AI langsung terbaca di Scene View.
        Gizmos.color = GetStateColor(currentState);
        Gizmos.DrawLine(transform.position, player.position);
    }
}
