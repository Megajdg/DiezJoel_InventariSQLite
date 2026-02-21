using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public Image icon;
    public Image glow;
    public TMP_Text nameText;
    public TMP_Text quantityText;
    public Button deleteButton;
    public Button shootButton;

    public InventorySlotUI slotUI;
    private HomeUI homeUI;

    public void Setup(InventoryItem item, HomeUI ui)
    {
        homeUI = ui;

        nameText.text = item.nomObjecte;
        if (item.nomObjecte == "Monkey Bomb")
            quantityText.text = "x" + item.municionEnCargador;
        else
            quantityText.text = item.municionEnCargador + "/" + item.municionEnReserva;

        icon.sprite = Resources.Load<Sprite>("Icons/" + item.nomObjecte);

        // Activar glow si está mejorada
        glow.sprite = Resources.Load<Sprite>("Icons/" + item.nomObjecte);
        glow.enabled = item.estaMillorada == 1;

        deleteButton.gameObject.SetActive(true);
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnDeletePressed);
        shootButton.gameObject.SetActive(true);
        shootButton.onClick.RemoveAllListeners();
        shootButton.onClick.AddListener(OnShootPressed);
    }

    public void Clear()
    {
        nameText.text = "";
        quantityText.text = "";
        icon.enabled = false;
        glow.enabled = false;
        deleteButton.gameObject.SetActive(false);
        shootButton.gameObject.SetActive(false);
    }

    private void OnShootPressed()
    {
        homeUI.OnShootButton(slotUI);
    }

    private void OnDeletePressed()
    {
        homeUI.DeleteItem(slotUI.item);
    }
}