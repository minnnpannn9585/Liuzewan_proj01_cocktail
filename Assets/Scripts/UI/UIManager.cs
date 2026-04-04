using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

// UI Manager for Bartender Game (Full English Version)
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("General UI")]
    public TextMeshProUGUI stepText;
    public TextMeshProUGUI customerNameText;
    public TextMeshProUGUI customerDemandText;
    public Image customerAvatar;
    public Transform itemButtonParent;
    public GameObject itemButtonPrefab;

    [Header("Step Control UI")]
    public Button nextButton;

    [Header("Item Hover Info UI")]
    public GameObject itemHoverPanel;
    public TextMeshProUGUI itemHoverNameText;
    public TextMeshProUGUI itemHoverDescText;

    [Tooltip("Hover panel offset from mouse position (pixels).")]
    public Vector2 hoverPanelOffset = new Vector2(18f, -18f);

    [Tooltip("Clamp hover panel within canvas rect.")]
    public bool clampHoverPanelToCanvas = true;

    [Header("Dialogue UI")]
    public DialogueController dialogueController;

    [Header("Popup Panels")]
    public GameObject additiveProcessPanel;
    public GameObject magicProcessPanel;
    public Transform additiveProcessParent;
    public Transform magicProcessParent;

    [Header("Result Screen UI")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI detailText;

    [Header("Step Correct Answers (Step 3/4/5/7/9) - by Item Name")]
    public string correctGlassItemName;          // Step 3
    public bool allowNoSelectionAsCorrect_Glass;

    public string correctBaseLiquorItemName;     // Step 4
    public bool allowNoSelectionAsCorrect_BaseLiquor;

    public string correctAdditiveItemName;       // Step 5
    public bool allowNoSelectionAsCorrect_Additive;

    public string correctMagicMaterialItemName;  // Step 7
    public bool allowNoSelectionAsCorrect_MagicMaterial;

    public string correctDecorationItemName;     // Step 9
    public bool allowNoSelectionAsCorrect_Decoration;

    [Header("Wrong Selection Feedback")]
    public GameObject errorPopupPrefab;          // prefab with TimeDestroySelf
    public Transform errorPopupParent;           // optional UI parent

    private readonly List<ItemButton> _spawnedItemButtons = new List<ItemButton>();

    // Hover follow cache
    private Canvas _rootCanvas;
    private RectTransform _hoverRect;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        if (dialogueController != null)
            dialogueController.gameObject.SetActive(true);

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.OnNextButtonClicked();
            });
        }

        CacheHoverPanel();
        ConfigureHoverPanelRaycasts();

        HideItemHoverInfo();
        SetNextButtonVisible(false);
    }

    private void Update()
    {
        UpdateHoverPanelFollowMouse();
    }

    private void CacheHoverPanel()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas == null)
            _rootCanvas = FindFirstObjectByType<Canvas>();

        if (itemHoverPanel != null)
            _hoverRect = itemHoverPanel.GetComponent<RectTransform>();
    }

    // Prevent hover panel from stealing pointer and causing flicker.
    private void ConfigureHoverPanelRaycasts()
    {
        if (itemHoverPanel == null)
            return;

        var cg = itemHoverPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = itemHoverPanel.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        foreach (var g in itemHoverPanel.GetComponentsInChildren<Graphic>(true))
            g.raycastTarget = false;
    }

    private void UpdateHoverPanelFollowMouse()
    {
        if (itemHoverPanel == null || _hoverRect == null || !itemHoverPanel.activeSelf)
            return;

        if (_rootCanvas == null)
            return;

        // Only handle Screen Space canvases here (most common UI setup).
        if (_rootCanvas.renderMode == RenderMode.WorldSpace)
            return;

        RectTransform canvasRect = _rootCanvas.transform as RectTransform;
        Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ? _rootCanvas.worldCamera : null;

        Vector2 screenPos = (Vector2)Input.mousePosition + hoverPanelOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 localPos);
        _hoverRect.anchoredPosition = localPos;

        if (clampHoverPanelToCanvas)
            ClampRectToCanvas(canvasRect, _hoverRect);
    }

    private static void ClampRectToCanvas(RectTransform canvasRect, RectTransform tooltipRect)
    {
        Canvas.ForceUpdateCanvases();

        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 tooltipSize = tooltipRect.rect.size;
        Vector2 pivot = tooltipRect.pivot;

        Vector2 pos = tooltipRect.anchoredPosition;

        float minX = -canvasSize.x * 0.5f + tooltipSize.x * pivot.x;
        float maxX = canvasSize.x * 0.5f - tooltipSize.x * (1f - pivot.x);
        float minY = -canvasSize.y * 0.5f + tooltipSize.y * pivot.y;
        float maxY = canvasSize.y * 0.5f - tooltipSize.y * (1f - pivot.y);

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        tooltipRect.anchoredPosition = pos;
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextButton != null)
            nextButton.gameObject.SetActive(visible);
    }

    public void SetNextButtonInteractable(bool interactable)
    {
        if (nextButton != null)
            nextButton.interactable = interactable;
    }

    public void ShowWrongSelectionPopup()
    {
        if (errorPopupPrefab == null)
            return;

        Transform parent = errorPopupParent != null ? errorPopupParent : transform;
        Instantiate(errorPopupPrefab, parent, false);
    }

    public void UpdateSelectionVisual(ItemData selected)
    {
        for (int i = 0; i < _spawnedItemButtons.Count; i++)
        {
            var btn = _spawnedItemButtons[i];
            bool isSelected = (selected != null && btn != null && btn.Item != null && btn.Item.itemName == selected.itemName);
            btn?.SetSelected(isSelected);
        }
    }

    public bool IsSelectionCorrectForStep(int step, ItemData selected)
    {
        return step switch
        {
            3 => IsCorrectByName(selected, correctGlassItemName, allowNoSelectionAsCorrect_Glass),
            4 => IsCorrectByName(selected, correctBaseLiquorItemName, allowNoSelectionAsCorrect_BaseLiquor),
            5 => IsCorrectByName(selected, correctAdditiveItemName, allowNoSelectionAsCorrect_Additive),
            7 => IsCorrectByName(selected, correctMagicMaterialItemName, allowNoSelectionAsCorrect_MagicMaterial),
            9 => IsCorrectByName(selected, correctDecorationItemName, allowNoSelectionAsCorrect_Decoration),
            _ => false
        };
    }

    private static bool IsCorrectByName(ItemData selected, string correctItemName, bool allowNoSelection)
    {
        if (selected == null)
            return allowNoSelection;

        if (string.IsNullOrWhiteSpace(correctItemName))
            return false;

        return string.Equals(selected.itemName, correctItemName, System.StringComparison.Ordinal);
    }

    // Hover info
    public void ShowItemHoverInfo(ItemData item)
    {
        if (item == null) return;

        if (itemHoverNameText != null)
            itemHoverNameText.text = item.itemName;

        if (itemHoverDescText != null)
        {
            itemHoverDescText.text =
                $"{item.description}\n" +
                $"Effect: Alcohol {item.strongValue} | Bitterness {item.bitterValue} | Thickness {item.thickValue}";
        }

        if (itemHoverPanel != null)
            itemHoverPanel.SetActive(true);

        // Position immediately so it doesn't "jump" next frame
        UpdateHoverPanelFollowMouse();
    }

    public void HideItemHoverInfo()
    {
        if (itemHoverPanel != null)
            itemHoverPanel.SetActive(false);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "GameScene") InitGameUI();
        else if (SceneManager.GetActiveScene().name == "ResultScene") InitResultUI();
    }

    private void InitGameUI()
    {
        Customer customer = BartenderGameData.Instance.currentCustomer;
        customerNameText.text = $"Customer: {customer.name}";
        customerDemandText.text = $"Requirements: Alcohol {customer.needStrong} | Bitterness {customer.needBitter} | Thickness {customer.needThick}";

        if (BartenderGameData.Instance.currentStep >= 3)
            UpdateStepUI(BartenderGameData.Instance.currentStep);
    }

    public void StartDialogue(System.Action onFinished)
    {
        ClearAllItemButtons(itemButtonParent);
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        HideItemHoverInfo();
        SetNextButtonVisible(false);

        stepText.text = "Current Step: Dialogue";

        if (dialogueController != null) dialogueController.PlayConfigured(onFinished);
        else onFinished?.Invoke();
    }

    public void UpdateStepUI(int step)
    {
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        ClearAllItemButtons(itemButtonParent);
        HideItemHoverInfo();

        bool needNext = step == 3 || step == 4 || step == 5 || step == 7 || step == 9;
        SetNextButtonVisible(needNext);

        // If "no selection is correct", Next should start enabled.
        bool canNextWithoutSelection = needNext && IsSelectionCorrectForStep(step, null);
        SetNextButtonInteractable(canNextWithoutSelection);

        string[] stepNames = {
            "Main Menu", "Cutscene", "Dialogue", "Select Glass", "Select Base Liquor", "Select Additives",
            "Process Additives", "Select Magic Ingredients", "Process Magic Mix", "Select Decoration"
        };

        stepText.text = (step >= 0 && step < stepNames.Length)
            ? $"Current Step: {stepNames[step]}"
            : $"Current Step: {step}";

        List<ItemData> items;
        switch (step)
        {
            case 3:
                items = BartenderGameData.Instance.GetItemsByType(ItemType.Glass);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 4:
                items = BartenderGameData.Instance.GetItemsByType(ItemType.BaseLiquor);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 5:
                items = BartenderGameData.Instance.GetItemsByType(ItemType.Additive);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 7:
                items = BartenderGameData.Instance.GetItemsByType(ItemType.MagicMaterial);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 9:
                items = BartenderGameData.Instance.GetItemsByType(ItemType.Decoration);
                GenerateItemButtons(items, itemButtonParent);
                break;
        }

        UpdateSelectionVisual(null);
    }

    public void ShowAdditiveProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(true);

        HideItemHoverInfo();
        SetNextButtonVisible(false);

        List<ItemData> processItems = BartenderGameData.Instance.GetItemsByType(ItemType.AdditiveProcess);
        GenerateItemButtons(processItems, additiveProcessParent);
    }

    public void ShowMagicProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(true);

        HideItemHoverInfo();
        SetNextButtonVisible(false);

        List<ItemData> magicItems = BartenderGameData.Instance.GetItemsByType(ItemType.MagicProcess);
        GenerateItemButtons(magicItems, magicProcessParent);
    }

    private void GenerateItemButtons(List<ItemData> items, Transform parent)
    {
        ClearAllItemButtons(parent);
        _spawnedItemButtons.Clear();

        foreach (var item in items)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, parent);
            ItemButton btn = btnObj.GetComponent<ItemButton>();
            _spawnedItemButtons.Add(btn);

            btn.SetItemData(item);
            btn.button.onClick.AddListener(() =>
            {
                GameManager.Instance.SelectItem(item);
            });
        }
    }

    private void ClearAllItemButtons(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        _spawnedItemButtons.Clear();
    }

    private void InitResultUI()
    {
        Cocktail cocktail = BartenderGameData.Instance.currentCocktail;
        Customer customer = BartenderGameData.Instance.currentCustomer;

        if (BartenderGameData.Instance.isWin)
        {
            resultText.text = $"🎉 Congratulations! {customer.name} loves your drink!";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = $"😞 Oops! {customer.name} is not satisfied with the drink!";
            resultText.color = Color.red;
        }

        detailText.text =
            $"Customer Requirements: Alcohol {customer.needStrong} | Bitterness {customer.needBitter} | Thickness {customer.needThick}\n" +
            $"Your Creation: Alcohol {cocktail.strong} | Bitterness {cocktail.bitter} | Thickness {cocktail.thick}\n" +
            $"Error Margin: Alcohol {BartenderGameData.Instance.errorValues[0]} | Bitterness {BartenderGameData.Instance.errorValues[1]} | Thickness {BartenderGameData.Instance.errorValues[2]} (Allowed: ±2)";
    }

    public void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    public void OnBackButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.BackToStart();
    }
}