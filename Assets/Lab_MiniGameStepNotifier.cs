using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Clase encargada de controlar las guías para el mini-juego de modificación genética.
public class Lab_MiniGameStepNotifier : MonoBehaviour
{
    public List<SO_LabStepNotifications> ScreenNotificationData; //Lista de información principal asociada a una pantalla del panel de ayuda
                     
    [Header("Componentes que hacen posibles las notificaciones UI & Audio")]
    public UILab_GameGuide UiPanelDisplayer;
    public AudioSource GuidePanelAudioSource;
    
    private Dictionary<string, Action> ActionsAssociatedToNotification;//  ( Nombre del SO de notificación (e.g. Presentación.asset) , Lista de funciones a ejecutar )

    public Action<string> OnStepNotificationRequested;


    private void Awake()
    {
        InitMapper();
        //Debug.Log("DIC:" + ActionsAssociatedToNotification.Keys.ToString());
    }

    // Populates the <Notification/Step name , Set of actions data>
    public void InitMapper()
    {
        ActionsAssociatedToNotification = new Dictionary<string, Action>();

        //Recorro la información de todas las pantallas que se quieren incluir en el mini-juego.
        foreach (var ithScreenData in ScreenNotificationData)
        {
            Action notificationCallBacks = null;

            //1. Creamos objeto con parámetros para mostrar cierta información en el panel de ayuda del mini-juego.
            // De momento la información se limita a una simple imagen de fondo que contiene todo lo que queremos mostrar.
            BasicHelpScreenParams uiPanelParams = new BasicHelpScreenParams(ithScreenData.UIBackground  );

            //2. Añado secuencias de acciones para cada etapa del mini-juego (e.g. Mostrar pantalla y reproducir audio)
            notificationCallBacks += () => { UiPanelDisplayer.Show(uiPanelParams); }; //--> muestra pantalla correspondiente (UIPanel Displayer se encarga del refresco del panel como tal)
            notificationCallBacks += () =>
            {
                GuidePanelAudioSource.clip = ithScreenData.NotificationClip;
                GuidePanelAudioSource.loop = false;
                GuidePanelAudioSource.Play();
            }; //---> Gestiona reproducción de notificación vía audio
            
            ActionsAssociatedToNotification.Add(ithScreenData.name, notificationCallBacks);

        }
        
    }

    //Obtiene el conjunto de acciones a realizar para cada paso del mini-juego
    public bool ExecuteNotificationStep(string AssetName)
    {
        Action actionsToPerformOnNotification;

        if (ActionsAssociatedToNotification.TryGetValue(AssetName, out actionsToPerformOnNotification))
        {
            actionsToPerformOnNotification?.Invoke();
            OnStepNotificationRequested?.Invoke(AssetName);
            return true;
        }
        else
            return false;
    }
    
    //Para añadir acciones asociadas a las diferentes fases del mini-juego desde otras clases.

    public void SubscribeToNotification(string notificationName,Action callbackToAdd)
    {
        
        OnStepNotificationRequested += (notifStr) =>
        {
            if(notificationName == notifStr)
            {
                callbackToAdd();
            }
        }
        ;
            
    }
    



}
