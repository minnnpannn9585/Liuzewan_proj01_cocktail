using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Item Button Component (Full English Version)
public class ItemButton : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button button;

    public void SetItemData(ItemData item)
    {
        if (item.itemSprite != null)
        {
            iconImage.sprite = item.itemSprite;
        }
        nameText.text = item.itemName;
        descText.text = $"{item.description}\nEffect: Alcohol {item.strongValue} | Bitterness {item.bitterValue} | Thickness {item.thickValue}";
    }
}