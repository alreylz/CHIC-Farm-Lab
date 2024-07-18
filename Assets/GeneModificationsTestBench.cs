using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using CHIC;

public class GeneModificationsTestBench : MonoBehaviour
{
//    /* Entradas al sistema de modificaciones */
//    public PlantVariety inPV;
//    public PlantModification modToApply;

//    Interactivo applyButton;
//    Canvas inPlantCanvas;
//    Canvas modCanvas;
//    Canvas outPlantCanvas;

//    public string newVarietyName = "Nueva variedad";
//    public SaveFormat saveFormat = SaveFormat.BINARY;

//    Image outPlantImg;
//    TextMeshProUGUI outPlantText;

//    private void Awake()
//    {
//       // OnValidate();
//    }

//    private void OnValidate()
//    {

//        //Input plant
//        if (inPlantCanvas == null) inPlantCanvas = transform.Find("General Canvas/Input Plant Variety").GetComponent<Canvas>();

//        Image inPlantImg =  inPlantCanvas.transform.Find("Plant Icon").GetComponent<Image>();
//        inPlantImg.sprite = inPV.plantIcon;

//        TextMeshProUGUI inPlantText = inPlantCanvas.transform.Find("Plant Info/Text").GetComponent<TextMeshProUGUI>();
//        inPlantText.SetText(inPV.PrintMarkdown());
//        //Modification to perform

//        if (modCanvas == null) modCanvas = transform.Find("General Canvas/Mod To Apply").GetComponent<Canvas>();
//        TextMeshProUGUI modText = modCanvas.transform.Find("Plant Info/Text").GetComponent<TextMeshProUGUI>();
//        Image modImage = modCanvas.transform.Find("Plant Icon").GetComponent<Image>();
//        modImage.sprite = modToApply.modificationIcon;
//        modText.SetText(modToApply.PrintMarkdown());



//        if (applyButton == null)
//        {
//            applyButton = transform.Find("General Canvas/Apply Button").GetComponent<Interactivo>();
//            applyButton.OnAirTap += () => { ApplyMod(modToApply, newVarietyName); };
//        }

//        ////Output plant
//        if (outPlantCanvas == null)
//        {
//            outPlantCanvas = transform.Find("General Canvas/Output Plant Variety").GetComponent<Canvas>();
//            outPlantImg = outPlantCanvas.transform.Find("Plant Icon").GetComponent<Image>();
//            outPlantText = outPlantCanvas.transform.Find("Plant Info/Text").GetComponent<TextMeshProUGUI>();
//        }


//    }



//    public void ApplyMod(PlantModification pm, string nameofVariety)
//    {

//        //Creamos variedad de planta en tiempo de ejecución; guardando una representación de esta en disco.
//        PlantVarietySerializable nu_plantVar =  PlantVarietySerializable.CreateModVariety(PlantVarietySerializable.FromPlantVariety(inPV), pm, nameofVariety);

//        PlantVarietySerializable loadVar = null;
//        if (nu_plantVar.Save(nameofVariety,format:saveFormat)){
//            //Cargamos la variedad generada para extraer datos de la misma; así podremos ver el resultado.
//            Debug.Log("Successful write");
//            loadVar = PersistentDataManager.Load(nameofVariety+".pv", format:saveFormat) as PlantVarietySerializable;
//            if(loadVar == null) { Debug.LogError(" ERROR LOADING FILE"); return; }
//        }
//        else
//        {
//            Debug.LogError("Error saving new plant variety");
//            return;
//        }

//        PlantVariety loadSimpleVar = loadVar.ToPlantVariety();

//        Debug.Log(loadVar.PrintBasics());


//        outPlantImg.sprite = loadSimpleVar.plantIcon;
//        outPlantText.SetText(loadSimpleVar.PrintMarkdown());

//        //La cargamos de disco, convertimos a un scriptable object (Plant Variety) y mostramos en el testbench

//    }


//    public void  GetAllModifications()
//    {


//      FileInfo [] files = new DirectoryInfo(Application.dataPath).GetFiles("*.pv");
//        foreach (var file in files)
//        {
//            Debug.Log(file.FullName);
//        }
//    }

    
}
