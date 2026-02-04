using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { FirstPerson, ThirdPerson }
    public CameraMode currentMode = CameraMode.FirstPerson;

    [Header("Targets")]
    public Transform player;
    public Transform pivot;

    [Header("Mouse")]
    public float mouseSensitivity = 3f;
    public float minPitch = -40f;
    public float maxPitch = 80f;

    [Header("First Person")]
    public Vector3 fpOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Third Person")]
    public float tpDistance = 3.5f;
    public float tpHeight = 2.2f;
    public Vector3 shoulderOffset = new Vector3(0.4f, 0f, 0f);
    public float topDownAngle = 20f;

    [Header("Collision")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionMask;

    [Header("Smoothing")]
    public float positionSmooth = 10f;
    public float rotationSmooth = 12f;

    float yaw;
    float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleInput();
        HandleRotation();
    }

    void LateUpdate()
    {
        HandlePosition();
    }

    void HandleInput()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (Input.GetKeyDown(KeyCode.V))
        {
            currentMode = currentMode == CameraMode.FirstPerson
                ? CameraMode.ThirdPerson
                : CameraMode.FirstPerson;
        }
    }

    void HandleRotation()
    {
        player.rotation = Quaternion.Lerp(
            player.rotation,
            Quaternion.Euler(0f, yaw, 0f),
            rotationSmooth * Time.deltaTime
        );

        pivot.localRotation = Quaternion.Lerp(
            pivot.localRotation,
            Quaternion.Euler(pitch, 0f, 0f),
            rotationSmooth * Time.deltaTime
        );
    }

    void HandlePosition()
    {
        Vector3 desiredPos;

        if (currentMode == CameraMode.FirstPerson)
        {
            desiredPos = player.position + fpOffset;
        }
        else
        {
            Quaternion tpRot = Quaternion.Euler(pitch + topDownAngle, yaw, 0f);
            Vector3 dir = tpRot * Vector3.back;

            desiredPos =
                player.position
                + Vector3.up * tpHeight
                + shoulderOffset
                + dir * tpDistance;

            desiredPos = HandleCollision(player.position + Vector3.up * tpHeight, desiredPos);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            positionSmooth * Time.deltaTime
        );

        transform.LookAt(player.position + Vector3.up * 1.5f);
    }

    Vector3 HandleCollision(Vector3 from, Vector3 to)
    {
        if (Physics.SphereCast(
            from,
            collisionRadius,
            (to - from).normalized,
            out RaycastHit hit,
            Vector3.Distance(from, to),
            collisionMask))
        {
            return hit.point + hit.normal * collisionRadius;
        }

        return to;
    }
}
