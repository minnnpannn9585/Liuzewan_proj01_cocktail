using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float animationDuration = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewGame()
    {
        BartenderGameData.Instance.currentCustomer = new Customer();
        BartenderGameData.Instance.currentCocktail = new Cocktail();
        BartenderGameData.Instance.currentStep = 1;
        BartenderGameData.Instance.tempSelectedItem = null;

        SceneManager.LoadScene("GameScene");
        PlayCustomerAnimation();
    }

    private void PlayCustomerAnimation()
    {
        Debug.Log("播放顾客出场动画...");
        Invoke(nameof(EnterDialogueStep), animationDuration);
    }

    private void EnterDialogueStep()
    {
        BartenderGameData.Instance.currentStep = 2;
        UIManager.Instance.StartDialogue(EnterSelectGlassStep);
    }

    private void EnterSelectGlassStep()
    {
        BartenderGameData.Instance.currentStep = 3;
        BartenderGameData.Instance.tempSelectedItem = null;
        UIManager.Instance.UpdateStepUI(BartenderGameData.Instance.currentStep);
    }

    public void SelectItem(ItemData selectedItem)
    {
        int step = BartenderGameData.Instance.currentStep;

        switch (step)
        {
            case 3:
            case 4:
            case 5:
            case 7:
            case 9:
            {
                BartenderGameData.Instance.tempSelectedItem = selectedItem;

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateSelectionVisual(selectedItem);

                    bool isCorrect = UIManager.Instance.IsSelectionCorrectForStep(step, selectedItem);
                    UIManager.Instance.SetNextButtonInteractable(isCorrect);

                    if (!isCorrect)
                        UIManager.Instance.ShowWrongSelectionPopup();
                }

                break;
            }

            case 6:
                HandleAdditiveProcess(selectedItem);
                break;

            case 8:
                HandleMagicProcess(selectedItem);
                break;
        }
    }

    public void OnNextButtonClicked()
    {
        int step = BartenderGameData.Instance.currentStep;
        ItemData selected = BartenderGameData.Instance.tempSelectedItem;

        // 允许“不选就是正确”的情况：selected 可为 null，但必须通过 UIManager 校验
        if (UIManager.Instance != null)
        {
            bool isCorrect = UIManager.Instance.IsSelectionCorrectForStep(step, selected);
            if (!isCorrect)
            {
                UIManager.Instance.ShowWrongSelectionPopup();
                UIManager.Instance.SetNextButtonInteractable(false);
                return;
            }
        }
        else
        {
            // 没 UIManager 就保持原逻辑：必须选择
            if (selected == null) return;
        }

        switch (step)
        {
            case 3:
                // 如果 selected 为 null 且允许空选正确，那么这里应跳步但不加属性
                if (selected != null) HandleGlassSelection(selected);
                else { BartenderGameData.Instance.currentStep = 4; UIManager.Instance.UpdateStepUI(4); }
                break;

            case 4:
                if (selected != null) HandleBaseLiquorSelection(selected);
                else { BartenderGameData.Instance.currentStep = 5; UIManager.Instance.UpdateStepUI(5); }
                break;

            case 5:
                if (selected != null) HandleAdditiveSelection(selected);
                else { BartenderGameData.Instance.currentStep = 7; UIManager.Instance.UpdateStepUI(7); }
                break;

            case 7:
                if (selected != null) HandleMagicMaterialSelection(selected);
                else { BartenderGameData.Instance.currentStep = 9; UIManager.Instance.UpdateStepUI(9); }
                break;

            case 9:
                if (selected != null) HandleDecorationSelection(selected);
                else
                {
                    // 不选装饰也可能正确：直接结算
                    CheckWin();
                    SceneManager.LoadScene("ResultScene");
                }
                break;
        }

        BartenderGameData.Instance.tempSelectedItem = null;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateSelectionVisual(null);
            // 下一步 UpdateStepUI 内会设置 Next 状态
        }
    }

    private void HandleGlassSelection(ItemData glass)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(glass);
        BartenderGameData.Instance.currentCocktail.RecordStep(1, $"选择杯子：{glass.itemName}");
        BartenderGameData.Instance.currentStep = 4;
        UIManager.Instance.UpdateStepUI(4);
    }

    private void HandleBaseLiquorSelection(ItemData baseLiquor)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(baseLiquor);
        BartenderGameData.Instance.currentCocktail.RecordStep(2, $"选择基酒：{baseLiquor.itemName}");
        BartenderGameData.Instance.currentStep = 5;
        UIManager.Instance.UpdateStepUI(5);
    }

    private void HandleAdditiveSelection(ItemData additive)
    {
        BartenderGameData.Instance.tempSelectedAdditive = additive;
        BartenderGameData.Instance.currentStep = 6;
        UIManager.Instance.ShowAdditiveProcessPanel();
    }

    private void HandleAdditiveProcess(ItemData process)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(BartenderGameData.Instance.tempSelectedAdditive);
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(process);

        BartenderGameData.Instance.currentCocktail.RecordStep(3, $"选择辅料：{BartenderGameData.Instance.tempSelectedAdditive.itemName}");
        BartenderGameData.Instance.currentCocktail.RecordStep(4, $"辅料加工：{process.itemName}");

        BartenderGameData.Instance.tempSelectedAdditive = null;

        BartenderGameData.Instance.currentStep = 7;
        BartenderGameData.Instance.tempSelectedItem = null;
        UIManager.Instance.UpdateStepUI(7);
    }

    private void HandleMagicMaterialSelection(ItemData magic)
    {
        BartenderGameData.Instance.tempSelectedMagic = magic;
        BartenderGameData.Instance.currentStep = 8;
        UIManager.Instance.ShowMagicProcessPanel();
    }

    private void HandleMagicProcess(ItemData process)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(BartenderGameData.Instance.tempSelectedMagic);
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(process);

        BartenderGameData.Instance.currentCocktail.RecordStep(5, $"选择魔法材料：{BartenderGameData.Instance.tempSelectedMagic.itemName}");
        BartenderGameData.Instance.currentCocktail.RecordStep(6, $"魔法操作：{process.itemName}");

        BartenderGameData.Instance.tempSelectedMagic = null;

        BartenderGameData.Instance.currentStep = 9;
        BartenderGameData.Instance.tempSelectedItem = null;
        UIManager.Instance.UpdateStepUI(9);
    }

    private void HandleDecorationSelection(ItemData decoration)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(decoration);
        BartenderGameData.Instance.currentCocktail.RecordStep(7, $"选择装饰：{decoration.itemName}");
        CheckWin();
        SceneManager.LoadScene("ResultScene");
    }

    private void CheckWin()
    {
        Cocktail cocktail = BartenderGameData.Instance.currentCocktail;
        Customer customer = BartenderGameData.Instance.currentCustomer;

        int strongDiff = Mathf.Abs(cocktail.strong - customer.needStrong);
        int bitterDiff = Mathf.Abs(cocktail.bitter - customer.needBitter);
        int thickDiff = Mathf.Abs(cocktail.thick - customer.needThick);

        BartenderGameData.Instance.isWin = (strongDiff <= 2 && bitterDiff <= 2 && thickDiff <= 2);
        BartenderGameData.Instance.errorValues = new int[] { strongDiff, bitterDiff, thickDiff };
    }

    public void RestartGame() => StartNewGame();

    public void BackToStart()
    {
        BartenderGameData.Instance.currentStep = 0;
        BartenderGameData.Instance.tempSelectedItem = null;
        SceneManager.LoadScene("StartScene");
    }
}