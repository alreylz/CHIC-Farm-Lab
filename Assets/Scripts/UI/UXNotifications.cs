using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CHIC;
using UnityEngine.UI;

public class UXNotifications : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property

    #region Singleton
    private static UXNotifications _Instance;
    public static UXNotifications Instance { get { return _Instance; } }
    #endregion

    public AudioSource AudioSrcNotificator;

    [Header("HUD - Opacity Controllers")]
    public CanvasGroup RootOpacityController;
    public CanvasGroup MainNotificationOpacityController;
    public CanvasGroup RootStatusOpacityController;
    public CanvasGroup ElapsedTimeOpacityController;
    public CanvasGroup GrabbedItemOpacityController;
    public CanvasGroup MoneyOpacityController;

    [Header("HUD - Canvases")]
    public GameObject MainNotificationCanvas;
    public GameObject StatusShowCanvas;
    /* WS UI Monitoring */
    public GameObject WSStatusCanvas;

    /* Voice Commands Tutorials */
    public GameObject VoiceCommandGuidesCanvas;

    [Header("HUD - Text Elems")]
    /* Text Components */
    public TextMeshProUGUI NotificationText;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI HourText;
    public TextMeshProUGUI GameDayText;
    public TextMeshProUGUI HeldItemText;
    public Text VoiceCommandHelp;

    public TextMeshProUGUI WSStatusText;
    public TextMeshProUGUI WSStatusMoreInfo;

    [Header("HUD - UI Sprites")]
    /* UI Sprite Components */
    public Image PopupImage;
    public Image WSStatusImage;

    public RectTransform ClockSprite;
    public RectTransform MoneySprite;
    public RectTransform HeldItemSprite;

    [Range(1f, 10f)]
    public float fadingSpeed = 0.5f;
    public float delayFadeSeconds = 4f;

    [SerializeField]
    List<AudioClip> _NotificationAudioClips = new List<AudioClip>(); //Lista de clips que se reproducen con cada una de las acciones notificadas

    Dictionary<string, AudioClip> _InstructionsAudioClips = new Dictionary<string, AudioClip>();
    Dictionary<string, GSNotificationBlueprint> _GSNotificationsDict = new Dictionary<string, GSNotificationBlueprint>();

    public bool bSpanish = true;

    Sprite infoSprite;
    Sprite blankSprite;
    
    private Coroutine _RepeatingCoroutine;

    private void Awake()
    {
        #region Singleton
        if (_Instance == null) { _Instance = this; }
        if (_Instance != null && _Instance != this) { Destroy(this); }
        #endregion Singleton


        //Init Controladores de opacidad
        if (RootOpacityController == null) RootOpacityController = GetComponent<CanvasGroup>();
        if (MainNotificationOpacityController == null) MainNotificationOpacityController = transform.Find("MainNotificationsCanvas").GetComponent<CanvasGroup>();
        if (RootStatusOpacityController == null) RootStatusOpacityController = transform.Find("StatusShowCanvas").GetComponent<CanvasGroup>();
        if (ElapsedTimeOpacityController == null) ElapsedTimeOpacityController = transform.Find("StatusShowCanvas/ElapsedTimePanel").GetComponent<CanvasGroup>();
        if (GrabbedItemOpacityController == null) GrabbedItemOpacityController = transform.Find("StatusShowCanvas/GrabbedItemPanel").GetComponent<CanvasGroup>();
        if (MoneyOpacityController == null) MoneyOpacityController = transform.Find("StatusShowCanvas/CoinPanel").GetComponent<CanvasGroup>();
        //Init referencias a contenedores de los dos componentes de la interfaz HUD
        if (MainNotificationCanvas == null) //Init de Canvas de notificaciones textuales (Center of screen; POP-UP)
        {
            MainNotificationCanvas = transform.Find("MainNotificationsCanvas").gameObject;
            NotificationText = MainNotificationCanvas.transform.Find("NotificationText").gameObject.GetComponent<TextMeshProUGUI>();
            PopupImage = MainNotificationCanvas.transform.Find("NotificationIcon").GetComponent<Image>();
        }
        if (StatusShowCanvas == null) //Init de Elementos que muestran estado general del juego (money, time)
        {
            StatusShowCanvas = transform.Find("StatusShowCanvas").gameObject;
            HeldItemText = StatusShowCanvas.transform.Find("GrabbedItemPanel/GrabbedItemName").GetComponent<TextMeshProUGUI>();
            HeldItemSprite = StatusShowCanvas.transform.Find("GrabbedItemPanel/ItemSprite").GetComponent<RectTransform>();
            MoneyText = StatusShowCanvas.transform.Find("CoinPanel/CoinNumbers").GetComponent<TextMeshProUGUI>();
            MoneySprite = StatusShowCanvas.transform.Find("CoinPanel/MoneyIcon").GetComponent<RectTransform>();
            HourText = StatusShowCanvas.transform.Find("ElapsedTimePanel/DayTime").GetComponent<TextMeshProUGUI>();
            GameDayText = StatusShowCanvas.transform.Find("ElapsedTimePanel/Day").GetComponent<TextMeshProUGUI>();
            ClockSprite = StatusShowCanvas.transform.Find("ElapsedTimePanel/TimeIcon").GetComponent<RectTransform>();
        }
        if (WSStatusCanvas == null)
        {
            WSStatusCanvas = StatusShowCanvas.transform.Find("WSStatus").gameObject;
            WSStatusText = WSStatusCanvas.transform.Find("WSStatusText").GetComponent<TextMeshProUGUI>();
            WSStatusImage = WSStatusCanvas.transform.Find("StatusImage").GetComponent<Image>();
            WSStatusMoreInfo = WSStatusCanvas.transform.Find("MoreInfoWSStatus").GetComponent<TextMeshProUGUI>();
        }
        if (VoiceCommandGuidesCanvas == null)
        {
            VoiceCommandGuidesCanvas = StatusShowCanvas.transform.Find("Panel_GuiasVoz").gameObject;
            VoiceCommandHelp = VoiceCommandGuidesCanvas.transform.Find("Text").GetComponent<Text>();
        }
        //Init Audio Player
        if (AudioSrcNotificator == null) GetComponent<AudioSource>(); AudioSrcNotificator.playOnAwake = false;

        _InstructionsAudioClips.Add("AbreTienda", Resources.Load<AudioClip>("Audio/Instrucciones/AbreTienda"));
        _InstructionsAudioClips.Add("Terrenos", Resources.Load<AudioClip>("Audio/Instrucciones/Terrenos"));
        _InstructionsAudioClips.Add("Plantas", Resources.Load<AudioClip>("Audio/Instrucciones/Plantas"));
        _InstructionsAudioClips.Add("DemasiadaLluvia", Resources.Load<AudioClip>("Audio/Instrucciones/DemasiadaLluvia"));
        _InstructionsAudioClips.Add("DemasiadoSol", Resources.Load<AudioClip>("Audio/Instrucciones/DemasiadoSol"));
        _InstructionsAudioClips.Add("Insectos", Resources.Load<AudioClip>("Audio/Instrucciones/Insectos"));
        _InstructionsAudioClips.Add("Cosecha", Resources.Load<AudioClip>("Audio/Instrucciones/Cosecha"));
        _InstructionsAudioClips.Add("ModGenetica", Resources.Load<AudioClip>("Audio/Instrucciones/ModGenetica"));
        _InstructionsAudioClips.Add("Semillas", Resources.Load<AudioClip>("Audio/Instrucciones/Semillas"));

         infoSprite = Resources.Load<Sprite>("Sprites/Icon_Info");
         blankSprite = Resources.Load<Sprite>("Sprites/Hexagon");

        //INICIALIZACIÓN DE DICCIONARIO DE NOTIFICACIONES
        //Primeros pasos en juego
        _GSNotificationsDict.Add("init", Resources.Load<GSNotificationBlueprint>("BluePrints/GSNotificationDataInit"));
        _GSNotificationsDict.Add("afterTerrain", Resources.Load<GSNotificationBlueprint>("BluePrints/GSNotificationDataAfterTerrain"));
        _GSNotificationsDict.Add("afterPlant", Resources.Load<GSNotificationBlueprint>("BluePrints/GSNotificationDataAfterPlant"));
    }

    private void Start()
    {

        if (LogsOn)
            Utilities.Info("Adding Listeners for auditory and visual feedback", this.GetType().ToString());
        ItemInventory.Instance.onAddedItem.AddListener(BoughtItemNotification);
        //ItemInventory.Instance.onRemovedItem.AddListener(UsedNotification);
        //ItemInventory.Instance.onGrabbedItem.AddListener(ItemVoicePickupNotification);

        
        #region Init UI Elems

        #endregion Init UI Elems

        //Event Subscriptions
        GameManager.OnDayNPassed += OnDayUpdated;
        GameManager.Instance.subscribeToChanges(OnGeneralChanges);
        //[REVISIT]FindObjectOfType<WSCommunication>().OnWSStatusChanged += OnWSStatusChanged;
        
        RootOpacityController.alpha = 1;
        MainNotificationOpacityController.alpha = 1; // -> POP-UP
        ElapsedTimeOpacityController.alpha = 0;
        MoneyOpacityController.alpha = 0;

        //[REVISIS] Not Worth for IFEMA BUILD 
        //StartCoroutine(UpdateHour());
        //StartCoroutine(ShowClockPeriodically(20f, 10f));

        //OpenShopVoiceHelp();

    }


    void OnGeneralChanges(GameManager info)
    {
        MoneyText.text = info.GetStringMoney;

    }


    // Hace Fade out de una notificación desplegada en la capa dada por clearLayer
    IEnumerator NotificationPlusClear(float delay = 0.0f, uint clearLayer = 0)
    {
        float step = 0.02f * fadingSpeed;
        CanvasGroup canvasToFadeOut;

        switch (clearLayer)
        {
            case 0: //Fade out 
                canvasToFadeOut = RootOpacityController;
                break;
            case 1://POPUP
                canvasToFadeOut = MainNotificationOpacityController;
                break;
            case 3://STATUS
                canvasToFadeOut = RootStatusOpacityController;
                break;
            case 4://ONLY TIME
                canvasToFadeOut = ElapsedTimeOpacityController;
                break;
            case 5://ONLY GRABBED
                canvasToFadeOut = GrabbedItemOpacityController;
                break;
            case 6: //ONLY MONEY
                canvasToFadeOut = MoneyOpacityController;
                break;
            default:
                canvasToFadeOut = RootOpacityController;
                break;
        }

        if (canvasToFadeOut.alpha == 0f) canvasToFadeOut.alpha = 1;


        if (delay > 0) yield return new WaitForSecondsRealtime(delay);

        for (float alpha = 1.0f - step; alpha >= 0; alpha -= step)
        {
            canvasToFadeOut.alpha = alpha;
            yield return new WaitForEndOfFrame();
        }
        canvasToFadeOut.alpha = 0;
        _RepeatingCoroutine = null;
    }



    private void Update()
    {

        //WSStatusText.SetText(WSCommunication.Instance.WSConnectionStatus == WSStatus.CONNECTED ? "CONNECTED" : "DISCONNECTED");
        //WSStatusMoreInfo.SetText(WSCommunication.Instance.WSUriSrc == WSURISource.IN_EDITOR_STRING ? WSCommunication.Instance.InEditorWSUri.ToString() : WSCommunication.Instance.WSUriFilename.ToString() );
        //Cargar sprite adecuado WSStatusImage.sprite = 
    }



    void OnDayUpdated(uint day)
    {
        GameDayText.text = "Day " + day;
        AnimateClock();
    }


    #region UI procedural animations
    public void AnimateClock()
    {
        StartCoroutine(DoAnimateZ(ClockSprite, 2f, 15f));
    }
    public void AnimateCoin() {
        StartCoroutine(DoAnimateZ(MoneySprite, 2f, 15f));
    }
    // TO-DO: Move to Utilities
    IEnumerator DoAnimateZ(RectTransform rt, float duration, float updateFreq)
    {

        float timeStep = 1f / updateFreq;
        float elapsed = 0;
        while (elapsed < duration) {
            rt.Rotate(Vector3.up, Mathf.Lerp(0f, 360f, elapsed), Space.World);
            elapsed += timeStep;
            yield return new WaitForSeconds(timeStep);
        }

        yield return null;
    }
    #endregion UI procedural animations


    IEnumerator ShowClockPeriodically(float period, float hold) {

        while (true)
        {
            yield return new WaitForSecondsRealtime(period);
            yield return StartCoroutine(NotificationPlusClear(hold, 4));
        }
    }

    IEnumerator UpdateHour()
    {
        while (true) {
            HourText.text = GameManager.Instance.TimeOfDay;
            yield return new WaitForSecondsRealtime(2f);
        }
    }

    #region UX Notification functions on game events



    public void OpenShopVoiceHelp()
    {
        VoiceCommandHelp.text = "Ayuda comando de voz";
    }





    // - WEB SOCKET
    public void WebSocketNotification(string message)
    {
        //MoneyText.text = GameManager.Instance.GetStringMoney;
        NotificationText.text = message;
        //MainNotificationOpacityController.alpha = 1;
        //Debug.Log("RUNNING WEB SOCKET NOTIFICATION");
        //TO-DO: Add Icon regarding notification
        //PopupImage.sprite = 

        //AudioSrcNotificator.clip = _NotificationAudioClips[0];
        //AudioSrcNotificator.Play();

    }

    // - NOTIFICACION A LA APP DE CONTROL POR WEBSOCKET
    //public void SendMessageToControlAppX(string type, string data, string value)
    //{
    //    //WSCommunication wscom = GetComponent<WSCommunication>();//old stuff
    //    FindObjectOfType<WSCommunication>().SendMessageFromUnityToApp(type,data, value);
    //}


    // - VENTA DE ELEMENTOS
    public void SoldItemNotification(Item i)
    {
        MoneyText.text = GameManager.Instance.GetStringMoney;
        NotificationText.text = "You've just sold a " + ((i.Type == ItemType.Farmland) ? "Farmland by <color=green> " + (i as Farmland).sellingPrice.ToString() : "Plant by <color=green> " + (i as Plant).SellingPrice.ToString()) + " $</color>";

        StartCoroutine(NotificationPlusClear(delayFadeSeconds, 6 /*money*/));
        StartCoroutine(NotificationPlusClear(delayFadeSeconds, 1 /*popup*/ ));
        //AnimateCoin();

        //TO-DO: Add Icon regarding notification
        //PopupImage.sprite = 

        AudioSrcNotificator.clip = _NotificationAudioClips[0];
        AudioSrcNotificator.Play();

    }
    // - COMPRA DE ELEMENTOS
    public void BoughtItemNotification(Item i)
    {
        MoneyText.text = GameManager.Instance.GetStringMoney;
        NotificationText.text = "You've just bought a " + ((i.Type == ItemType.Farmland) ? "Farmland by <color=green> " + (i as Farmland).sellingPrice.ToString() : "Plant by <color=red> " + (i as Plant).SellingPrice.ToString()) + " $</color>";
        //TO-DO: Add Icon regarding notification
        Sprite nuSprite = Resources.Load<Sprite>("Sprites/Recurso 1");
        if (nuSprite != null)
            PopupImage.sprite = nuSprite;
        else Debug.LogError("SHIT, Sprite at " + "Sprites/Recurso 1  doesn't exist");



        StartCoroutine(NotificationPlusClear(delayFadeSeconds, 6 /*money*/));
        StartCoroutine(NotificationPlusClear(delayFadeSeconds, 1 /*popup*/ ));
        AnimateCoin();



        AudioSrcNotificator.clip = _NotificationAudioClips[0];
        AudioSrcNotificator.Play();

    }
    // - COMIENZO DE OLEADA
    public void SurgeStartNotification()
    {
        NotificationText.text = "Beware! take your fields may be in danger!";

        StartCoroutine(NotificationPlusClear(delayFadeSeconds, 1 /*popup*/ ));
        //TO-DO: Add Icon regarding notification
        // PopupImage.sprite = 
        //TO-DO: Record Voice notification
        // AudioSrcNotificator.clip = _NotificationAudioClips[?];
        // AudioSrcNotificator.Play();
    }
    // - FIN DE OLEADA
    public void SurgeCompletedNotification()
    {
        NotificationText.text = "Congrats, your plants are safe again";
        if (_RepeatingCoroutine != null)
        {
            Debug.Log("Stop Coroutine Scan and start new one");
            StopCoroutine(_RepeatingCoroutine);
        }

        _RepeatingCoroutine = StartCoroutine(NotificationPlusClear(delayFadeSeconds, 1 /*popup*/ ));
        //TO-DO: Add Icon regarding notification
        // PopupImage.sprite = 
        //TO-DO: Record Voice notification
        // AudioSrcNotificator.clip = _NotificationAudioClips[?];
        // AudioSrcNotificator.Play();
    }
    // - USED ITEM NOTIFICATION
    public void UsedNotification(Item i)
    {
        NotificationText.SetText("You used  a " + i.GetType().ToString());
        StartCoroutine(NotificationPlusClear(delayFadeSeconds, 1 /*popup*/ ));
        //TO-DO: Add Icon regarding notification
        // PopupImage.sprite =
        AudioSrcNotificator.clip = _NotificationAudioClips[1];
        AudioSrcNotificator.Play();

    }

    // - SCANNING ROOM
    public void ScanningNotification(int type = 0) {

        NotificationText.SetText("Look around while Hololens detects your environment ");


        if (_RepeatingCoroutine != null)
        {
            Debug.Log("Stop Coroutine Scan and start new one");
            StopCoroutine(_RepeatingCoroutine);
        }
        if (type == 1)
        {
            _RepeatingCoroutine = StartCoroutine(NotificationPlusClear(3f, 1 /*popup*/ ));
            Debug.Log("No conflicting coroutines");
        }
        AudioSrcNotificator.clip = _NotificationAudioClips[2];
        AudioSrcNotificator.Play();

        // Enviamos notificacion a la Applicacion de control
        //SendMessageToControlApp("mr", "Scan", "ON");
        // 

    }
    // - FINISHED SCANNING
    public void EndScanningNotification()
    {
        NotificationText.SetText("Finished scanning!");
        StartCoroutine(NotificationPlusClear(2f, 1 /*popup*/ ));
        //TO-DO: Add Icon regarding notification
        // PopupImage.sprite = 
        AudioSrcNotificator.clip = _NotificationAudioClips[3];
        AudioSrcNotificator.Play();

        // Enviamos notificacion a la Applicacion de control
        //SendMessageToControlApp("mr", "Scan", "OFF");
        // 

    }

    // - INIT GAME
    public void InitGameNotificationX()
    {
        GameStatusNotification("init");
    }


    // 
    public void GameStatusNotification(string pStatusId)
    {
 
         GSNotificationBlueprint gsn = _GSNotificationsDict[pStatusId];
        if (gsn == null)
        {
            Debug.Log("No se encuentra (o no está el fichero) la notificacion con id: " + pStatusId);
        }
        else
        {
            if (bSpanish)
            {
                NotificationText.SetText(gsn.sTextSpa);
            }
            else
            {
                NotificationText.SetText(gsn.sTextEng);
            }
            PopupImage.sprite = gsn.spIcon;
            if (gsn.aAudio != null)
            {
                if(LogsOn) Debug.Log("entra al audio");

                AudioSrcNotificator.clip = gsn.aAudio;
                AudioSrcNotificator.Play();
            }

            StartCoroutine(NotificationPlusClear(gsn.fDuration, 1 /*popup*/ ));
        }
    }

    #endregion UX Notification functions on game events


    public void WordChosenNotification()
    {
        NotificationText.SetText("OK WORD/NOT OK + Explanation");
        NotificationText.SetText("OK WORD/NOT OK + Explanation");
        StartCoroutine(NotificationPlusClear(2f, 1 /*popup*/ ));
        //TO-DO: Add Icon regarding notification
        // PopupImage.sprite = 
        AudioSrcNotificator.clip = _NotificationAudioClips[1];
        AudioSrcNotificator.Play();
    }

    public void PlayAudioInstructions(string audioInstId)
    {
        AudioSrcNotificator.clip = _InstructionsAudioClips[audioInstId];
        AudioSrcNotificator.Play();

    }

    public void ChangeConnectionStatus(string statusTxt)
    {
        WSStatusText.SetText(statusTxt);
    }

    /* TO-DO
     * public void ItemVoicePickupNotification(Item i)
    {
        NotificationText.text = "YOU GRABBED " + i.ToString();
        StartCoroutine(NotificationPlusClear(delayFadeSeconds));
    }*/



}
