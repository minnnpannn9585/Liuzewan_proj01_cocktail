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
    public int currentStep = 0; // 0:主菜单 1:过场动画 2:选杯子 3:选基酒 4:选辅料 5:辅料加工 6:选魔法材料 7:魔法操作 8:选装饰 9:判定结果
    public bool isWin;
    public int[] errorValues; // [strongDiff, bitterDiff, thickDiff]（新增浓稠度误差）

    // 临时存储选中的辅料/魔法材料（用于加工/操作后累加属性）
    public ItemData tempSelectedAdditive;
    public ItemData tempSelectedMagic;

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