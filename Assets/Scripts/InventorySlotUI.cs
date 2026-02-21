using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int slotIndex;
    public Image highlight;

    private HomeUI homeUI;
    public InventoryItem item;
    public InventoryItemUI itemUI;

    public void Setup(HomeUI ui, int index, InventoryItem itemData, InventoryItemUI uiItem)
    {
        homeUI = ui;
        slotIndex = index;
        item = itemData;
        itemUI = uiItem;
        highlight.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        homeUI.SelectSlot(slotIndex);
    }

    public void SetSelected(bool selected)
    {
        highlight.enabled = selected;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null) homeUI.ShowTooltip(item, transform.position);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        homeUI.HideTooltip();
    }
}
