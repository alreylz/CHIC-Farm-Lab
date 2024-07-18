using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{

    public GameObject RootShopCanvas;
    
    private void Awake()
    {
        if (RootShopCanvas == null)
        {
            RootShopCanvas = transform.GetChild(0).gameObject;
        } 
    }


    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Shop voice toggle suscribed");
        PlayerSpeechManager.onSpeechRecognition += ShopVoiceToggle;
    }
    
    private void ShopVoiceToggle(string recognized)
    {
        switch (recognized)
        {
            case "Open Shop":
            case "Abrir Tienda":
                OpenShop();
                break;
            case "Close Shop":
            case "Cerrar Tienda":
                CloseShop();
                break;
        }

    }
    
    public void OpenShop() { RootShopCanvas.SetActive(true); }
    public void CloseShop() { RootShopCanvas.SetActive(false); }

}
