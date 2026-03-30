using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Item Button Component (Icon only + Hover to show details in a shared panel)
public class ItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public Button button;

    private ItemData _item;

    public void SetItemData(ItemData item)
    {
        _item = item;

        if (iconImage != null && item != null && item.itemSprite != null)
            iconImage.sprite = item.itemSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item == null)
            return;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowItemHoverInfo(_item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.HideItemHoverInfo();
    }
}