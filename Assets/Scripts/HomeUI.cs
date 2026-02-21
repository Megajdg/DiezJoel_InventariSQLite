using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class HomeUI : MonoBehaviour
{
    public TMP_Text welcomeText;
    [SerializeField] DatabaseManager dbManager;

    public Transform inventoryContainer;
    public GameObject inventoryItemPrefab;

    public int selectedSlotIndex = -1;

    public List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    public WeaponTooltipUI tooltip;

    void Start()
    {
        string user = PlayerPrefs.GetString("LoggedUser", "Invitado");
        welcomeText.text = "Welcome, " + user + "!";

        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        int inventariID = PlayerPrefs.GetInt("InventariID", -1);
        if (inventariID == -1) return;

        foreach (Transform child in inventoryContainer)
            Destroy(child.gameObject);

        slotUIs.Clear();

        List<InventoryItem> items = dbManager.GetInventoryItems(inventariID);

        for (int i = 0; i < 8; i++)
        {
            GameObject obj = Instantiate(inventoryItemPrefab, inventoryContainer);
            InventoryItemUI itemUI = obj.GetComponent<InventoryItemUI>();
            InventorySlotUI slotUI = obj.GetComponent<InventorySlotUI>();

            InventoryItem itemData = i < items.Count ? items[i] : null;

            // Asignar el item al slot
            slotUI.Setup(this, i, itemData, itemUI);

            // Decirle al itemUI cuál es su slot
            itemUI.slotUI = slotUI;

            // Actualizar la UI del item
            if (itemData != null)
                itemUI.Setup(itemData, this);
            else
                itemUI.Clear();

            slotUIs.Add(slotUI);
        }
    }

    public void SelectSlot(int index)
    {
        selectedSlotIndex = index;

        // Actualizar visualmente
        for (int i = 0; i < slotUIs.Count; i++)
            slotUIs[i].SetSelected(i == index);

        Debug.Log("Slot seleccionado: " + index);
    }

    public void ShowTooltip(InventoryItem item, Vector3 position)
    {
        tooltip.Show(item);
    }

    public void HideTooltip()
    {
        tooltip.Hide();
    }

    public void OnUpgradeButton()
    {
        if (selectedSlotIndex == -1)
        {
            Debug.Log("No hay arma seleccionada.");
            return;
        }

        int inventariID = PlayerPrefs.GetInt("InventariID");
        List<InventoryItem> items = dbManager.GetInventoryItems(inventariID);

        if (selectedSlotIndex >= items.Count)
        {
            Debug.Log("El slot seleccionado está vacío.");
            return;
        }

        InventoryItem arma = items[selectedSlotIndex];

        if (arma.estaMillorada == 1)
        {
            Debug.Log("Esta arma ya está mejorada.");
            return;
        }

        dbManager.UpgradeInventoryWeapon(inventariID, arma.objecteID);

        ApplyMaxAmmoToWeapon(inventariID, arma.objecteID);

        RefreshInventoryUI();

        Debug.Log("Arma mejorada con éxito.");
    }

    public void DeleteItem(InventoryItem item)
    {
        int inventariID = PlayerPrefs.GetInt("InventariID");

        dbManager.DeleteItemFromInventory(inventariID, item.objecteID);

        string keyPrefix = inventariID + "_" + item.objecteID;

        // Si estaba mejorada, borrar el flag PAP
        PlayerPrefs.DeleteKey("PAP_" + keyPrefix);

        RefreshInventoryUI();
    }

    public void OnShootButton(InventorySlotUI slot)
    {
        if (slot == null || slot.item == null) return;

        var arma = slot.item;

        // Si no tiene balas recargar automáticamente
        if (arma.municionEnCargador <= 1 && arma.nomObjecte != "Monkey Bomb" && arma.municionEnReserva > 0)
        {
            Recargar(arma);
        }
        else if (arma.municionEnCargador <= 0 && arma.nomObjecte == "Monkey Bomb")
        {
            Recargar(arma);
        }
        else if (arma.municionEnCargador >= 0)
        {
            arma.municionEnCargador--;
            if (arma.municionEnCargador < 0)
                arma.municionEnCargador = 0;
        }

        int inventariID = PlayerPrefs.GetInt("InventariID"); 
        string keyPrefix = inventariID + "_" + arma.objecteID;

        // Guardar
        PlayerPrefs.SetInt("MAG_" + keyPrefix, arma.municionEnCargador);
        PlayerPrefs.SetInt("RES_" + keyPrefix, arma.municionEnReserva);

        // Actualizar SOLO este slot
        slot.itemUI.Setup(arma, this);
    }

    private void Recargar(InventoryItem arma)
    {
        if (arma.municionEnReserva <= 1) return;

        int needed = arma.quantitatMax;
        int toLoad = Mathf.Min(needed, arma.municionEnReserva);

        arma.municionEnCargador = toLoad;
        arma.municionEnReserva -= toLoad;

        int inventariID = PlayerPrefs.GetInt("InventariID");
        string keyPrefix = inventariID + "_" + arma.objecteID;

        PlayerPrefs.SetInt("MAG_" + keyPrefix, arma.municionEnCargador);
        PlayerPrefs.SetInt("RES_" + keyPrefix, arma.municionEnReserva);
    }

    public void OnMaxAmmoButton()
    {
        foreach (var slot in slotUIs)
        {
            if (slot.item == null) continue;

            InventoryItem arma = slot.item;

            // Si es Monkey Bomb reserva = 0
            if (arma.nomObjecte == "Monkey Bomb")
            {
                arma.municionEnCargador = arma.quantitatMax;
                arma.municionEnReserva = 0;
            }
            else
            {
                // Armas normales todo al máximo
                arma.municionEnCargador = arma.quantitatMax;
                arma.municionEnReserva = arma.quantitatMax * 10;
            }

            int inventariID = PlayerPrefs.GetInt("InventariID");
            string keyPrefix = inventariID + "_" + arma.objecteID;

            // Guardar
            PlayerPrefs.SetInt("MAG_" + keyPrefix, arma.municionEnCargador);
            PlayerPrefs.SetInt("RES_" + keyPrefix, arma.municionEnReserva);

            // Actualizar UI
            slot.itemUI.Setup(arma, this);
        }
    }

    public void ApplyMaxAmmoToWeapon(int inventariID, int objecteID)
    {
        string keyPrefix = inventariID + "_" + objecteID;

        // Cargar stats del arma desde la BD
        InventoryItem arma = dbManager.GetInventoryItem(inventariID, objecteID);

        if (arma == null) return;

        if (arma.nomObjecte == "Monkey Bomb")
        {
            arma.municionEnCargador = arma.quantitatMax;
            arma.municionEnReserva = 0;
        }
        else
        {
            arma.municionEnCargador = arma.quantitatMax;
            arma.municionEnReserva = arma.quantitatMax * 10;
        }

        PlayerPrefs.SetInt("MAG_" + keyPrefix, arma.municionEnCargador);
        PlayerPrefs.SetInt("RES_" + keyPrefix, arma.municionEnReserva);
    }


    public void OnLogoutButton()
    {
        PlayerPrefs.DeleteKey("LoggedUser");
        PlayerPrefs.DeleteKey("InventariID");
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}