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

    [Header("Item Hover Info UI")]
    public GameObject itemHoverPanel;
    public TextMeshProUGUI itemHoverNameText;
    public TextMeshProUGUI itemHoverDescText;

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
            dialogueController.gameObject.SetActive(true);

        HideItemHoverInfo();
    }

    // Hover info API (called by ItemButton)
    public void ShowItemHoverInfo(ItemData item)
    {
        if (item == null)
            return;

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
    }

    public void HideItemHoverInfo()
    {
        if (itemHoverPanel != null)
            itemHoverPanel.SetActive(false);
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

    private void InitGameUI()
    {
        Customer customer = BartenderGameData.Instance.currentCustomer;
        customerNameText.text = $"Customer: {customer.name}";
        customerDemandText.text = $"Requirements: Alcohol {customer.needStrong} | Bitterness {customer.needBitter} | Thickness {customer.needThick}";

        if (BartenderGameData.Instance.currentStep >= 3)
        {
            UpdateStepUI(BartenderGameData.Instance.currentStep);
        }
    }

    public void StartDialogue(System.Action onFinished)
    {
        ClearAllItemButtons(itemButtonParent);
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        HideItemHoverInfo();

        stepText.text = "Current Step: Dialogue";

        if (dialogueController != null)
        {
            dialogueController.PlayConfigured(onFinished);
        }
        else
        {
            onFinished?.Invoke();
        }
    }

    public void UpdateStepUI(int step)
    {
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        ClearAllItemButtons(itemButtonParent);
        HideItemHoverInfo();

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
    }

    public void ShowAdditiveProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(true);

        HideItemHoverInfo();

        List<ItemData> processItems = BartenderGameData.Instance.GetItemsByType(ItemType.AdditiveProcess);
        GenerateItemButtons(processItems, additiveProcessParent);
    }

    public void ShowMagicProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(true);

        HideItemHoverInfo();

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