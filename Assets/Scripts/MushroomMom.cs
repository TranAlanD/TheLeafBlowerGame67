using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Mushroom Mom NPC for Level 2. Shows dialogue, accepts baby mushroom delivery,
/// then accidentally crushes baby and launches player away in anger.
///
/// SETUP: Do NOT tag as "Blowable" - should be static and unaffected by leaf blower.
/// Add trigger collider for interaction range. Baby mushroom should use "Pickupable" tag.
/// </summary>
public class MushroomMom : MonoBehaviour
{
    [Header("Delivery Settings")]
    [Tooltip("Key to press to deliver baby mushroom")]
    public KeyCode deliverKey = KeyCode.E;

    [Tooltip("How close player needs to be")]
    public float interactionRange = 4f;

    [Tooltip("Name of the baby mushroom GameObject (set this in Inspector)")]
    public string babyMushroomName = "BabyMushroom";

    [Header("Initial Dialogue")]
    [Tooltip("Lines shown when player first approaches WITHOUT baby")]
    public string[] initialDialogue = {
        "Please save my little boy down at the bottom of the cave!",
        "He glows just like me!",
        "You can follow the blue mushrooms back up using your leaf blower!",
        "Watch out for the other red mushrooms down there. They're mean cave mushrooms",
        "Nothing like us surface mushrooms, we glow a healthy green in the dark!"
    };

    [Tooltip("Lines shown when player approaches WITH baby")]
    public string[] babyPresentDialogue = {
        "Oh! You found him! My precious baby!",
        "Press E to give him back to me!"
    };

    [Tooltip("How long each dialogue line stays on screen (seconds)")]
    public float dialogueLineDuration = 3f;

    [Header("Explosion Sequence")]
    [Tooltip("Transform where baby is moved before explosion (e.g., Mom's hands)")]
    public Transform babyHandoffPosition;

    [Tooltip("Time to move baby to handoff position (seconds)")]
    public float handoffDuration = 1f;

    [Tooltip("Pause after baby reaches position before explosion (seconds)")]
    public float preExplosionPause = 1f;

    [Tooltip("Particle effect for baby explosion")]
    public ParticleSystem explosionEffect;

    [Tooltip("Sound for baby explosion")]
    public AudioClip explosionSound;

    [Tooltip("Pause after explosion before dialogue (seconds)")]
    public float dramaticPauseDuration = 2f;

    [System.Serializable]
    public struct DialogueLine
    {
        public string speaker; // "Mom" or "Player"
        public string text;
    }

    [Tooltip("Dialogue after baby explodes")]
    public DialogueLine[] argumentDialogue = {
        new DialogueLine { speaker = "Mom", text = "what" },
        new DialogueLine { speaker = "Player", text = "What..." },
        new DialogueLine { speaker = "Mom", text = "did you press E?" },
        new DialogueLine { speaker = "Player", text = "You told me to..." },
        new DialogueLine { speaker = "Mom", text = "E is for Explode, for EXPLODE!" },
        new DialogueLine { speaker = "Mom", text = "Why would I tell you to press E?" },
        new DialogueLine { speaker = "Player", text = "WHY WOULD E BE FOR EXPLODE WHO WOULD DESIGN IT LIKE THAT?" },
        new DialogueLine { speaker = "Player", text = "And you did tell me to press E!" }, // new dialogue line after this
        new DialogueLine { speaker = "Mom", text = "Get out of here..." },
        new DialogueLine { speaker = "Player", text = "Look im sorry but that is an obvious flaw" },
        new DialogueLine { speaker = "Mom", text = "GET OUT" }
    };

    [Tooltip("Time between each argument line (seconds)")]
    public float argumentLineDelay = 1.5f;

    [Header("Angry Launch")]
    [Tooltip("Use attack animation? (if false, uses direct launch)")]
    public bool useAttackAnimation = true;

    [Tooltip("Animator component for rigged attack animation (optional)")]
    public Animator mushroomAnimator;

    [Tooltip("Attack animation trigger name (for Animator)")]
    public string attackTriggerName = "Attack";

    [Tooltip("SimpleAttackAnimation component (for non-rigged meshes - procedural)")]
    public SimpleAttackAnimation simpleAttackAnimation;

    [Tooltip("LegacyAttackAnimation component (for animation clips on non-rigged meshes)")]
    public LegacyAttackAnimation legacyAttackAnimation;

    [Tooltip("Delay before launching player (to sync with animation)")]
    public float launchDelay = 0.5f;

    [Tooltip("Force to launch player away")]
    public float launchForce = 50f;

    [Tooltip("Direction to launch player (normalized automatically)")]
    public Vector3 launchDirection = new Vector3(0, 1, 1);

    [Tooltip("Event triggered when player is launched (use to load next level)")]
    public UnityEvent onPlayerLaunched;

    [Header("Voice Sounds")]
    [Tooltip("Talking sound to play when Mom speaks (loops during dialogue)")]
    public AudioClip momTalkingSound;

    [Tooltip("Talking sound to play when Player speaks (loops during dialogue)")]
    public AudioClip playerTalkingSound;

    [Tooltip("Talking sound to play when Cop speaks (loops during dialogue)")]
    public AudioClip copTalkingSound;

    [Tooltip("Volume for talking sounds")]
    [Range(0f, 1f)]
    public float talkingSoundVolume = 0.5f;

    [Header("Police Officer Scene")]
    [Tooltip("Police officer who intervenes (optional - if null, uses original dialogue)")]
    public PoliceOfficer policeOfficer;

    [Tooltip("Police car that drives in (optional)")]
    public PoliceCar policeCar;

    [Tooltip("Time for characters to rotate to face speaker")]
    public float rotationDuration = 0.5f;

    [Header("Visual Effects")]
    [Tooltip("Material to change to when angry (optional)")]
    public Material angryMaterial;

    private bool hasShownInitialDialogue = false;
    private bool hasShownBabyDialogue = false;
    private bool babyDelivered = false;
    private bool isPlayerInRange = false;
    private PlayerCarry playerCarry = null;
    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private InteractionPrompt interactionPrompt;
    private float debugTimer = 0f;
    private VoiceAudioManager voiceManager;

    void Start()
    {
        Debug.Log($"<color=magenta>========== MUSHROOM MOM START ========== \nGameObject: {gameObject.name}\nPosition: {transform.position}\nInteraction Range: {interactionRange}</color>");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get voice manager (must be manually added)
        voiceManager = GetComponent<VoiceAudioManager>();
        if (voiceManager == null)
        {
            Debug.LogWarning("VoiceAudioManager not found! Add it manually for voice effects. Using basic audio instead.");
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        // Get or add interaction prompt
        interactionPrompt = GetComponent<InteractionPrompt>();
        if (interactionPrompt == null)
        {
            interactionPrompt = gameObject.AddComponent<InteractionPrompt>();
            interactionPrompt.promptHeight = 3f;
            interactionPrompt.showRange = interactionRange;
        }

        Debug.Log("<color=magenta>MushroomMom initialized successfully!</color>");
    }

    void Update()
    {
        if (babyDelivered)
        {
            Debug.LogError("Update blocked - babyDelivered is true!");
            return;
        }

        // ALWAYS search for player (simpler approach like TestMom)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            float distance = Vector3.Distance(transform.position, playerObj.transform.position);

            if (distance <= interactionRange)
            {
                isPlayerInRange = true;
                if (playerCarry == null)
                {
                    playerCarry = playerObj.GetComponent<PlayerCarry>();
                    Debug.LogError($"Found PlayerCarry component: {(playerCarry != null ? "YES" : "NO")}");
                }
            }
            else
            {
                isPlayerInRange = false;
            }
        }

        // Debug status every 2 seconds
        debugTimer += Time.deltaTime;
        if (debugTimer >= 2f)
        {
            debugTimer = 0f;
            Debug.LogError($"[MOM STATUS] InRange: {isPlayerInRange}, HasPlayerCarry: {playerCarry != null}, BabyDelivered: {babyDelivered}");
        }

        // Check if player is carrying baby
        bool playerHasBaby = false;
        if (isPlayerInRange && playerCarry != null)
        {
            PickupableItem carriedItem = playerCarry.GetCarriedItem();
            playerHasBaby = carriedItem != null && carriedItem.gameObject.name == babyMushroomName;
        }

        // Show appropriate dialogue based on whether player has baby
        if (isPlayerInRange && !hasShownInitialDialogue && !playerHasBaby)
        {
            Debug.LogError("SHOWING INITIAL DIALOGUE!");
            StartCoroutine(ShowDialogue(initialDialogue));
            hasShownInitialDialogue = true;
        }
        else if (isPlayerInRange && playerHasBaby && !hasShownBabyDialogue)
        {
            Debug.LogError("SHOWING BABY PRESENT DIALOGUE!");
            StartCoroutine(ShowDialogue(babyPresentDialogue));
            hasShownBabyDialogue = true;
        }

        // Check for baby delivery
        if (Input.GetKeyDown(deliverKey))
        {
            Debug.LogError($"F KEY PRESSED! InRange: {isPlayerInRange}, HasPlayerCarry: {playerCarry != null}");
        }

        if (isPlayerInRange && playerCarry != null && Input.GetKeyDown(deliverKey))
        {
            PickupableItem carriedItem = playerCarry.GetCarriedItem();
            Debug.LogError($"Checking carried item: {(carriedItem != null ? carriedItem.gameObject.name : "NULL")}");

            if (carriedItem != null && carriedItem.gameObject.name == babyMushroomName)
            {
                Debug.LogError("Delivery key pressed with baby! Starting sequence...");
                StartCoroutine(BabyDeliverySequence(carriedItem));
            }
            else if (carriedItem != null)
            {
                Debug.LogError($"Wrong item: {carriedItem.gameObject.name}, expected: {babyMushroomName}");
            }
            else
            {
                Debug.LogError("No item being carried!");
            }
        }
    }

    IEnumerator ShowDialogue(string[] dialogue)
    {
        Debug.Log("<color=cyan>=== MUSHROOM MOM DIALOGUE ===</color>");

        // Get player transform for rotation
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Transform playerTransform = playerObj != null ? playerObj.transform : null;

        foreach (string line in dialogue)
        {
            Debug.Log($"<color=yellow>Mushroom Mom: {line}</color>");
            DialogueUI.ShowText(line);

            // Rotate Mom to look at Player
            if (playerTransform != null)
            {
                yield return StartCoroutine(RotateToLook(transform, playerTransform));
            }

            // Play talking sound for mom
            if (momTalkingSound != null)
            {
                if (voiceManager != null)
                {
                    voiceManager.PlayVoice(momTalkingSound, talkingSoundVolume);
                }
                else if (audioSource != null)
                {
                    // Fallback without voice manager
                    audioSource.PlayOneShot(momTalkingSound, talkingSoundVolume);
                }
            }

            yield return new WaitForSeconds(dialogueLineDuration);

            // Stop voice when line ends
            if (voiceManager != null)
            {
                voiceManager.StopVoice();
            }
        }
        DialogueUI.Hide();
    }

    IEnumerator BabyDeliverySequence(PickupableItem babyMushroom)
    {
        babyDelivered = true;

        Debug.Log("<color=green>Baby mushroom delivered to Mom!</color>");

        // Get player reference for launching later
        GameObject player = playerCarry.gameObject;

        // Pause before explosion (baby stays in carry position)
        yield return new WaitForSeconds(preExplosionPause);

        // EXPLOSION!
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, babyMushroom.transform.position, Quaternion.identity);
            explosion.Play();
            Destroy(explosion.gameObject, 3f);
        }

        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Destroy the baby mushroom
        Destroy(babyMushroom.gameObject);

        Debug.Log("<color=red>*SQUISH* Baby mushroom exploded!</color>");

        // Dramatic pause
        yield return new WaitForSeconds(dramaticPauseDuration);

        // Use police officer scene if available, otherwise use original dialogue
        if (policeOfficer != null)
        {
            yield return StartCoroutine(PoliceOfficerScene(player));
        }
        else
        {
            // Show original argument dialogue with rotations
            Debug.Log("<color=cyan>=== ARGUMENT SEQUENCE ===</color>");

            for (int i = 0; i < argumentDialogue.Length; i++)
            {
                DialogueLine line = argumentDialogue[i];
                string formattedText = $"[{line.speaker}]: {line.text}";
                Debug.Log($"<color=yellow>{formattedText}</color>");
                DialogueUI.ShowText(formattedText);

                // Determine who is speaking and who they're talking to
                Transform speakerTransform = null;
                Transform listenerTransform = null;

                if (line.speaker == "Mom")
                {
                    speakerTransform = transform;
                    listenerTransform = player.transform; // Mom talks to Player
                }
                else if (line.speaker == "Player")
                {
                    speakerTransform = player.transform;
                    listenerTransform = transform; // Player talks to Mom
                }

                // Rotate speaker to look at listener
                if (speakerTransform != null && listenerTransform != null)
                {
                    yield return StartCoroutine(RotateToLook(speakerTransform, listenerTransform));
                }

                // Play appropriate talking sound based on speaker
                AudioClip soundToPlay = line.speaker == "Mom" ? momTalkingSound : playerTalkingSound;
                if (soundToPlay != null)
                {
                    if (voiceManager != null)
                    {
                        voiceManager.PlayVoice(soundToPlay, talkingSoundVolume);
                    }
                    else if (audioSource != null)
                    {
                        // Fallback without voice manager
                        audioSource.PlayOneShot(soundToPlay, talkingSoundVolume);
                    }
                }

                yield return new WaitForSeconds(argumentLineDelay);

                // Stop voice when line ends
                if (voiceManager != null)
                {
                    voiceManager.StopVoice();
                }
            }
        }

        // Get angry
        Debug.Log("<color=red>Mushroom Mom is ANGRY!</color>");

        if (angryMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = angryMaterial;
        }

        // Play attack animation if enabled
        if (useAttackAnimation)
        {
            if (mushroomAnimator != null)
            {
                // Use rigged Animator
                mushroomAnimator.SetTrigger(attackTriggerName);
                yield return new WaitForSeconds(launchDelay);
            }
            else if (legacyAttackAnimation != null)
            {
                // Use legacy Animation component (for animation clips)
                legacyAttackAnimation.TriggerAttack();
                yield return new WaitForSeconds(launchDelay);
            }
            else if (simpleAttackAnimation != null)
            {
                // Use simple script-based animation
                simpleAttackAnimation.TriggerAttack();
                yield return new WaitForSeconds(launchDelay);
            }
            else
            {
                Debug.LogWarning("useAttackAnimation is true but no animation component assigned!");
            }
        }

        // Launch player
        LaunchPlayer(player);

        DialogueUI.Hide();

        // Trigger level transition event
        onPlayerLaunched?.Invoke();

        // Wait 1 second then load Level 3
        yield return new WaitForSeconds(1f);
        Debug.Log("<color=cyan>Loading Level 3...</color>");
        SceneManager.LoadScene("Level3");
    }

    IEnumerator PoliceOfficerScene(GameObject player)
    {
        Debug.Log("<color=cyan>=== POLICE OFFICER SCENE ===</color>");

        // Define the full dialogue sequence
        DialogueLine[] fullDialogue = {
            new DialogueLine { speaker = "Mom", text = "Wait! I meant press 'F' not 'E'" },
            new DialogueLine { speaker = "Player", text = "You're kidding." },
            new DialogueLine { speaker = "Mom", text = "Shit I remember now. 'E' was for 'Explode' not 'Interact'" },
            new DialogueLine { speaker = "Mom", text = "I'm calling 911 we need to get this sorted out" },
            new DialogueLine { speaker = "Player", text = "I was just trying to help" },
            new DialogueLine { speaker = "Cop", text = "I'll be the judge of that, son." },
            new DialogueLine { speaker = "Cop", text = "Hey there ma I hear we've had some 'technical difficulties', is that right?" },
            new DialogueLine { speaker = "Mom", text = "I asked this man for help and he went and..." },
            new DialogueLine { speaker = "Mom", text = "he exploded my son!" },
            new DialogueLine { speaker = "Player", text = "How did you get here so fast?" },
            new DialogueLine { speaker = "Cop", text = "It's a cops duty to be there when his citizens need him." },
            new DialogueLine { speaker = "Player", text = "Listen this has been blown so far out of proportion" },
            new DialogueLine { speaker = "Cop", text = "I heard someone was blown up disproportionately too" },
            new DialogueLine { speaker = "Cop", text = "Ma'am you said someone exploded before, did I hear that right?" },
            new DialogueLine { speaker = "Mom", text = "Yes, my son Mushterd" },
            new DialogueLine { speaker = "Cop", text = "And was he a mushroom?" },
            new DialogueLine { speaker = "Mom", text = "Yes" },
            new DialogueLine { speaker = "Cop", text = "OH...yeah I don't really deal in that sort of thing..." },
            new DialogueLine { speaker = "Mom", text = "What? What do you even mean?" },
            new DialogueLine { speaker = "Cop", text = "Yeah...I only help real human beings not talking mushrooms." },
            new DialogueLine { speaker = "Mom", text = "Are you serious?" },
            new DialogueLine { speaker = "Cop", text = "..." },
            new DialogueLine { speaker = "Mom", text = "F*** 12, I despise pigs" },
            new DialogueLine { speaker = "Player", text = "I'm sorry that happened" },
            new DialogueLine { speaker = "Mom", text = "Just get out" },
            new DialogueLine { speaker = "Player", text = "This is so awkward" },
            new DialogueLine { speaker = "Player", text = "There are clearly flaws in the system here" },
            new DialogueLine { speaker = "Player", text = "I'm not going back to jail over this." },
            new DialogueLine { speaker = "Mom", text = "No... I'm sending you to a place much worse than that." }
        };

        // Track cop entrance
        bool carArrived = false;
        bool copEntered = false;

        for (int i = 0; i < fullDialogue.Length; i++)
        {
            DialogueLine line = fullDialogue[i];
            string formattedText = $"[{line.speaker}]: {line.text}";
            Debug.Log($"<color=yellow>{formattedText}</color>");
            DialogueUI.ShowText(formattedText);

            // Police car drives in when Mom says "I'm calling 911" (index 2)
            if (!carArrived && i == 2 && policeCar != null)
            {
                StartCoroutine(policeCar.DriveIn());
                carArrived = true;
            }

            // Get transforms for rotation
            Transform speakerTransform = null;
            Transform listenerTransform = null;
            AudioClip soundToPlay = null;

            switch (line.speaker)
            {
                case "Mom":
                    speakerTransform = transform;
                    soundToPlay = momTalkingSound;
                    // Figure out who Mom is talking to
                    if (i + 1 < fullDialogue.Length)
                    {
                        string nextSpeaker = fullDialogue[i + 1].speaker;
                        if (nextSpeaker == "Cop") listenerTransform = policeOfficer.transform;
                        else if (nextSpeaker == "Player") listenerTransform = player.transform;
                    }
                    break;

                case "Player":
                    speakerTransform = player.transform;
                    soundToPlay = playerTalkingSound;
                    // Figure out who Player is talking to
                    if (i + 1 < fullDialogue.Length)
                    {
                        string nextSpeaker = fullDialogue[i + 1].speaker;
                        if (nextSpeaker == "Cop") listenerTransform = policeOfficer.transform;
                        else if (nextSpeaker == "Mom") listenerTransform = transform;
                    }
                    break;

                case "Cop":
                    speakerTransform = policeOfficer.transform;
                    soundToPlay = copTalkingSound;
                    // Figure out who Cop is talking to
                    if (i + 1 < fullDialogue.Length)
                    {
                        string nextSpeaker = fullDialogue[i + 1].speaker;
                        if (nextSpeaker == "Mom") listenerTransform = transform;
                        else if (nextSpeaker == "Player") listenerTransform = player.transform;
                    }
                    break;
            }

            // Cop enters after first line
            if (!copEntered && i == 4)
            {
                yield return StartCoroutine(policeOfficer.EnterScene());
                copEntered = true;
            }

            // Rotate speaker to look at listener
            if (speakerTransform != null && listenerTransform != null)
            {
                yield return StartCoroutine(RotateToLook(speakerTransform, listenerTransform));
            }

            // Play talking sound
            if (soundToPlay != null)
            {
                if (line.speaker == "Cop")
                {
                    policeOfficer.PlayVoice();
                }
                else
                {
                    if (voiceManager != null)
                    {
                        voiceManager.PlayVoice(soundToPlay, talkingSoundVolume);
                    }
                    else if (audioSource != null)
                    {
                        audioSource.PlayOneShot(soundToPlay, talkingSoundVolume);
                    }
                }
            }

            yield return new WaitForSeconds(argumentLineDelay);

            // Stop voice when line ends
            if (line.speaker == "Cop")
            {
                policeOfficer.StopVoice();
            }
            else if (voiceManager != null)
            {
                voiceManager.StopVoice();
            }

            // Cop exits after "You are so useless" line
            if (copEntered && line.speaker == "Mom" && line.text == "You are so useless")
            {
                yield return StartCoroutine(policeOfficer.ExitScene());
            }
        }
    }

    IEnumerator RotateToLook(Transform speaker, Transform target)
    {
        if (speaker == null || target == null) yield break;

        Quaternion startRotation = speaker.rotation;
        Vector3 direction = target.position - speaker.position;
        direction.y = 0; // Keep rotation horizontal only

        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            speaker.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        speaker.rotation = targetRotation;
    }

    void LaunchPlayer(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null) return;

        Vector3 launchVelocity = launchDirection.normalized * launchForce;
        playerMovement.ApplyKnockback(launchVelocity);

        Debug.Log($"<color=red>LAUNCHED PLAYER with force {launchForce}!</color>");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=cyan>MushroomMom trigger entered by: {other.gameObject.name} (Tag: {other.tag})</color>");
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerCarry = other.GetComponent<PlayerCarry>();
            Debug.Log($"<color=green>Player in range! PlayerCarry component: {(playerCarry != null ? "Found" : "Not Found")}</color>");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerCarry = null;
            Debug.Log("<color=yellow>Player left range</color>");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = babyDelivered ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw center point
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);

        // Draw launch direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, launchDirection.normalized * 5f);
    }
}
