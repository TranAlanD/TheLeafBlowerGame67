using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Simple dialogue UI that shows text at bottom of screen.
/// Automatically creates canvas and text components.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    private static DialogueUI instance;
    private Canvas canvas;
    private TextMeshProUGUI dialogueText;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateUI();
    }

    void CreateUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("DialogueCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        // Create background panel
        GameObject panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(canvasObj.transform);

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.05f);
        panelRect.anchorMax = new Vector2(0.9f, 0.25f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create text
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(panelObj.transform);

        dialogueText = textObj.AddComponent<TextMeshProUGUI>();

        // ENABLE AUTO SIZING PROPERLY
        dialogueText.enableAutoSizing = true;
        dialogueText.fontSizeMin = 18f;
        dialogueText.fontSizeMax = 48f;

        dialogueText.enableWordWrapping = true;
        dialogueText.overflowMode = TextOverflowModes.Overflow;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.text = "";

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
    }

    /// <summary>
    /// Show dialogue text on screen
    /// </summary>
    public static void ShowText(string text)
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("DialogueUIManager");
            instance = obj.AddComponent<DialogueUI>();
        }

        instance.dialogueText.text = text;
        instance.canvasGroup.alpha = 1;
    }

    /// <summary>
    /// Hide dialogue UI
    /// </summary>
    public static void Hide()
    {
        if (instance != null)
        {
            instance.canvasGroup.alpha = 0;
        }
    }
}
