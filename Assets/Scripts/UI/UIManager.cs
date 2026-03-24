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
        if (BartenderGameData.Instance.currentStep >= 2)
        {
            UpdateStepUI(BartenderGameData.Instance.currentStep);
        }
    }

    // Update Step UI
    public void UpdateStepUI(int step)
    {
        // Hide all popups
        if (additiveProcessPanel != null) additiveProcessPanel.SetActive(false);
        if (magicProcessPanel != null) magicProcessPanel.SetActive(false);

        // Clear existing buttons
        ClearAllItemButtons(itemButtonParent);

        // Step name config (Full English)
        string[] stepNames = { 
            "Main Menu", "Cutscene", "Select Glass", "Select Base Liquor", "Select Additives", 
            "Process Additives", "Select Magic Ingredients", "Process Magic Mix", "Select Decoration" 
        };
        stepText.text = $"Current Step: {stepNames[step]}";

        // Map step to item type
        ItemType[] stepItemTypes = { 
            ItemType.Glass, ItemType.BaseLiquor, ItemType.Additive, 
            ItemType.AdditiveProcess, ItemType.MagicMaterial, 
            ItemType.MagicProcess, ItemType.Decoration 
        };

        // Generate buttons by current step
        List<ItemData> items = new List<ItemData>();
        switch (step)
        {
            case 2: // Select Glass
                items = BartenderGameData.Instance.GetItemsByType(stepItemTypes[0]);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 3: // Select Base Liquor
                items = BartenderGameData.Instance.GetItemsByType(stepItemTypes[1]);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 4: // Select Additives
                items = BartenderGameData.Instance.GetItemsByType(stepItemTypes[2]);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 6: // Select Magic Ingredients
                items = BartenderGameData.Instance.GetItemsByType(stepItemTypes[4]);
                GenerateItemButtons(items, itemButtonParent);
                break;
            case 8: // Select Decoration
                items = BartenderGameData.Instance.GetItemsByType(stepItemTypes[6]);
                GenerateItemButtons(items, itemButtonParent);
                break;
        }
    }

    // Show Additive Processing Popup
    public void ShowAdditiveProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        additiveProcessPanel.SetActive(true);

        List<ItemData> processItems = BartenderGameData.Instance.GetItemsByType(ItemType.AdditiveProcess);
        GenerateItemButtons(processItems, additiveProcessParent);
    }

    // Show Magic Mix Processing Popup
    public void ShowMagicProcessPanel()
    {
        ClearAllItemButtons(itemButtonParent);
        magicProcessPanel.SetActive(true);

        List<ItemData> magicItems = BartenderGameData.Instance.GetItemsByType(ItemType.MagicProcess);
        GenerateItemButtons(magicItems, magicProcessParent);
    }

    // Generate Item Buttons
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

    // Clear all buttons under target parent
    private void ClearAllItemButtons(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    // Initialize Result Screen UI
    private void InitResultUI()
    {
        Cocktail cocktail = BartenderGameData.Instance.currentCocktail;
        Customer customer = BartenderGameData.Instance.currentCustomer;

        // Show win/lose result
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

        // Show detailed stats
        detailText.text = 
            $"Customer Requirements: Alcohol {customer.needStrong} | Bitterness {customer.needBitter} | Thickness {customer.needThick}\n" +
            $"Your Creation: Alcohol {cocktail.strong} | Bitterness {cocktail.bitter} | Thickness {cocktail.thick}\n" +
            $"Error Margin: Alcohol {BartenderGameData.Instance.errorValues[0]} | Bitterness {BartenderGameData.Instance.errorValues[1]} | Thickness {BartenderGameData.Instance.errorValues[2]} (Allowed: ±2)";
    }

    // Button click handlers for Result Scene
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