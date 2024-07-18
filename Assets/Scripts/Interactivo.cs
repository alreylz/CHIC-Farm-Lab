using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
using CHIC;

//[RequireComponent(typeof(Collider)),RequireComponent(typeof(SphereCollider))]

/* Creamos un Unity Event que no tenga por defecto 0 parámetros;
 * UnityEvent es una clase abstracta que permite definir delegados persistentes con un n�mero y tipo de par�metros dado por 
 *  --> UnityEvent<TYPE_HERE>
 * (tipos que contienen punteros a funciones y que en este caso no se borran al cerrar Unity)
 * y pueden poblarse desde el Inspector
 * */
[System.Serializable]
public class InteractivoInspectorPersistentEvent : UnityEvent { };

public class Interactivo : MonoBehaviour
{

    /* [FLAG] usesInteractionRange: Activa o desactiva el resto de opciones asociadas a permitir la
     * interacci�n exclusivamente cuando uno se haya situado suficientemente cerca del objeto */
    [Header("Interaction type (Configuration)")]
    public bool isDistanceBasedInteraction = false;
    [Header("Object interaction status")]
    [SerializeField]
    #region Propiedades para ver estado del objeto desde el exterior
    protected bool isFocused = false;
    public bool IsFocused
    {
        get { return isFocused; }
    }
    #endregion Focus & Interaction status
    [SerializeField]
    protected bool playerInInteractionRange = false;

    [Header("Debug display configuration + Gizmos")]
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property
    public Color gizmosColor;
    
    [Header("Fine Grain Configuration")]
    [SerializeField]
    public SphereCollider interactionRangeCollider;
    /* Permite mover la posici�n asociada al radio de interacci�n para este objeto (radio definido por una esfera) */

    private enum interactionRadiusOrigin { center, forwards, backwards, right, left };

    #region Esfera de interacci�n (Interaction range code)

    [Tooltip("Indica en qué direcci�n respecto al centro del objeto interactivo se va a colocar el centro del radio de interacci�n.")]
    /* offsetDirection: Por defecto el radio de interaci�n se coloca en el centro; es accesible desde todas direcciones*/
    //[SerializeField]
    private interactionRadiusOrigin offsetDirection = interactionRadiusOrigin.center;

    public float interactionRadius = 0f;
    /* interactionRadius: Usado para limitar la distancia a la que hay que estar para poder interactuar con el objeto 
    /* respecto al centro elegido para la esfera de interacci�n) */

    public float interactionCenterOffset = 0;
    /* Offset respecto al centro del objeto donde se construye una esfera dentro de la 
    /* cual hay que encontrarse para poder interactuar con el objeto. */

    [SerializeField]
    protected Vector3 interactionSphereCenter;
    [SerializeField]
    protected Vector3 normalisedDirectionVector; // (0,0,0) if center, 1 or -1 in vector component otherwise
    #endregion Esfera de interacci�n (Interaction range code)

    [Header("Eventos de interación ")]
    // In Inspector Events
    public InteractivoInspectorPersistentEvent onAirTap;
    public InteractivoInspectorPersistentEvent onHold;
    public InteractivoInspectorPersistentEvent onNavigation;
    public InteractivoInspectorPersistentEvent onGaze;
    public InteractivoInspectorPersistentEvent onGazeOut;
    /* Eventos a los que se pueden suscribir otras clases en tiempo de ejecuci�n */
    public Action OnInteraction;
    public Action OnAirTap;
    public Action OnHold;
    public Action OnNavigation;
    public Action OnGaze;
    public Action OnGazeOut;

    // [PENDING] Constructor
    public Interactivo()
    {

    }

    /* Métodos para definir si el objeto está siendo enfocado por el usuario */
    public void SetAsFocused()
    {
        isFocused = true;
        onGaze?.Invoke();
        OnGaze?.Invoke();
    }
    public void clearFocus()
    {
        isFocused = false;
        onGazeOut?.Invoke();
        OnGazeOut?.Invoke();
    }
    #region Interaction by proximity (Updating proximity flag)
    private void OnTriggerEnter(Collider colliding)
    {
        if (isDistanceBasedInteraction && colliding.gameObject.GetComponent<Player>())
        {
            if (LogsOn)
                Utilities.Info("Player entered " + gameObject.name + "interaction range",  this.GetType().ToString(), this.gameObject, funcName: "OnTriggerEnter");
            playerInInteractionRange = true;
        }
    }
    private void OnTriggerExit(Collider colliding)
    {
        if (isDistanceBasedInteraction && colliding.gameObject.GetComponent<Player>())
        {
            if (LogsOn) Utilities.Info("Player left " + gameObject.name + "interaction range", this.GetType().ToString(), this.gameObject, funcName: "OnTriggerEnter");
            playerInInteractionRange = false;
        }
    }
    #endregion Interaction by proximity


    public virtual void AirTap()
    {
        if (isFocused && playerInInteractionRange || isFocused && !isDistanceBasedInteraction)
        {
            OnAirTap?.Invoke(); // Runtime configured methods
            onAirTap?.Invoke(); // In inspector configured methods
            OnInteraction?.Invoke(); // General interaction method
        }
    }
    public virtual void Hold()
    {
        if (isFocused && playerInInteractionRange || isFocused && !isDistanceBasedInteraction)
        {
            OnHold?.Invoke(); // Runtime configured methods
            onHold?.Invoke(); // In inspector configured methods
            OnInteraction?.Invoke(); // General interaction method
        }
    }
    public virtual void Navigation()
    {
        if (isFocused && playerInInteractionRange || isFocused && !isDistanceBasedInteraction)
        {
            OnNavigation?.Invoke(); // Runtime configured methods
            onNavigation?.Invoke(); // In inspector configured methods
            OnInteraction?.Invoke(); // General interaction method
        }
    }






    private void InspectorConfigurationApply()
    {
        /* De acuerdo a la direcci�n relativa al objeto seleccionada (e.g. forward, center...) definimos el centro de la 
         * esfera que marca el �rea dentro de la cual este objeto es interactivo, definiendo el centro de la esfera con el offset marcado*/
        switch (offsetDirection)
        {
            case interactionRadiusOrigin.center:
                interactionSphereCenter = Vector3.zero;
                normalisedDirectionVector = Vector3.zero;
                break;
            case interactionRadiusOrigin.forwards:
                interactionSphereCenter = transform.position + interactionCenterOffset * transform.forward;
                normalisedDirectionVector = transform.forward.normalized;
                break;
            case interactionRadiusOrigin.backwards:
                interactionSphereCenter = transform.position - interactionCenterOffset * transform.forward;
                normalisedDirectionVector = -transform.forward.normalized;
                break;
            case interactionRadiusOrigin.right:
                interactionSphereCenter = transform.position + interactionCenterOffset * transform.right;
                normalisedDirectionVector = transform.right.normalized;
                break;
            case interactionRadiusOrigin.left:
                interactionSphereCenter = transform.position - interactionCenterOffset * transform.right;
                normalisedDirectionVector = -transform.right.normalized;
                break;
        }
    }


    /// <summary>
    /// Asigna o crea un componente Sphere collider en el objeto interactivo en el caso de que no exista y le da valores por defecto
    /// </summary>
    private void setupInteractionCollider(bool isPlayingcall = true)
    {

        /* Configuramos el collider esf�rico que existe en este componente (o lo creamos), 
        * de forma que lo centremos en la posici�n marcada con los Gizmos de Unity; solo hacemos esto si el no est� seteada la referencia al trigger */
        if (interactionRangeCollider == null)
        {
            foreach (Collider colliderIt in GetComponents<Collider>())
            {
                //Debug.Log("Collider Found:" + colliderIt.name);
                //Si uno de los colliders que encuentra es esf�rico, asignamos la referencia al atributo de esta clase, lo configuramos y salimos del bucle.
                if (colliderIt is SphereCollider _interactionRangeCollider /*Class attribute*/ )
                {
                    //Debug.Log("<b> SphereCollider found for: " + colliderIt.name + "</b>  ");
                    _interactionRangeCollider.name = "(Interactivo) Interaction Range Trigger";
                    _interactionRangeCollider.radius = 0f;//interactionRadius * ((transform.localScale.x)<1 ? (1/transform.localScale.x) : 1.0f);
                    _interactionRangeCollider.center = interactionSphereCenter;
                    _interactionRangeCollider.isTrigger = true; //Configuramos para que al entrar y salir en el volumen del collider se ejecuten OnTriggerEnter y OnTriggerExit
                    this.interactionRangeCollider = _interactionRangeCollider;
                    return;
                }
            }
            //Si no se encuentra un collider esf�rico se crea uno si estamos en playMode
            if (!isPlayingcall) { Debug.LogWarning(gameObject.name + " Needs a SphereCollider to work properly. Create one or it will be created automatically in Play Mode"); return; }
            interactionRangeCollider = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider; //Crea un collider esf�rico o asigna a null si falla
            interactionRangeCollider.radius = interactionRadius * ((transform.localScale.x) < 1 ? (1 / transform.localScale.x) : 1.0f);
            interactionRangeCollider.center = interactionSphereCenter;
            interactionRangeCollider.isTrigger = true; //Configuramos para que al entrar y salir en el volumen del collider se ejecuten OnTriggerEnter y OnTriggerExit
        }
        else
        {
            //UPDATE RADIUS AND STUFF; only possible from this component
            interactionRangeCollider.radius = interactionRadius * ((transform.localScale.x) < 1 ? (1 / transform.localScale.x) : 1.0f);
            interactionRangeCollider.center = interactionSphereCenter;
            interactionRangeCollider.isTrigger = true; //Configuramos para que al entrar y salir en el volumen del collider se ejecuten OnTriggerEnter y OnTriggerExit
            return;
        }
    }


    public virtual void Awake()
    {
        //Aseguramos la existencia de un collider esférico que controle si estamos dentro del rango de interacci�n, notificando que esto no ocurre en gameplay
        //setupInteractionCollider();
    }



    private void OnValidate()
    {
        InspectorConfigurationApply();
    }




}