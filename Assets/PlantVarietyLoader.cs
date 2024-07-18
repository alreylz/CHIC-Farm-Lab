using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using System;
using CHIC;
/// <summary>
/// Carga todas las variedades de plantas que ha generado el usuario o están disponibles de alguna manera. Proporciona además un punto de acceso único a las variedades.
/// </summary>
public class PlantVarietyLoader : Singleton<PlantVarietyLoader>
{

    #region Debug Property
    [SerializeField]
    private bool _LogsOn = false;
    public bool LogsOn { get { return _LogsOn; } set { _LogsOn = value; } }
    #endregion Debug Property


    [Header("Source Varieties")]
    [SerializeField]
    private List<PlantVariety> _KnownPlantVarieties = new List<PlantVariety>();
    public List<PlantVariety> KnownPlantVarieties
    {
        get => _KnownPlantVarieties;
    }
    
    private void Awake()
    {
        Instance = this;
        LoadPlantVarietiesIfNotAlreadyInList();
    }

    void OnEnable()
    {
        LoadPlantVarietiesIfNotAlreadyInList();
    }
    public void TryUpdate()
    {
        LoadPlantVarietiesIfNotAlreadyInList();
    }
    public void LoadPlantVarietiesIfNotAlreadyInList()
    {
        //a) Cargado de plantas creadas at design time (definidas en el editor como Scriptable Objects)
       /* string[] guidsToPlantVarieties = AssetDatabase.FindAssets("t:PlantVariety", new[] { "Assets" });
        foreach (var guid in guidsToPlantVarieties)
        {
            //Debug.LogWarning(guid);
            PlantVariety retrievedPlant = AssetDatabase.LoadAssetAtPath<PlantVariety>(AssetDatabase.GUIDToAssetPath(guid));
            if (_KnownPlantVarieties.Contains(retrievedPlant)) continue;
            else _KnownPlantVarieties.Add(retrievedPlant);
        }
        */

        //b) Cargado de plantas creadas en tiempo de ejecución (aquellas con extensión ".pv")
        string[] foundFiles = PersistentDataManager.GetAllFilepathsWithExtension("pv");
        foreach (var f in foundFiles)
        {
            //Debug.Log("found :" + f);
            PlantVarietySerializable sP = PersistentDataManager.LoadFullPath(f) as PlantVarietySerializable;
            if (sP == null) continue;
            //Debug.LogWarning(sP.PrintBasics());
            PlantVariety retrievedVariety = sP.ToPlantVariety();
            if (!_KnownPlantVarieties.Contains(retrievedVariety)) _KnownPlantVarieties.Add(retrievedVariety);

        }

    }
    
}
