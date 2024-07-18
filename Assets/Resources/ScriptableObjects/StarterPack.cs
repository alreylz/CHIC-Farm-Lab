using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Starter Pack", menuName = "CHIC-FarmLab/Inventory/StarterPack")]
public class StarterPack : ScriptableObject
{
    public List<PlantSpecies> _Plants;
    public List<FarmlandType> _Fields;
    //public List<ToolType> _Tools;
}