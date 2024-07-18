
using UnityEngine;
using CHIC;
/* Incluir este script para todos los elementos que pueden ser comprados en el juego */
public class Buyable : MonoBehaviour
{
    public Item ItemInfo { get; private set; }
    public ScriptableObject itemBluePrint; 
    /* Un ScriptableObject que permite crear instancias persistentes (blueprints) que corresponden a tipologías concretas de Items (es decir, un lugar de plantación, una planta) */
   
    private GameManager _GlobalStatus;
    private ItemInventory _UserInventory;

    /* Permite modificar el precio de venta al que se puede adquirir Item independientemente del precio base definido por el Blueprint */

    public bool changePriceLocally = false;

    public float InShopPrice { get; private set; }

    public bool isOneOfAKind = false;
    
    private Color logsColor = Color.HSVToRGB(141.0f, 90.0f, 45.0f); /*Money green*/
    public bool logsOn = false;

    private void Awake()
    {
        ItemInfo = initDataItemInstance(itemBluePrint);
    }

    private void Start(){

        /*Obtengo referencias a objetos de scope global: el GameManager y el inventario */
        if (_GlobalStatus == null) _GlobalStatus = GameManager.Instance;
        if (_UserInventory == null) _UserInventory = ItemInventory.Instance;

        float priceFromBluePrint = 0f;
        if ((itemBluePrint as PlantSpecies) != null) priceFromBluePrint= (itemBluePrint as PlantSpecies).purchasingPrice;
        else priceFromBluePrint = (itemBluePrint as FarmlandType).purchasingPrice;

        if (!changePriceLocally) InShopPrice = priceFromBluePrint;


    }

    public void Buy()
    {

        bool[] buyError = { false, false };
        /* Comprobamos espacio en el Inventario y tratamos de comprar el Item (viendo si el usuario tiene suficiente dinero */
        if (  !(buyError[0] = _UserInventory.isFull()) && !(buyError[1] = !_GlobalStatus.TrySubstractMoney(InShopPrice)) )
        {
            _UserInventory.AddItem(ItemInfo);
            UXNotifications.Instance.BoughtItemNotification(ItemInfo);

        }
        else
        {
            if (buyError[0]) Utilities.Warn(" BUY Prevented, INVENTORY is <i>full</i>", this.GetType().ToString(), funcName:"Buy");
            if(buyError[1]) Utilities.Warn(" BUY Prevented, SAVINGS are <i>insufficient</i>", this.GetType().ToString(), funcName: "Buy");
        }
        if (isOneOfAKind == true) Destroy(gameObject,1); //Si es un objeto de compra de un solo uso, entonces lo destruimos, ya no se puede volver a comprar
        else ItemInfo = initDataItemInstance(this.itemBluePrint);
    }


    /* Genera una nueva instancia (con nuevo ID para que se pueda comprar otra planta del mismo tipo y guardarla en el inventario */
    private Item initDataItemInstance(ScriptableObject itemBluePrint)
    {
        /* A partir del blueprint asociada a este objeto, disponible en la tienda,
         * generamos una instancia de datos; i.e. un objeto Item  de alguno de los tipos definidos 
         */
        Item itemInfo = null;
        if (itemBluePrint is PlantSpecies){ itemInfo = new Plant(itemBluePrint as PlantSpecies); InShopPrice = ((Plant)itemInfo).PurchasingPrice; }
        else if (itemBluePrint is FarmlandType){ itemInfo = new Farmland(itemBluePrint as FarmlandType); InShopPrice =  ((Farmland)itemInfo).purchasingPrice; }
        //MORE ITEM TYPES HERE
        else {
            Utilities.Error("Inicialización de Item a partir de Blueprint incorrecta.", this.GetType().ToString(), funcName: "initDataitemInstance");
        }
        if (logsOn) Utilities.Info(" ItemInfo:" + itemInfo.ToString() + "itemBluePrint: " + itemBluePrint != null ? "init" : "null", this.GetType().ToString());
        return itemInfo;

    }



}


