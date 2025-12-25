using System.Collections;
using UnityEngine;

public class FPS_Cursor : MonoBehaviour
{
    public float mouseSensitivity;
    float xRotation = 0;

    public Camera playerCamera;

    public float shakeIntensity = 1;
    public bool isShaking = false;
    public bool isShakeRunning = false;

    public int topAngle;
    public int bottomAngle;

    public float sprintBackDistance = 3f;
    public float sprintLerpSpeed = 5f;

    public float cameraRadius = 0.3f;
    public LayerMask collisionMask;

    private Vector3 camOriginPos;

    public static FPS_Cursor Instance { get; private set; }

    void Awake()
    {
        // Basic singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        camOriginPos = playerCamera.transform.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topAngle, bottomAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);

        Vector3 targetPos = camOriginPos;

        if (Input.GetKey(KeyCode.LeftShift))
            targetPos = camOriginPos - new Vector3(0f, -.2f, .5f) * sprintBackDistance;

        Vector3 desiredWorldPos = transform.TransformPoint(targetPos);
        Vector3 originWorld = transform.TransformPoint(camOriginPos);
        Vector3 dir = desiredWorldPos - originWorld;
        float dist = dir.magnitude;

        if (Physics.SphereCast(originWorld, cameraRadius, dir.normalized, out RaycastHit hit, dist, collisionMask))
            playerCamera.transform.position = originWorld + dir.normalized * (hit.distance - 0.01f);
        else
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPos, sprintLerpSpeed * Time.deltaTime);

        if (isShaking && !isShakeRunning)
            StartCoroutine(CameraShakeRoutine());
    }

    IEnumerator CameraShakeRoutine()
    {
        isShakeRunning = true;
        camOriginPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            playerCamera.transform.localPosition = camOriginPos + Random.insideUnitSphere * shakeIntensity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = camOriginPos;
        isShaking = false;
        isShakeRunning = false;
    }

    public void TriggerShake()
    {
        isShaking = true;
    }
}
