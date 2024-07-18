using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using System;

/* Objeto acoplado a la representación 3D de un terreno de plantación y que maneja el ciclo de vida de la planta y del propio terreno */
public class FarmlandManager : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property
    #region Fields + Properties
    [Header("Objetos de datos (Planta creciendo y Terreno)")]
    /* Define si hay una planta creciendo en este campo de cultivo o no*/
    private bool _IsGrowingPlant = false;
    public bool IsGrowingPlant { get => _IsGrowingPlant; }
    /* Datos asociados al conjunto de plantas que está creciendo en este terreno (e.g. vida, valor nutritivo) */
    public Plant growingPlantsSpeciesData = null;
    
    /* Datos que reflejan la naturaleza del terreno como tal (tamaño, plantas que puede contener, representaciones varias, etc) */
    private Farmland farmlandData;
    public Farmland FarmlandData
    {
        get => farmlandData;
    }
    

    /* Diccionario de plantas instanciadas como hijas del terreno de plantación (Posición, Objeto) */
    Dictionary<Vector3, GameObject> instantiatedPlantGOs = new Dictionary<Vector3, GameObject>();
    //Devuelve todas las plantas que hay en el campo creciendo
    public List<GameObject> getAllGrowingPlants()
    {
        return  new List<GameObject>(instantiatedPlantGOs.Values);
    }
    //Devuelve una planta al azar de las que están creciendo en el terreno
    public GameObject GetPlantObject { get {

            if (IsGrowingPlant == false || instantiatedPlantGOs == null ) return null;
            int pIndex = UnityEngine.Random.Range(0, instantiatedPlantGOs.Values.Count);
            return (new List<GameObject> (instantiatedPlantGOs.Values))[pIndex];
        }
    }


    [Header("COLOCACIÓN DE PLANTAS [REVISIT]")]
    [Range(-2f,1f)]
    public float onTopOffset = -0.12f;
    
    [Range(1f, 10f)]
    public float ScaleFlowers = 1f;

    [Header("Other")]
    /* Trigger detector de semillas */
    public BoxCollider seedColliderDetector;
    #endregion Fields + Properties

    #region Eventos/Acciones/Delegados

    public Action<Plant> OnFarmStart; //Al plantar un elemento
    public Action OnFarmEnd; //Al completarse el crecimiento de la planta
    public Action<float> OnPlantHealthChanged;  // Cambios en vida
    public Action<float> OnGrowingStatusChanged; //Cambios en estado de crecimiento
    public Action<Plant> OnPlantStatusChanged; // Cambios en el estado de la planta
    public Action<Farmland> OnFarmlandEmpty;

    #endregion


    public string ItemStatusUIPrefabPath = "Prefabs/UI/3DFarmlandStatusUI";

    public GameObject ItemStatusUI;

    

    /* Inicializa el script con el tipo de campo asociado; al ser este soltado sobre una superficie + crea un trigger que detecte semillas 
     *      Permite la creación dinámica de terrenos a partir de objetos del inventario 
     *  return false: si no se logra inicializar el terreno de plantación*/
    public bool InitFarmlandData(Farmland farmlandData)
    {
        this.farmlandData = farmlandData as Farmland;

        // Por defecto no hay planta creciendo:
        _IsGrowingPlant = false;
        growingPlantsSpeciesData = null;

        /* Comprobamos que existe un SEED DETECTION TRIGGER  */
        /* utilizado para detectar si se lanzan semillas sobre este terreno */
        if (seedColliderDetector == null) {
            
            //A. Check for colliders somewhere else in the inner hierarchy. De cara a modelos más complejos (v2)
            if (transform.GetComponentsInChildren<BroadcastChildCollider>() is BroadcastChildCollider[] childBColliders)
            {
                foreach (var bCollider in childBColliders)
                {
                    Debug.Log("Found broadcast collider");
                    if (bCollider.name == "SeedDetectionCollider")
                    {
                        Debug.Log("SEED DETECTION COLLIDER ACTIVATED");
                        bCollider.TriggerEnter.AddListener(OnSeedDetected);
                        seedColliderDetector = bCollider.broadcastCollider as BoxCollider;
                        if (seedColliderDetector.isTrigger == false)
                        {
                            seedColliderDetector.isTrigger = true;
                            if (LogsOn) Debug.LogWarning("Farmland seed detector was misconfigured: setting isTrigger = true");
                        }
                    }
                }
            }
            //B. Check for BoxCollider in the object itself. Funcionará para Models v1
            else if (transform.GetComponent<BoxCollider>() is BoxCollider collider)
            {
                seedColliderDetector = collider;
                if (seedColliderDetector.isTrigger == false)
                {
                    seedColliderDetector.isTrigger = true;
                    if (LogsOn) Debug.LogWarning("Farmland seed detector was misconfigured: setting isTrigger=true");
                }
            }
            //C. Can't find any collider
            else
            {
                if (LogsOn) Debug.LogError("Farmland couldn't find a Box Collider to use as seed Detector.");
            }    
        }

        gameObject.transform.localScale *= farmlandData.fieldDimensions.x;
        
        /* AÑADO UI DE PLANTACIÓN */
        GameObject UI3D = Resources.Load(ItemStatusUIPrefabPath) as GameObject ;



        //Colocación de la interfaz de usuario
        UI3D = Instantiate(UI3D,gameObject.transform.position , Quaternion.identity, gameObject.transform);
        // UI3D.transform.localScale *= ;
        ItemStatusUI = UI3D;
        //Por defecto oculto la UI
        UI3D.SetActive(false);

        Interactable getInfoComponent = gameObject.AddComponent<Interactable>();
        getInfoComponent.RequiresFocus = true;
        getInfoComponent.VoiceCommand = "Info";
        getInfoComponent.OnClick.AddListener(ToggleUI);




        OnFarmlandEmpty?.Invoke(farmlandData);
        if (this.farmlandData == null) return false;


        PlayerSpeechManager.onSpeechRecognition += OnSpeechFromUser;
        return true;
    }

    private void OnSeedDetected(Collider other)
    {
        OnTriggerEnter(other);
    }
    /* Maneja el proceso de plantación a través de la colisión de una semilla con el terreno de plantación (SIN USAR SISTEMA DE EQUIPO) */
    private void OnTriggerEnter(Collider other)
    {

        if (!IsGrowingPlant)
        { 
            /* Si no hay ningún conjunto de plantas creciendo en este terreno de plantación 
             * y en algún momento se detecta el choque con una semilla (objeto con tag "Planting seed") 
             * ==> Creamos plantas 3D, Lanzamos Co-rutina de crecimiento y eliminamos semillas utilizadas del inventario */
            if (other.gameObject.CompareTag("Planting Seed"))
            {
                if (LogsOn)
                    CHIC.Utilities.Info("Seed entered " + gameObject.name + "'s field: Setting up growing...", this.GetType().ToString(), funcName: "OnTriggerEnter");
                
                //Obtengo los datos de la semilla y los cargo en el campo; estos están incluidos en el InventoryItemManager
                InventoryItemManager inventoryItemHandler = other.gameObject.GetComponent<InventoryItemManager>();

                if (inventoryItemHandler == null)
                {
                    CHIC.Utilities.Error("Information regarding the plant to grow couldn't be loaded (E1) ", this.GetType().ToString(), funcName: "OnTriggerEnter");
                    return;
                }
                
                growingPlantsSpeciesData = inventoryItemHandler.ItemData as Plant;


                Debug.Log(growingPlantsSpeciesData.ToString());
                if (growingPlantsSpeciesData == null)
                {
                    CHIC.Utilities.Error("Information regarding the plant to grow couldn't be loaded (E2) ", this.GetType().ToString(), funcName: "OnTriggerEnter");
                    return;
                }

                //Actualizamos FLAG que marca que existe una planta creciendo en este terreno 
                _IsGrowingPlant = true;

                /* Mostramos que la planta se ha materializado a través de objetos 3D de plantas, tantas como quepan en el terreno */
                for (int i = 0; i < farmlandData.fittingPlants; i++)
                {
                    if (LogsOn)
                        CHIC.Utilities.Info("Creating PLANT in Farmfield" + farmlandData.ID.ToString(),this.GetType().ToString());
                    /* Añadimos la representación de la planta i a la lista de objetos que representan plantas */
                    //[PROBLEMA AQUÍ]
                    GameObject newPlant3D = SpawnIfNotTooCloseOrAlreadyOccupiedPosition(growingPlantsSpeciesData.PlantPrefab, transform, farmlandData.fieldDimensions);
                    /* Definimos la escala de la planta en cero para hacerla crecer alterando la escala */
                    newPlant3D.transform.localScale = Vector3.zero; 
                    instantiatedPlantGOs.Add(newPlant3D.transform.position, newPlant3D);
                }
                
                
                
                StartCoroutine(GrowPlant(15, ScaleFlowers/*3Hz*/));

                /* Una vez se ha plantado el conjunto de plantas destruimos el objeto que representa la semilla y des-equipamos del inventario*/
                inventoryItemHandler.SetThisItemAsUsed();
            }

            if (LogsOn)
                CHIC.Utilities.Info(" Not a seed collided with " + gameObject.name + " : "+other.gameObject.name, this.GetType().ToString(), funcName: "OnTriggerEnter");

        }
    }



    public void DamagePlant(float damage)
    {
        if (growingPlantsSpeciesData != null) //TZ  
        {
            if (growingPlantsSpeciesData.Hurt(damage))
            {
                //Lógica de destrucción de planta
                foreach (var plant in instantiatedPlantGOs.Values) {
                    Destroy(plant);
                }
                instantiatedPlantGOs.Clear();
                DisposePlant();
                StopAllCoroutines();
                OnFarmlandEmpty?.Invoke(farmlandData);
            }
            if (growingPlantsSpeciesData != null)  //TZ
            {
                OnPlantHealthChanged?.Invoke(growingPlantsSpeciesData.HealthPercentage);
            }
        } 
    }
   
   
    public void ToggleUI()
    {
        ItemStatusUI.SetActive(!ItemStatusUI.activeInHierarchy);
    }



    public void OnSpeechFromUser( string str)
    {
        /*
        if(GameManager.Instance.ThePlayer.ObjectOnFocus == gameObject)
        switch (str)
        {
            case "Field Info":
                ToggleUI();
                break;
            case "Sell This": Sell();
                break;
        }
        */

    }


  

    /* Actualiza el tamaño de una planta hasta un nivel máximo, modificando la escala con una frecuencia de refresco dada */
    IEnumerator GrowPlant(uint scaleFreq, float maxScaleFlowers=2f)
    {

        OnFarmStart?.Invoke(growingPlantsSpeciesData);
        OnPlantStatusChanged?.Invoke(growingPlantsSpeciesData);
        OnGrowingStatusChanged?.Invoke(growingPlantsSpeciesData.GrowthPercentage);
        

        /* Tiempo total de crecimiento (s) = días del juego requeridos * equivalencia en segundos */
        float growthTotalTime = growingPlantsSpeciesData.GrowingTimeRequired * GameManager.Instance.secondsToGameTimeUnit;

        //Timer para cálculo de tiempo pasado desde la última actualización de la escala de las plantas.
        float lastScaleUpdateTime = Time.realtimeSinceStartup;
        /* Crecimiento a dotar desde la última vez */
        float deltaGrowth = 0f;
        float localScale = 0f;

        float repeatingPeriod = 1 / scaleFreq;
        //StartCoroutine(DebugGrowingPlant());

        while (growingPlantsSpeciesData.UpdateGrowth(deltaGrowth))
            {
                /* crecimiento -> t elapsed / growthTotalTime */
                deltaGrowth = (Time.realtimeSinceStartup - lastScaleUpdateTime) / growthTotalTime;
                /* escalado en el intervalo [0, maxScaleFlowers] */
                localScale = growingPlantsSpeciesData.GrowthPercentage * maxScaleFlowers;

                // ESCALADO DE FLORES
                foreach (GameObject plant3D in instantiatedPlantGOs.Values)
                {
                    plant3D.transform.localScale = localScale * Vector3.one;
                }

                lastScaleUpdateTime = Time.realtimeSinceStartup;

                OnPlantStatusChanged?.Invoke(growingPlantsSpeciesData);
                OnGrowingStatusChanged?.Invoke(growingPlantsSpeciesData.GrowthPercentage);
                yield return new WaitForSecondsRealtime(repeatingPeriod);

            }

        OnFarmEnd?.Invoke();

        StopCoroutine(DebugGrowingPlant());
        
        
    }

    #region Plant Spawning
    /* Obtiene la posición e instancia el objeto 3D de la planta */
    public GameObject SpawnWithinPlaneBoundaries(GameObject prefabToInstantiate, Transform plane, Vector2 dimensions)
    {
        // Instancio el prefab como hijo del plano
        GameObject newObject = Instantiate(prefabToInstantiate, plane.transform.position, Quaternion.identity, plane);
        // Calculamos posición aleatoria
        float rand_x = UnityEngine.Random.Range(-(dimensions.x / 2), dimensions.x / 2);  
        float rand_z = UnityEngine.Random.Range(-(dimensions.y / 2), dimensions.y / 2);
        //Lo movemos para colocarlo en una posición aleatoria dentro de los boundaries del plano de X,Z (algo que será paralelo al suelo)
        newObject.transform.localPosition = new Vector3(rand_x, 0, rand_z);
        //newObject.transform.localPosition = new Vector3(rand_x, rand_z, onTopOffset);

        return newObject;
        
    }
    /* Evita que las plantas aparezcan demasiado cerca entre sí dentro del un terreno y hace el proceso aleatorio, para que aparezcan las plantas en distintas posiciones del campo */
    public GameObject SpawnIfNotTooCloseOrAlreadyOccupiedPosition(GameObject prefabToInstantiate, Transform plane, Vector2 dimensions)
    {
        //Pruebo y creo un objeto en el plano
        GameObject tryingObj = null;
        bool isFarEnough = false;
        float maxDistanceB2Plants = 0.02f;

        while (tryingObj==null)
        {
            tryingObj = SpawnWithinPlaneBoundaries(prefabToInstantiate, plane, dimensions);
            Debug.Log("Spawning a plant, at least trying");
            //Compruebo que no se haya colocado sobre otro existente 
            if (!instantiatedPlantGOs.ContainsKey(tryingObj.transform.position))
            {
                foreach (var key in instantiatedPlantGOs.Keys) //Vemos si la planta instanciada está a una distancia suficiente de otras 
                {
                    if (Vector3.Magnitude(key - tryingObj.transform.position) >= maxDistanceB2Plants )
                    {
                        isFarEnough = true;
                    }
                    else
                    {
                        isFarEnough = false;
                        tryingObj = null;
                        break;
                    }

                }
            }
            else {  tryingObj = null; } 
        }

        return tryingObj;
    }
    #endregion Plant Spawning
   
    /* Encargado de ejecutar una venta; bien del terreno o bien de la plantación */
    public void Sell()
    {
        /* EQUIPMENT
         * if (ItemInventory.Instance.heldItem != null){
            Debug.LogException(new System.NotImplementedException("Equip Item not implemented") );
            return;
        }   */

        if (growingPlantsSpeciesData != null && _IsGrowingPlant == true) // Hay una planta creciendo en el terreno
        {
            //Vender planta
            SellPlant();
            StopAllCoroutines();
            OnFarmlandEmpty?.Invoke(farmlandData);

        }
        else
        {
            Debug.Log("SELLING TERRAIN");
            SellTerrain();
            //Vender terreno
        }




    }

    #region Acciones usuario sobre plantación
    public void SellPlant()
    {
        //Venta de planta dadas sus características
        GameManager.Instance.AddMoney(farmlandData.fittingPlants * growingPlantsSpeciesData.SellingPrice);
        //DESTRUIR TODAS LAS PLANTAS 3D
        foreach(GameObject plant3D in instantiatedPlantGOs.Values) {
            Destroy(plant3D);
        }
        instantiatedPlantGOs.Clear();
        UXNotifications.Instance.SoldItemNotification(growingPlantsSpeciesData);
        //DESASIGNAR LA ESPECIE DE PLANTA QUE ESTÁ CRECIENDO
        DisposePlant();
    }
    public void SellTerrain()
    {
        //INCREMENTO DINERO 
        GameManager.Instance.AddMoney(farmlandData.sellingPrice);
        //Elimino del conjunto de elementos en plantación
        RealWorldOverlay.Instance.OverlaidItems.Remove(gameObject);
        UXNotifications.Instance.SoldItemNotification(farmlandData);
        //DESTRUYO EL OBJETO DE TERRENO
        Destroy(gameObject);

    }
    #endregion Acciones usuario sobre plantación

    #region Funciones auxiliares + Debug
    IEnumerator DebugGrowingPlant()
    {
        while (true)
        {
            if (!_IsGrowingPlant) Debug.Log("NOTHING GROWING IN FARMLAND" + farmlandData.ID.ToString());
            else Debug.Log(growingPlantsSpeciesData.SpeciesName + " GROWING IN FARMLAND" + farmlandData.ID.ToString() + " GROWTH : " + growingPlantsSpeciesData.GrowthPercentage + " STATUS :" + growingPlantsSpeciesData.GrowthStatus.ToString());
            yield return new WaitForSeconds(4f);
        }
    }
    private void DisposePlant()
    {
        _IsGrowingPlant = false;
        growingPlantsSpeciesData = null;
    }


    #endregion

}
