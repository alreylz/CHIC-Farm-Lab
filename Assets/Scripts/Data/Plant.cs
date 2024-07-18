using UnityEngine;
/* Plant:
 * - Clase que contiene instancias no MonoBehaviour (solo datos) para pasar información acerca de una planta durante el gameplay; 
 *  permitiendo crear Scriptable objects que representen especies concretas de plantas genéricas */
 [System.Serializable]

 public enum GrowingStatus
{
    NONE, /* Planta no ha sido utilizada aún */
    GERMINATION,
    GROWING,
    FLOWERING
}
[System.Serializable]
public class Plant : Item {


    [SerializeField]
    private string speciesName;
    public string SpeciesName { get => speciesName; }

    #region Representaciones 3D/2D
    /* Icono de la planta, para representaciones en 2D de la misma (e.g. semillas equipadas de planta X) */
    [SerializeField]
    private Sprite plantIcon;
    public Sprite PlantIcon { get { return plantIcon; } }
    /* Prefab de semillas asociado a estas plantas */
    [SerializeField]
    private GameObject seedPrefab;
    public GameObject SeedPrefab { get { return seedPrefab; } }
    [SerializeField]
    /* Prefab del saco de semillas asociado a esta planta */
    private GameObject sackPrefab;
    public GameObject SackPrefab { get { return sackPrefab; } }
    /* Prefab de la raíz (Especialmente importante para achicoria, se muestra junto a la información de UI de Farmland) */
    private GameObject rootPrefab;
    public GameObject RootPrefab { get { return rootPrefab; } }
    /* Prefab de la planta final */
    [SerializeField]
    private GameObject plantPrefab;
    public GameObject PlantPrefab { get => plantPrefab; }

    #endregion Representaciones 2D/3D


    /* Valor nutricional, que afecta al precio de venta */
    [SerializeField]
    private float nutritiousValue;
    public float NutritiousValue { get => nutritiousValue; }

    /* Precio por unidad de valor nutritivo */
    public static float ValueRate = 0.1f;
    /* Máxima ganancia en porcentaje (100% equivale a no ganar nada), vender por el mismo precio que compraste */
    public static uint MaxProfit = 200U;

    /* Precio de compra de las semillas de la planta */
    public float PurchasingPrice { get; private set; }
    public float immediateSellingPrice { get => PurchasingPrice * 0.9f; }
    public float SellingPrice {
        get {
            float WeightedProfitFactor = ((float)MaxProfit / 100.0f) * ((0.6f * HealthPercentage) + (0.4f * GrowthPercentage));
            /* Precio = %Profit * precioOriginal + extra añadido por valor nutricional */
            float priceTag = WeightedProfitFactor * (PurchasingPrice) + NutritiousValue * ValueRate;
            return priceTag;
            }
    }

    /*Estado de crecimiento*/
    public uint GrowingTimeRequired { get; private set; }
    public float GrowthPercentage { get; private set; }
    public GrowingStatus GrowthStatus {
        get => (GrowthPercentage < 0)? GrowingStatus.NONE : (GrowthPercentage <= 0.2) ? GrowingStatus.GERMINATION : (GrowthPercentage <= 0.6) ? GrowingStatus.GROWING : GrowingStatus.FLOWERING;
    }

    /* Vida actual de la planta y max Vida; multiplicadores del precio */
    public float CurrentHealth { get; private set; }
    public float HealthPercentage { get => CurrentHealth / BaseHealth; }
    public float BaseHealth { get; private set; }

    /* Valor de resistencia de la planta */
    [SerializeField]
    public float Resistance { get; private set; }

    /* [REVISIT] */
    public Plant()
    {
        ID = -1;
    }

    /* Constructor desde objeto persistente (que define tipologías de plantas) */
    public Plant(PlantSpecies species)
    {
        ID = NextItemID;
        type = ItemType.Plant;

        speciesName = species.name;

        plantIcon = species.plantIcon;
        seedPrefab = species.seedPrefab;
        sackPrefab = species.sackPrefab;
        rootPrefab = species.rootPrefab;
        plantPrefab = species.plantPrefab;

        GrowingTimeRequired = species.growingTime; 
        BaseHealth = species.baseHealth;
        CurrentHealth = BaseHealth;
        
        //Marcamos la planta como en el inventario; sin estado de crecimiento
        GrowthPercentage = -1f;
        Resistance = species.resistance;
        PurchasingPrice = species.purchasingPrice;
        
    }
    
    public bool IsInInventory()
    {
        return (GrowthStatus == GrowingStatus.NONE) ? true : false;
    }
    public bool IsInWorldSpace() { return !IsInInventory(); }
    
    /* Si la planta ha alcanzado el máximo de crecimiento, devuelve false, ya que no ha podido seguir creciendo*/
    public bool UpdateGrowth(float step)
    {
        if (GrowthPercentage == -1f) GrowthPercentage = step;
        GrowthPercentage += step;
        if (GrowthPercentage > 1) {
            GrowthPercentage = 1f; return false;
        }
        return true; 

    }

    /* Daña a la planta y devuelve true si la planta se ha quedado sin vida */
    public bool Hurt(float damage) {

        CurrentHealth -= (damage  - (Resistance*0.5f* damage)) ; //La resistencia evita hasta un 50% del daño     
        return (CurrentHealth <= 0)? true:false ;
    }


    public override string ToString()
    {
        if (ID < 0) return null;
        string plantString = " Planta: \n" +
            "ID: " + this.ID + "\n" +
            "Species: " + this.SpeciesName + "\n" +
            "Original Price:" + this.PurchasingPrice + "\n" +
            "Nutritious value: " + this.NutritiousValue + "\n"+
            "Base Health: "+ this.BaseHealth + "\n"+
            "Resistance: " + this.NutritiousValue + "\n"+
            "Growing time required :" + this.GrowingTimeRequired + "days \n";
            plantString.Replace("\\n", "\n");

        return plantString;
    }




}
