using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    public Transform Selling3DObj;
    public float rotateSpeed = 5;

    /* UI Elements */
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemPrice;
    [SerializeField]
    private GameObject ToDisplayPrefab; 
    public GameObject ToBuyPrefabSlot; /* Representación del elemento a comprar en la tienda (e.g. saco de semillas ) */

    public Buyable ItemToSell;
    private Item itemData;

    // Start is called before the first frame update
    void Start()
    {

        if (ItemToSell == null) ItemToSell = gameObject.GetComponent<Buyable>();
        Debug.Log(ItemToSell.ItemInfo.Type.ToString());
        if (ToBuyPrefabSlot == null) ToBuyPrefabSlot = transform.Find("SellingItem3D").gameObject;
        switch (ItemToSell.ItemInfo.Type)
        {
            case ItemType.Farmland:
                Farmland itemDataO1 = ItemToSell.ItemInfo as Farmland;
                itemData = itemDataO1;
                ItemName.SetText(itemDataO1.Name);
                ToDisplayPrefab = itemDataO1.shopPrefab;
                break;
            case ItemType.Plant:
                
                Plant itemDataO2 = ItemToSell.ItemInfo as Plant;
                itemData = itemDataO2;
                ItemName.SetText(itemDataO2.SpeciesName);
                ToDisplayPrefab = itemDataO2.SackPrefab;
                break;
        }

        //Display price
        ItemPrice.SetText(ItemToSell.InShopPrice.ToString() + "$");
        //Set shop prefab to replace default
        ToBuyPrefabSlot.GetComponent<MeshFilter>().mesh = ToDisplayPrefab.GetComponent<MeshFilter>().sharedMesh;
        ToBuyPrefabSlot.GetComponent<MeshRenderer>().material = ToDisplayPrefab.GetComponent<MeshRenderer>().sharedMaterial;

    }

    // Update is called once per frame
    void Update()
    {
        Selling3DObj.Rotate(new Vector3(0,rotateSpeed*Time.deltaTime, 0 /*0.5f * rotateSpeed * Time.deltaTime*/));
    }
}
