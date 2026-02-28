using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// UI管理类（所有BartenderGameData调用已100%检查）
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // UI组件引用（必须在Inspector面板赋值）
    [Header("通用UI")]
    public Text stepText;          
    public Text customerNameText;  
    public Text customerDemandText;
    public Image customerAvatar;   

    [Header("步骤选择UI")]
    public Transform itemButtonParent; 
    public GameObject itemButtonPrefab;

    [Header("结果界面UI")]
    public Text resultText;        
    public Text detailText;        

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
    }

    private void Start()
    {
        // 根据当前场景初始化UI
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            InitGameUI();
        }
        else if (SceneManager.GetActiveScene().name == "ResultScene")
        {
            InitResultUI();
        }
    }

    // 初始化游戏场景UI
    private void InitGameUI()
    {
        Customer customer = BartenderGameData.Instance.currentCustomer;
        customerNameText.text = $"顾客：{customer.name}";
        customerDemandText.text = $"需求：浓烈度{customer.needStrong} | 苦度{customer.needBitter} | 酸度{customer.needSour}";
        
        UpdateStepUI(BartenderGameData.Instance.currentStep);
    }

    // 更新步骤UI（核心逻辑已检查）
    public void UpdateStepUI(int step)
    {
        // 1. 清空原有按钮
        foreach (Transform child in itemButtonParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 设置步骤文本
        string[] stepNames = { "", "选择酒杯", "选择基酒", "选择辅料", "辅料加工", "最终操作" };
        stepText.text = $"当前步骤：{stepNames[step]}";

        // 3. 获取当前步骤对应的物品
        ItemType[] stepItemTypes = { ItemType.Glass, ItemType.BaseLiquor, ItemType.Additive, ItemType.Process, ItemType.Action };
        List<ItemData> items = BartenderGameData.Instance.GetItemsByType(stepItemTypes[step - 1]);

        // 4. 创建物品按钮
        foreach (var item in items)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, itemButtonParent);
            ItemButton btn = btnObj.GetComponent<ItemButton>();
            
            btn.SetItemData(item);
            btn.button.onClick.AddListener(() => 
            {
                GameManager.Instance.SelectItem(item);
            });
        }
    }

    // 初始化结果界面UI
    private void InitResultUI()
    {
        Cocktail cocktail = BartenderGameData.Instance.currentCocktail;
        Customer customer = BartenderGameData.Instance.currentCustomer;

        // 显示胜负结果
        if (BartenderGameData.Instance.isWin)
        {
            resultText.text = $"🎉 恭喜！{customer.name}非常满意你的调酒！";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = $"😞 抱歉！{customer.name}觉得口味不符！";
            resultText.color = Color.red;
        }

        // 显示详细属性
        detailText.text = 
            $"顾客需求：浓烈度{customer.needStrong} | 苦度{customer.needBitter} | 酸度{customer.needSour}\n" +
            $"你的作品：浓烈度{cocktail.strong} | 苦度{cocktail.bitter} | 酸度{cocktail.sour}\n" +
            $"误差值：浓烈度{BartenderGameData.Instance.errorValues[0]} | 苦度{BartenderGameData.Instance.errorValues[1]} | 酸度{BartenderGameData.Instance.errorValues[2]}（允许误差±2）";
    }
}