using UnityEngine;

[System.Serializable]
public enum ItemType
{
    Plant,
    Farmland
}
[System.Serializable]
public class Item 
{
    private static long _NextItemInstanceID = -1;

    /* SetItemsInstanceID : permite inicializar el ID único de elementos del juego a un valor pasado como argumento, 
     *  de forma que initValue sea el ID asociado al siguiente Item que sea creado */
    public static void SetItemsInstanceID (long initValue)
    {
        _NextItemInstanceID = initValue-1;
    }

    /* Identificador único de cada elemento del juego de tipo Item que lo distingue de forma inequívoca */

    public long ID { get; protected set; } 

    public long NextItemID { get { return ++_NextItemInstanceID; } }
    
    protected ItemType type;
    public ItemType Type { get => type;  }

    public override string  ToString()
    {
        return "Item: ID = "+ID+" Type: "+Type.ToString()+ " Next ItemID:" + NextItemID.ToString();
    }


}
