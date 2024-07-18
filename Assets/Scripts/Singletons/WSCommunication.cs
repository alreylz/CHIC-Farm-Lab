using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;
#if UNITY_EDITOR
    using System.Net.WebSockets;
#else
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
#endif

/* Clase para mensajes incoming */
public class CommandData
{
    public string idObject;
    public string operation;
    public string param1;
    public string param2;
    public string param3;
    public override string ToString()
    {
        return operation + "(" + param1 + "," + param2 + "," + param3 + ")";
    }
    public CommandData (string operat)
    {
        operation = operat;
    }
}

public class MonitorData
{
    public string source;
    public string dataID;
    public string value;

    public override string ToString()
    {
        return dataID + "-" + value;
    }
    public MonitorData(string tipo, string pData, string pValue)
    {
        source = tipo; //tipo "mr" se muestra en app, "debug" solo en la consola del server
        dataID   = pData;
        value  = pValue;

    }
}


public enum WSStatus
{
    CONNECTED = 1,
    DISCONNECTED = 0,
}
public enum WSURISource {
    LOCAL_FILE,
    IN_EDITOR_STRING,
    TRY_A_BUNCH //Future
}



public class WS_Status_Info
{
    public WSStatus status;
    public string additionalInfo;
    //IP / URI
    //Last command received
    //Queue status
    // Application.persistentDataPath

    public WS_Status_Info(bool status, string info)
    {
        if (status == true) this.status = WSStatus.CONNECTED;
        else this.status = WSStatus.DISCONNECTED;

        additionalInfo = info;
    }
}




public class WSCommunication : MonoBehaviour
{

    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property

    #region Singleton pattern
    private static WSCommunication _Instance;
    public static WSCommunication Instance
    {
        get { return _Instance; }
        private set { _Instance = value; }
    }
    #endregion Singleton pattern

    private readonly object wsStatusLock = new object();


    // OTHER URIs:
    // Uri u = new Uri("ws://blah blah.us-west-1.compute.amazonaws.com:3333");
    // Uri u = new Uri("ws://WebSocketApp--andreabellucci.repl.co");
    // Uri u = new Uri("ws://WebSocketApp--telmoagustinagu.repl.co");


    [Header("Configuration")]
    public WSURISource WSUriSrc;
    public string InEditorWSUri;
    public string WSUriFilename;
    Uri uri = null;
    [Range(0.1f,3f)]
    public float dequeuePeriod; // Periodo de comprobación de comandos recibidos
    public bool connectOnStartup = true; //Habilita o deshabilita la conexión al crearse una instancia 
    
    WSStatus _WSConnectionStatus = 0;
    public WSStatus WSConnectionStatus
    {
        get
        {
            lock (wsStatusLock) {
                return _WSConnectionStatus;
            }
        }
        set
        {
            lock (wsStatusLock)
            {
                _WSConnectionStatus = value;
            }
        }
    }

    #if UNITY_EDITOR
    ClientWebSocket cws = null;
    ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
    #else
    MessageWebSocket mws = null;
    #endif

    /*Last command received */
    private CommandData dataRead = null;
    /* Cola de comandos pendientes de ejecutar */
    private ConcurrentQueue<CommandData> commandsToProcess;

    /* Cola de información pendientes de enviar */
    //TO DO

    
    /* Eventos a los que se pueden suscribir otras clases */
    public Action<CommandData> OnCommandReceived;
    

    public Action<WS_Status_Info> OnWSStatusChanged;


    void Start()
    {
        if( Instance!=null &&  Instance!= this){ Destroy(this); }
        else
        {
            Instance = this;
        }
        
       // Inicializamos la cola de comandos recibidos remotamente usando el Web Socket
        commandsToProcess = new ConcurrentQueue<CommandData>();
        OnCommandReceived += EnqueueReceivedCommand;
        //Lanzamos conexión
        if (connectOnStartup)
        {
           
           switch (WSUriSrc)
            {
                case WSURISource.IN_EDITOR_STRING:
                    ConnectServerEditorURI();
                    break;
                case WSURISource.LOCAL_FILE:
                    ConnectServerReadFile(WSUriFilename);
                    break;
                case WSURISource.TRY_A_BUNCH:
                    //Future
                    break;
            }
            //Monitorizar la cola de comandos recibidos para ejecutarlos
            StartCoroutine(CheckPendingCommands_v1(dequeuePeriod));
        }
        

        

    }
    
 
    /* Conexión a servidor dado en archivo de configuración */
    public void ConnectServerReadFile(string filename)
    { 
        //-> Future (check string uri is valid) 
        uri = new Uri(ReadDataFromFile(filename));
        //Conexión de WS in separate thread
        ConnectAndListen();
    }

    /* Conexión a servidor dado en Unity Inspector */
    public void ConnectServerEditorURI()
    {
        if (InEditorWSUri == null || !InEditorWSUri.StartsWith("ws") )
        {
            Debug.LogError("WSCommunication::ConnectServerEditorURI --- In-Editor WS Uri isn't valid. WS Connection aborted.");
            return;
        }
            uri = new Uri(InEditorWSUri);
            if(uri !=null)
                ConnectAndListen();
    } 

    async void ConnectAndListen()
    {
        try
        {
            // Conecta a web socket y este método (ConnectandListen) no prosigue hasta que WSConnect Devuelva
            await WSConnect();
            // Comienza a escuchar comandos

            #if UNITY_EDITOR
                        GetStuff_Enqueue();
            #else
                        // INCOMING MESSAGES for UWP are Implemented as callbacks given by the WebSocketMessage Class
            #endif
        }
        catch(Exception e)
        {
            Debug.LogError("WSCommunication::ConnectAndListen -- "+e.Message);
        }

    }

    
    public void SendMessageWebSocket(string dataID, string value) /* Envío de mensajes para monitorizado */
    {
        MonitorData mData = new MonitorData("mr", dataID, value);
        string message = JsonUtility.ToJson(mData);
        //Send and forget
        SendMessageAsync(message);

    }
    async void SendMessageAsync(string message)/* Método auxiliar para poder llamar a SendMessageToApp*/
    {
        try
        {
            await SendMessageToApp(message);
        }
        catch(Exception e)
        {
            DebugWS("Send message Async error");
        }
    }
    
    /* Corutina de dequeue: Monitorizado de comandos remotos pendientes de ejecutar */
    public IEnumerator CheckPendingCommands_v1(float periodSeconds)
    {
        while (true)
        {
            if (commandsToProcess.Count == 0) { yield return new WaitForSeconds(periodSeconds); continue; }
            CommandData cmd = null;
            commandsToProcess?.TryDequeue(out cmd);
            if (cmd == null) { yield return new WaitForSeconds(periodSeconds ); continue; }
            Debug.Log("CheckPendingCommands_v1::Dequeued("+cmd.ToString()+")");
            ExecuteCommand(cmd.idObject, cmd.operation, cmd.param1, cmd.param2, cmd.param3);
            yield return new WaitForSecondsRealtime(periodSeconds);
        }
        yield return null;
    }

    /* Conexión a WS (previa configuración de URI) */
    async Task WSConnect()
    {
    #if UNITY_EDITOR
        DebugWS("Creating socket (Unity Editor)...");
        cws = new ClientWebSocket();
    #else //HOLOLENS
        // Creamos el socket
        DebugWS("Creating socket (Hololens)...");
        mws = new Windows.Networking.Sockets.MessageWebSocket();
        // Messages set as UTF8 encoded; Mandamos strings directamente
        mws.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Utf8; 
        mws.MessageReceived += OnWSMessageReceived;
        mws.Closed +=  WebSocket_Closed;
    #endif

        try
        {
            #if UNITY_EDITOR
            //Ejecuta de forma síncrona; hasta que no se hace esto no ocurre nada más
             await cws.ConnectAsync(uri, CancellationToken.None);
            #else
            await mws.ConnectAsync(uri).AsTask();
            #endif
            #if UNITY_EDITOR
            if (cws.State == WebSocketState.Open)
            {
                Debug.LogFormat("<color=blue> WS Communication: <b>{0}</b></color>", "Connected from Unity");          
            }
            #else
                Debug.LogFormat("<color=blue> WS Communication: <b>{0}</b></color>", "Connected to Hololens at"+ mws.Information.LocalAddress.DisplayName );
            #endif
            await SendMessageToApp("vr");
            DebugWS("Sent Hello");
            WSConnectionStatus = WSStatus.CONNECTED;
        }
        catch (Exception e)
        {
            //UXNotifications.Instance.WebSocketNotification("Exception using Connect Async"+ e.Message);
            //Debug.Log(e.StackTrace);
            WSConnectionStatus = WSStatus.DISCONNECTED;
        }
        
    }
    
    async Task SendMessageToApp(string message)
    {
        ArraySegment<byte> b = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        #if UNITY_EDITOR
        await cws.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
        #else
        await SendMessageUsingMessageWebSocketAsync(message);
        #endif
    }
    

    /* Recepción de Comandos en Unity Editor */
#if UNITY_EDITOR
    async void GetStuff()
    {
        Debug.Log("Waiting for messages");
        
        WebSocketReceiveResult r = await cws.ReceiveAsync(buf, CancellationToken.None);
        String jsonString = Encoding.UTF8.GetString(buf.Array, 0, r.Count);
        dataRead = JsonUtility.FromJson<CommandData>(jsonString);
        if (jsonString != null)
            DebugWS(" Issued " + dataRead.operation + " " + dataRead.param1 );
        ExecuteCommand(dataRead.idObject, dataRead.operation, dataRead.param1, dataRead.param2, dataRead.param3);

        GetStuff();
    }
    async void GetStuff_Enqueue()
    {
        WebSocketReceiveResult r; // Mensaje obtenido
        ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
        try
        {
             r = await cws.ReceiveAsync(buf, CancellationToken.None);
        }
        catch ( Exception e)
        {
            DebugWS("Error in ReceiveAsync (GetStuffQueued) from Unity");
            WSConnectionStatus = WSStatus.DISCONNECTED;
            return;
        }

        String jsonString = Encoding.UTF8.GetString(buf.Array, 0, r.Count);
        CommandData dataReadLocalThread = JsonUtility.FromJson<CommandData>(jsonString);
        if (jsonString != null && dataReadLocalThread != null )
        {
            //DebugWS(" Enqueue " + dataReadLocalThread.operation + " " + dataReadLocalThread.param1);
            commandsToProcess.Enqueue(dataReadLocalThread);
        }

        GetStuff_Enqueue();
    }

#endif
    public void EnqueueReceivedCommand(CommandData cmd)
    {
        commandsToProcess.Enqueue(cmd);
    }


    private void Update()
    {
        //if(WSConnectionStatus == WSStatus.CONNECTED)
          //  DebugWS("hey", true);
    }



#if UNITY_EDITOR

#else
    /* SEND DATA TO JS SERVER */
    private async Task SendMessageUsingMessageWebSocketAsync(string message)
    {

    try {
        //UXNotifications.Instance.WebSocketNotification("Sending message using MessageWebSocket: " + message);
        using (var dataWriter = new DataWriter(mws.OutputStream))
        {
            dataWriter.WriteString(message);
            await dataWriter.StoreAsync();
            dataWriter.DetachStream();
        }
    }
    catch(Exception e){
        //UXNotifications.Instance.WebSocketNotification("SendMessageUsing..." + e.Message);
        Thread.Sleep(2000);
    } 
        
    }
    
    private void OnWSMessageReceived (MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
    {
        try
        {
           
            if(sender == null){
                //UXNotifications.Instance.WebSocketNotification("SENDER IS NULL");
                //Thread.Sleep(2000);
            }
            if(args == null){
                 //UXNotifications.Instance.WebSocketNotification("ARGS ARE NULL");
                 //Thread.Sleep(2000);
            }
            else{
                //UXNotifications.Instance.WebSocketNotification(" MESSAGE TYPE: "+ args.MessageType + 
                   // "MESSAGE COMPLETE:" + args.IsMessageComplete);
                    //Thread.Sleep(2000);
           }


            using (DataReader dataReader = args.GetDataReader())
            {
                   
                if (dataReader != null) 
                {
                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string message = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                    /*if(message == null){ UXNotifications.Instance.WebSocketNotification("MESSAGE IS NULL"); Thread.Sleep(2000);}
                    else { UXNotifications.Instance.WebSocketNotification("Message received from MessageWebSocket: " + message);   Thread.Sleep(2000);}
        */            
        mws.Dispose();
                    //UXNotifications.Instance.WebSocketNotification("DISPOSED SUCCESSFULLY");// Thread.Sleep(2000);
                    //Convert to object from JSON Received
                    CommandData dataReadLocal = JsonUtility.FromJson<CommandData>(message);
                    //if(dataReadLocal!=null){ UXNotifications.Instance.WebSocketNotification("JSON CONVERSION SUCCESSFUL"); /*Thread.Sleep(1000);*/}
                    //else{ UXNotifications.Instance.WebSocketNotification("COMMAND DATA IS NULL; json is shit"); /*Thread.Sleep(1000);*/ }
                    //UXNotifications.Instance.WebSocketNotification("dataReadLocal.operation =" + dataReadLocal.operation);
                    
                    //HERE PROBLEM               
                    OnCommandReceived?.Invoke(dataReadLocal);
                    
                
                }
            }
        
        }
        catch(Exception e)
        {
    
            //UXNotifications.Instance.WebSocketNotification("Exception reading data" + e.Data + e.StackTrace); 
            

        }


    }

    private void WebSocket_Closed(Windows.Networking.Sockets.IWebSocket sender, Windows.Networking.Sockets.WebSocketClosedEventArgs args)
    {
         
        
        //UXNotifications.Instance.WebSocketNotification("CLOSED SOCKET");
        //Debug.Log("WebSocket_Closed; Code: " + args.Code + ", Reason: \"" + args.Reason + "\"");
        // Add additional code here to handle the WebSocket being closed.
        //WSConnected = false;
        //OnCommandReceived?.Invoke(new CommandData("CLOSED SOCKET"));
        //ConnectExternalServer();
        ConnectAndListen();
    }

#endif






    public void ExecuteCommand(string idObject, string operation, string param1, string param2, string param3)
    {

        switch (operation)
        {
            case "openInv":
                GameCMD.OpenInventory();
                break;
            case "closeInv":
                GameCMD.CloseInventory();
                break;
            case "invFollowUsr":
                GameCMD.InventoryFollowUser(true);
                break;
            case "invStatic":
                GameCMD.InventoryFollowUser(false);
                break;
            case "openShop":
                GameCMD.OpenShop();
                break;
            case "closeShop":
                GameCMD.CloseShop();
                break;
            case "addMoney":
                GameCMD.AddMoney(Int32.Parse(param1));
                break;
            case "subsMoney":
                GameCMD.SubstractMoney(Int32.Parse(param1));
                break;
            case "spawnPlague":
                GameCMD.SpawnPlague();
                break;
            case "killPlague":
                GameCMD.PlagueAnnihilation();
                break;
            case "startPuzzle":
                GameCMD.StartMiniGame();
                break;
            // case "endpuzzle":
                //
            case "reset":
                GameCMD.Reset();
                break;
            //case "startPlantGame":
              //  GameCMD.StartPlantGame();
                //break;
            case "playAudioInst":
                GameCMD.PlayAudioInst(param1);
                break;
            case "showHideWeather":
                GameCMD.ShowHideWeather(param1, param2);
                break;
                /*
            case "CLOSED SOCKET":
                UXNotifications.Instance.ChangeConnectionStatus("NO Connection");
                break;
            case "OPENED SOCKET":
                UXNotifications.Instance.ChangeConnectionStatus("Connected");
                break;
                */
        }


    }

    private void DebugWS(string message, bool isRemote=false)
    {
        if (LogsOn && !isRemote)
        {
            Debug.LogFormat("<color=blue> WS Communication: <b>{0}</b></color>", message);
        }
        else
        {
            SendMessageWebSocket("debug", new StringBuilder().AppendFormat("<color=blue> WS Communication: <b>{0}</b></color> ", message).ToString() );
        }
    }


    /* (MOVE SOMEWHERE ELSE) Read data from file & return content*/
    public string ReadDataFromFile(string filename)
    {
        string path = string.Format("{0}/{1}.txt", Application.persistentDataPath, filename);
        byte[] data = UnityEngine.Windows.File.ReadAllBytes(path);
        string info = Encoding.ASCII.GetString(data);

        return info;
    }




    //public void ConnectExternalServer() /* Conexión a servidor externo */
    //{
    //    WSUri = "ws://WebSocketApp--coredamnwork.repl.co/";
    //    uri = new Uri(WSUri);

    //    ConnectAndListen();
    //    //Monitorizar la cola de comandos recibidos para ejecutarlos
    //    StopAllCoroutines();
    //    StartCoroutine(CheckPendingCommands_v1(dequeuePeriod));
    //}




    #region Async understanding (DELETE FINAL) 

    /* ASYNC UNDERSTANDING */
    public async void WaitAndExecuteAsync(int seconds/*, Task toDo*/)
    {
        DateTime beforeWaitT;
        Debug.Log(beforeWaitT = DateTime.Now);
        await Task.Delay(seconds * 1000);
        f();
        g();

        //Debug.Log(" fCompletion from function start" + (fCompletion - beforeWaitT).TotalSeconds);
        //Debug.Log(" gCompletion from function start" + (gCompletion - beforeWaitT).TotalSeconds);
    }

    public async Task<DateTime> f()
    {
        await Task.Delay(1000);
        Debug.Log("f() ");
        return DateTime.Now;
    }

    public async Task<DateTime> g()
    {
        Debug.Log("g()");
        return DateTime.Now;
    }

    #endregion Async understanding (DELETE FINAL) 

}
