using UnityEngine;
using UnityEngine.UI;

// 物品按钮组件（独立文件）
public class ItemButton : MonoBehaviour
{
    public Image iconImage;    // 物品图标
    public Text nameText;      // 物品名称
    public Text descText;      // 物品描述
    public Button button;      // 按钮组件

    // 设置物品数据
    public void SetItemData(ItemData item)
    {
        if (item.itemSprite != null)
        {
            iconImage.sprite = item.itemSprite;
        }
        nameText.text = item.itemName;
        descText.text = $"{item.description}\n属性影响：浓烈度{item.strongValue} | 苦度{item.bitterValue} | 酸度{item.sourValue}";
    }
}