using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MysteryBoxUI : MonoBehaviour
{
    public DatabaseManager dbManager;

    public TMP_Text weaponNameText;
    public Image weaponIcon;

    public Button startButton;
    public Button acceptButton;
    public Button rejectButton;

    public HomeUI homeUI;

    private List<InventoryItem> allWeapons;
    private InventoryItem selectedWeapon;

    private bool isRolling = false;

    void Start()
    {
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
        weaponNameText.gameObject.SetActive(false);
        weaponIcon.gameObject.SetActive(false);

        // Cargar TODAS las armas de la BD
        allWeapons = dbManager.GetAllWeapons();
    }

    public void StartMysteryBox()
    {
        if (isRolling) return;

        isRolling = true;
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);

        StartCoroutine(RollWeapons());
    }

    IEnumerator RollWeapons()
    {
        weaponNameText.gameObject.SetActive(true);
        weaponIcon.gameObject.SetActive(true);
        startButton.interactable = false;

        float rollTime = 3f;
        float timer = 0f;

        while (timer < rollTime)
        {
            InventoryItem randomWeapon = allWeapons[Random.Range(0, allWeapons.Count)];

            weaponNameText.text = randomWeapon.nomObjecte;
            weaponIcon.sprite = Resources.Load<Sprite>("Icons/" + randomWeapon.nomObjecte);

            timer += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        // Seleccionar arma final
        selectedWeapon = allWeapons[Random.Range(0, allWeapons.Count)];

        weaponNameText.text = selectedWeapon.nomObjecte;
        weaponIcon.sprite = Resources.Load<Sprite>("Icons/" + selectedWeapon.nomObjecte);

        isRolling = false;

        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
    }

    public void AcceptWeapon()
    {
        int inv = PlayerPrefs.GetInt("InventariID");

        // Cargar inventario actual
        List<InventoryItem> items = dbManager.GetInventoryItems(inv);

        bool esApilable = selectedWeapon.esPotApilar == 1;

        // Si es arma NO apilable y ya hay 8 armas reemplazar la seleccionada
        if (!esApilable && items.Count >= 8)
        {
            if (homeUI.selectedSlotIndex == -1)
            {
                Debug.Log("No hay slot seleccionado para reemplazar.");
                return;
            }

            // Comprobar si el arma nueva ya está en el inventario
            bool yaExiste = items.Exists(i => i.objecteID == selectedWeapon.objecteID);
            if (yaExiste)
            {
                Debug.Log("El arma nueva ya está en el inventario. No se reemplaza nada.");
                return;
            }

            InventoryItem itemAReemplazar = items[homeUI.selectedSlotIndex];

            // Borrar arma antigua
            dbManager.DeleteItemFromInventory(inv, itemAReemplazar.objecteID);

            // Insertar arma nueva
            dbManager.AddOrReplaceItem(inv, selectedWeapon.objecteID, 1);
        }
        else
        {
            // Caso normal
            int cantidad = esApilable ? 3 : 1;
            dbManager.AddOrReplaceItem(inv, selectedWeapon.objecteID, cantidad);
        }

        homeUI.ApplyMaxAmmoToWeapon(inv, selectedWeapon.objecteID);

        homeUI.RefreshInventoryUI();

        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
        weaponNameText.gameObject.SetActive(false);
        weaponIcon.gameObject.SetActive(false);
        startButton.interactable = true;
    }

    public void RejectWeapon()
    {
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
        weaponNameText.gameObject.SetActive(false);
        weaponIcon.gameObject.SetActive(false);
        startButton.interactable = true;
    }
}