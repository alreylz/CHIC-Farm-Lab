using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{

    bool _IsActive; // Indica desde el punto de vista lógico si la tienda está abierta o no
    public bool IsActive { get => _IsActive; }

    [SerializeField]
    //PlantVariety Item
    List<ScriptableObject> _AvailableForSaleItemList;


    //List<>

 


    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
