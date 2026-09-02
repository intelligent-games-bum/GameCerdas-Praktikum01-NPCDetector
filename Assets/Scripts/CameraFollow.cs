using UnityEngine;

/// <summary>
/// Kamera third person sederhana yang mengikuti target dari jarak tetap.
///
/// Tidak mengikuti rotasi target, karena Player pada praktikum ini
/// hanya bergeser tanpa berputar. Dengan begitu arah WASD tetap
/// konsisten terhadap layar.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    [Tooltip("Object yang diikuti kamera. Drag Player dari Hierarchy.")]
    private Transform target;

    [Header("Posisi")]
    [SerializeField]
    [Tooltip("Jarak kamera terhadap target. Y = tinggi, Z negatif = di belakang.")]
    private Vector3 offset = new Vector3(0f, 9f, -9f);

    [SerializeField]
    [Min(0f)]
    [Tooltip("Waktu peredaman gerak kamera. 0 = kaku, semakin besar semakin lembut.")]
    private float smoothTime = 0.2f;

    [Header("Arah Pandang")]
    [SerializeField]
    [Tooltip("Arahkan kamera ke target setiap frame.")]
    private bool lookAtTarget = true;

    [SerializeField]
    [Tooltip("Geser titik pandang ke atas agar target tidak berada di tepi bawah layar.")]
    private Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

    private Vector3 currentVelocity;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError(
                "[CameraFollow] Field 'target' belum diisi. " +
                "Drag object Player ke Inspector Main Camera.", this);
            return;
        }

        // Bila script terpasang pada object yang sama dengan target,
        // object itu akan mengejar posisinya sendiri ditambah offset
        // sehingga melayang menjauh tanpa henti.
        if (target == transform)
        {
            Debug.LogError(
                "[CameraFollow] Target tidak boleh object ini sendiri. " +
                "Script ini seharusnya dipasang pada Main Camera, " +
                "bukan pada Player.", this);
            enabled = false;
            return;
        }

        // Tempatkan kamera langsung di posisi akhir supaya tidak
        // terlihat meluncur dari titik awal saat Play ditekan.
        transform.position = target.position + offset;
        AimAtTarget();
    }

    /// <summary>
    /// LateUpdate dipakai agar kamera bergerak setelah Player selesai
    /// berpindah pada frame ini. Bila memakai Update, kamera bisa
    /// tampak bergetar karena mengejar posisi frame sebelumnya.
    /// </summary>
    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            smoothTime);

        AimAtTarget();
    }

    private void AimAtTarget()
    {
        if (!lookAtTarget)
        {
            return;
        }

        transform.LookAt(target.position + lookAtOffset);
    }
}
