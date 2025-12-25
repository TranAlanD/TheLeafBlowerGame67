using UnityEngine;

public class LeafBlowerMechanics : MonoBehaviour
{
    private Rigidbody rigidBody;
    public GameObject playerCamera;
    private PlayerMovement playerMovement;
    public float blowRange = 5f;
    public float blowRadius = 1f;
    [Tooltip("Continuous airflow applied every frame to nearby rigidbodies.")]
    public float baselineForce = 2f;
    private string leavesTag = "Leaves";
    private string blowableTag = "Blowable";
    private string spiderTag = "Spider";
    public float blowForce = 10f;
    public GameObject leafBlower;
    public GameObject leafProjectile;
    [Header("Charge Blast Settings")]
    [Tooltip("Maximum charge level (hold right-click to charge)")]
    public float maxCharge = 2f;

    [Tooltip("Number of charges available before landing (like double jump)")]
    public int maxCharges = 2;

    [Tooltip("Multiplier for recoil power. Lower = weaker launch. Try 0.1 to 0.5")]
    public float recoilPowerMultiplier;

    [Tooltip("Camera shake intensity. Lower = less shake. Try 0.1 to 1.0")]
    public float cameraShakeMultiplier = 0.3f;

    private float charge = 0f;
    private int currentCharges = 2;
    private bool isCharging = false;
    private bool isBlowing = false;
    private bool chargeReady = false;
    private bool manualBlowHeld = false;
    private bool isGrounded = false;
    private Transform compartment;
    private Vector3 compartmentStartPos;
    [Tooltip("Rotation offset for the leaf blower model. Try (0,180,0) if facing backwards")]
    public Vector3 visualRotationOffset = new Vector3(0, 180, 0);
    private float blowTime;
    private int leafRange = 5;
    private bool leafBool = false;

    [Header("Charged Blast Visuals")]
    public GameObject chargedBlastEffectPrefab;

    [Header("Charge Blast Sounds")]
    public AudioSource chargeAudioSource;
    public AudioClip chargeClip1;
    public AudioClip chargeClip2;

    [Header("Leaf Blowing Sound")]
    public AudioClip leafBlowClip;      
    public float leafBlowVolume = 0.5f; 
    private bool isLeafBlowingPlaying = false;

    [Header("Activation")]
    public bool isActivated = false;



    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rigidBody = GetComponent<Rigidbody>();

        // Find camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.gameObject;
            if (playerCamera == null)
            {
                // Try to find camera as child
                playerCamera = GetComponentInChildren<Camera>()?.gameObject;
            }
            Debug.LogError($"LeafBlower camera auto-found: {(playerCamera != null ? playerCamera.name : "NOT FOUND")}");
        }

        if (leafBlower != null)
        {
            compartment = leafBlower.transform.Find("Compartment");
            if (compartment != null)
            {
                compartmentStartPos = compartment.localPosition;
            }
        }
    }

    void Update()
    {
        if (!isActivated) return;
        // Right-click: Start charging (only if charges available)
        if (Input.GetMouseButtonDown(1))
        {
            if (currentCharges > 0)
            {
                isCharging = true;
                chargeReady = false;
            }
            else
            {
                Debug.Log("<color=red>No charges left! Land to recharge.</color>");
            }
        }

        // Right-click release: Fire charged blast
        if (Input.GetMouseButtonUp(1))
        {
            isCharging = false;
            if (charge > 0.01f && currentCharges > 0)
            {
                FireChargedBlast();
                currentCharges--; // Use one charge
                Debug.Log($"<color=yellow>Charges remaining: {currentCharges}/{maxCharges}</color>");
            }
            charge = 0f; // Reset charge after firing
        }

        // Left-click hold: Continuous blow (no player recoil)
        manualBlowHeld = Input.GetMouseButton(0);
        isBlowing = manualBlowHeld;

        // Play leaf blowing sound if not already playing
        if (manualBlowHeld && !isLeafBlowingPlaying)
        {
            chargeAudioSource.clip = leafBlowClip;
            chargeAudioSource.loop = true; // keep looping while holding
            chargeAudioSource.volume = leafBlowVolume;
            chargeAudioSource.Play();
            isLeafBlowingPlaying = true;
        }
        // Stop leaf blowing sound when released
        else if (!manualBlowHeld && isLeafBlowingPlaying)
        {
            chargeAudioSource.Stop();
            chargeAudioSource.loop = false; // reset loop for next time
            isLeafBlowingPlaying = false;
        }


        // Visual feedback for charging
        if (compartment != null)
        {
            compartment.localScale = new Vector3(1 + charge * 0.2f, 1 + charge * 0.2f, 1 + charge * 0.2f);
            compartment.localPosition = compartmentStartPos + (new Vector3(0, -charge * 0.1f, charge * 0.1f));
        }

        // Point leaf blower in camera direction (with optional rotation offset)
        leafBlower.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(visualRotationOffset);
    }

    void FixedUpdate()
    {
        ApplyBaselineStream();

        // Charge up when holding right-click
        if (isCharging)
        {
            Charge();
        }

        // Continuous blow when holding left-click
        if (manualBlowHeld)
        {
            Blow();
        }

        // Return player to upright when not blowing
        if (!manualBlowHeld)
        {
            Quaternion currentRot = transform.rotation;
            Vector3 euler = currentRot.eulerAngles;
            euler.x = 0f;
            euler.z = 0f;
            Quaternion uprightRot = Quaternion.Euler(euler);
            transform.rotation = Quaternion.Slerp(currentRot, uprightRot, Time.deltaTime * 5f);
        }
    }

    void Charge()
    {
        if (charge < maxCharge)
        {
            charge += 0.02f;
        }
        else
        {
            charge = maxCharge;
        }
        RaycastHit hit;
        Camera cam = FPS_Cursor.Instance.playerCamera;
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit,leafRange)){
            if (hit.transform.tag == "Pile") {
                leafBool = true;
            }
        }
    }

    void Blow()
    {
        if (leafBool) {
            leafBool = false;
            GameObject obj = Instantiate(leafProjectile, transform.position + transform.up * 0.5f + transform.forward * 3f + transform.right * 0.75f, playerCamera.transform.rotation);
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = playerCamera.transform.forward * 50f;
            }
        }
        // Where the blower is pointing
        Vector3 blowerDirection = playerCamera.transform.forward;
        Vector3 blowerRight = playerCamera.transform.right;

        // Position spherecast origin based on where the blower is pointing
        Vector3 origin = transform.position + blowerDirection * 2f + blowerRight * 0.75f;

        // Perform spherecast to find targets
        RaycastHit[] hits = Physics.SphereCastAll(origin, blowRadius, blowerDirection, blowRange);

        foreach (RaycastHit hit in hits)
        {
            Collider col = hit.collider;
            if (col == null) continue;

            // Handle leaves: destroy and notify LeafManager
            if (col.CompareTag(leavesTag))
            {
                GameObject leaf = col.gameObject;
                LeafManager leafManager = FindObjectOfType<LeafManager>();
                if (leafManager != null)
                    leafManager.leafTracker.LeafDestroyed();
                Destroy(leaf);
                // continue here if you don't want leaves to also be pushed
                continue;
            }

            // Handle rotating blocks
            if (col.CompareTag("RotatingBlock"))
            {
                RotateOnLeafBlower rotator = col.GetComponent<RotateOnLeafBlower>();
                if (rotator != null)
                {
                    rotator.RotateBlocks();
                }
            }

            // Apply physics to blowable targets and spiders
            if (col.CompareTag(blowableTag) || col.CompareTag(spiderTag))
            {
                Rigidbody rb = col.attachedRigidbody;
                if (rb == null) continue;

                // Avoid applying force to the player themselves
                if (rb == rigidBody) continue;

                // Push objects away from the blower nozzle position
                Vector3 forceDir = col.transform.position - transform.position - blowerRight * 0.75f;
                rb.AddForce(forceDir.normalized * blowForce, ForceMode.Impulse);
            }
        }
    }


    void FireChargedBlast()
    {
        // Get camera direction
        Vector3 blowerDirection = playerCamera.transform.forward;

        // Calculate blast strength based on charge (0 to maxCharge)
        float chargePercent = charge / maxCharge;
        float blastStrength = charge * 10f; // Multiply for stronger effect

        // Apply force to nearby objects/enemies
        ApplyImpulseToTargets(blowForce * (1f + chargePercent * 3f), ForceMode.Impulse);

        // ONE BIG instant recoil for player (opposite direction)
        Vector3 recoilForce = -blowerDirection * blastStrength * recoilPowerMultiplier;
        // Determine where the player will launch toward
        Vector3 effectPosition = transform.position + recoilForce.normalized * 1f; // 1f is offset so effect is in front of player


        if (playerMovement != null)
        {
            playerMovement.ApplyKnockback(recoilForce);
        }
        else if (rigidBody != null)
        {
            rigidBody.AddForce(recoilForce, ForceMode.VelocityChange);
        }

        // Camera shake based on charge
        float shakeIntensity = charge * cameraShakeMultiplier * 5f;
        Vector3 randomShake = new Vector3(
            UnityEngine.Random.Range(-0.2f, 0.2f),
            UnityEngine.Random.Range(-0.2f, 0.2f),
            UnityEngine.Random.Range(-0.2f, 0.2f)
        );
        transform.Rotate(shakeIntensity * randomShake);

        if (chargedBlastEffectPrefab != null)
        {
            // Place the effect slightly in the direction the player is launched
            Vector3 effectOrigin = transform.position + recoilForce.normalized * 1f; // offset along launch direction
            GameObject effect = Instantiate(chargedBlastEffectPrefab, effectOrigin, Quaternion.LookRotation(recoilForce.normalized));
            Destroy(effect, 2f);
        }

        // Play different clip depending on which charge it is
        if (currentCharges == maxCharges) // First charge being used
        {
            chargeAudioSource.clip = chargeClip1;
        }
        else if (currentCharges == maxCharges - 1) // Second charge being used
        {
            chargeAudioSource.clip = chargeClip2;
        }

        chargeAudioSource.Play();


        Debug.Log($"<color=cyan>CHARGED BLAST! Power: {chargePercent * 100f}%</color>");
    }

    void OnCollisionEnter(Collision collision)
    {
        // Reset charges when landing on a platform
        if (collision.gameObject.CompareTag("Platform"))
        {
            if (!isGrounded)
            {
                currentCharges = maxCharges;
                isGrounded = true;
                Debug.Log($"<color=green>Landed! Charges refilled: {currentCharges}/{maxCharges}</color>");
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Maintain grounded state while on platform
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Leave ground when jumping/falling off platform
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
        }
    }

    void ApplyBaselineStream()
    {
        if (baselineForce <= 0f)
        {
            return;
        }

        ApplyImpulseToTargets(baselineForce * Time.fixedDeltaTime, ForceMode.Force);
    }

    void ApplyImpulseToTargets(float forceMagnitude, ForceMode forceMode)
    {
        // Use camera direction for blowing (where player is aiming)
        Vector3 blowerDirection = playerCamera.transform.forward;
        Vector3 blowerRight = playerCamera.transform.right;

        // Position spherecast origin based on where the blower is pointing
        Vector3 origin = transform.position + blowerDirection * 2f + blowerRight * 0.75f;
        RaycastHit[] hits = Physics.SphereCastAll(origin, blowRadius, blowerDirection, blowRange);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag(blowableTag) || hit.collider.CompareTag(spiderTag))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;

                if (rb != null)
                {
                    if (rb == rigidBody)
                    {
                        continue;
                    }
                    // Push objects away from the blower nozzle position
                    Vector3 forceDir = hit.collider.transform.position - transform.position - blowerRight * 0.75f;
                    rb.AddForce(forceDir.normalized * forceMagnitude, forceMode);
                }
            }
        }
    }

}
