using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Farmland: *
 * - Clase que contiene instancias no MonoBehaviour (solo datos) para pasar información acerca de una un terreno de plantación  */
[System.Serializable]
public class Farmland : Item
{
    public string Name;
    public long FarmlandID { get; private set; }

    public Sprite farmlandIcon;
    public GameObject shopPrefab;
    public GameObject fieldPrefab;

    public uint fittingPlants; /* Número de plantas que caben en el trozo de campo */
    public Vector2 fieldDimensions = new Vector2(1, 1);
    public float FarmingArea
    {
        get
        {
            return fieldDimensions.x * fieldDimensions.y;
        }
    }

    public float purchasingPrice;
    public float sellingPrice;
    
    public Farmland(FarmlandType farmlandType)
    {
        ID = NextItemID;
        type = ItemType.Farmland;
        Name = "Farmland " + ID.ToString(); 
        farmlandIcon = farmlandType.farmlandIcon;
        shopPrefab = farmlandType.shopPrefab;
        fieldPrefab = farmlandType.fieldPrefab;
        fittingPlants = farmlandType.FittingPlants;
        fieldDimensions = farmlandType.fieldDimensions;
        purchasingPrice = farmlandType.purchasingPrice;
        sellingPrice = farmlandType.sellingPrice;
    }
   
    public override string ToString()
    {
        string plantString = "<b> Farmland: </b> \n" +
            "ID: " + this.ID + "\n" +
            "Selling price: " + this.sellingPrice + "\n" +
            "Dimensions: "+ fieldDimensions.x.ToString() + fieldDimensions.y.ToString()+ "\n"+
            "Can grow "+ this.fittingPlants + " plants \n"+
            "Shopping price:" + this.purchasingPrice ;
        plantString.Replace("\\n", "\n");

        return plantString;
    }
}
