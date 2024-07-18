using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewInventoryItemConfig", menuName = "CHIC-FarmLab/Configs/InventoryItemsConfig")]
public class SO_InventoryItemsConfiguration : ScriptableObject
{
   

    public string comment;
    [Header("Correspondencia Tipo <-> Display ")]
    public List<InventoryItemDisplayConfig> displayConfigList;
    /* Obtiene la configuración asociada (cuáles es la escala y rotación) de los objetos de un tipo en el Inventario */
    public InventoryItemDisplayConfig GetInventoryDisplayConfigByType(Type type)
    {
        foreach(var configElem in displayConfigList)
        {
            if (configElem.name == type.ToString()) return configElem;
        }
        return null;
    }


}
