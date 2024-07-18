using UnityEngine;
using System.Text;
[CreateAssetMenu(fileName = "New Plant Modification", menuName = "CHIC-FarmLab/Blueprints/PlantModification")]
public class PlantModification : ScriptableObject , System.IEquatable<PlantModification>
{
    new public string name = "New Plant Modification" ;
    public string nombre = "Nueva modificáción de planta";

    public string detail = "Here you can add details regarding the modification represented by this blueprint";
    public string detalle = "Aquí puedes incluir detalles en relación a esta modificación para que el usuario comprenda lo que hace";

    [Header("In-Game Visuals")]
    public Sprite modificationIcon; //Quizá esto sería más bien un icono que represente la modificación y no el aspecto de la planta en el menú
    public Sprite modPlantIcon;
    public GameObject sackPrefab;
    public GameObject rootPrefab;
    public GameObject plantPrefab;

    [Header("-Basics - Plant modifiers (values added or substracted to original plant characteristics) ")]
    public int healthMod;
    public uint terpenesProductionMod;
    public uint inulinProductionMod;

    [Header("Future")]
    [Range(-0.1f,0.1f)]
    public float growingTimeMod;  /* Un porcentaje positivo o negativo*/
    public uint leavesProductionMod;
    public float resistanceToActiveDamageMod;
    public uint maxTemperatureMod;
    public uint minTemperatureMod;

    public float maxAcceptableWaterLevelMod;
    public float minAcceptableWaterLevelMod;

    public float waterComsumptionPerTimeUnitMod;
    public float timeToDamageFromExtremeTemperatureMod;
    public float timeToDamageFromDroughtMod;
    public float timeToDamageFromFloodMod;

    public bool Equals(PlantModification other)
    {
        if (name == other.name && healthMod == other.healthMod && terpenesProductionMod == other.terpenesProductionMod && inulinProductionMod == other.inulinProductionMod && detail == other.detail)
            return true;
        return false;
       
    }
    
    public override string ToString()
    {
        StringBuilder strBuilder = new StringBuilder().AppendFormat("Modification Name:{0}\n", name);
        strBuilder.AppendFormat("HP:{0}\t IP:{1}\t TP:{2}\n", healthMod,terpenesProductionMod,inulinProductionMod);

            return strBuilder.ToString();
    }

    public string PrintBasics()
    {
        return string.Format("MOD\n name: {0}\n health:{1}\n terpene prod:{2}\n inulin prod:{3}", name, healthMod, terpenesProductionMod, inulinProductionMod); 
            
    }

    public string PrintMarkdown()
    {
        string fmtString = "<u><b>MOD</b></u>\n Nombre: <i>{0}</i>\n Health: {1}\n inulinProd:{2}\n terpeneProd: {3}";
        return string.Format(fmtString, name, healthMod > 0 ? "+" + healthMod.ToString(): "-"+healthMod , inulinProductionMod, terpenesProductionMod);
    }



}
