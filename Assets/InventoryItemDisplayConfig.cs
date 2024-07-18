using UnityEngine;
/* Datos en relación a tipo de elemento */
//[CreateAssetMenu(fileName = "NewItemDisplayConfig", menuName = "CHIC-FarmLab/Configs/ItemDisplayConfig")]
[System.Serializable]
public class InventoryItemDisplayConfig //: ScriptableObject
{
    public new string name = "Introduce aquí el nombre del elemento o tipo que quieres configurar para su visualización";
    public Vector3 scaleInInventory = Vector3.one;
    public Vector3 scaleOutInventory = Vector3.one;
    public Vector3 rotationInInventory = Vector3.zero;
    public Vector3 rotationOutInventory = Vector3.zero;
    public Vector3 offsetInInventory = Vector3.zero;
}