using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CHIC;
//using UnityEditor;

/* Conjunto de delegados cuyos métodos suscritos desde el editor no desaparecen al cerrar Unity y que funciona de forma análoga a un delegado
 *  en este caso, cuando derivamos del tipo UnityEvent, las variables de puntero función del nuevo tipo requieren inicialización de objeto (new)
 *  una vez inicializados a los que clases externas a esta se pueden suscribir con los métodos .AddListener( ) y 
 *  ejecutar con .Invoke() tal y como ocurre en las variables de delegado o eventos normales */
public class OnInventoryUpdated : UnityEvent<List<Item>> { };
[System.Serializable]
public class OnRemovedItem : UnityEvent<Item> { };
[System.Serializable]
public class OnAddedItem : UnityEvent<Item> { };
[System.Serializable]
public class OnGrabbedItem : UnityEvent<Item> { };


public class ItemInventory : Singleton<ItemInventory>
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property

    [Header("[Starter Pack] Configuration")]
    public bool UseStarterPack = true;
    public StarterPack _StarterPackData;
    
    [SerializeField]
    [Header("[Items] en el inventario")]
    private List<Item> _InventoryItems = new List<Item>(9);

    public List<Item> InventoryItems
    {
        get
        {
            if (_InventoryItems == null) { _InventoryItems = new List<Item>(); }
            return _InventoryItems;
        }
    }

    private uint MaxSize { get; set; } /* Por defecto MaxSize = 0 */

    private Item heldItem; /* [FUTURE] Útil para el input de voz o como modo auxiliar de interacción */

    /* Delegado cuyos métodos se ejecutan actualizar, eliminar o añadir algo al inventario */
    public OnInventoryUpdated onInventoryUpdated = new OnInventoryUpdated();
    [Header("Persistent event subscription")]
    public OnRemovedItem onRemovedItem = new OnRemovedItem();
    public OnAddedItem onAddedItem = new OnAddedItem();
    public OnGrabbedItem onGrabbedItem = new OnGrabbedItem();
    
    private void Awake()
    {
        
        Instance = this; // Requisito para asegurar que sea un Singleton (clase con una sola instancia globalmente)
        if (MaxSize == 0){ MaxSize = 9; }
        _InventoryItems = new List<Item>(9);

        if (UseStarterPack)
        {
            LoadStarterPack();
        }

        if (_InventoryItems == null ||_InventoryItems.Count == 0 )
            Utilities.Error("Instancia del Inventario NO INICIALIZADA ", this.GetType().ToString(), funcName: "Awake");


    }

    /* Carga de elementos por defecto en el inventario */
    public void LoadStarterPack()
    {
        //Si no se ha definido un Starter pack como asset, usar el basado en código
        if (_StarterPackData == null)
        {
            StartCoroutine(StarterPackHardcoded());
            Debug.Log("HARDCODED");
        }
        else
        {
            if (!LoadStarterPackAsset(_StarterPackData))
                Utilities.Error("Error initialising Inventory from SOs", this.GetType().ToString(), funcName: "LoadStarterPack");
        }


        if (InventoryItems.Count > 0) Utilities.Info("STARTER PACK INITIALISED SUCCESSFULLY !", this.GetType().ToString(), funcName: "LoadStarterPack");


    }



    public bool AddItem(Item i, bool notify = true)
    {
        if (InventoryItems.Count < MaxSize)
        {
            if (!InventoryItems.Contains(i))
            {
                InventoryItems.Add(i);
                if (LogsOn) Utilities.Info(" [+1 item] in INVENTORY " + i.ID, this.GetType().ToString(), funcName:"AddItem" );
                
                /* Trigger events: */
                if (notify) onAddedItem?.Invoke(i);
                onInventoryUpdated?.Invoke(_InventoryItems);
                return true;
            }
            else
            {
                if (LogsOn)
                    Utilities.Warn("Item <i>" + i.ID + "</i> already in INVENTORY", this.GetType().ToString(), funcName:"AddItem");
                return false;
            }
        }
        else
        {
            if (LogsOn)
                Utilities.Warn("INVENTORY IS FULL : maxSize=" + InventoryItems.Count,this.GetType().ToString(), funcName:"Additem");
        }
        return false;
    }
    public Item RemoveItem(Item i)
    {
        if (InventoryItems.Count == 0)
        {
            if (LogsOn) Utilities.Info("Inventory is Empty", this.GetType().ToString(), funcName:"RemoveItem");
        }
        Item retrievedItem = InventoryItems.Find(it => (it.ID == i.ID));
        if (InventoryItems.Remove(i))
        {
            /*Trigger events:*/
            onRemovedItem?.Invoke(retrievedItem);
            // UX Notification 
            UXNotifications.Instance.UsedNotification(retrievedItem);
            onInventoryUpdated?.Invoke(_InventoryItems);
            return retrievedItem;
        }
        return null;
    }

    public void ResetToInitial()
    {
        InventoryItems.Clear();
        if (UseStarterPack) LoadStarterPack();
        
    }

    public bool isFull()
    {
        if (InventoryItems.Count == MaxSize) return true;
        else return false;
    }




    #region Starter Pack Implementations
    /* Inicializa el inventario con una serie de elementos Predefinidos  */
    IEnumerator StarterPackHardcoded()
    {
        string itemTypesPath = "BluePrints/Legacy/";
        string plantTypesPath = itemTypesPath + "Plants/";
        string farmlandTypesPath = itemTypesPath + "Farmfields/";

        //In Inventory: Items at startup
        if (LogsOn) //Utilities.Info(this.GetType().ToString(), Color.black, gameObject, "INICIALIZANDO STARTER PACK ...");
        if (AddItem(new Plant(Resources.Load(plantTypesPath + "Achicoria") as PlantSpecies), false) && LogsOn)
            if (LogsOn) ////Utilities.Info(this.GetType().ToString(), Color.black, gameObject, "[+1]...");
        yield return new WaitForSeconds(0.1f);
        if (AddItem(new Plant(Resources.Load(plantTypesPath + "Achicoria") as PlantSpecies), false) && LogsOn)
            //Utilities.Info(this.GetType().ToString(), Color.black, gameObject, "[+1]...");
        yield return new WaitForSeconds(0.1f);
        if (AddItem(new Plant(Resources.Load(plantTypesPath + "Achicoria") as PlantSpecies), false) && LogsOn)
            if (LogsOn) //Utilities.Info(this.GetType().ToString(), Color.black, gameObject, "[+1]...");
        if (AddItem(new Plant(Resources.Load(plantTypesPath + "Achicoria") as PlantSpecies), false) && LogsOn)
            if (LogsOn) ////Utilities.Info(this.GetType().ToString(), Color.black, gameObject, "[+1]...");
                yield return new WaitForSeconds(0.1f);
        if (AddItem(new Plant(Resources.Load(plantTypesPath + "Achicoria") as PlantSpecies), false) && LogsOn)
            if (LogsOn) ////Utilities.Info(this.GetType().ToString(), Color.black, gameObject, "[+1]...");
                yield return new WaitForSeconds(0.1f);

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < 3; i++)
        {
            if (AddItem(new Farmland(Resources.Load(farmlandTypesPath + "TerrenoV2") as FarmlandType), false) && LogsOn)
                if (LogsOn) Utilities.Info("Farmland Added to Inventory", this.GetType().ToString(), funcName:"StarterPackHardcoded");
            yield return new WaitForSeconds(0.1f);
        }

        onInventoryUpdated.Invoke(InventoryItems);

        yield return new WaitForSeconds(2f);
#if (UNITY_EDITOR)
        //Utilities.clearUnityConsole();
#endif
    }
   
    public bool LoadStarterPackAsset(StarterPack pack)
    {




        if (LogsOn) Utilities.Info("INICIALIZANDO STARTER PACK ...",this.GetType().ToString(),funcName: "LoadStarterPackAsset");
        if (pack._Fields.Count + pack._Plants.Count >= MaxSize)
        {
            return false;
        }
        //Load all fields
        foreach (var field in _StarterPackData._Fields)
        {
            //AssetDatabase.AddObjectToAsset(field, _StarterPackData);
            AddItem(new Farmland(field));
            if (LogsOn) Utilities.Info("Farmland [+1]", this.GetType().ToString(), funcName: "LoadStarterPackAsset");
            
        }
        //Load all plants
        foreach (var plant in _StarterPackData._Plants)
        {
            //AssetDatabase.AddObjectToAsset(plant, _StarterPackData);
            AddItem(new Plant(plant));
            if (LogsOn) Utilities.Info("Plant [+1]", this.GetType().ToString(), funcName: "LoadStarterPackAsset");
        }
        return true;
    }

    #endregion




    #region NOT WORKING
    /*[REVISIT] Permite a un usuario "equiparse" un elemento para su posterior uso sin Drag and drop */
    public bool HoldItem(Item i)
    {
        if (heldItem == null)
        {
            heldItem = i;
            RemoveItem(i);
        }
        onGrabbedItem?.Invoke(heldItem);
        return (i.ID != heldItem.ID) ? true : false;
    }

    public bool DropItem(Item i)
    {
        Debug.Log("JUST DROPPED AN ITEM " + i);
        //Spawnear en una posición dada por el head ray y si se queda por el mundo más de x segundos, devolver al inventario
        return (i.ID != heldItem.ID) ? true : false;
    }
    #endregion NOT WORKING



}


