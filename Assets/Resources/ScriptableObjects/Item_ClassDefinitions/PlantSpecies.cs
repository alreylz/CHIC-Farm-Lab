using UnityEngine;

//[LEGACY] delete on changes 
[System.Serializable]
[CreateAssetMenu(fileName = "New Plant Species", menuName = "CHIC-Project/PlantSpecies" )]
public class PlantSpecies : ScriptableObject
{
    /* Nombre de la especie */
    new public string name = "New Plant Species "; 

    /* Representaciones IN-GAME */
    public Sprite plantIcon;
    public GameObject seedPrefab;
    public GameObject sackPrefab;
    public GameObject rootPrefab;
    public GameObject plantPrefab;
    /* Datos para lógica del juego */

    public float baseHealth; /* Vida máxima de los ejemplares de planta de esta especie */
    public uint growingTime; /* Tiempo en días que tarda en crecer la planta */
    public float nutritiousValue = 100 ;  /*Valor nutritivo; es un añadido al beneficio al vender las plantas */
    public float resistance = 0; /*Resistencia a plagas */
    public float purchasingPrice = 200; /* Precio de compra en tienda*/

    //Valores susceptibles de ser modificados

    //Propiedad alimenticia
    public uint inulin;
    //Propiedad farmacéutica
    public uint terpenes;

}
