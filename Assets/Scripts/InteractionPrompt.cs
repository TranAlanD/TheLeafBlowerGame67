using UnityEngine;
using TMPro;

/// <summary>
/// Shows a floating "F" prompt above interactable objects.
/// Automatically creates Canvas with TextMeshPro displaying "F".
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [Header("Prompt Settings")]
    [Tooltip("Text to display (default: F)")]
    public string promptText = "F";

    [Tooltip("Height above object to show prompt")]
    public float promptHeight = 2f;

    [Tooltip("Size of the text")]
    public float textSize = 1f;

    [Tooltip("Color of the text")]
    public Color textColor = Color.white;

    [Header("Visibility")]
    [Tooltip("Only show prompt when player is in range?")]
    public bool hideWhenFar = true;

    [Tooltip("Range to show prompt")]
    public float showRange = 5f;

    [Tooltip("Should prompt always face camera?")]
    public bool billboardToCamera = true;

    private Canvas canvas;
    private TextMeshProUGUI textMesh;
    private Transform playerTransform;
    private bool isVisible = false;
    private bool forceHidden = false;

    void Start()
    {
        CreatePromptUI();

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"<color=cyan>InteractionPrompt on {gameObject.name} found player: {player.name}</color>");
        }
        else
        {
            Debug.LogError($"InteractionPrompt on {gameObject.name} could NOT find player with 'Player' tag!");
        }

        // Start hidden if hideWhenFar is enabled
        if (hideWhenFar)
        {
            SetVisible(false);
        }
    }

    void CreatePromptUI()
    {
        // Create canvas as child, but use world position to avoid parent scale issues
        GameObject canvasObj = new GameObject("PromptCanvas");
        canvasObj.transform.SetParent(transform);

        // Calculate world position accounting for parent scale
        Vector3 worldPos = transform.position + (Vector3.up * promptHeight);
        canvasObj.transform.position = worldPos;

        // Reset local scale to counter parent scaling
        canvasObj.transform.localScale = Vector3.one;
        Vector3 parentScale = transform.lossyScale;
        canvasObj.transform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Scale canvas
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 2);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f) * textSize;

        // Create text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;

        textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = promptText;
        textMesh.fontSize = 144;
        textMesh.color = textColor;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(200, 200);
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
    }

    void Update()
    {
        if (canvas == null) return;

        // Update canvas world position to stay above object
        Vector3 worldPos = transform.position + (Vector3.up * promptHeight);
        canvas.transform.position = worldPos;

        // Billboard to camera
        if (billboardToCamera && Camera.main != null)
        {
            canvas.transform.LookAt(Camera.main.transform);
            canvas.transform.Rotate(0, 180, 0); // Flip to face camera
        }

        // Handle visibility based on player distance
        if (hideWhenFar && playerTransform != null && !forceHidden)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool shouldBeVisible = distance <= showRange;

            if (shouldBeVisible != isVisible)
            {
                SetVisible(shouldBeVisible);
            }
        }
        else if (forceHidden && isVisible)
        {
            SetVisible(false);
        }
    }

    /// <summary>
    /// Manually show or hide the prompt
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (visible)
        {
            // Re-enable automatic visibility management
            forceHidden = false;
            // Let Update() handle showing based on distance
        }
        else
        {
            // Force hide and keep hidden
            forceHidden = true;
            isVisible = false;
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Change the prompt text dynamically
    /// </summary>
    public void SetPromptText(string newText)
    {
        promptText = newText;
        if (textMesh != null)
        {
            textMesh.text = newText;
        }
    }
}
