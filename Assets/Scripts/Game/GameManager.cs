using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 过场动画时长（秒），可调整
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

    // 开始新游戏（主菜单点击）
    public void StartNewGame()
    {
        // 1. 初始化游戏数据（固定顾客）
        BartenderGameData.Instance.currentCustomer = new Customer();
        BartenderGameData.Instance.currentCocktail = new Cocktail();
        BartenderGameData.Instance.currentStep = 1; // 进入过场动画步骤

        // 2. 加载游戏场景并播放过场动画
        SceneManager.LoadScene("GameScene");
        PlayCustomerAnimation();
    }

    // 播放顾客出场过场动画（预留接口，可替换为实际动画调用）
    private void PlayCustomerAnimation()
    {
        Debug.Log("播放顾客出场动画...");
        // 动画播放完成后进入对话步骤
        Invoke(nameof(EnterDialogueStep), animationDuration);
    }

    // 进入对话步骤（从 DialogueController 读取对话数据）
    private void EnterDialogueStep()
    {
        BartenderGameData.Instance.currentStep = 2;
        UIManager.Instance.StartDialogue(EnterSelectGlassStep);
    }

    // 对话结束后进入选杯子步骤
    private void EnterSelectGlassStep()
    {
        BartenderGameData.Instance.currentStep = 3;
        UIManager.Instance.UpdateStepUI(BartenderGameData.Instance.currentStep);
    }

    // 通用物品选择方法（适配所有步骤）
    public void SelectItem(ItemData selectedItem)
    {
        switch (BartenderGameData.Instance.currentStep)
        {
            case 3: // 选杯子
                HandleGlassSelection(selectedItem);
                break;
            case 4: // 选基酒
                HandleBaseLiquorSelection(selectedItem);
                break;
            case 5: // 选辅料
                HandleAdditiveSelection(selectedItem);
                break;
            case 6: // 辅料加工
                HandleAdditiveProcess(selectedItem);
                break;
            case 7: // 选魔法材料
                HandleMagicMaterialSelection(selectedItem);
                break;
            case 8: // 魔法材料操作
                HandleMagicProcess(selectedItem);
                break;
            case 9: // 选装饰
                HandleDecorationSelection(selectedItem);
                break;
        }
    }

    // 处理杯子选择
    private void HandleGlassSelection(ItemData glass)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(glass);
        BartenderGameData.Instance.currentCocktail.RecordStep(1, $"选择杯子：{glass.itemName}");
        BartenderGameData.Instance.currentStep = 4; // 进入选基酒
        UIManager.Instance.UpdateStepUI(4);
    }

    // 处理基酒选择
    private void HandleBaseLiquorSelection(ItemData baseLiquor)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(baseLiquor);
        BartenderGameData.Instance.currentCocktail.RecordStep(2, $"选择基酒：{baseLiquor.itemName}");
        BartenderGameData.Instance.currentStep = 5; // 进入选辅料
        UIManager.Instance.UpdateStepUI(5);
    }

    // 处理辅料选择（选中后弹出加工面板）
    private void HandleAdditiveSelection(ItemData additive)
    {
        BartenderGameData.Instance.tempSelectedAdditive = additive;
        BartenderGameData.Instance.currentStep = 6; // 进入辅料加工
        UIManager.Instance.ShowAdditiveProcessPanel(); // 弹出加工面板
    }

    // 处理辅料加工
    private void HandleAdditiveProcess(ItemData process)
    {
        // 先累加辅料属性，再累加加工属性
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(BartenderGameData.Instance.tempSelectedAdditive);
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(process);
        // 记录步骤
        BartenderGameData.Instance.currentCocktail.RecordStep(3, $"选择辅料：{BartenderGameData.Instance.tempSelectedAdditive.itemName}");
        BartenderGameData.Instance.currentCocktail.RecordStep(4, $"辅料加工：{process.itemName}");
        // 清空临时存储
        BartenderGameData.Instance.tempSelectedAdditive = null;
        // 进入选魔法材料
        BartenderGameData.Instance.currentStep = 7;
        UIManager.Instance.UpdateStepUI(7);
    }

    // 处理魔法材料选择（选中后弹出操作面板）
    private void HandleMagicMaterialSelection(ItemData magic)
    {
        BartenderGameData.Instance.tempSelectedMagic = magic;
        BartenderGameData.Instance.currentStep = 8; // 进入魔法操作
        UIManager.Instance.ShowMagicProcessPanel(); // 弹出操作面板
    }

    // 处理魔法材料操作
    private void HandleMagicProcess(ItemData process)
    {
        // 先累加魔法材料属性，再累加操作属性
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(BartenderGameData.Instance.tempSelectedMagic);
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(process);
        // 记录步骤
        BartenderGameData.Instance.currentCocktail.RecordStep(5, $"选择魔法材料：{BartenderGameData.Instance.tempSelectedMagic.itemName}");
        BartenderGameData.Instance.currentCocktail.RecordStep(6, $"魔法操作：{process.itemName}");
        // 清空临时存储
        BartenderGameData.Instance.tempSelectedMagic = null;
        // 进入选装饰
        BartenderGameData.Instance.currentStep = 9;
        UIManager.Instance.UpdateStepUI(9);
    }

    // 处理装饰选择（最后一步，直接判定）
    private void HandleDecorationSelection(ItemData decoration)
    {
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(decoration);
        BartenderGameData.Instance.currentCocktail.RecordStep(7, $"选择装饰：{decoration.itemName}");
        // 判定胜负
        CheckWin();
        // 加载结果场景
        SceneManager.LoadScene("ResultScene");
    }

    // 判定胜负（新增浓稠度判定）
    private void CheckWin()
    {
        Cocktail cocktail = BartenderGameData.Instance.currentCocktail;
        Customer customer = BartenderGameData.Instance.currentCustomer;

        // 计算三个属性的误差
        int strongDiff = Mathf.Abs(cocktail.strong - customer.needStrong);
        int bitterDiff = Mathf.Abs(cocktail.bitter - customer.needBitter);
        int thickDiff = Mathf.Abs(cocktail.thick - customer.needThick);

        // 误差±2以内获胜
        BartenderGameData.Instance.isWin = (strongDiff <= 2 && bitterDiff <= 2 && thickDiff <= 2);
        BartenderGameData.Instance.errorValues = new int[] { strongDiff, bitterDiff, thickDiff };
    }

    // 重新开始游戏
    public void RestartGame()
    {
        StartNewGame();
    }

    // 返回主菜单
    public void BackToStart()
    {
        BartenderGameData.Instance.currentStep = 0;
        SceneManager.LoadScene("StartScene");
    }
}