using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections.Generic;
using System.Collections;

/* Esta clase escucha todos los eventos de input que se generan desde el Input Provider activo (el dispositivo) */
[RequireComponent(typeof(CapsuleCollider)), RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IMixedRealityGestureHandler, IMixedRealityInputActionHandler
{

    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property
    /* Focus management; required to filter interactions and trigger/cancel actions on interactive objects */
    [Header("Focus tracking information")]
    [Range(1f,3f)]
    public float refreshFrequency = 1;
    [SerializeField]
    public GameObject ObjectOnFocus { get => objectOnFocus; }
    [SerializeField]
    private GameObject objectOnFocus = null;
    [SerializeField]
    private Interactivo prevInteractiveOnFocus = null;

    public Vector3 ImpactPoint { private set; get; }


    /* Componentes imprescindibles para la activación de triggers */
    [Header("Interaction + Force related component")]
    [SerializeField]
    private CapsuleCollider bodyCollider; /* Simulates the body of the user, so that it can interact by proximity with virtual objects */
    [SerializeField]
    private Rigidbody playerRigidBody;
    
    
    /* Referencia a componente de input asociado a la mirada */
    private IMixedRealityGazeProvider _Cached_gazeProvider; 
    private static IMixedRealityInputSystem _Mr_inputSystem;
    public static IMixedRealityInputSystem MR_InputSystem
    {
        get
        {
            /* Obtenemos una referencia al objeto que monitoriza el input en los dispositivos físicos, convirtiéndolos en abstracciones como Select o Menu y también encargado de la creación de Punteros */
            if (_Mr_inputSystem == null) MixedRealityServiceRegistry.TryGetService<IMixedRealityInputSystem>(out _Mr_inputSystem);
            return _Mr_inputSystem;
        }
    }


    private void OnEnable()
    {
        // Instruct Input System that we would like to receive all input events of type
        // IMixedRealitySourceStateHandler and IMixedRealityInputActionHandler
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityGestureHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
    }
    private void OnDisable()
    {
        // This component is being destroyed
        // Instruct the Input System to disregard us for input event handling
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityGestureHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
    }
    
    void Awake()
    {
        //Buscamos si existe un collider de cápsula como componente entre los componentes de Tipo Collider y lo asignamos correctamente (representa el cuerpo del usuario)
        if(bodyCollider == null) {
            foreach ( Collider col in gameObject.GetComponents<Collider>())
            {
                if(col is CapsuleCollider cc)
                {
                    bodyCollider = cc;
                }
            }
            if (bodyCollider == null)
            {
                bodyCollider = gameObject.AddComponent<CapsuleCollider>();
                //[PENDING] Set configuration arguments
            }
        }
        //Asignamos la referencia a un GazeProvider, que controla todo el trackeo de la cabeza en Hololens 1
        _Cached_gazeProvider = MR_InputSystem.GazeProvider;
        //Asignamos el RigidBody al objeto para que combinado con el collider pueda hacer que se ejecuten los triggers.
        if (playerRigidBody == null)
        {
            playerRigidBody = GetComponent<Rigidbody>() as Rigidbody;
            if(playerRigidBody == null ) playerRigidBody = gameObject.AddComponent(typeof(Rigidbody)) as  Rigidbody;
            playerRigidBody.isKinematic = true;
            playerRigidBody.useGravity = false; //Desactivamos la gravedad puesto que el usuario se mueve en el mundo real y ya aplica esta normal indirectamente
        }
        
        StartCoroutine(updateFocusBasedOnGaze());
        PlayerSpeechManager.onSpeechRecognition += OnSpeechCommandDetected;
    }

    
    /* Actualiza el objeto que está siendo apuntado con la mirada partiendo de la funcionalidad del GazeProvider (Hololens 1) */
    public IEnumerator updateFocusBasedOnGaze()
    {
        while (true)
        {

            /* Obtain the object being pointed at, hit by the raycast or spherecast (Depending on supported pointers by Data providers) */
            /* GazeProvider object (which implements the interface IMixedRealityGazeProvider):
            Proporciona información sobre la mirada, objetos siendo apuntados, distancias, normales...*/

            //Si el Gaze Provider.GazeTarget != null --> Significa que el rayo de raycast choca con algún objeto
            objectOnFocus = MR_InputSystem.GazeProvider.GazeTarget as GameObject;
            ImpactPoint = MR_InputSystem.GazeProvider.HitPosition;
            //Si el objeto al que apuntamos actualmente es interactivo tenemos que setear el objeto como enfocado y si el objeto anterior era diferente, notificamos que ya no le estamos apuntando
            if (objectOnFocus != null && objectOnFocus.GetComponent<Interactivo>() is Interactivo interactiveObject)
            {
                if (!interactiveObject.IsFocused) { interactiveObject.SetAsFocused(); }
                if (interactiveObject != prevInteractiveOnFocus && prevInteractiveOnFocus != null)
                {
                    prevInteractiveOnFocus.clearFocus();
                }
                prevInteractiveOnFocus = interactiveObject; // Mantengo referencia al último objeto que enfoqué para deshacer el foco cuando se deja de apuntar al objeto

                // Enviamos notificacion a la Applicacion de control
                //TZ UXNotifications.Instance.SendMessageToControlApp("mr", "objectOnFocus", interactiveObject.name);
                // 

            }
            else //Si el objeto enfocado no es interactivo o simplemente no se apunta a nada con un collider, entonces comprobamos el estado de foco y lo limpiamos en caso de que no se haya hecho antes
            {
                if (prevInteractiveOnFocus != null && prevInteractiveOnFocus.IsFocused) {
                    prevInteractiveOnFocus.clearFocus();

                    // Enviamos notificacion a la Applicacion de control
                    //TZ  UXNotifications.Instance.SendMessageToControlApp("mr","objectOnFocus", "");
                    // 

                };
            }


            yield return new WaitForSeconds(1.0f/refreshFrequency);
        }


        yield return null;

    }

    private string terminalContent = "Enter a command here" ;
    
    #region Interfaz de Manejo de gestos (Select, Hold y Manipulate) [Hololens 1 Only]

    public void OnGestureStarted(InputEventData eventData)
    {
        string gestureType = eventData.MixedRealityInputAction.Description;
        if (LogsOn) Debug.Log("<color=green> Player.cs </color> GESTURE <b>" + eventData.MixedRealityInputAction.Description + " ID:" + eventData.MixedRealityInputAction.Id + "</b> STARTED ");
    }

    void IMixedRealityGestureHandler.OnGestureUpdated(InputEventData eventData)
    {
        string gestureType = eventData.MixedRealityInputAction.Description;
        if(LogsOn) Debug.Log("<color=green> Player.cs </color> GESTURE <b>" + eventData.MixedRealityInputAction.Description + " ID:" + eventData.MixedRealityInputAction.Id + "</b> UPDATED ");
    }

    void IMixedRealityGestureHandler.OnGestureCompleted(InputEventData eventData)
    {
        string gestureType = eventData.MixedRealityInputAction.Description;
        if(LogsOn) Debug.Log("<color=green> Player.cs </color> GESTURE <b>" + eventData.MixedRealityInputAction.Description + " ID:" + eventData.MixedRealityInputAction.Id + "</b> COMPLETED ");
    }

    void IMixedRealityGestureHandler.OnGestureCanceled(InputEventData eventData)
    {
        string gestureType = eventData.MixedRealityInputAction.Description;
        if(LogsOn) Debug.Log("<color=red> Player.cs </color> GESTURE <b>" + eventData.MixedRealityInputAction.Description + " ID:" + eventData.MixedRealityInputAction.Id + "</b> CANCELED ");
    }

    #endregion  Interfaz de Manejo de gestos(Select, Hold y Manipulate )


    



    public void OnSpeechCommandDetected(string keyword)
    {

        InventoryItemManager ii = objectOnFocus?.GetComponent<InventoryItemManager>();

        if (ii != null) return;

        switch (keyword)
        {
            /*case "Coger":
            case "Grab" :
                ItemInventory.Instance.HoldItem(ii.ItemData);
                break;
            case "Soltar":
            case "Drop":
                ItemInventory.Instance.DropItem(ii.ItemData);
                break;*/
            default:
                break;

        }
    }






    /* Visualización IN-EDITOR */
    private void OnDrawGizmos()
    {
        //Draw player icon in 3d view
        Gizmos.DrawIcon(/*center*/transform.position, /*nameOfIcon*/"playerIcon.png", /*Allow Scaling?*/ true);

        Gizmos.color = Color.blue;
        //Draw line from Hololens origin to the point where the ray collides with a surface (uses gaze provider only when the player is running, otherwise it performs raycast explicitly)
        if (Application.isPlaying) { 
             Gizmos.DrawLine(_Cached_gazeProvider.GazeOrigin, _Cached_gazeProvider.HitPosition);
        }
        else
        {
            Ray ray = new Ray (/*origin*/ transform.position, /* direction*/ Vector3.forward);
            RaycastHit hitInfo;
            float maxDistance = 5.0f; /* Stop ray when it is already 5m away from us */
            Physics.Raycast(ray, out hitInfo, /*Stop Ray when d>=*/ maxDistance, /* Mask */LayerMask.GetMask("Interactivo"), /*should hit triggers?*/ QueryTriggerInteraction.Ignore);
            Gizmos.DrawLine(ray.origin, hitInfo.point);
        }
       
}

    void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
    {
        string gestureType = eventData.MixedRealityInputAction.Description;
        //Si el objeto enfocado contiene algún componente Interactivo o derivado de este (subclase)
        if (objectOnFocus?.GetComponent<Interactivo>() is Interactivo toInteractWithObject)
        {
            switch (gestureType)
            {
                case "Navigation Action": /* Navigation ( Hold + move  gesture) */
                    toInteractWithObject.Navigation();
                    break;
            }

        }
    }

    void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
    {
        string gestureType = eventData.MixedRealityInputAction.Description;
        //Si el objeto enfocado contiene algún componente Interactivo o derivado de este (subclase)
        if (objectOnFocus?.GetComponent<Interactivo>() is Interactivo toInteractWithObject)
        {
            switch (gestureType)
            {
                case "Select": /*  (Air tap gesture) --> Trigger corresponding interaction*/
                    toInteractWithObject.AirTap(); break;
                case "Hold Action": /* Hold (Maintained Air tap gesture) */
                    toInteractWithObject.Hold(); break;
              
            }
        }
        //[FUTURE] else if (objectOnFocus?.transform.parent.parent.name == "Spatial Awareness System" || objectOnFocus?.transform.parent.parent.name == "TestBench"/*|| objectOnFocus.name.StartsWith("Spatial Object Mesh")? true : false */)
        //{ //MUESTRA DE MALLA DEL MUNDO AL HACER AIRTAP

        //    switch (gestureType)
        //    {
        //        case "Select": /*  (Air tap gesture) --> Trigger corresponding interaction*/
        //            //CAMBIO DE COLOR DE LA MALLA (PENDIENTE METERLE UN FADE A INVISIBLE) [FUTURE]
        //            objectOnFocus.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.cyan);
        //            objectOnFocus.GetComponent<MeshRenderer>().material.SetColor("_WireColor", Color.white);
        //            break;

        //    }

        //}
    }


    
}