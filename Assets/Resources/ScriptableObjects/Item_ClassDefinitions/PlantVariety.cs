using UnityEngine;
using CHIC;
using System;

[CreateAssetMenu(fileName = "New Plant Variety", menuName = "CHIC-FarmLab/Blueprints/PlantVarietyBlueprint")]
public class PlantVariety : ScriptableObject, IEquatable<PlantVariety>
{
    private Guid guid = System.Guid.NewGuid();
    new public string name = "New Plant Variety (Basic Chicory)";
    public string nombre = "Nueva variedad de planta (Achicoria básica)";
    public string especie = "Especie por defecto (e.g. Achicoria)";
    public string species = "Default plant species (e.g. Chicory)";

    //public bool isMod = false;

    public string infoEN = "Enter here a description about this variety and why it is special .";
    public string infoES = "Incluye aquí información que consideres relevante para el usuario acerca de esta variedad.";

    [Header("In-Game representations")]
    /* Representaciones IN-GAME */
    public Sprite plantIcon;
    public GameObject seedPrefab;
    public GameObject sackPrefab;
    public GameObject rootPrefab;
    public GameObject plantPrefab;
    
    [Header("Characteristics")]
    /* Datos para lógica del juego */
    public float health; /* Vida de los ejemplares de esta variedad */
    public uint growingTime; /* Tiempo en días que tarda en crecer la planta */
    public float sproutingTime;
    public float leavesGrowingTime;
    public float floweringTime;

    [Header("Production values")]
    public uint terpeneProduction;
    public uint inulinProduction;
    public uint leavesProduction;

    [Header("Needs / Environmental conditions")]
    public uint resistanceToActiveDamage;
    public Interval_UInt acceptableTemperature;
    public float timeToDamageFromExtremeTemperature;

    public Interval_Float acceptableWaterLevels;
    public float waterConsumptionPerTimeUnit;
    public float timeToDamageFromDrought;
    public float timeToDamageFromFlood;

    public float inShopPrice = 200; /* Precio de compra en tienda */

    public string PrintBasics()
    {
        string fmtString = "PLANT VARIETY\n Nombre {0}\n Health: {1}\n inulinProduction:{2}\n terpeneProduction: {3}";
        return string.Format(fmtString, nombre, health, inulinProduction, terpeneProduction);
    }

    public string PrintMarkdown()
    {
        string fmtString = "<u><b>PLANT VARIETY</b></u>\n Nombre: <i>{0}</i>\n Health: {1}\n inulinProd:{2}\n terpeneProd: {3}";
        return string.Format(fmtString, nombre, health, inulinProduction, terpeneProduction);
    }

    
    /* Permite alterar los valores de una instancia de esta clase */
    public void Init(string name, string species, Sprite plantIcon/*, GameObject seedPrefab*/, GameObject sackPrefab, GameObject rootPrefab, GameObject plantPrefab, float health, uint growingTime, uint terpeneProduction, uint inulinProduction, uint leavesProduction, uint resistanceToActiveDamage, Interval_UInt acceptableTemperature, float timeToDamageFromExtremeTemperature, Interval_Float acceptableWaterLevels, float waterConsumptionPerTimeUnit, float timeToDamageFromDrought, float timeToDamageFromFlood, float inShopPrice)
    {
        this.name = name;
        this.nombre = name;
        this.especie = species;
        this.species = species;

        /* REVISIT @coredamnwork */
        //this.infoEN = "Enter here a description about this variety and why it is special .";
        //this.infoES = "Incluye aquí información que consideres relevante para el usuario acerca de esta variedad.";

        /* Representaciones IN-GAME */
        this.plantIcon = plantIcon;
        this.sackPrefab = sackPrefab;
        this.rootPrefab = rootPrefab;
        this.plantPrefab = plantPrefab;

        /* Datos para lógica del juego */
        this.health = health; /* Vida de los ejemplares de esta variedad */
        this.growingTime = growingTime; /* Tiempo en días que tarda en crecer la planta */

        // FUTURE REVISIT @coredamnwork
        // this.sproutingTime;
        // this.leavesGrowingTime ;
        // this.floweringTime;
        
        this.terpeneProduction = terpeneProduction;
        this.inulinProduction = inulinProduction;
        this.leavesProduction = leavesProduction;


        this.resistanceToActiveDamage = resistanceToActiveDamage;
        this.acceptableTemperature = acceptableTemperature;
        this.timeToDamageFromExtremeTemperature = timeToDamageFromExtremeTemperature;

        this.acceptableWaterLevels = acceptableWaterLevels;
        this.waterConsumptionPerTimeUnit = waterConsumptionPerTimeUnit;
        this.timeToDamageFromDrought = timeToDamageFromDrought;
        this.timeToDamageFromFlood = timeToDamageFromFlood;

        this.inShopPrice = inShopPrice;

    }
    
    /* Permite la creación de una instancia de Variedad de planta donde los valores no explícitos son seteados por defecto (e.g. imágenes, iconos que representan a la variedad...);
     * [Uso para creación de variedades en tiempo de ejecución con persitencia (se guardan en archivo .pv)]*/
    public void InitEssentials(string name, string species/*, bool isMod*/, float health, uint growingTime, uint terpeneProduction, uint inulinProduction, uint leavesProduction, uint resistanceToActiveDamage, Interval_UInt acceptableTemperature, float timeToDamageFromExtremeTemperature, Interval_Float acceptableWaterLevels, float waterConsumptionPerTimeUnit, float timeToDamageFromDrought, float timeToDamageFromFlood, float inShopPrice)
    {
        this.name = name;
        this.nombre = name;
        this.especie = species;
        this.species = species;

        //this.isMod = isMod;

        /* REVISIT @coredamnwork */
        //this.infoEN = "Enter here a description about this variety and why it is special .";
        //this.infoES = "Incluye aquí información que consideres relevante para el usuario acerca de esta variedad.";

        /* Representaciones IN-GAME */
        this.plantIcon = Resources.Load("2D-Art/2048w/Mod Plant Icon (v1) - AuxiliaryHighRes") as Sprite;
        this.sackPrefab = Resources.Load("Prefabs/Defaults/sackPrefab") as GameObject;
        this.rootPrefab = Resources.Load("Models/ChicRoot") as GameObject;
        this.plantPrefab = Resources.Load("Models/Achicoria_Plant") as GameObject;

        /* Datos para lógica del juego */
        this.health = health; /* Vida de los ejemplares de esta variedad */
        this.growingTime = growingTime; /* Tiempo en días que tarda en crecer la planta */

        // FUTURE REVISIT @coredamnwork
        // this.sproutingTime;
        // this.leavesGrowingTime ;
        // this.floweringTime;


        this.terpeneProduction = terpeneProduction;
        this.inulinProduction = inulinProduction;
        this.leavesProduction = leavesProduction;


        this.resistanceToActiveDamage = resistanceToActiveDamage;
        this.acceptableTemperature = acceptableTemperature;
        this.timeToDamageFromExtremeTemperature = timeToDamageFromExtremeTemperature;

        this.acceptableWaterLevels = acceptableWaterLevels;
        this.waterConsumptionPerTimeUnit = waterConsumptionPerTimeUnit;
        this.timeToDamageFromDrought = timeToDamageFromDrought;
        this.timeToDamageFromFlood = timeToDamageFromFlood;

        this.inShopPrice = inShopPrice;
    }

    public bool Equals(PlantVariety other)
    {
        return (guid == other.guid) ? true : false;
    }
}
