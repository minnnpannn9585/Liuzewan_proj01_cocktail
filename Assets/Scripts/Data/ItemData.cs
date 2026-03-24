using UnityEngine;

// 物品属性数据类（新增浓稠度）
[System.Serializable]
public class ItemData
{
    public string itemName;       // 物品名称
    public ItemType itemType;     // 物品类型
    public int strongValue;       // 烈度影响
    public int bitterValue;       // 苦度影响
    public int thickValue;        // 浓稠度影响（新增）
    public string description;    // 描述
    public Sprite itemSprite;     // 物品图片
}