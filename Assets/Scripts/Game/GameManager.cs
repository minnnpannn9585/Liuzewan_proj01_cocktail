using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float animationDuration = 2f;
    [Header("Transition")]
    public float transitionDuration = 2f;

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
        BartenderGameData.Instance.drinkIndex = 0;
        StartDrinkFromCutscene();
    }

    private void StartDrinkFromCutscene()
    {
        BartenderGameData.Instance.currentCustomer = new Customer();
        BartenderGameData.Instance.currentCocktail = new Cocktail();
        BartenderGameData.Instance.currentStep = 1;

        BartenderGameData.Instance.tempSelectedItem = null;
        BartenderGameData.Instance.tempSelectedAdditive = null;
        BartenderGameData.Instance.tempSelectedMagic = null;

        SceneManager.LoadScene("GameScene");
        PlayCustomerAnimation();
    }

    private void StartNextDrinkFromDialogue()
    {
        BartenderGameData.Instance.currentCocktail = new Cocktail();
        BartenderGameData.Instance.currentStep = 2;

        BartenderGameData.Instance.tempSelectedItem = null;
        BartenderGameData.Instance.tempSelectedAdditive = null;
        BartenderGameData.Instance.tempSelectedMagic = null;

        UIManager.Instance.StartDialogue(EnterSelectGlassStep);
    }

    private void PlayCustomerAnimation()
    {
        Debug.Log("播放顾客出场动画...");
        Invoke(nameof(EnterDialogueStep), animationDuration);
    }

    private void EnterDialogueStep()
    {
        Bgm.Instance.SwitchToGameBgm();
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
                if (BartenderGameData.Instance.tempSelectedItem != null &&
                    selectedItem != null &&
                    BartenderGameData.Instance.tempSelectedItem.itemName == selectedItem.itemName)
                {
                    BartenderGameData.Instance.tempSelectedItem = null;
                    selectedItem = null;
                }
                else
                {
                    BartenderGameData.Instance.tempSelectedItem = selectedItem;
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateSelectionVisual(selectedItem);

                    bool isCorrect = UIManager.Instance.IsSelectionCorrectForStep(step, selectedItem);
                    UIManager.Instance.SetNextButtonInteractable(isCorrect);

                    if (selectedItem != null && !isCorrect)
                        UIManager.Instance.ShowWrongSelectionPopup();
                }

                break;
            }

            case 6:
                HandleAdditiveProcess(selectedItem);
                break;

            case 8:
                // 第8步是 shake 小游戏，不再通过选按钮处理
                break;
        }
    }

    public void OnNextButtonClicked()
    {
        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayNextButtonSfx();

        int step = BartenderGameData.Instance.currentStep;
        ItemData selected = BartenderGameData.Instance.tempSelectedItem;

        bool needSelectionValidation = step == 3 || step == 4 || step == 5 || step == 7 || step == 9;
        if (needSelectionValidation)
        {
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
                if (selected == null) return;
            }
        }

        switch (step)
        {
            case 3:
                if (selected != null) HandleGlassSelection(selected);
                else TransitionToStep(4);
                break;

            case 4:
                if (selected != null) HandleBaseLiquorSelection(selected);
                else TransitionToStep(5);
                break;

            case 5:
                if (selected != null) HandleAdditiveSelection(selected);
                else TransitionToStep(7);
                break;

            case 7:
                if (selected != null) HandleMagicMaterialSelection(selected);
                else
                {
                    BartenderGameData.Instance.tempSelectedMagic = null;
                    TransitionToStep(8);
                }
                break;

            case 8:
                BartenderGameData.Instance.currentCocktail.RecordStep(6, "摇晃调制完成");
                if (SfxManager.Instance != null)
                    SfxManager.Instance.PlayPouringSfx();
                TransitionToStep(9);
                break;

            case 9:
                if (selected != null) HandleDecorationSelection(selected);
                else HandleDrinkFinished();
                break;
        }

        BartenderGameData.Instance.tempSelectedItem = null;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateSelectionVisual(null);
    }

    private void TransitionToStep(int nextStep, System.Action onArrive = null)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowTransitionAnimation(nextStep, transitionDuration, () =>
            {
                BartenderGameData.Instance.currentStep = nextStep;
                UIManager.Instance.UpdateStepUI(nextStep);
                onArrive?.Invoke();
            });
        }
        else
        {
            BartenderGameData.Instance.currentStep = nextStep;
            onArrive?.Invoke();
        }
    }

    private void HandleGlassSelection(ItemData glass)
    {
        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayIceDropSfx();

        BartenderGameData.Instance.currentCocktail.AddItemAttributes(glass);
        BartenderGameData.Instance.currentCocktail.RecordStep(1, $"选择杯子：{glass.itemName}");
        TransitionToStep(4);
    }

    private void HandleBaseLiquorSelection(ItemData baseLiquor)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(baseLiquor);
        BartenderGameData.Instance.currentCocktail.RecordStep(2, $"选择基酒：{baseLiquor.itemName}");
        TransitionToStep(5);
    }

    private void HandleAdditiveSelection(ItemData additive)
    {
        BartenderGameData.Instance.tempSelectedAdditive = additive;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowTransitionAnimation(6, transitionDuration, () =>
            {
                BartenderGameData.Instance.currentStep = 6;
                UIManager.Instance.ShowAdditiveProcessPanel();
            });
        }
        else
        {
            BartenderGameData.Instance.currentStep = 6;
        }
    }

    private void HandleAdditiveProcess(ItemData process)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(BartenderGameData.Instance.tempSelectedAdditive);
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(process);

        BartenderGameData.Instance.currentCocktail.RecordStep(3, $"选择辅料：{BartenderGameData.Instance.tempSelectedAdditive.itemName}");
        BartenderGameData.Instance.currentCocktail.RecordStep(4, $"辅料加工：{process.itemName}");

        BartenderGameData.Instance.tempSelectedAdditive = null;
        BartenderGameData.Instance.tempSelectedItem = null;

        TransitionToStep(7);
    }

    private void HandleMagicMaterialSelection(ItemData magic)
    {
        BartenderGameData.Instance.tempSelectedMagic = magic;
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(magic);
        BartenderGameData.Instance.currentCocktail.RecordStep(5, $"选择魔法材料：{magic.itemName}");

        TransitionToStep(8);
    }

    private void HandleDecorationSelection(ItemData decoration)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(decoration);
        BartenderGameData.Instance.currentCocktail.RecordStep(7, $"选择装饰：{decoration.itemName}");
        HandleDrinkFinished();
    }

    private void HandleDrinkFinished()
    {
        if (UIManager.Instance != null)
        {
            // 播放最后一步（完成后）的过渡动画，索引为 10
            UIManager.Instance.ShowTransitionAnimation(10, transitionDuration, ShowResultAndContinue);
        }
        else
        {
            ShowResultAndContinue();
        }
    }

    private void ShowResultAndContinue()
    {
        UIManager.Instance.ShowResultPanelAndWaitForClick(() =>
        {
            if (BartenderGameData.Instance.drinkIndex == 0)
            {
                BartenderGameData.Instance.drinkIndex = 1;
                StartNextDrinkFromDialogue();
            }
            else
            {
                // 第二轮结束后显示Final Panel而不是返回主菜单
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowFinalPanel();
                }
            }
        });
    }

    public void RestartGame() => StartNewGame();

    public void BackToStart()
    {
        BartenderGameData.Instance.currentStep = 0;
        BartenderGameData.Instance.tempSelectedItem = null;
        SceneManager.LoadScene("StartScene");
    }
}
