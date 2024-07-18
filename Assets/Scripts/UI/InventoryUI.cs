using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using CHIC;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class InventoryUI : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property
    
    public GameObject itemPanelPrefab; /* Prefab consistente en un objeto empty que contiene objetos 3D (por defecto un cubo) que representan al objeto físicamente en el inventario;
                                       /*   se utiliza para poder generar las representaciones individuales de cada objeto en el inventario */
    [SerializeField]
    private ItemInventory _UserInventory;

    [SerializeField]
    private GameObject grid3DPanel;
    [SerializeField]
    private GridObjectCollection objCollection; /* Objeto del MRTK que se encarga de alinear los objetos en un plano por nosotros, sobre el cual desplegamos las representaciones de 
                                                /* nuestros objetos de inventario*/

    [Header("Scaling, Following user and so on...")]
    public bool isFollowingUser = true;


    [SerializeField]
    Sprite fixInventoryBtnSprite;

    [SerializeField]
    Sprite followMeBtnSprite;

    [SerializeField]
    Image secondaryBtnBackground;


    public ManipulationHandler UIInventoryManipulationHandler;
    public BoundingBox UIInventoryBoundingBox;
    public SolverHandler UIInventorySolverHandler;
    public RadialView UIInventoryRadialView;
    
    [Header("Inventory UI configuration")]
    [Range(1,4)]
    public uint NumColumns;
    [Range(1, 4)]
    public uint NumRows;
    [Header("Inventory Cell Configuration")]
    [SerializeField][Range(0f,3.5f)]
    private float _CellHeight;
    public float CellHeight { get => _CellHeight ;  }
    [SerializeField]
    [Range(0f, 3.5f)]
    private float _CellWidth;
    public float CellWidth { get => _CellWidth; }


    bool needsRefresh = true;


    private void OnValidate()
    {
        if (objCollection == null) objCollection = transform.Find("Inventory UI (Canvas)/Grid3DPanel").GetComponent<GridObjectCollection>() as GridObjectCollection;
        if (grid3DPanel == null) grid3DPanel = transform.Find("Inventory UI (Canvas)/Grid3DPanel").gameObject;

        /* Obtengo la altura y anchura del recuadro del Inventario */
        //Vector3[] corners = new Vector3[4];
        //grid3DPanel.GetComponent<RectTransform>().GetWorldCorners(corners);

        objCollection.Rows = (int) NumRows;
        objCollection.CellWidth = _CellWidth;
        objCollection.CellHeight = _CellHeight;

        objCollection.UpdateCollection();
       
    }
    

    /* DEBUGGING PURPOSES */
    private void OnDrawGizmos()
    {
        
        uint numCells = NumRows * NumColumns;
        
        Vector3[] corners = new Vector3[4];
        grid3DPanel.GetComponent<RectTransform>().GetWorldCorners(corners);


        float scaleCornerGizmos = 0.03f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(corners[0], scaleCornerGizmos* Vector3.one);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(corners[1], scaleCornerGizmos * Vector3.one);
        Gizmos.color = Color.black;
        Gizmos.DrawCube(corners[2], scaleCornerGizmos * Vector3.one);
        Gizmos.color = Color.green;
        Gizmos.DrawCube(corners[3], scaleCornerGizmos * Vector3.one);

        Vector3 placeCellAt = corners[2];




        /* DRAWING SPAWNED CELLS */
        /* int gizmoSpawnedCells = 0;
        for (int r = 0; r < NumRows && gizmoSpawnedCells < numCells ; r++)
            {
            for (int c = 0; c < NumColumns; c++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(placeCellAt,0.001f); // Esfera en esquina superior izquierda de cada celda

                //Dibujo líneas de celda
                Gizmos.DrawLine(placeCellAt, placeCellAt + new Vector3(CellWidth, 0f, 0f)); //TOP HORIZONTAL
                Gizmos.DrawLine(placeCellAt, placeCellAt + new Vector3(0f,-CellHeight,0f)); //LEFT 
                Gizmos.DrawLine(placeCellAt + new Vector3(CellWidth, 0f, 0f), placeCellAt + new Vector3(CellWidth, -CellHeight, 0f));//RIGHT
                Gizmos.DrawLine(placeCellAt + new Vector3(0f, -CellHeight, 0f), placeCellAt + new Vector3(CellWidth, -CellHeight, 0f));//BOTTOM
                


                //Draw Actual Cell
                //Gizmos.DrawWireCube(PlaceAtCenter, new Vector3(-0.5f * CellWidth, -0.5f * CellHeight, 0.01f));

                placeCellAt += new Vector3(CellWidth, 0, 0f);
        
                gizmoSpawnedCells++;
            }
            placeCellAt = corners[2] + new Vector3(0, (r+1) * (-CellHeight), 0);


        }
        */




    }



    #region Interaction Implementation (Cerrar, Seguir al usuario, etc.)
    public void ExecButtonFollowMe()
    {
        SetFollowUser();
    }
    public void OpenInventory()
    {
        transform.GetChild(0).gameObject.SetActive(true); //Lo hacemos visible
        if (!isFollowingUser) SetManipulation(false, true); // Activamos la manipulación si el usuario anteriormente la tenía activada.
        StartCoroutine(RefreshInventoryTrick());
    }
    public void CloseInventory()
    {
        transform.GetChild(0).gameObject.SetActive(false);  //Lo hacemosinvisible
        SetManipulation(false, false); //Desactivamos el manipulador para evitar que se vea la Bounding box
    }
    public void SetFollowUser(bool toggle=true, bool value=true)
    {
        if (toggle)
        {
            UIInventoryRadialView.enabled = !UIInventoryRadialView.enabled;
            UIInventorySolverHandler.enabled = !UIInventorySolverHandler.enabled;
        }
        else
        {
            UIInventoryRadialView.enabled = value;
            UIInventorySolverHandler.enabled = value;

        }
        if (UIInventoryRadialView.enabled && UIInventorySolverHandler.enabled) { isFollowingUser = true; SetManipulation(false, false); secondaryBtnBackground.sprite = fixInventoryBtnSprite; }
        else { isFollowingUser = false; SetManipulation(false,true); secondaryBtnBackground.sprite = followMeBtnSprite; }

    }
    public void SetManipulation(bool toggle = true, bool value = true)
    {
        if (toggle)
        {
            UIInventoryBoundingBox.enabled = !UIInventoryBoundingBox.enabled;
            UIInventoryManipulationHandler.enabled = !UIInventoryManipulationHandler.enabled;
        }
        else
        {
            UIInventoryBoundingBox.enabled = value;
            UIInventoryManipulationHandler.enabled = value;
        }
    }
    #endregion  

    private void Start()
    {
        if(itemPanelPrefab == null) itemPanelPrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryItem") ; //[REVISIT]

        while(_UserInventory == null) _UserInventory = ItemInventory.Instance; /* Inicializamos la referencia al inventario global del usuario */
        if (objCollection == null) objCollection = gameObject.GetComponentInChildren<GridObjectCollection>() as GridObjectCollection;

        if (objCollection == null) objCollection = transform.Find("Inventory UI (Canvas)/Grid3DPanel").GetComponent<GridObjectCollection>() as GridObjectCollection;
        if (grid3DPanel == null) grid3DPanel = transform.Find("Inventory UI (Canvas)/Grid3DPanel").gameObject;

        objCollection.Rows = (int)NumRows;
        objCollection.CellWidth = _CellWidth;
        objCollection.CellHeight = _CellHeight;
       
        //Para seguimiento del usuario, manipulación natural y escalado.
        if (UIInventoryRadialView == null) UIInventoryRadialView = GetComponent<RadialView>();
        if (UIInventorySolverHandler == null) UIInventorySolverHandler = GetComponent<SolverHandler>();
        if (UIInventoryManipulationHandler == null) UIInventoryManipulationHandler = GetComponent<ManipulationHandler>();
        if (UIInventoryBoundingBox == null) UIInventoryBoundingBox = GetComponent<BoundingBox>();
        
        #region Subscripción a eventos 
       
        if (LogsOn)
            Utilities.Info("<i>OnInventoryUpdated</i> listener SUBSCRIBED to Inventory Event",this.GetType().ToString(), funcName: "Start");
        PlayerSpeechManager.onSpeechRecognition += OnGlobalSpeechCommand;
        _UserInventory.onInventoryUpdated.AddListener(OnInventoryUpdated); /* Subscribe to the defined Unity Event para ejecutar un método local cuando se actualiza el inventario */
        #endregion Susbscripción a eventos

        RefreshChildren(ItemInventory.Instance.InventoryItems);

        StartCoroutine(RefreshInventoryTrick());
    }

    /* Borrado de todos los elementos mostrados */
    private void RefreshChildren(List<Item> itemsToDisplay)
    {
        //DELETE ALL CHILDREN IN INVENTORY
        foreach (Transform child in objCollection.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }
        GameObject inventorySlot_new;
        //Instanciación de los elementos en forma de Inventory Slot
        foreach (var i in itemsToDisplay)
        {
            inventorySlot_new = Instantiate(itemPanelPrefab, objCollection.transform) as GameObject; /* Instancia el empty que representa una casilla del Inventario */
            if (inventorySlot_new == null)
            {
                if (LogsOn)
                    Utilities.Error("Inventory Slot <i>instance</i> is NULL ", this.GetType().ToString());
                return;
            }
            var slotManager = inventorySlot_new.GetComponent<InventoryItemManager>(); /* Pasa la información del objeto a su representación (info de la planta, terreno, etc.)*/
            if (slotManager == null)
            {
                if (LogsOn)
                    Utilities.Error("Inventory Slot <i>manager</i> is NULL ",this.GetType().ToString());
                return;
            }
            slotManager.ItemData = i; /* Inicializamos un Item del inventario con los datos del item lógico; 
                                                                                       /* un InventoryItemManager está incluido en el prefab de una casilla y maneja 
                                                                                       /* la manipulación de objetos del inventario */
            inventorySlot_new.SetActive(true);
        }
        
    }

    
    IEnumerator RefreshInventoryTrick()
    {
        while (UIInventoryBoundingBox.gameObject.activeSelf) {
            objCollection.UpdateCollection();
            needsRefresh = false;
            yield return new WaitUntil(()=>needsRefresh);
        }

    }
    
    public void OnInventoryUpdated(List<Item> items)
    {
        /*Actualiza el inventario habiéndose detectado la actualización del mismo */
        RefreshChildren(items);
        //Debug.ClearDeveloperConsole();
        //Debug.Log("INVENTORY UPDATED!");
        needsRefresh = true;
        if (LogsOn) Utilities.Info("Inventory UI <i>UPDATED</i>",this.GetType().ToString() ,funcName:"OnInventoryUpdated");


    }


    public void OnGlobalSpeechCommand(string command)
    {
        switch (command)
        {
            case "Open Inventory":
            case "Abrir Inventario":
                OpenInventory();
                break;
            case "Close Inventory":
            case "Cerrar Inventario":
                CloseInventory();
                break;
            case "Follow Me":
            case "Sígueme":
                SetFollowUser(false,true);
                break;
            case "No me sigas":
            case "Don't follow me":
                SetFollowUser(false,false);
                break;
        }
    }





}
