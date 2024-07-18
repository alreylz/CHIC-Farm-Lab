using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input.Utilities;

public class UIEnemyInfo : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property
    
    /*Objeto 3D que representa al enemigo, que es también el objeto en el que está situado este script*/
    private GameObject enemyObject;
    /*Cámara principal, hacia donde tiene que apuntar la UI del enemigo para que el usuario pueda verla*/
    Camera holoCamera;

    /*Script que contiene los datos y métodos asociados a un Enemigo */
    private MobBehavior enemyData;

    public Interactivo focusData;
    /*Rect transform del Canvas que soporta toda la UI del enemigo*/
    public  RectTransform recTransformRootCanvasHP;
    /*Canvas, Panel e hijos que reflejan la vida del enemigo (prefab de toda la jerarquía) */
    public GameObject uiCanvasSetPrefab; //PENDING : Hacer que si no existe un UI entonces se cree una vida por defecto
    /*Componente Rect transform de la barra de vida, necesario para su modificación directa cuando se produce algún cambio*/
   public  RectTransform healthBar;

    private void Awake()
    {

        if (enemyObject == null) enemyObject = gameObject;
        if (holoCamera == null) holoCamera = Camera.main;
        //Obtener referencia a los datos del enemigo
        if(focusData==null) focusData = enemyObject.GetComponent<Interactivo>();
        if(enemyData== null) enemyData = enemyObject.GetComponent<MobBehavior>();
        

        if (recTransformRootCanvasHP == null) recTransformRootCanvasHP = GetComponentInChildren(typeof(RectTransform)) as RectTransform;
        if( healthBar == null) { Debug.LogError("HEALTH BAR NOT INITIALISED"); }
        recTransformRootCanvasHP.GetComponentInChildren<TextMeshProUGUI>().SetText(enemyData.mobData.name);

        //Añadimos un listener para cuando ocurre un cambio en los datos del enemigo.
        enemyData.MobDataChanged += DisplayMobData;
    }

    // Update is called once per frame
    void Update()
    {
        //Si el elemento ha dejado de estar enfocado y tiene la UI activa en la jerarquía, entonces la desactivamos.
        if (!focusData.IsFocused)
        {
            recTransformRootCanvasHP.gameObject.SetActive(false);
        }
        else if(focusData.IsFocused && !recTransformRootCanvasHP.gameObject.activeInHierarchy) {
           
            recTransformRootCanvasHP.gameObject.SetActive(true);
        }
        
    }


    void DisplayMobData(MobData mobData)
    {
        if (focusData.IsFocused) {
            //Debug.Log("<color=green>" + mobData.name + "</color> <color=red>" + mobData.Health + "</color>");
            //Debug.Log("MAX VIDA:"+ mobData.BaseHealth + " VIDA ACTUAL: " + mobData.Health);
            //float wholeBarWidth = recTransformRootCanvasHP.offsetMax.y;

            float percentOFHP = mobData.Health / mobData.BaseHealth;
            
            //Alteramos la propiedad "right" del inspector para reducir la vida del enemigo
            //healthBar.offsetMax = new Vector2( -percentOFHP, -wholeBarWidth);

            healthBar.GetComponent<Image>().fillAmount = percentOFHP;

        }

    }

}
