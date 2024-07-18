using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
/* Componente encargado de la creación aleatoria de variedades */
[RequireComponent(typeof(Lab_PlantVarietyExplorer_Controller))]
public class Lab_RndModMenu_Controller : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _LogsOn = false;
    public bool LogsOn { get { return _LogsOn; } set { _LogsOn = value; } }
    #endregion Debug Property

    [Header("Data Sources - Active plant & Available Modifications")]
    [SerializeField]
    PlantModificationLoader _ModLoader;
    [SerializeField]
    Lab_PlantVarietyExplorer_Controller _SrcVarietyController;
    [SerializeField]
    PlantVariety _SrcVariety;

    [Header("Modification Tracking")]
    [SerializeField]
    private List<PlantModification> availableModifications;
    [SerializeField]
    private PlantModification chosenModificationToApply;
    [Header("Result of last modification")]

    [SerializeField]
    private PlantVariety outputVariety;


    public Action<PlantVariety, PlantVariety> OnPlantModificationCompleted;

    private void Awake()
    {
        if (_SrcVarietyController == null)
        {
            _SrcVarietyController = GetComponent<Lab_PlantVarietyExplorer_Controller>();
        }
        _SrcVarietyController.onActivePlantVarietyUpdated.AddListener((p) => _SrcVariety = p); //Suscripción a cambios en explorador de variedades, para saber a qué variedad aplicaríamos la modificación.

    }

    private void OnEnable()
    {
        if (_ModLoader != null && (availableModifications == null || availableModifications.Count == 0))
            availableModifications = _ModLoader.PossibleModifications;
        else
        {
            if(_ModLoader == null)
                CHIC.Utilities.Error("_ModLoader is null, couldn't load all plant modifications ", this.GetType().ToString(), gameObject);
        }
        if (LogsOn) CHIC.Utilities.DebugList<PlantModification>(availableModifications, this.GetType().ToString());
        
    }
    private void OnDisable()
    {
        outputVariety = null;
    }


    //[PENDING @telmoz] Definir aquí cuál es el criterio de elección de una modificación u otra.
    public void ChooseRandomModification()
    {
        int index;
        chosenModificationToApply = CHIC.Utilities.ChooseRandomlyFromList(availableModifications, out index);
    }


    public void ApplyModification(PlantVariety varietyToMutate)
    {
        _SrcVariety = varietyToMutate;
    }

    public void ApplyModification()
    {
        if (chosenModificationToApply == null) ChooseRandomModification();
        string modName = chosenModificationToApply.name;

        //1. Generamos nueva variedad de planta desde Src y modificación:
        PlantVarietySerializable nuPlant = PlantVarietySerializable.CreateModVariety(
            PlantVarietySerializable.FromPlantVariety(_SrcVariety), chosenModificationToApply, modName);
        //2. Nueva variedad a almacenamiento Persistente:
        if (!nuPlant.Save(modName, SaveFormat.BINARY))
        {
            Debug.LogError("Error GUARDANDO nueva variedad generada llamada " + modName);
        }
        //3.Cargado de datos desde disco para comprobar el correcto funcionamiento de planta
        PlantVarietySerializable nuPlantLoad = (PersistentDataManager.Load(modName + ".pv") as PlantVarietySerializable);
        if (nuPlantLoad == null) { Debug.LogError("Error CARGANDO la variedad que se acaba de generar desde disco "); return; }

        outputVariety = nuPlantLoad.ToPlantVariety();
        if (outputVariety != null)
            OnPlantModificationCompleted?.Invoke(outputVariety, _SrcVariety);

        Debug.Log("¡¡¡ NUEVA VARIEDAD FUE GENERADA !!!" + outputVariety.PrintMarkdown());

        //4.[PENDING @coredamnwork] Añadir semillas al inventario.
    }


    /*  public void LoadPlantVarieties()
      {
          //a) Cargado de plantas creadas at design time (definidas en el editor como Scriptable Objects)
          string[] guidsToPlantVarieties = AssetDatabase.FindAssets("t:PlantVariety", new[] { "Assets" });
          foreach (var guid in guidsToPlantVarieties)
          {
              //Debug.LogWarning(guid);
              PlantVariety retrievedPlant = AssetDatabase.LoadAssetAtPath<PlantVariety>(AssetDatabase.GUIDToAssetPath(guid));
              if (allKnownVarieties.Contains(retrievedPlant)) continue;
              else allKnownVarieties.Add(retrievedPlant);
          }

          //b) Cargado de plantas creadas en tiempo de ejecución (aquellas con extensión ".pv")
          string[] foundFiles = PersistentDataManager.GetAllFilepathsWithExtension("pv");
          foreach (var f in foundFiles)
          {
              //Debug.Log("found :" + f);

              PlantVarietySerializable sP = PersistentDataManager.LoadFullPath(f) as PlantVarietySerializable;
              if (sP == null) continue;
              //Debug.LogWarning(sP.PrintBasics());
              PlantVariety retrievedVariety = sP.ToPlantVariety();
              if (!allKnownVarieties.Contains(retrievedVariety)) allKnownVarieties.Add(retrievedVariety);

          }

      }

      public void LoadAllPlantMods()
      {
          //Cargamos modificaciones creadas desde el editor de Unity (como Scriptable Objects)
          string[] guidsToPlantMods = AssetDatabase.FindAssets("t:PlantModification", new[] { "Assets" });
          foreach (var guid in guidsToPlantMods)
          {
              //Debug.LogWarning(guid);
              PlantModification retrievedPM = AssetDatabase.LoadAssetAtPath<PlantModification>(AssetDatabase.GUIDToAssetPath(guid));
              if (availableModifications.Contains(retrievedPM)) continue ;
              else availableModifications.Add(retrievedPM);
          }

      }

      */

}