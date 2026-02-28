using System.Collections.Generic;
using UnityEngine;

public class BartenderGameData : MonoBehaviour
{
    public static BartenderGameData Instance;

    // 所有物品数据（public，可在Inspector配置）
    public List<ItemData> allItems = new List<ItemData>();

    // 当前游戏状态数据（public，其他脚本可直接访问）
    public Cocktail currentCocktail;
    public Customer currentCustomer;
    public int currentStep = 0; // 0:开始 1:选酒杯 2:选基酒 3:选辅料 4:选加工 5:选操作 6:结果
    public bool isWin;
    public int[] errorValues; // [strongDiff, bitterDiff, sourDiff]

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

        // 初始化鸡尾酒
        currentCocktail = new Cocktail();
    }

    // 根据类型获取物品列表（public，其他脚本可调用）
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