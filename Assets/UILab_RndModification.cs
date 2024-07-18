using System.Collections.Generic;
using UnityEngine;
using CHIC;
using System;
/// <summary>
/// UIScreen: Clase abstracta (que no se puede instanciar) que define la funcionalidad estándar de un controlador de interfaz de usuario (UI)
/// Las clases derivadas se encargan principalmente de mostrar una determinada pantalla y gestionar el aspecto visual de la misma, permitiendo la
/// aparición de diferentes elementos (paneles, dialogs, otras pantallas...)
/// </summary>
public abstract class UIScreen : MonoBehaviour, IUIMenuLayer, IEquatable<UIScreen>
{
    private Guid _guid = System.Guid.NewGuid();
    public Guid guid { get => _guid; }

    
    [SerializeField]
    protected Transform rootCanvas;
    public Transform RootCanvas { get => rootCanvas; }
    //Operaciones: Mostrar, Ocultar, Mostrar pasando parámetros
    public virtual void Hide() {
        if (rootCanvas != null)
            rootCanvas.gameObject.SetActive(false);
    }
    public abstract void Show<T>(T screenParams) where T : IUIMenuLayerParams;
    public virtual void Show()
    {
        if (rootCanvas != null)
            rootCanvas.gameObject.SetActive(true);
    }

    public virtual bool Equals(UIScreen other)
    {

        return this.guid == other.guid;
    }
}


/* Se encarga del control de los elementos visuales de la pantalla de modificación aleatoria de planta */
public class UILab_RndModification : UIScreen
{

    [SerializeField]
    private UIPlantSummary originPanel;
    [SerializeField]
    private UIPlantSummary endPanel;

    [SerializeField]
    private GameObject unknownResultPlaceholder;

    [Header("Controlador de la lógica de esta pantalla")]
    public Lab_RndModMenu_Controller MyModController;
    public Lab_PlantVarietyExplorer_Controller MySrcVarController;


    [Header("Configuración de display de planta modificada")]
    private Color titleColor;
    private Color colorIfWorseStats;
    private Color colorIfBetterStats;


    private void Awake()
    {
        //1. Obtengo el Componente con los datos de la lógica de modificación de plantas
        if (MyModController == null)
            MyModController = GetComponent<Lab_RndModMenu_Controller>();
        //2. Obtengo componente de lógica de exploración de variedades existentes
        if (MySrcVarController == null)
            MySrcVarController = GetComponent<Lab_PlantVarietyExplorer_Controller>();

        //Obtengo todos los paneles que se encuentran dentro de esta pantalla.
        List<IUIPanel<IUIPanelParams>> panelesEnEstaPantalla = CHIC.Utilities.GetAllComponentsOfTypeInChildren<IUIPanel<IUIPanelParams>>(transform, hierarchy: gameObject.name);

        var component = panelesEnEstaPantalla[0];
        if (!(component is IUIPanel<UIModPlantSummaryParams>))
        {
            originPanel = (UIPlantSummary)panelesEnEstaPantalla[0];
            endPanel = (UIPlantSummary)panelesEnEstaPantalla[1];
        }
        else
        {
            originPanel = (UIPlantSummary)panelesEnEstaPantalla[1];
            endPanel = (UIPlantSummary)panelesEnEstaPantalla[0];
        }

    }

    private void OnEnable()
    {
        MySrcVarController.onActivePlantVarietyUpdated.AddListener(ShowScrPlantPanel);
        MyModController.OnPlantModificationCompleted += ShowOutputModificationPanel;
        MyModController.OnPlantModificationCompleted += (pn,po) => HideOutPlaceholder() ;

        ShowOutPlaceholder();
    }
    private void OnDisable()
    {
        MySrcVarController.onActivePlantVarietyUpdated.RemoveListener(ShowScrPlantPanel);
        MyModController.OnPlantModificationCompleted -= ShowOutputModificationPanel;
        MyModController.OnPlantModificationCompleted += (pn, po) => ShowOutPlaceholder();
    }

    public void ShowOutPlaceholder() => unknownResultPlaceholder.SetActive(true);
    public void HideOutPlaceholder() => unknownResultPlaceholder.SetActive(false);

    public void ShowScrPlantPanel(PlantVariety srcPlant)
    {

        // Enviar datos de planta a mostrar a script de tipo UIPlantSummary 1 : la que muestra esos datos en la jerarquía

        //1. Construcción de parámetros para panel de muestra de planta "Origen"
        UIPlantSummaryParams sparams = new UIPlantSummaryParams();
        sparams.plantData = srcPlant;
        sparams.minMaxValuesforPlantProperty = new Dictionary<string, Interval_Float>();
        sparams.minMaxValuesforPlantProperty.Add("health", new Interval_Float(0, 300));
        sparams.minMaxValuesforPlantProperty.Add("terpeneProduction", new Interval_Float(0, 300));
        sparams.minMaxValuesforPlantProperty.Add("inulinProduction", new Interval_Float(0, 300));

        // Ejecutar Show para provocar la muestra del panel o refresco de este
        originPanel.Show(sparams);
    }
    public void HideScrPlantPanel()
    {
        originPanel.Hide();
    }


    private void ShowOutputModificationPanel(PlantVariety nuPlant, PlantVariety oldPlant)
    {
        UIModPlantSummaryParams outputPanelParams = new UIModPlantSummaryParams();
        outputPanelParams.plantData = nuPlant;
        outputPanelParams.minMaxValuesforPlantProperty = new Dictionary<string, Interval_Float>();
        outputPanelParams.minMaxValuesforPlantProperty.Add("health", new Interval_Float(0, 300));
        outputPanelParams.minMaxValuesforPlantProperty.Add("terpeneProduction", new Interval_Float(0, 300));
        outputPanelParams.minMaxValuesforPlantProperty.Add("inulinProduction", new Interval_Float(0, 300));

        outputPanelParams.oldPlantData = oldPlant;
        outputPanelParams.titleColour = titleColor;
        outputPanelParams.worseStatsColour = colorIfWorseStats;
        outputPanelParams.betterStatsColour = colorIfBetterStats;

        endPanel.Show(outputPanelParams);

    }
    private void HideOutputModificationPanel()
    {
        endPanel.Hide();
    }
    
    /* Abrir y cerrar la propia ventana asociada a este sub-menú */
    public override void Show()
    {
        rootCanvas.gameObject.SetActive(true);
    }
    public override void Hide()
    {
        rootCanvas.gameObject.SetActive(false);
    }

    public override void Show<T>(T screenParams) 
    {
        Show();
    }



    //[@coredamnwork] PENDING Ventana de aviso.


}
