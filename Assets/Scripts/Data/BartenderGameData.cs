using System.Collections.Generic;
using UnityEngine;

public class BartenderGameData : MonoBehaviour
{
    public static BartenderGameData Instance;

    // 所有物品数据
    public List<ItemData> allItems = new List<ItemData>();

    // 当前游戏状态
    public Cocktail currentCocktail;
    public Customer currentCustomer;

    // 0:主菜单 1:过场动画 2:对话 3:选杯子 4:选基酒 5:选辅料 6:辅料加工 7:选魔法材料 8:魔法操作 9:选装饰 10:判定结果
    public int currentStep = 0;

    public bool isWin;
    public int[] errorValues; // [strongDiff, bitterDiff, thickDiff]

    // 临时存储选中的辅料/魔法材料（用于加工/操作后累加属性）
    public ItemData tempSelectedAdditive;
    public ItemData tempSelectedMagic;

    // 新增：用于“选择后点 Next 才确认”的步骤（3/4/5/7/9）
    public ItemData tempSelectedItem;

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
        currentCocktail = new Cocktail();
    }

    // 根据类型获取物品列表
    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> result = new List<ItemData>();
        foreach (var item in allItems)
        {
            if (item.itemType == type)
            {
                result.Add(item);
            }
        }
        return result;
    }
}