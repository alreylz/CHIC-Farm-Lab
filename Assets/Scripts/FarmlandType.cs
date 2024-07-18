using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Farmland Type", menuName = "CHIC-Project/FarmlandType")]
public class FarmlandType : ScriptableObject
{
    new public string name = "Terreno por defecto";

    public Sprite farmlandIcon; 
    public GameObject shopPrefab;
    public GameObject fieldPrefab;
    /* Tamaño del terreno de plantación (por defecto 0.5m) */ 
    public Vector2 fieldDimensions = new Vector2(0.5f, 0.5f);

    /* Área de la zona de plantación en metros cuadrados */
    public float FarmingArea {
        get { return fieldDimensions.x * fieldDimensions.y;
        }
    }

    /* Número de plantas que caben en el trozo de campo; por 2 por metro cuadrado */
    public uint FittingPlants
    {
        get { return (FarmingArea <= 0.5f) ? 1U : (FarmingArea <= 1) ? 2U : (uint) Mathf.FloorToInt(2 * FarmingArea); }
    } 

    public uint purchasingPrice; // Precio de adquisición en tienda
    public uint sellingPrice; // Precio de venta en caso de querer prescindir del terreno una vez comprado
  
}
