using UnityEngine;
using CHIC;
using System.Collections;


/*Estados del elemento del inventario (de cara a la máquina de estados programada en la clase InventoryItemManager */
public enum InventoryItemState
{
    InInventoryPlaced,
    InInventoryDragged,
    OutsideInventoryDragged,
    OutsideInventoryReleased
}

/* Script that is attached to any object within the Inventory, so that it can be manipulated and controlled using gestures */
public class InventoryItemManager : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = true;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property

    [SerializeField]
    private Item _ItemData;/* Datos acerca del elemento lógico, el que está -de hecho- guardado en el Inventario (e.g. datos asociados a una planta X, a un terreno Y, etc) */
    public Item ItemData { 
        get { return _ItemData;  }
        set
        {
            ApplyDisplayConfig(value); //Aplica configuración de rotación, display, etc.

            gameObject.name = ItemData.Type.ToString() +" "+ ItemData.ID.ToString();
            child3D_in_Inventory.layer = inventoryItemMask;
            child3D_out_Inventory.layer = inventoryItemMask;
        }
    }

    [Header("Configuración: ajuste de tamaño y visualización de elementos")]
    public bool displayConfigPreview = false;
    public SO_InventoryItemsConfiguration inventoryItemsConfig;

    [Header("Configuración de preview de acción de usuario (Línea de trayectoria)")]
    public bool usePreview = true;
    public WorldFallPreview fallPreviewer;

    [Header("Capas identificadora de elementos usados sobre el mundo e interfaz:")]
    public int usedItemMask = 14;
    public int inventoryItemMask = 13;

    [Header("Bounding box (Box collider trigger) asociada al inventario o donde sea que está este Componente, al salirse de tal trigger se producen los cambios en la máquina de estados")]
    [SerializeField]
    BoxCollider boxBoundsInOut;
    [Header("Estado del elemento del inventario (Máquina de estados)")]
    
    [SerializeField]
    bool isInInventory = true;
    [SerializeField]
    bool held = false;
    [SerializeField]
    bool wasUsed = false;
    /* Refleja el estado de este item de inventario */
    [SerializeField]
    private InventoryItemState _inventoryItemState = InventoryItemState.InInventoryPlaced;
    [SerializeField]
    private InventoryItemState _inventoryItemPrevState;

    [Header("Objetos 3D que identifican al elemento (IN/OUT Inventory)")]
    /* Manejan representaciones del Item tanto dentro como fuera del inventario */
    public GameObject child3D_in_Inventory;
    public GameObject child3D_out_Inventory;

    [Header("Other")]
    public Rigidbody rb; /* Maneja la gravedad cuando el objeto sale del inventario*/
    public BoxCollider ItemCollider { get; private set; }
    public Vector3 p0; /* Permite recolocar el slot en donde se encontraba inicialmente */
    private Vector3 offsetDisplay;
    private InventoryUI _InventoryUIController;
    [SerializeField]
    private ItemInventory _UserInventory;

    private GameManager _GlobalStatus;
    

    private void OnEnable()
    {

        if(_UserInventory==null) _UserInventory = ItemInventory.Instance;
        if(_GlobalStatus==null) _GlobalStatus = GameManager.Instance;
        
        //Set Default 3D object as fallback representation in case there was an error loading actual pretty objects
        if (transform.childCount > 1) transform.Find("DefaultInventoryItem")?.gameObject.SetActive(false);
        //else transform.Find("DefaultInventoryItem").gameObject.SetActive(false);

        if (ItemCollider == null) ItemCollider = gameObject.GetComponent<BoxCollider>() as BoxCollider;
        _InventoryUIController = FindObjectOfType<InventoryUI>();


        if (boxBoundsInOut == null) boxBoundsInOut = FindObjectOfType<InventoryUI>().GetComponent<BoxCollider>();


        StartCoroutine(ConfigPreview());

        /* Preview Trajectory */
        if ( usePreview && fallPreviewer == null)
        {
            fallPreviewer = GetComponent<WorldFallPreview>();
            if (fallPreviewer == null) gameObject.AddComponent<WorldFallPreview>();
            fallPreviewer.enabled = false;
        }
    }

    private void ApplyDisplayConfig(Item value)
    {
        var displayConfig = inventoryItemsConfig.GetInventoryDisplayConfigByType(value.GetType());

        /* Manejar diferentes nombres para representación fuera y dentro del inventario dependiendo del tipo de Item */
        switch (value.Type)
        {
            case ItemType.Plant:
                Plant plantInfo = value as Plant;
                if (plantInfo == null) { Utilities.Error("Item info couldn't be loaded as a <i>PLANT</i>", this.GetType().ToString(), this.gameObject, funcName: "ApplyDisplayConfig"); return; }
                _ItemData = plantInfo;
                
                //Aplicamos configuración de visualización (IN)
                if (child3D_in_Inventory == null)
                {
                    child3D_in_Inventory = Instantiate(plantInfo.SackPrefab, gameObject.transform.position + displayConfig.offsetInInventory,
                        Quaternion.Euler(displayConfig.rotationInInventory), transform);
                    updateStateMachine();
                }
                child3D_in_Inventory.transform.localScale = displayConfig.scaleInInventory;
                child3D_in_Inventory.transform.localRotation = Quaternion.Euler(displayConfig.rotationInInventory);
                
                //Aplicamos configuración de visualización (OUT)
                if (child3D_out_Inventory == null)
                {
                    child3D_out_Inventory = Instantiate(plantInfo.SeedPrefab, gameObject.transform.position, Quaternion.Euler(displayConfig.rotationOutInventory), transform);
                    updateStateMachine();
                }
                child3D_out_Inventory.transform.localRotation = Quaternion.Euler(displayConfig.rotationOutInventory);
                child3D_out_Inventory.transform.localScale = displayConfig.scaleOutInventory ;

                gameObject.tag = "Planting Seed";
                break;

            case ItemType.Farmland:
                Farmland farmlandInfo = value as Farmland;
                if (farmlandInfo == null) { Utilities.Error("Item info couldn't be loaded as a <i>FARMLAND</i>",this.GetType().ToString(), this.gameObject, funcName: "ApplyDisplayConfig"); return; }
                _ItemData = farmlandInfo;

                if (child3D_in_Inventory == null)
                {
                    child3D_in_Inventory = Instantiate(farmlandInfo.fieldPrefab, gameObject.transform.position + displayConfig.offsetInInventory, Quaternion.Euler(displayConfig.rotationInInventory), transform);
                    updateStateMachine();
                }
                child3D_in_Inventory.transform.localRotation = Quaternion.Euler(displayConfig.rotationInInventory);
                child3D_in_Inventory.transform.localScale = displayConfig.scaleInInventory;

                if (child3D_out_Inventory == null)
                {
                    child3D_out_Inventory = Instantiate(farmlandInfo.fieldPrefab, gameObject.transform.position, Quaternion.Euler(displayConfig.rotationOutInventory), transform);
                    updateStateMachine();
                }
                

                child3D_out_Inventory.transform.localRotation = Quaternion.Euler(displayConfig.rotationOutInventory);
                child3D_out_Inventory.transform.localScale = displayConfig.scaleOutInventory;
                
                break;
                //case ItemType.Tool:
        }
    }

    private IEnumerator ConfigPreview()
    {
        while(true)
        {
            yield return new WaitUntil(() => displayConfigPreview == true);
            if (displayConfigPreview) ApplyDisplayConfig(_ItemData);
            yield return new WaitForSeconds(1f);
        }
    }
    

    public void setDragged()
    {
         held = true;
        //Para que deje de seguir al usuario el menú cuando se coge algo; evitando frustración.
         if (_InventoryUIController.isFollowingUser) _InventoryUIController.SetFollowUser();
         updateStateMachine();
    }

    public void setReleased()
    {
        held = false;
        if (usePreview || fallPreviewer != null) fallPreviewer.enabled = false;
        updateStateMachine();
    }




    

    #region State Machine 
    private void updateStateMachine()
    {

        if (!held && isInInventory) //Suelto y en el inventario
        {
            //Update Status ==> Objeto colocado en inventario
            _inventoryItemState = InventoryItemState.InInventoryPlaced;
            if (usePreview) fallPreviewer.enabled = false;
            // Si objeto cogido pero no sacado del inventario ==> reposicionar en inventario
            if (_inventoryItemPrevState == InventoryItemState.InInventoryDragged || _inventoryItemPrevState == InventoryItemState.OutsideInventoryReleased)
                RepositionInInventory();
            //Guardar posición en el inventario
            p0 = transform.localPosition;

        }
        else if (held && isInInventory) //Agarrado y dentro del inventario
        {
            //Update Status ==> Objeto siendo cogido en el inventario
            _inventoryItemState = InventoryItemState.InInventoryDragged;
            //Permitir movimiento (solamente necesario si viene de estar fijo)
            if (_inventoryItemPrevState == InventoryItemState.InInventoryPlaced)
            {
                rb.freezeRotation = false;
                rb.constraints = RigidbodyConstraints.None;
            }

        }
        else if (held && !isInInventory) //Fuera y agarrado
        {
           //Update Status ==> Objeto fuera del inventario y siendo agarrado
            _inventoryItemState = InventoryItemState.OutsideInventoryDragged;
            //Si se ha soltado fuera de los boundaries del  inventario => activamos gravedad y activamos el prefab externo
            rb.useGravity = true;
            child3D_out_Inventory.SetActive(true);
            child3D_in_Inventory.SetActive(false);

            //Asignar capa de ítems utilizados al ser sacados del inventario a nueva capa para posterior identificación (de cara a colisiones, triggers, etc.)

           
            CHIC.Utilities.SetLayerRecursively(gameObject.transform, usedItemMask);

            if (usePreview) fallPreviewer.enabled = true;
        }
        else if (!held && !isInInventory) //Fuera y suelto
        {
            //Update Status ==> Objeto fuera del inventario una vez ha sido soltado por el usuario
            _inventoryItemState = InventoryItemState.OutsideInventoryReleased;
            //Si ha pasado X tiempo y no choca con nada, lo devolvemos al inventario!!
            StartCoroutine(DriftCheck());
        }

        if(LogsOn) Utilities.Info("Item " + ItemData.ID.ToString() + " status "+ _inventoryItemState.ToString(), this.GetType().ToString(), this.gameObject, funcName: "UpdateStateMachine");

        _inventoryItemPrevState = _inventoryItemState; /* Guardamos el estado del la State Machine del que provenimos (e.g. soltamos el elemento y antes estaba fuera del inventario)*/
    }
    #endregion State Machine


    /* Manejo de retorno de elementos al inventario si no interaccionan con nada 2s después de haber caído */
    private IEnumerator DriftCheck(float maxFallTime = 2f)
    {
        if (_inventoryItemState == InventoryItemState.OutsideInventoryReleased)
        {
            yield return new WaitForSecondsRealtime(maxFallTime);
            _inventoryItemPrevState = InventoryItemState.OutsideInventoryReleased;
            if (_inventoryItemState == InventoryItemState.OutsideInventoryReleased && !wasUsed) { RepositionInInventory(); yield return null; }
        }
    }
    
    #region OnTriggerEnter & Exit Control de entrada y salida del collider trigger externo (e.g. región del inventario)
    private void OnTriggerExit(Collider other)
    {
        //Si el collider trigger del que sale es el del inventario o el collider trigger que se haya asignado a BoxBoundsInOut
        if(other == boxBoundsInOut)
        {
            isInInventory = false; //Sale del inventario
            child3D_in_Inventory.SetActive(false); //Se desactiva el prefab de elemento IN-Inventory
            child3D_out_Inventory.SetActive(true); //Se activa el prefab de uso en el mundo
            updateStateMachine();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other  == boxBoundsInOut)
        {
            child3D_out_Inventory.SetActive(false);
            child3D_in_Inventory.SetActive(true);
            isInInventory = true;
            updateStateMachine();
        }

    }

    #endregion


    
    /*FUTURE: A PARTIR DE AQUí SE LE PUEDE DAR BASTANTES VUELTAS AL CÓDIGO; SE JUNTAN RESPONABILIDADES DEMASIADO A MENUDO */

    /* Resetea la posición del elemento dentro del inventario, situándolo fijo dentro de este */
    private void RepositionInInventory()
    {
        //Si parte de fuera del inventario
        if(_inventoryItemPrevState == InventoryItemState.OutsideInventoryDragged || _inventoryItemPrevState == InventoryItemState.OutsideInventoryReleased)
        {
            // Reasignamos la capa del objeto actual para que vuelva a ser interfaz de usuario y no colisione por accidente con objetos
            child3D_in_Inventory.layer = inventoryItemMask;
            child3D_out_Inventory.layer = inventoryItemMask;
            gameObject.layer = inventoryItemMask;
        }
        isInInventory = true;
        //Desactivamos representación fuera del inventario
        child3D_out_Inventory.SetActive(false);
        //Activamos objeto representación en el Inventario
        child3D_in_Inventory.SetActive(true);
        //Reset position and add offset iff applicable
        var config = inventoryItemsConfig.GetInventoryDisplayConfigByType(ItemData.GetType());
        transform.localPosition = p0;
        // Reactivamos las restricciones de posición y rotación originales y desactivamos la gravedad, haciendo que el objeto pueda flotar
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        transform.rotation = transform.parent.rotation;
        
    }

    /* Control de interacción entre objetos sacados del inventario y el resto del mundo a través de la colisión */
    private void OnCollisionEnter(Collision collision)
    {
        /* Manejamos colisiones con el mundo real */
        if(!isInInventory && !held) { 

            if(ItemData is Farmland && collision.gameObject.layer == LayerMask.NameToLayer("Spatial Awareness")) //CONTROL DE COLOCACIÓN DE ZONAS DE PLANTACIÓN
            {
                float angleDegPlantThreshold = 60;
                Vector3 realWorldCollisionPoint = collision.GetContact(0).point;
                Vector3 collisionDirVector = collision.GetContact(0).normal;
                Vector3 worldUp = Vector3.up;

                if (LogsOn)  Debug.Log("Angle of field to be created: " + Vector3.Angle(worldUp, collisionDirVector));
                
                //Si ángulo es demasiado pronunciado retornamos el terreno al inventario
                if(Vector3.Angle(collisionDirVector, worldUp) > angleDegPlantThreshold) {
                    //Utilities.Info("InventoryItemManager.cs", Color.black, gameObject, "El ángulo entre el campo de plantación y la superficie debe ser <"+ angleDegPlantThreshold+ "º; º="+ Vector3.Angle(collisionDirVector, worldUp));
                    _inventoryItemPrevState = InventoryItemState.OutsideInventoryReleased;
                    RepositionInInventory();
                    return;
                }

                bool itemUseSuccess = RealWorldOverlay.Instance.TrySpawnItem(realWorldCollisionPoint, Quaternion.Euler(inventoryItemsConfig.GetInventoryDisplayConfigByType(typeof(Farmland)).rotationOutInventory), ItemData);
                if (!itemUseSuccess)
                {
                    Utilities.Warn(" Couldn't Spawn Item over real world", this.GetType().ToString(), funcName: "OnCollisionEnter");
                    return;
                }
                //Si no hay error situando terreno sobre mundo: Eliminamos objeto del inventario
                SetThisItemAsUsed();
            }
            else
            {
               if(LogsOn) Debug.Log( gameObject.name +"  Has collided with  " + collision.gameObject.name+ " but there is no behaviour associated");
            }
        }

    }
    
    //bool TrySpawnFarmland3DAt( Vector3 point, Vector3 normal, GameObject prefab, float scale=0.3f )
    //{
    //    if (Time.realtimeSinceStartup - lastTSpawned > 1f / maxFreqSpawning) return false;

    //    lastTSpawned = Time.realtimeSinceStartup;

    //    Quaternion rotationOfPrefabFromConfig = Quaternion.Euler(inventoryItemsConfig.GetInventoryDisplayConfigByType(typeof(Farmland)).rotationOutInventory);
    //    //GameObject nuGO = Instantiate(prefab , point, Quaternion.LookRotation(normal) /* Quaternion.identity */, _GlobalStatus.GameWorld.transform) as GameObject;
    //    GameObject nuGO = Instantiate(prefab, point, rotationOfPrefabFromConfig, GameManager.GameWorld.transform) as GameObject;

    //    if (nuGO == null) return false;
    //    nuGO.transform.localScale = scale * Vector3.one ;
    //    SetLayerRecursively(nuGO.transform, usedItemMask);
    //    //nuGO.layer = usedItemMask; // Marcado como objeto utilizado en el mundo

    //    //Creación de manejador del ciclo de vida e interacción de una plantación
    //    FarmlandManager fm = nuGO.GetComponent<FarmlandManager>();
    //    if(fm == null)
    //        fm = nuGO.AddComponent<FarmlandManager>();
    //    if (!fm.InitFarmlandData((ItemData as Farmland))) Debug.Log("Error initialising Farmland data (Inventory Item Manager)");
    //    //Añadido de plantación a objetos Colocados sobre el mundo real
    //    RealWorldOverlay.Instance.addItemToWorldSpace(nuGO);
    //    return true;

    //}

    /* Elimina un elemento del Inventario, así como su representación */
    public void SetThisItemAsUsed()
    {
        gameObject.SetActive(false);
        wasUsed = true;
        _UserInventory.RemoveItem(ItemData); //[RECUERDA] : Notificaciones manejadas desde el propio Inventario
        Destroy(gameObject,2f);
    }



    /* REVISIT -- MOVER DE AQUÍ PARA AISLAR EL SCRIPT Y HACERLO REUTILIZABLE, no dependiente completamente de que exista un inventario */
    IEnumerator resetInventoryFollow()
    {
        yield return new WaitForSeconds(2f);

        if (!_InventoryUIController.isFollowingUser) _InventoryUIController.SetFollowUser();
    }





}
