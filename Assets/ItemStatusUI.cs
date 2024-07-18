
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using System;
using System.Text;

public class ItemStatusUI : MonoBehaviour
{
    public bool _LogsOn = false;
    private bool LogsOn {
        get => _LogsOn;
        }


    [Header("Farmland data source")]
    /* Field Data source */
    public FarmlandManager fieldStatusManager;

    private bool _IsInit {
        get
        {
            if(Item3DContainer != null && title != null && GrowingData!=null && Price!=null && GrowingStatusBar !=null && HealthBar != null)
            {
                return true;
            }
            return false;
        }
        }

    //Componentes de la UI
    public GameObject Item3DContainer;
    public TextMeshProUGUI title;
    public TextMeshProUGUI subtitle;

    public TextMeshProUGUI Price;
    public TextMeshProUGUI GrowingData;

    public TextMeshProUGUI smallTextInfo;

    
    public GameObject GrowingStatusBar; //BAR
    public GameObject HealthBar;//BAR

    public GameObject Button;

   
    [Range(0f,1.8f)][SerializeField]
    private float healthlevel;
    [Range(0f, 1.8f)]
    [SerializeField]
    private float growingStatusLevel;
    
    private const float _MaxScaleBars = 1.8f;




    
    private void Awake()
    {

        if (fieldStatusManager == null)
        {
            //Obtengo los datos del terreno de plantación asociado.
            fieldStatusManager = gameObject.GetComponentInParent<FarmlandManager>();
            if (fieldStatusManager == null) CHIC.Utilities.Error(" Couldn't find Farmland Manager associated to UI", this.GetType().ToString(), gameObject, "Awake");
        }
        
        if (!_IsInit) // Inicializamos los elementos que cambian en tiempo de ejecución según estado del terreno de plantación
        {
            GameObject UI3D = gameObject;
            Item3DContainer = UI3D.transform.Find("3DViewItem").gameObject;

            title = UI3D.transform.Find("ItemName").GetComponent<TextMeshProUGUI>(); // Text component for name of plant
            subtitle = UI3D.transform.Find("ItemSubtitle").GetComponent<TextMeshProUGUI>(); // Text component for subtitle 

            GrowingData = UI3D.transform.Find("GrowingStatusText").GetComponent<TextMeshProUGUI>(); // Growing status (e.g. Germination)
            Price = UI3D.transform.Find("Price").GetComponent<TextMeshProUGUI>(); // Price for sale, changing as the plant grows

            //Status visualisation elementos
            GrowingStatusBar = UI3D.transform.Find("GrowingStatusBar").Find("ScalableCube").gameObject; 
            HealthBar = UI3D.transform.Find("HPBar").Find("ScalableCube").gameObject;
             
            if (LogsOn)
            {
                StringBuilder strB = new StringBuilder("");
                strB.AppendFormat("UI3D: {0}, 3DViewItem: {1}, Info: {2}, Price: {3}, GrowingStatusBar: {4}, HealthBar {5}", UI3D.name, Item3DContainer.name, title.text, Price.text, GrowingStatusBar.gameObject, HealthBar.gameObject);
                CHIC.Utilities.Info(strB.ToString(), this.GetType().ToString(), funcName: "Awake");
            }
        }



        //(Por si estamos usando un prefab) comprobamos si el terreno de plantación tiene algo creciendo dentro
        if (fieldStatusManager.IsGrowingPlant == false) ShowBareFarmfieldData(fieldStatusManager.FarmlandData);



        //Escucha de eventos de ciclo de vida del terreno de plantación.
        fieldStatusManager.OnFarmlandEmpty += ShowBareFarmfieldData;
        fieldStatusManager.OnPlantHealthChanged += UpdateHealthUI;
        fieldStatusManager.OnGrowingStatusChanged += UpdateGrowingStatus;
        fieldStatusManager.OnPlantStatusChanged += ShowPlantStatus;
        fieldStatusManager.OnFarmStart += ShowPlantStatus;

        //Elementos de interacción con UI Interacción 
        Button.GetComponent<Interactable>().OnClick.AddListener(fieldStatusManager.Sell);

        GrowingData.gameObject.SetActive(false);
    }

    /* Cambia entre modo display Farmfield info y Plant info ; true = plant Info mode*/
    public void ChangeShowMode(bool mode = false)
    {
        Item3DContainer.SetActive(mode);
        //GrowingData.gameObject.SetActive(mode);
        GrowingStatusBar.SetActive(mode);
        GrowingStatusBar.transform.parent.gameObject.SetActive(mode);
        HealthBar.SetActive(mode);
        HealthBar.transform.parent.gameObject.SetActive(mode);
        smallTextInfo.gameObject.SetActive(!mode);
        title.gameObject.SetActive(mode);
        
    }

    public void ShowPlantStatus(Plant p)
    {

        ChangeShowMode(true);
        //Si no aparece un objeto 3D que representa la raíz, lo instanciamos desde el prefab
        if(Item3DContainer.transform.childCount <= 0)
        {
            GameObject root3d = Instantiate(p.RootPrefab, Item3DContainer.transform) as GameObject;
            (root3d.AddComponent(typeof(RotateRoot)) as RotateRoot).speed = 0.5f;
        }
        
        
        title.text = p.SpeciesName;
        subtitle.text = p.Resistance.ToString();
        //GrowingData.text = p.GrowthStatus.ToString();
        Price.text = "Price " + Mathf.RoundToInt(p.SellingPrice).ToString() + " $";

    }

    public void ShowBareFarmfieldData(Farmland farmlandData)
    {
       
        ChangeShowMode(false); //Ocultamos los elementos relacionados con un terreno en el que está creciendo una planta y dejamos solo los del terreno yermo.

        for (int ch = 0; ch < Item3DContainer.transform.childCount; ch++)
            GameObject.Destroy(Item3DContainer.transform.GetChild(ch).gameObject);

        smallTextInfo.text = "Drop seeds here for them to grow";
        Price.text = "Price =" + farmlandData.sellingPrice +" $";
    }



    //Actualización de visor de atributos (e.g. vida, estado de crecimiento)
    public void UpdateHealthUI(float healthPercentage)
    {
        healthlevel = healthPercentage * _MaxScaleBars;
        HealthBar.transform.localScale = new Vector3(1, healthlevel, 1);
    }
    public void UpdateGrowingStatus(float growthPercentage)
    {
        growingStatusLevel = growthPercentage * _MaxScaleBars;
        GrowingStatusBar.transform.localScale = new Vector3(1, growingStatusLevel, 1);
    }

    
#if (UNITY_EDITOR)
    private void OnValidate()
    {
        GrowingStatusBar.transform.Find("ScalableCube").transform.localScale = new Vector3(1, growingStatusLevel, 1);
        HealthBar.transform.Find("ScalableCube").transform.localScale = new Vector3(1, healthlevel, 1);
    }
    #endif


}
