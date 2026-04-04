using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Item Button Component (Icon only + Hover + Selection overlay)
public class ItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public Button button;

    [Header("Selection Visual")]
    public Image selectionOverlay;

    private ItemData _item;

    public ItemData Item => _item;

    public void SetItemData(ItemData item)
    {
        _item = item;

        if (iconImage != null && item != null && item.itemSprite != null)
            iconImage.sprite = item.itemSprite;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectionOverlay == null)
            return;

        var c = selectionOverlay.color;
        c.a = selected ? 0.45f : 0f; // tint overlay
        selectionOverlay.color = c;
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