using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Menggerakkan Player dengan WASD / Arrow Key.
/// Gerakan murni lewat Transform (tanpa Rigidbody) agar sederhana dan stabil.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    [Min(0f)]
    [Tooltip("Kecepatan gerak Player dalam unit per detik.")]
    private float moveSpeed = 5f;

    [Header("Debug (read only)")]
    [SerializeField]
    [Tooltip("Arah gerak hasil pembacaan input, sudah dinormalisasi.")]
    private Vector3 moveDirection;

    private void Update()
    {
        ReadInput();
        Move();
    }

    /// <summary>
    /// Membaca keyboard lalu menyusunnya menjadi satu vektor arah.
    /// Vektor dinormalisasi agar gerakan diagonal tidak lebih cepat
    /// daripada gerakan lurus.
    /// </summary>
    private void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;

        // Keyboard bisa null bila tidak ada perangkat yang terhubung.
        if (keyboard == null)
        {
            moveDirection = Vector3.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        // Bidang gerak adalah XZ. Sumbu Y dibiarkan nol agar Player
        // tetap menempel pada Ground.
        moveDirection = new Vector3(horizontal, 0f, vertical);

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection = moveDirection.normalized;
        }
    }

    /// <summary>
    /// Menerapkan perpindahan. Dikalikan Time.deltaTime agar kecepatan
    /// tidak bergantung pada frame rate.
    /// </summary>
    private void Move()
    {
        if (moveDirection == Vector3.zero)
        {
            return;
        }

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}
