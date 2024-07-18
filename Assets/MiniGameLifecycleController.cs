using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;


public enum PhaseMiniGame
{
    TUTORIAL,
    ONGOING,
    MOD_COMPLETED,
    TIMED_OUT
}


//Controla el estado y la ejecución de transiciones en la máquina de estados
public class MiniGameLifecycleController :  StateMachineMonitor
{
    public Animator LabGameSM; // Máquina de estados para controlar las guías para el tutorial de la realización de modificaciones genéticas.
    public DNAChainStatusMonitor DNAInteractionMonitor; // Monitorización de interacción con el propio ADN (corte y reemplazo).

    public int CurrentStateHash { get => LabGameSM.GetCurrentAnimatorStateInfo(0).fullPathHash; }
    
    [Header("Phase monitoring")]
    [SerializeField]
    private PhaseMiniGame _MiniGamePhase;
    public PhaseMiniGame MiniGamePhase
    {
        get => _MiniGamePhase;
        set {
            if (_MiniGamePhase != value)
            {
                _MiniGamePhase = value; OnGameStatusChanged?.Invoke(_MiniGamePhase);
            }
        }
    } // --> Cambiado desde scripts dentro de la propia máquina de estados, ejecutando 

    [Header("Public subscription events")]
    public UnityAction<PhaseMiniGame> OnGameStatusChanged;
    //--> Ejecutado cada vez que hay un cambio de fase en el minijuego (e.g. Tutorial)
    public UnityAction<int> OnModificationApplied;
    //--> Ejecutado cuando el usuario ha completado el juego seleccionando una cadena de ADN para reemplazar a la original (argumento es el índice de la modificación efectuada)

    private void Awake()
    {
        DNAInteractionMonitor.OnAppliedModification += (indexPlant) => UserCompletedPlantMod(); // -> Suscribe al componente que controla la interacción del usuario, esperando a que este complete el juego.
        DNAInteractionMonitor.OnAppliedModification += (indexPlant) => OnModificationApplied?.Invoke(indexPlant) ; // -> Propagamos la detección de juego completado y el índice de modificación.

        DNAInteractionMonitor.OnCrisprUsed += UserCompletedCrisprCut;

       /* OnSMStateEnter += (str) =>
        {
            if (str == "Pepe")
                Debug.Log("ACTUALMENTE SM EN ESTADO PEPE ");
        };*/
    }

    #region Manejo de transiciones de State Machine del Mini-juego del Lab
    public void NextStateGuide()
    {
        LabGameSM.SetTrigger("NextIntroScreen");
    }

    public void PrevStateGuide()
    {
        LabGameSM.SetTrigger("PrevIntroScreen");
    }

    public void SkipTutorial()
    {
        LabGameSM.SetTrigger("SkipTutorial");
    }

    public void UserCompletedCrisprCut()
    {
        LabGameSM.SetTrigger("PerformedCut");
    }

    public void UserCompletedPlantMod()
    {
        LabGameSM.SetTrigger("ModificationDone");
    }
    public void SetTimedOut()
    {
        LabGameSM.SetTrigger("Timeout");
    }

    public void StartGame()
    {
        LabGameSM.SetTrigger("StartGame");
    }

    #endregion  Manejo de transiciones de State Machine del Mini-juego del Lab


}

