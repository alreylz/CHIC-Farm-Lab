using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using CHIC;

/* class PlayerSpeechManager:
 *  Responsable de manejar comandos de voz que permiten mostrar y ocultar diversos elementos 
 *  innecesarios en muchos momentos del juego como el Inventario o la tienda. 
 *  También permite reemplazar la interacción a través de Air tap, evitando frustración del usuario y haciendo
 *  el juego más accesible a personas que directamente 
 *  no pueden interaccionar de esta manera con los elementos del juego. 
 *  [!] Publica un evento al que instancias pueden suscribir métodos propios para reaccionar a keywords concretas.
 *  --------------------------------------------------------------------------------*/
public class PlayerSpeechManager : MonoBehaviour, IMixedRealitySpeechHandler
{

    #region Debug Property
    private bool _LogsOn = true;
    public bool LogsOn{
        get => _LogsOn;
        set => _LogsOn = value;
    }
    #endregion Debug Property

    public delegate void OnSpeechRecognized(string recognized);
    public static event OnSpeechRecognized onSpeechRecognition;

    #region Global Handler Subscription 
    private static IMixedRealityInputSystem mr_inputSystem;
    public static IMixedRealityInputSystem MR_InputSystem
    {
        get
        {
            /* Obtenemos una referencia al objeto que monitoriza el input en los dispositivos físicos, convirtiéndolos en abstracciones como Select o Menu y también encargado de la creación de Punteros */
            if (mr_inputSystem == null) MixedRealityServiceRegistry.TryGetService<IMixedRealityInputSystem>(out mr_inputSystem);
            return mr_inputSystem;
        }
    }
    private void OnEnable()
    {
        /*Suscripción al registro de Handlers de input globales (aquellos que no requieren que se enfoque en ellos */
        MR_InputSystem.RegisterHandler<IMixedRealitySpeechHandler>(this);
    }
    private void OnDisable()
    {
        /*De-suscripción al registro de Handlers de input globales (aquellos que no requieren que se enfoque en ellos */
        MR_InputSystem.UnregisterHandler<IMixedRealitySpeechHandler>(this);
    }
    #endregion Global Handler Subscription 
    
    
    /* Gestión global de los comandos de voz (para añadir más hay que añadirlos al profile activo de SpeechRecognition */
    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        string recognized = eventData.Command.Keyword;
        
        if(LogsOn)
            Utilities.Info(" Word detected: " + recognized.ToUpper(), this.GetType().ToString());
        
        // Invocamos los métodos suscritos a input de voz.
        onSpeechRecognition?.Invoke(recognized);

    }
}
