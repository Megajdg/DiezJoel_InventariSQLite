using UnityEngine;
using TMPro;

public class WeaponTooltipUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text upgradedText;
    public TMP_Text damageText;
    public TMP_Text ammoText;
    public TMP_Text maxAmmoText;
    public TMP_Text costText;
    public TMP_Text descriptionText;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show(InventoryItem item)
    {
        nameText.text = item.nomObjecte;
        if (item.estaMillorada == 0)
            upgradedText.text = "Mejorada: No";
        else upgradedText.text = "Mejorada: Si";
        damageText.text = "Daño: " + item.dany;
        ammoText.text = "Munición: " + item.quantitatMax;
        costText.text = "Coste: " + item.cost;
        if (item.nomObjecte == "Monkey Bomb")
            maxAmmoText.text = "Recámara: No";
        else
            maxAmmoText.text = "Recámara: " + item.quantitatMax * 10;
        descriptionText.text = item.descripcio;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
