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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Hide popups on init
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        if (dialogueController != null)
            dialogueController.gameObject.SetActive(true); // controller 自己会 Hide 面板
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            InitGameUI();
        }
        else if (SceneManager.GetActiveScene().name == "ResultScene")
        {
            InitResultUI();
        }
    }

    // Initialize Game Scene UI
    private void InitGameUI()
    {
        Customer customer = BartenderGameData.Instance.currentCustomer;
        customerNameText.text = $"Customer: {customer.name}";
        customerDemandText.text = $"Requirements: Alcohol {customer.needStrong} | Bitterness {customer.needBitter} | Thickness {customer.needThick}";

        // Init step UI if already in gameplay
        if (BartenderGameData.Instance.currentStep >= 3)
        {
            UpdateStepUI(BartenderGameData.Instance.currentStep);
        }
    }

    public void StartDialogue(System.Action onFinished)
    {
        // 对话期间隐藏可交互物品按钮
        ClearAllItemButtons(itemButtonParent);
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        stepText.text = "Current Step: Dialogue";

        if (dialogueController != null)
        {
            dialogueController.PlayConfigured(onFinished);
        }
        else
        {
            // 没绑定对话控制器就直接跳过，避免卡死
            onFinished?.Invoke();
        }
    }

    // Update Step UI
    public void UpdateStepUI(int step)
    {
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        ClearAllItemButtons(itemButtonParent);

        string[] stepNames = {
            "Main Menu", "Cutscene", "Dialogue", "Select Glass", "Select Base Liquor", "Select Additives",
            "Process Additives", "Select Magic Ingredients", "Process Magic Mix", "Select Decoration"
        };

        if (step >= 0 && step < stepNames.Length)
            stepText.text = $"Current Step: {stepNames[step]}";
        else
            stepText.text = $"Current Step: {step}";

        List<ItemData> items;
        switch (step)
        {
            case 3: // Select Glass
                items = BartenderGameData.Instance.GetItemsByType(ItemType.Glass);
                GenerateItemButtons(items, itemButtonParent);
                break;

            case 4: // Select Base Liquor
                items = BartenderGameData.Instance.GetItemsByType(ItemType.BaseLiquor);
                GenerateItemButtons(items, itemButtonParent);
                break;

            case 5: // Select Additives
                items = BartenderGameData.Instance.GetItemsByType(ItemType.Additive);
                GenerateItemButtons(items, itemButtonParent);
                break;

            case 7: // Select Magic Ingredients
                items = BartenderGameData.Instance.GetItemsByType(ItemType.MagicMaterial);
                GenerateItemButtons(items, itemButtonParent);
                break;

            case 9: // Select Decoration
                items = BartenderGameData.Instance.GetItemsByType(ItemType.Decoration);
                GenerateItemButtons(items, itemButtonParent);
                break;
        }
    }

    public void ShowAdditiveProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        additiveProcessPanel.SetActive(true);

        List<ItemData> processItems = BartenderGameData.Instance.GetItemsByType(ItemType.AdditiveProcess);
        GenerateItemButtons(processItems, additiveProcessParent);
    }

    public void ShowMagicProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        magicProcessPanel.SetActive(true);

        List<ItemData> magicItems = BartenderGameData.Instance.GetItemsByType(ItemType.MagicProcess);
        GenerateItemButtons(magicItems, magicProcessParent);
    }

    private void GenerateItemButtons(List<ItemData> items, Transform parent)
    {
        ClearAllItemButtons(parent);
        foreach (var item in items)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, parent);
            ItemButton btn = btnObj.GetComponent<ItemButton>();
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
        {
            Destroy(child.gameObject);
        }
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
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnBackButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BackToStart();
        }
    }
}