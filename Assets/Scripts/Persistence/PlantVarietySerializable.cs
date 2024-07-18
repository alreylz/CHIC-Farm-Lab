using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using CHIC;

// Uso para crear variedades de plantas en tiempo de ejecución
[System.Serializable]
public class PlantVarietySerializable : IPersistentObject<PlantVarietySerializable>
{
    string name = "New Plant Variety (Basic Chicory)";
    string nombre = "Nueva variedad de planta (Achicoria básica)";
    string especie = "Especie por defecto (e.g. Achicoria)";
    string species = "Default plant species (e.g. Chicory)";

    string infoEN = "Enter here a description about this variety and why it is special .";
    string infoES = "Incluye aquí información que consideres relevante para el usuario acerca de esta variedad.";

    /* Representaciones IN-GAME */
    /*Sprite plantIcon;
    GameObject seedPrefab;
    GameObject sackPrefab;
    GameObject rootPrefab;
    GameObject plantPrefab;
    */

    /* Datos para lógica del juego */
    float health; /* Vida de los ejemplares de esta variedad */
    uint growingTime; /* Tiempo en días que tarda en crecer la planta */
    float sproutingTime;
    float leavesGrowingTime;
    float floweringTime;

    uint terpeneProduction;
    uint inulinProduction;
    uint leavesProduction;

    uint resistanceToActiveDamage;
    public Interval_UInt acceptableTemperature;
    float timeToDamageFromExtremeTemperature;

    public Interval_Float acceptableWaterLevels;
    float waterConsumptionPerTimeUnit;
    float timeToDamageFromDrought;
    float timeToDamageFromFlood;

    float inShopPrice = 200; /* Precio de compra en tienda */


    public PlantVarietySerializable() { }
    public PlantVarietySerializable(string name, string species, Sprite plantIcon/*, GameObject seedPrefab*/, GameObject sackPrefab, GameObject rootPrefab, GameObject plantPrefab, float health, uint growingTime, uint terpeneProduction, uint inulinProduction, uint leavesProduction, uint resistanceToActiveDamage, Interval_UInt acceptableTemperature, float timeToDamageFromExtremeTemperature, Interval_Float acceptableWaterLevels, float waterConsumptionPerTimeUnit, float timeToDamageFromDrought, float timeToDamageFromFlood, float inShopPrice)
    {
        this.name = name;
        this.nombre = name;
        this.especie = species;
        this.species = species;

        /* REVISIT @coredamnwork */
        //this.infoEN = "Enter here a description about this variety and why it is special .";
        //this.infoES = "Incluye aquí información que consideres relevante para el usuario acerca de esta variedad.";

        /* Representaciones IN-GAME */
        /*this.plantIcon = plantIcon;
        this.sackPrefab = sackPrefab;
        this.rootPrefab = rootPrefab;
        this.plantPrefab = plantPrefab;
        */

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

    
    /* Revisit @ serializar iconos ??? */
    public static PlantVarietySerializable FromPlantVariety(PlantVariety pV)
    {
        return new PlantVarietySerializable(pV.name, pV.species, pV.plantIcon, pV.sackPrefab, pV.rootPrefab, pV.plantPrefab,
           pV.health, pV.growingTime, pV.terpeneProduction, pV.inulinProduction, pV.leavesProduction,
           pV.resistanceToActiveDamage, pV.acceptableTemperature, pV.timeToDamageFromExtremeTemperature, pV.acceptableWaterLevels,
           pV.waterConsumptionPerTimeUnit, pV.timeToDamageFromDrought, pV.timeToDamageFromFlood, pV.inShopPrice);
    }
    
    /*ToPlantVariety(): Parte de una representación de Planta serializable y la devuelve el equivalente Scriptable Object; en este caso, una PlantVariety */
    public PlantVariety ToPlantVariety()
    {
        // 1.Creamos nueva instancia de variedad de planta con valores que vienen de la variedad creada en tiempo de ejecución.
        PlantVariety p_Variety = ScriptableObject.CreateInstance<PlantVariety>();

        //2.Inicialización con valores alterados
        p_Variety.InitEssentials(this.name, this.species,
            this.health, this.growingTime, this.terpeneProduction, this.leavesProduction, this.inulinProduction,
            this.resistanceToActiveDamage, this.acceptableTemperature, this.timeToDamageFromExtremeTemperature,
            this.acceptableWaterLevels, this.waterConsumptionPerTimeUnit, this.timeToDamageFromDrought, this.timeToDamageFromFlood,
            this.inShopPrice);


        return p_Variety;
    }

    
    // Parto de una variedad de plantas ya existente y genero otra que serializo (e.g. la guardo en disco)
    // Es al fin y al cabo una variedad que el usuario ya ha aprendido a crear.


    /* Crea una variedad de planta nueva a partir de otra, con un nombre para la variedad especificado */
    public static PlantVarietySerializable CreateModVariety(PlantVarietySerializable baseVariety, PlantModification mod, string nuVarietyName)
    {
        PlantVarietySerializable nu_Variety;

        string nu_name = nuVarietyName;

        /*Aplicamos modificaciones de estilo a la nueva variedad (para que se muestre en la interfaz que estamos ante una planta modificada */
        Sprite nu_plantIcon = mod.modPlantIcon;
        GameObject nu_sackPrefab = mod.sackPrefab;
        GameObject nu_rootPrefab = mod.rootPrefab;
        GameObject nu_plantPrefab = mod.plantPrefab;
        /*Aplicamos modificaciones de lógica del juego a variedad original*/

        float nu_health = baseVariety.health + mod.healthMod;
        uint nu_growingTime = (uint)(Mathf.RoundToInt(baseVariety.growingTime + baseVariety.growingTime * mod.growingTimeMod));  /* Tiempo en días que tarda en crecer la planta; incremento máximo X percent */

        uint nu_terpeneProduction = baseVariety.terpeneProduction + mod.terpenesProductionMod;
        uint nu_inulinProduction = baseVariety.inulinProduction + mod.inulinProductionMod;
        uint nu_leavesProduction = baseVariety.leavesProduction + mod.leavesProductionMod;

        uint nu_resistanceToActiveDamage = baseVariety.resistanceToActiveDamage + (uint)mod.resistanceToActiveDamageMod;


        /* FUTURE USE (PERHAPS) */
        Interval_UInt nu_acceptableTemperature = new Interval_UInt(baseVariety.acceptableTemperature.Max + mod.minTemperatureMod, baseVariety.acceptableTemperature.Max + mod.maxTemperatureMod);

        float nu_timeToDamageFromExtremeTemperature = baseVariety.timeToDamageFromExtremeTemperature + mod.timeToDamageFromExtremeTemperatureMod;

        Interval_Float nu_acceptableWaterLevels = new Interval_Float(baseVariety.acceptableWaterLevels.Min + mod.minAcceptableWaterLevelMod,
            baseVariety.acceptableWaterLevels.Max + mod.maxAcceptableWaterLevelMod);
        float nu_waterConsumptionPerTimeUnit = baseVariety.waterConsumptionPerTimeUnit + mod.waterComsumptionPerTimeUnitMod;
        float nu_timeToDamageFromDrought = baseVariety.timeToDamageFromDrought + mod.timeToDamageFromDroughtMod;
        float nu_timeToDamageFromFlood = baseVariety.timeToDamageFromFlood + mod.timeToDamageFromFloodMod; ;

        /* REVISIT @coredamnwork */
        float inShopPrice = 200; // Precio de compra en tienda  debería quizá cambiar respecto al de plantas normalitas 

        //Inicialización con valores alterados
        nu_Variety = new PlantVarietySerializable(nu_name, baseVariety.species, nu_plantIcon, nu_sackPrefab, nu_rootPrefab, nu_plantPrefab, nu_health, nu_growingTime, nu_terpeneProduction, nu_inulinProduction, nu_leavesProduction,
                nu_resistanceToActiveDamage, nu_acceptableTemperature, nu_timeToDamageFromExtremeTemperature, nu_acceptableWaterLevels, nu_waterConsumptionPerTimeUnit, nu_timeToDamageFromDrought, nu_timeToDamageFromFlood, inShopPrice);

        return nu_Variety;
    }


    public string PrintBasics()
    {
        string fmtString = "PLANT VARIETY\n Nombre {0}\n Health: {1}\n inulinProduction:{2}\n terpenProduction: {3}";
        return string.Format(fmtString, nombre, health, inulinProduction, terpeneProduction);
    }


    #region IPersistentObject Interface

    public bool Save(string filename, SaveFormat format)
    {
        if (!filename.EndsWith(".pv")) filename += ".pv";
        return PersistentDataManager.Save(filename, this, format: format);

    }

    #endregion IPersistentObject Interface



}
