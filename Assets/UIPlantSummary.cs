using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CHIC;


public interface IUIPanelParams { }
public interface IUIMenuLayerParams { }

public interface IUIMenuLayer
{
    //Muestra lo necesario en una pantalla de UI dados unos parámetros de entrada
    void Show<T>(T screenParams) where T : IUIMenuLayerParams;
    void Show();
    void Hide();

}

// panel de vista de datos generales de una variedad de planta (solo lectura)
// parámetros -- > Plant data



//Todos los paneles que despliegan datos de solo visión implementan esta interfaz para que otros elementos más arriba en la jerarquía puedan localizarlos
// Requiere un tipo de parámetros que es una clase que implementa la interfaz IUIPanelParams
public interface IUIPanel<T> where T : IUIPanelParams
{
    bool Init(T parameters);
    bool Show(T parameters);
    bool Show();
    void Hide();
    T GetParams();
}



/// <summary>
/// Clase cuyas instancias encapsulan argumentos a un panel de UI de muestra de propiedades de la planta.
/// </summary>
public class UIPlantSummaryParams : IUIPanelParams
{
    public PlantVariety plantData; // Datos de la planta
    public Dictionary<string, Interval_Float> minMaxValuesforPlantProperty;  // Valores Min y máx


    public UIPlantSummaryParams() {

    }
    public UIPlantSummaryParams(PlantVariety pData, string [] properties, Interval_Float[] minMaxValues)
    {
        plantData = pData;
        minMaxValuesforPlantProperty = new Dictionary<string, Interval_Float>();
        for (int i=0; i<3; i++)
            minMaxValuesforPlantProperty.Add(properties[i], minMaxValues[i]);
    }
    
    public UIPlantSummaryParams(PlantVariety pData, string[] properties = null)
    {
        plantData = pData;
        minMaxValuesforPlantProperty = new Dictionary<string, Interval_Float>();

        //a) Si defino a mano los nombres de las propiedades a mostrar --> Doy rangos por defecto.
        if (properties != null)
        {
            Interval_Float[] defaultPropRange = GetPropertiesDefaultRange(properties);

            for (int i = 0; i < 3; i++)
                minMaxValuesforPlantProperty.Add(properties[i], defaultPropRange[i]);
        }
        //b) Si no, elijo también por defecto incluso las propiedades a mostrar (Vida, Terpenos e Inulina)
        else
        {
            string[] defaultShowingPropertyNames = new string[] { "health", "terpeneProduction", "inulinProduction" };

            Interval_Float[] defaultPropertyRanges =  GetPropertiesDefaultRange(defaultShowingPropertyNames);

            for (int i = 0; i < 3; i++)
                minMaxValuesforPlantProperty.Add(defaultShowingPropertyNames[i], defaultPropertyRanges[i]);
            
        }
    }



    //[REVISIT] @coredamnwork --> Gestionar valores máximos y mínimos de propiedades.
    public Interval_Float[] GetPropertiesDefaultRange(string [] props)
    {
        Interval_Float[] minMaxValues = new Interval_Float[3];
        //Valor mínimo para todas las propiedades == 0, máximo de momento son 1000
        for (int i = 0; i < 3; i++)
            minMaxValues[i] = new Interval_Float(0f, 1000f);

        return minMaxValues;
    }
    

}

public class UIModPlantSummaryParams : UIPlantSummaryParams
{
    // Contiene lo mismo que UIPlantSummaryParams + Los siguientes datos
    public PlantVariety oldPlantData; // Datos de la planta recién creada
    public Color titleColour;
    public Color worseStatsColour;
    public Color betterStatsColour;
    
}


// implementa la actualización de valores de una variedad de planta 
public class UIPlantSummary : MonoBehaviour, IUIPanel<IUIPanelParams>
{
    
    public string strID;
    public Transform rootCanvas;

    IUIPanelParams _Parameters;

    public TextMeshProUGUI plantName;
    public Image icon;

    [Header("Pares (etiqueta, valor)")]
    public TextMeshProUGUI[] p1 = new TextMeshProUGUI[2];
    public TextMeshProUGUI[] p2 = new TextMeshProUGUI[2];
    public TextMeshProUGUI[] p3 = new TextMeshProUGUI[2];
    private TextMeshProUGUI[][] propArray;
    [Header("Barras de carga")]
    
    public Image bar1;
    public Image bar2;
    public Image bar3;

    private Image[] imgBarArray;

    public void Hide()
    {
        rootCanvas.gameObject.SetActive(false);
    }

    public bool Init(IUIPanelParams parameters)
    {
        //Definimos los parámetros con los que posteriormente mostrar la pantalla actual (e.g. menú de propidades principales de la planta ==> Params son datos de la planta)
        if (parameters != null) _Parameters = parameters;
        else { Debug.LogError("Init UI Plant Summary; params are null so couldnt initialize panel"); return false; }
        
        propArray = new TextMeshProUGUI[3][] { p1 , p2, p3 } ;
        imgBarArray = new Image[] { bar1, bar2, bar3 };

        return true;
        
    }

    public bool Show(IUIPanelParams parameters)
    {
        if (!Init(parameters)) return false;

        return Show();

    }
    
    //Realiza inicialización de datos básicos de una planta, sin cambios ni formato especial
    private bool DisplayPanelSimple(UIPlantSummaryParams simpleParamPlantData)
    {
        plantName.SetText(simpleParamPlantData.plantData.name);
        icon.sprite = simpleParamPlantData.plantData.plantIcon;

        int visualPropToInit = 0;
        //Si el diccionario de valores máximos y mínimos de propiedades no es nulo:
        if (simpleParamPlantData.minMaxValuesforPlantProperty is Dictionary<string, Interval_Float> minmaxVals)
        {
            int numProps = minmaxVals.Count;
            if (numProps > 3)
            {
                Debug.LogWarning(" solo se mostrarán las tres primeras propiedades encontradas en el diccionario de valores");
            }
            // Inicialización de barras de propiedades y sus etiquetas
            foreach (var dEntry in minmaxVals)
            {

                int maxLabelLength = 11;

                string propLabel = dEntry.Key;
                if (propLabel.Length >= maxLabelLength) propLabel = propLabel.Substring(0, maxLabelLength);

                //Referencia a texto de propiedad a desplegar; 
                propArray[visualPropToInit][0].SetText(propLabel);  //nombre==label (e.g. terpenos)
                                                                     // Buscamos entre los campos del objeto planta y desplegamos el valor
                var fieldToDisplay = simpleParamPlantData.plantData.GetType().GetField(dEntry.Key);
                var typeOfField = fieldToDisplay.FieldType;
                var value = fieldToDisplay.GetValue(simpleParamPlantData.plantData);
                float valueF = System.Convert.ToSingle(value);

                
                string valuePropStr = ((int)valueF).ToString();
                if(valuePropStr.Length >= maxLabelLength) valuePropStr = valuePropStr.Substring(0, maxLabelLength);
                propArray[visualPropToInit][1].SetText(valuePropStr);

                //Asigno el porcentaje de llenado de la barra
                imgBarArray[visualPropToInit].fillAmount = (valueF - dEntry.Value.Min) / (dEntry.Value.Max - dEntry.Value.Min);
                visualPropToInit++; //Para inicializar la próxima propiedad.
                if (visualPropToInit >= 3) break;
            }

        }
        else
        {
            Debug.LogError("THIS SHOULD NEVER HAPPEN 1");
            return false;
        }
        
        return true;

    }

    //Realiza cambios en el formato una vez inicializado el panel con los valores adecuados
    private bool DisplayPanelMod(UIModPlantSummaryParams modParamPlantData)
    {

        //a)Cambiar color de fondo de título

        if (icon is Image backgroundTitle)
            backgroundTitle.color = modParamPlantData.titleColour;
        else { Debug.LogWarning("Couldnt find tiutle background for mod panel"); };

        //b) Cambiar color de labels si stats actuales son peores o mejores respecto a las de origen.
         var nuPlant = modParamPlantData.plantData;
        var oldPlant = modParamPlantData.oldPlantData;
        
        if (nuPlant.health < oldPlant.health) p1[1].color = modParamPlantData.worseStatsColour;
        else if (nuPlant.health > oldPlant.health) p1[1].color = modParamPlantData.betterStatsColour;
        
        if (nuPlant.terpeneProduction < oldPlant.terpeneProduction) p2[1].color = modParamPlantData.worseStatsColour;
        else if (nuPlant.terpeneProduction > oldPlant.terpeneProduction) p2[1].color = modParamPlantData.betterStatsColour;

        if (nuPlant.inulinProduction < oldPlant.inulinProduction) p3[1].color = modParamPlantData.worseStatsColour;
        else if (nuPlant.inulinProduction > oldPlant.inulinProduction) p3[1].color = modParamPlantData.betterStatsColour;

        return true;
    }
    
    public bool Show()
    {



        if (_Parameters is UIModPlantSummaryParams plantSummaryParamsMod)
        {
            if (!DisplayPanelSimple(plantSummaryParamsMod)) return false;
            if (!DisplayPanelMod(plantSummaryParamsMod)) return false;
            rootCanvas.gameObject.SetActive(true);


        }
        else if (_Parameters is UIPlantSummaryParams plantSummaryParams)
        {
            if (!DisplayPanelSimple(plantSummaryParams)) return false;
            rootCanvas.gameObject.SetActive(true);

        }
        else
        {
            Debug.LogError("Parameters different from UIPlantSummaryParams were entered");
        }
        
        return true;


    }
    
    public IUIPanelParams GetParams()
    {
        return _Parameters;
    }
}
