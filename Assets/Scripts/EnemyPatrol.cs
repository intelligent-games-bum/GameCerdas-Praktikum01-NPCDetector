using UnityEngine;

/// <summary>
/// Membuat Enemy mondar-mandir di antara dua titik.
///
/// Titik patroli dihitung dari posisi awal Enemy saat Play ditekan,
/// sehingga tidak perlu membuat waypoint object di scene.
///
/// Bila komponen EnemyDetector ikut terpasang, patroli dapat dihentikan
/// otomatis saat Enemy menyadari kehadiran Player.
/// </summary>
[RequireComponent(typeof(EnemyDetector))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Jalur Patroli")]
    [SerializeField]
    [Tooltip("Perpindahan dari posisi awal menuju titik ujung patroli.")]
    private Vector3 patrolOffset = new Vector3(0f, 0f, 6f);

    [Header("Gerak")]
    [SerializeField]
    [Min(0f)]
    [Tooltip("Kecepatan patroli dalam unit per detik.")]
    private float moveSpeed = 2f;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Lama diam di setiap ujung sebelum berbalik, dalam detik.")]
    private float waitTime = 1f;

    [SerializeField]
    [Tooltip("Putar badan Enemy menghadap arah jalannya.")]
    private bool faceMovementDirection = true;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Kecepatan berputar saat berbalik arah, dalam derajat per detik.")]
    private float turnSpeed = 360f;

    [Header("Reaksi terhadap Player")]
    [SerializeField]
    [Tooltip("Hentikan patroli ketika Enemy berada pada state Alert.")]
    private bool haltWhenAlert = true;

    [Header("Debug (read only)")]
    [SerializeField]
    [Tooltip("Titik yang sedang dituju saat ini.")]
    private Vector3 currentTarget;

    [SerializeField]
    [Tooltip("Sisa waktu tunggu di ujung jalur.")]
    private float waitTimer;

    private EnemyDetector detector;
    private Vector3 pointA;
    private Vector3 pointB;

    private void Awake()
    {
        detector = GetComponent<EnemyDetector>();
    }

    private void Start()
    {
        // Posisi saat Play ditekan menjadi salah satu ujung jalur.
        pointA = transform.position;
        pointB = pointA + patrolOffset;

        currentTarget = pointB;
    }

    private void Update()
    {
        if (IsHalted())
        {
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        MoveTowardsTarget();
    }

    /// <summary>
    /// Patroli berhenti saat Enemy sudah menyadari Player, sehingga
    /// perubahan perilaku terlihat jelas, bukan sekadar berganti warna.
    /// </summary>
    private bool IsHalted()
    {
        if (!haltWhenAlert)
        {
            return false;
        }

        return detector != null && detector.CurrentState == EnemyState.Alert;
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = currentTarget - transform.position;

        if (faceMovementDirection)
        {
            RotateTowards(direction);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            moveSpeed * Time.deltaTime);

        // Perbandingan jarak dipakai sebagai ambang, karena posisi float
        // jarang sama persis dengan target.
        if (Vector3.Distance(transform.position, currentTarget) < 0.05f)
        {
            SwitchTarget();
        }
    }

    private void SwitchTarget()
    {
        currentTarget = currentTarget == pointA ? pointB : pointA;
        waitTimer = waitTime;
    }

    private void RotateTowards(Vector3 direction)
    {
        // Abaikan komponen vertikal agar Enemy tidak menunduk atau mendongak.
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Menggambar jalur patroli di Scene View.
    /// Saat belum Play, jalur dihitung langsung dari posisi Enemy
    /// supaya bisa diatur tanpa menjalankan game.
    /// </summary>
    private void OnDrawGizmos()
    {
        Vector3 start = Application.isPlaying ? pointA : transform.position;
        Vector3 end = Application.isPlaying ? pointB : transform.position + patrolOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(start, Vector3.one * 0.3f);
        Gizmos.DrawWireCube(end, Vector3.one * 0.3f);
    }
}
