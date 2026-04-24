using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    [Header("Result Panels (GameScene)")]
    public GameObject resultPanel1;
    public GameObject resultPanel2;

    [Header("Transition Animations - Drink 1")]
    [Tooltip("第一轮调酒的动画游戏物体数组")]
    public GameObject[] drink1StepTransitionPanels = new GameObject[11];

    [Header("Transition Animations - Drink 2")]
    [Tooltip("第二轮调酒的动画游戏物体数组")]
    public GameObject[] drink2StepTransitionPanels = new GameObject[11];

    [Header("Drink 1 Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] drink1DialogueLines;

    [Header("Drink 2 Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] drink2DialogueLines;

    [Header("Shake Mini Game (Step 8)")]
    public GameObject shakeMiniGamePanel;
    public Image shakeGlassImage;
    [Tooltip("第二轮展示的Shake杯子图片")]
    public Image shakeGlassImage2; // 新增第二阶段的杯子图片
    public float shakeRequiredPathLength = 1200f;
    public Vector2 shakeGlassOffset = Vector2.zero;

    [Header("Shake Mini Game Sub-Panels")]
    [Tooltip("第一轮展示的Shake子物体")]
    public GameObject shakeMiniGameDrink1Child;
    [Tooltip("第二轮展示的Shake子物体")]
    public GameObject shakeMiniGameDrink2Child;

    [Header("Dialogue UI")]
    public DialogueController dialogueController;

    [Header("Popup Panels")]
    public GameObject additiveProcessPanel;
    
    [Header("Additive Process Sub-Panels")]
    [Tooltip("第一轮展示的子物体")]
    public GameObject additiveProcessDrink1Child;
    [Tooltip("第二轮展示的子物体")]
    public GameObject additiveProcessDrink2Child;

    public GameObject magicProcessPanel;
    public Transform additiveProcessParent;
    public Transform magicProcessParent;

    [Header("Additive Process (Step 6) - Fixed 3 Buttons")]
    public Button[] additiveProcessButtons = new Button[3];
    public ItemData[] additiveProcessItems = new ItemData[3];
    public int correctAdditiveProcessIndex = 0;

    [Header("Wrong Selection Feedback")]
    public GameObject errorImagePrefab;
    public Transform errorPopupParent;

    [Header("Result Screen UI (Legacy ResultScene, unused for flow=2)")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI detailText;

    [Header("Step Correct Answers (Active Set) - by Item Name")]
    public string correctGlassItemName;
    public bool allowNoSelectionAsCorrect_Glass;

    public string correctBaseLiquorItemName;
    public bool allowNoSelectionAsCorrect_BaseLiquor;

    public string correctAdditiveItemName;
    public bool allowNoSelectionAsCorrect_Additive;

    public string correctMagicMaterialItemName;
    public bool allowNoSelectionAsCorrect_MagicMaterial;

    public string correctDecorationItemName;
    public bool allowNoSelectionAsCorrect_Decoration;

    [Header("Drink 1 Correct Answers - by Item Name")]
    public string drink1_correctGlassItemName;
    public bool drink1_allowNoSelectionAsCorrect_Glass;
    public string drink1_correctBaseLiquorItemName;
    public bool drink1_allowNoSelectionAsCorrect_BaseLiquor;
    public string drink1_correctAdditiveItemName;
    public bool drink1_allowNoSelectionAsCorrect_Additive;
    public string drink1_correctMagicMaterialItemName;
    public bool drink1_allowNoSelectionAsCorrect_MagicMaterial;
    public string drink1_correctDecorationItemName;
    public bool drink1_allowNoSelectionAsCorrect_Decoration;

    [Header("Drink 2 Correct Answers - by Item Name")]
    public string drink2_correctGlassItemName;
    public bool drink2_allowNoSelectionAsCorrect_Glass;
    public string drink2_correctBaseLiquorItemName;
    public bool drink2_allowNoSelectionAsCorrect_BaseLiquor;
    public string drink2_correctAdditiveItemName;
    public bool drink2_allowNoSelectionAsCorrect_Additive;
    public string drink2_correctMagicMaterialItemName;
    public bool drink2_allowNoSelectionAsCorrect_MagicMaterial;
    public string drink2_correctDecorationItemName;
    public bool drink2_allowNoSelectionAsCorrect_Decoration;

    private readonly List<ItemButton> _spawnedItemButtons = new List<ItemButton>();

    private Canvas _rootCanvas;
    private RectTransform _hoverRect;

    private bool _shakeRunning;
    private float _shakeAccumulatedLength;
    private Vector2 _shakeLastMouseScreenPos;
    private System.Action _shakeOnFinished;
    private bool _shakeIgnoreNextDelta;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);
        if (shakeMiniGamePanel != null) shakeMiniGamePanel.SetActive(false);
        if (resultPanel1 != null) resultPanel1.SetActive(false);
        if (resultPanel2 != null) resultPanel2.SetActive(false);

        if (drink1StepTransitionPanels != null)
        {
            foreach (var panel in drink1StepTransitionPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        if (drink2StepTransitionPanels != null)
        {
            foreach (var panel in drink2StepTransitionPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

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
        UpdateShakeMiniGame();
    }

    public void ShowTransitionAnimation(int targetStep, float duration, System.Action onComplete)
    {
        StartCoroutine(CoShowTransition(targetStep, duration, onComplete));
    }

    private IEnumerator CoShowTransition(int targetStep, float duration, System.Action onComplete)
    {
        ClearAllItemButtons(itemButtonParent);
        HideItemHoverInfo();
        SetNextButtonVisible(false);
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (shakeMiniGamePanel != null) shakeMiniGamePanel.SetActive(false);

        GameObject activeTransition = null;

        int drinkIndex = BartenderGameData.Instance != null ? BartenderGameData.Instance.drinkIndex : 0;
        GameObject[] currentPanels = drinkIndex == 0 ? drink1StepTransitionPanels : drink2StepTransitionPanels;

        if (currentPanels != null && targetStep >= 0 && targetStep < currentPanels.Length)
        {
            activeTransition = currentPanels[targetStep];
        }

        if (activeTransition == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        activeTransition.SetActive(true);
        activeTransition.transform.SetAsLastSibling();

        yield return new WaitForSeconds(Mathf.Max(duration, 0.1f));

        if (activeTransition != null) activeTransition.SetActive(false);
        onComplete?.Invoke();
    }

    public void ShowResultPanelForSeconds(float seconds, System.Action onDone)
    {
        StartCoroutine(CoShowResultPanel(seconds, onDone));
    }

    private IEnumerator CoShowResultPanel(float seconds, System.Action onDone)
    {
        GameObject activeResultPanel = null;
        if (BartenderGameData.Instance != null)
        {
            activeResultPanel = (BartenderGameData.Instance.drinkIndex == 0) ? resultPanel1 : resultPanel2;
        }

        if (activeResultPanel != null) activeResultPanel.SetActive(true);

        yield return new WaitForSeconds(seconds);

        if (activeResultPanel != null) activeResultPanel.SetActive(false);
        onDone?.Invoke();
    }

    private void ApplyCorrectConfigForCurrentDrink()
    {
        int idx = BartenderGameData.Instance != null ? BartenderGameData.Instance.drinkIndex : 0;

        if (idx == 0)
        {
            correctGlassItemName = drink1_correctGlassItemName;
            allowNoSelectionAsCorrect_Glass = drink1_allowNoSelectionAsCorrect_Glass;

            correctBaseLiquorItemName = drink1_correctBaseLiquorItemName;
            allowNoSelectionAsCorrect_BaseLiquor = drink1_allowNoSelectionAsCorrect_BaseLiquor;

            correctAdditiveItemName = drink1_correctAdditiveItemName;
            allowNoSelectionAsCorrect_Additive = drink1_allowNoSelectionAsCorrect_Additive;

            correctMagicMaterialItemName = drink1_correctMagicMaterialItemName;
            allowNoSelectionAsCorrect_MagicMaterial = drink1_allowNoSelectionAsCorrect_MagicMaterial;

            correctDecorationItemName = drink1_correctDecorationItemName;
            allowNoSelectionAsCorrect_Decoration = drink1_allowNoSelectionAsCorrect_Decoration;
        }
        else
        {
            correctGlassItemName = drink2_correctGlassItemName;
            allowNoSelectionAsCorrect_Glass = drink2_allowNoSelectionAsCorrect_Glass;

            correctBaseLiquorItemName = drink2_correctBaseLiquorItemName;
            allowNoSelectionAsCorrect_BaseLiquor = drink2_allowNoSelectionAsCorrect_BaseLiquor;

            correctAdditiveItemName = drink2_correctAdditiveItemName;
            allowNoSelectionAsCorrect_Additive = drink2_allowNoSelectionAsCorrect_Additive;

            correctMagicMaterialItemName = drink2_correctMagicMaterialItemName;
            allowNoSelectionAsCorrect_MagicMaterial = drink2_allowNoSelectionAsCorrect_MagicMaterial;

            correctDecorationItemName = drink2_correctDecorationItemName;
            allowNoSelectionAsCorrect_Decoration = drink2_allowNoSelectionAsCorrect_Decoration;
        }
    }

    private string[] GetDialogueLinesForCurrentDrink()
    {
        int idx = BartenderGameData.Instance != null ? BartenderGameData.Instance.drinkIndex : 0;
        return idx == 0 ? (drink1DialogueLines ?? new string[0]) : (drink2DialogueLines ?? new string[0]);
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

    private void SpawnErrorImage()
    {
        if (errorImagePrefab == null)
            return;

        Transform parent = errorPopupParent != null ? errorPopupParent : transform;
        Instantiate(errorImagePrefab, parent, false);
    }

    public void ShowWrongSelectionPopup()
    {
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayErrorSfx();
        }
            
        SpawnErrorImage();
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
        if (shakeMiniGamePanel != null) shakeMiniGamePanel.SetActive(false);
        if (resultPanel1 != null) resultPanel1.SetActive(false);
        if (resultPanel2 != null) resultPanel2.SetActive(false);

        HideItemHoverInfo();
        SetNextButtonVisible(false);

        stepText.text = "Current Step: Dialogue";

        ApplyCorrectConfigForCurrentDrink();

        if (dialogueController != null)
            dialogueController.PlayLines(GetDialogueLinesForCurrentDrink(), onFinished);
        else
            onFinished?.Invoke();
    }

    public void UpdateStepUI(int step)
    {
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        if (step != 8)
        {
            if (shakeMiniGamePanel != null) shakeMiniGamePanel.SetActive(false);
            _shakeRunning = false;
            _shakeOnFinished = null;
            _shakeIgnoreNextDelta = false;
        }

        if (resultPanel1 != null) resultPanel1.SetActive(false);
        if (resultPanel2 != null) resultPanel2.SetActive(false);

        ClearAllItemButtons(itemButtonParent);
        HideItemHoverInfo();

        bool needNext = step == 3 || step == 4 || step == 5 || step == 7 || step == 9;
        SetNextButtonVisible(needNext);

        bool canNextWithoutSelection = needNext && IsSelectionCorrectForStep(step, null);
        SetNextButtonInteractable(canNextWithoutSelection);

        string[] stepNames = {
            "Main Menu", "Cutscene", "Dialogue", "Select Glass", "Select Base Liquor", "Select Additives",
            "Process Additives", "Select Magic Ingredients", "Shake Mini Game", "Select Decoration"
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
            case 8:
                StartShakeMiniGame(() =>
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.OnNextButtonClicked();
                });
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
        HideItemHoverInfo();
        SetNextButtonVisible(false);

        if (additiveProcessPanel != null)
            additiveProcessPanel.SetActive(true);

        // 根据当前的轮次控制子物体的开关
        int drinkIndex = BartenderGameData.Instance != null ? BartenderGameData.Instance.drinkIndex : 0;
        if (additiveProcessDrink1Child != null)
            additiveProcessDrink1Child.SetActive(drinkIndex == 0);
        if (additiveProcessDrink2Child != null)
            additiveProcessDrink2Child.SetActive(drinkIndex == 1);

        for (int i = 0; i < additiveProcessButtons.Length; i++)
        {
            int index = i;

            if (additiveProcessButtons[index] == null)
                continue;

            additiveProcessButtons[index].onClick.RemoveAllListeners();
            additiveProcessButtons[index].onClick.AddListener(() =>
            {
                if (index == correctAdditiveProcessIndex)
                {
                    ItemData processItem = additiveProcessItems != null && index < additiveProcessItems.Length
                        ? additiveProcessItems[index]
                        : null;
                    if (GameManager.Instance != null)
                        GameManager.Instance.SelectItem(processItem);
                }
                else
                {
                    ShowWrongSelectionPopup();
                }
            });
        }
    }

    public void StartShakeMiniGame(System.Action onFinished)
    {
        ClearAllItemButtons(itemButtonParent);
        HideItemHoverInfo();
        SetNextButtonVisible(false);

        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        if (shakeMiniGamePanel != null) shakeMiniGamePanel.SetActive(true);

        // 根据当前的轮次控制 Shake 子物体的开关
        int drinkIndex = BartenderGameData.Instance != null ? BartenderGameData.Instance.drinkIndex : 0;
        if (shakeMiniGameDrink1Child != null)
            shakeMiniGameDrink1Child.SetActive(drinkIndex == 0);
        if (shakeMiniGameDrink2Child != null)
            shakeMiniGameDrink2Child.SetActive(drinkIndex == 1);

        _shakeRunning = true;
        _shakeAccumulatedLength = 0f;
        _shakeLastMouseScreenPos = Input.mousePosition;
        _shakeIgnoreNextDelta = true;
        _shakeOnFinished = onFinished;

        UpdateShakeGlassPosition(_shakeLastMouseScreenPos);
    }

    private void UpdateShakeMiniGame()
    {
        if (!_shakeRunning)
            return;

        Vector2 cur = Input.mousePosition;

        if (_shakeIgnoreNextDelta)
        {
            _shakeLastMouseScreenPos = cur;
            _shakeIgnoreNextDelta = false;
            UpdateShakeGlassPosition(cur);
            return;
        }

        float delta = Vector2.Distance(cur, _shakeLastMouseScreenPos);
        delta = Mathf.Clamp(delta, 0f, 200f);

        _shakeAccumulatedLength += delta;
        _shakeLastMouseScreenPos = cur;

        UpdateShakeGlassPosition(cur);

        if (_shakeAccumulatedLength >= shakeRequiredPathLength)
            EndShakeMiniGame();
    }

    

    private void UpdateShakeGlassPosition(Vector2 mouseScreenPos)
    {
        if (_rootCanvas == null)
            return;

        if (_rootCanvas.renderMode == RenderMode.WorldSpace)
            return;

        // 根据当前的流程索引（第一杯还是第二杯）选择对应的杯子
        int drinkIndex = BartenderGameData.Instance != null ? BartenderGameData.Instance.drinkIndex : 0;
        Image activeGlassImage = drinkIndex == 0 ? shakeGlassImage : (shakeGlassImage2 != null ? shakeGlassImage2 : shakeGlassImage);

        if (activeGlassImage == null)
            return;

        RectTransform canvasRect = _rootCanvas.transform as RectTransform;
        Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ? _rootCanvas.worldCamera : null;

        RectTransform glassRect = activeGlassImage.rectTransform;

        Vector2 screenPos = mouseScreenPos + shakeGlassOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 localPos);
        glassRect.anchoredPosition = localPos;
    }

    private void EndShakeMiniGame()
    {
        _shakeRunning = false;

        if (shakeMiniGamePanel != null)
            shakeMiniGamePanel.SetActive(false);

        var cb = _shakeOnFinished;
        _shakeOnFinished = null;
        cb?.Invoke();
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

    private void CacheHoverPanel()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas == null)
            _rootCanvas = FindFirstObjectByType<Canvas>();

        if (itemHoverPanel != null)
            _hoverRect = itemHoverPanel.GetComponent<RectTransform>();
    }

    private void UpdateHoverPanelFollowMouse()
    {
        if (itemHoverPanel == null || _hoverRect == null || !itemHoverPanel.activeSelf)
            return;

        if (_rootCanvas == null)
            return;

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

    private void InitResultUI()
    {
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
