using UnityEngine;

// 物品属性数据类（独立文件，可序列化）
[System.Serializable]
public class ItemData
{
    public string itemName;       // 物品名称
    public ItemType itemType;     // 物品类型
    public int strongValue;       // 浓烈度影响
    public int bitterValue;       // 苦度影响
    public int sourValue;         // 酸度影响
    public string description;    // 描述
    public Sprite itemSprite;     // 物品图片
}