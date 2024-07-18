using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System;
using CHIC;

/*Solo hay una clase de este tipo y además usamos "sealed" para hacer que no se pueda heredar de ella*/
public sealed class GameManager : Singleton<GameManager>
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property
    
    #region Delegates / Events / Actions / Funcs (eventos a los que instancias de otras clases se pueden suscribir)

    //Creamos un tipo que es de punteros a funciones que devuelven void y reciben un elemento GameManager
    public delegate void GeneralStateChange(GameManager globalGameInfo);

    /* Delegado y evento combinados en Action; permite suscripción de objetos a el tiempo que ha pasado en el juego */
    public static Action<uint> OnDayNPassed;
    public static Action OnDayPassed;

    //Nuevo tipo que contiene punteros a funciones, solo accesible por medio del  método subscribeToChanges()
    private GeneralStateChange OnGeneralStateChanged;
    /*Función para incluir funciones a ser llamadas cuando se producen cambios globales */
    public void subscribeToChanges(GeneralStateChange callback)
    {
        OnGeneralStateChanged += callback;
    }

    #endregion

    [Header("Global Configuration of the game")]
    [Range(1,20)]
    public uint secondsToGameTimeUnit = 2;
    public uint initialMoney = 320;

    [Header("Game Info")]
    [SerializeField]
    private float _Money;
    public float Money { get => _Money; }
    [SerializeField]
    private uint _GameDaysElapsed;  /* Número entero de días de juego que han pasado desde el inicio */
    public uint CurrentDay { get => _GameDaysElapsed; }
    private uint GameDaysElapsed { get => (uint)Mathf.FloorToInt(_GameTimeSpent / secondsToGameTimeUnit); }
    [SerializeField]
    private float _DailyCycleStatus; /* [0,1] que indica el momento del día en el que nos encontramos */
    public string TimeOfDay { get => PercentageToTimeOfDay(_DailyCycleStatus); }
    [SerializeField]
    private float _GameTimeSpent; /* Tiempo real desde que comenzó el juego */
    
   
   

    public string GetStringMoney
    {
        get { return Money.ToString() + "$"; }
    }



    public Player ThePlayer;
    

    public GameObject panelTitulos;
    public GameObject panelGuiasVoz;

    public Dictionary<string, GameObject> weatherDict = new Dictionary<string, GameObject>();

    public static GameObject GameWorld;

    /* Transform de referencia respecto al que todo el resto se sitúan, a excepción de aquellos que tienen que ver directamente con el MRTK */

    /* AUX FUNCTION TO SOLVE MRTK BUG */
    void removeExtraCursors(int noAllowedCursors)
    {
        GameObject[] goList = FindObjectsOfType<GameObject>();

        if (goList == null) return;

        int noCursorsFound = 0;
        foreach (GameObject go in goList)
        {
            if(go.name == "DefaultCursor(Clone)" && noCursorsFound>=noAllowedCursors)
            {
                //Destruye cursores y evita que salten excepciones sin parar al ser duplicados por un error en el MRTK
                DestroyImmediate(go);
            }
            else if(go.name == "DefaultCursor(Clone)")
            {
                noCursorsFound++;
            }
        }

    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //removeExtraCursors(0);
    }
#endif



    private void Awake()
    {
        Instance = this;
        // Inicializa el valor de la propiedad que da un ID a elementos del juego único 
        Item.SetItemsInstanceID(0);
        
        if (LogsOn) OnGeneralStateChanged += PrintGameManagerData;

        if (GameWorld == null)
            GameWorld = GameObject.FindGameObjectWithTag("GameWorld");
            if (ThePlayer == null) ThePlayer = FindObjectOfType<Player>();

        _Money = initialMoney;

        GameObject weatherObj;
        weatherObj = GameObject.Find("Sun");
        if (weatherObj != null) {
            weatherDict.Add("Sun", weatherObj);
            weatherObj.SetActive(false);
        }
        weatherObj = GameObject.Find("CloudStorm");
        if (weatherObj != null)
        {
            weatherDict.Add("CloudStorm", weatherObj);
            weatherObj.SetActive(false);
        }
        

    }


    // Start is called before the first frame update
    void Start()
    {
       if(OnGeneralStateChanged!=null) OnGeneralStateChanged.Invoke(this);
        ThePlayer = FindObjectOfType<Player>();
    }


    //Mostrar de media hora en media hora
    private string PercentageToTimeOfDay(float percent)
    {
        float toMins = 1440 * percent;
        int hour = Mathf.FloorToInt(toMins/ 60f);
        int min = (int) toMins - hour * 60;
        return hour.ToString("D2") + ":" + min.ToString("D2");

    }


    private void LateUpdate()
    {

        Debug.developerConsoleVisible = false; //TZ para no mostrar la consola

        _GameTimeSpent = Time.realtimeSinceStartup;
       
        //Si hemos superado un nuevo día
        if (_GameDaysElapsed != GameDaysElapsed)
        { 
            _GameDaysElapsed = GameDaysElapsed; // Caching de los días reales que han pasado completamente
            //Notificación a clases suscritas eventos asociados al paso del tiempo en el juego
            OnGeneralStateChanged?.Invoke(this);
            OnDayNPassed?.Invoke(CurrentDay);
            OnDayPassed?.Invoke();
        }
        else
        {
            //Actualización de porcentaje de día que ha pasado
            _DailyCycleStatus =  (_GameTimeSpent - _GameDaysElapsed * secondsToGameTimeUnit)/secondsToGameTimeUnit;
            //Debug.Log("Hours passed: "+ _DailyCycleStatus);
        }
        
        if (LogsOn)
        {
            Debug.Log("Real time:" + _GameTimeSpent);
            Debug.Log("Days Elapsed:" + _GameDaysElapsed);
            Debug.Log("Time of the day: " + PercentageToTimeOfDay(_DailyCycleStatus));

        }



    }



    public void AddMoney(float toAddmoney)
    {
        if (toAddmoney <= 0) { TrySubstractMoney(-toAddmoney); return; }
        this._Money += toAddmoney;

        // Enviamos notificacion a la Applicacion de control
        //UXNotifications.Instance.SendMessageToControlApp("mr", "Money", this._Money.ToString());
        // 

        UXNotifications.Instance.AnimateCoin();
        OnGeneralStateChanged?.Invoke(this);
    }

    public bool TrySubstractMoney(float toSubstractMoney)
    {
        if (Money - toSubstractMoney < 0) { return false; }
        _Money -= toSubstractMoney;

        // Enviamos notificacion a la Applicacion de control
        //UXNotifications.Instance.SendMessageToControlApp("mr", "Money", this._Money.ToString());
        // 

        UXNotifications.Instance.AnimateCoin();
        OnGeneralStateChanged?.Invoke(this);
        return true;
    }


    void PrintGameManagerData(GameManager gm)
    {
        string toPrint = "-----Game Data-----\n" +
        " GAME-DAY: " + gm.CurrentDay + "\n" +
        " TIME: " + gm.TimeOfDay + "\n" +
        " MONEY:" + gm.Money + "\n"+
        "-----------------\n";
    }

    public IEnumerator RemoveAfterSeconds(int seconds, GameObject obj)
    {
        yield return new WaitForSeconds(seconds);
        obj.SetActive(false);
    }


    public void ResetGameManager() {

        _Money = initialMoney;
        
    }



}







