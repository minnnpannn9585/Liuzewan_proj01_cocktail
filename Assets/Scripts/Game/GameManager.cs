using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
        // 1. 生成新顾客
        BartenderGameData.Instance.currentCustomer = new Customer();
        BartenderGameData.Instance.currentCustomer.GenerateRandomCustomer();
        
        // 2. 重置鸡尾酒
        BartenderGameData.Instance.currentCocktail = new Cocktail();
        
        // 3. 重置步骤
        BartenderGameData.Instance.currentStep = 1;
        
        // 4. 加载游戏场景
        SceneManager.LoadScene("GameScene");
    }

    // 选择物品（核心交互逻辑）
    public void SelectItem(ItemData selectedItem)
    {
        // 1. 记录属性和步骤
        BartenderGameData.Instance.currentCocktail.AddItemAttributes(selectedItem);
        BartenderGameData.Instance.currentCocktail.RecordStep(
            BartenderGameData.Instance.currentStep - 1, 
            $"选择{selectedItem.itemName}"
        );

        // 2. 进入下一步
        BartenderGameData.Instance.currentStep++;

        // 3. 判断是否完成所有步骤
        if (BartenderGameData.Instance.currentStep > 5)
        {
            CheckWin();
            SceneManager.LoadScene("ResultScene");
        }
        else
        {
            UIManager.Instance.UpdateStepUI(BartenderGameData.Instance.currentStep);
        }
    }

    // 判定胜负（逻辑已检查）
    private void CheckWin()
    {
        Cocktail cocktail = BartenderGameData.Instance.currentCocktail;
        Customer customer = BartenderGameData.Instance.currentCustomer;

        // 计算属性误差
        int strongDiff = Mathf.Abs(cocktail.strong - customer.needStrong);
        int bitterDiff = Mathf.Abs(cocktail.bitter - customer.needBitter);
        int sourDiff = Mathf.Abs(cocktail.sour - customer.needSour);

        // 误差±2以内即获胜
        BartenderGameData.Instance.isWin = (strongDiff <= 2 && bitterDiff <= 2 && sourDiff <= 2);
        BartenderGameData.Instance.errorValues = new int[] { strongDiff, bitterDiff, sourDiff };
    }

    // 重新开始游戏
    public void RestartGame()
    {
        StartNewGame();
    }

    // 返回开始界面
    public void BackToStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}