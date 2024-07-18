using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase que aplica la configuración inicial del minijuego, permitiendo propagar la información de la planta de partida y las modificaciones concretas a realizar.
/// Permite también definir si se mostrará el tutorial del minijuego o se pasará directamente al juego de modificación.
/// </summary>

[RequireComponent(typeof(MiniGameLifecycleController),typeof(MiniGameTimeoutHandler))]
public class MiniGameConfiguration : MonoBehaviour
{
    [SerializeField]
    private PlantVariety _SrcVariety;
    public PlantVariety SrcVariety { get => _SrcVariety; set=> _SrcVariety = value; }

    [SerializeField]
    private PlantModification[] _AvailableModsToPerform;
    public PlantModification[] AvailableModsToPerform
    {
        get => _AvailableModsToPerform;
        set => _AvailableModsToPerform = value;
    }

    [SerializeField]
    private float _TimeLimit;
    public float TimeLimit { get => _TimeLimit; }
    
    [SerializeField]
    private bool _ShowTutorial;
    public bool ShowTutorial { get  => _ShowTutorial; set=> _ShowTutorial = value; }

    public bool SetTimeLimit(float timeLimitSeconds)
    {
        if (timeLimitSeconds > 0)
        {
            _TimeLimit = timeLimitSeconds;
            return true;
        }
        return false;
    }

    
    public bool ApplyConfiguration(PlantVariety src, PlantModification [] modsCanApply, float time, bool showTut=true)
    {
        SrcVariety = src;
        AvailableModsToPerform = modsCanApply;
        SetTimeLimit(time);
        _ShowTutorial = showTut;
        return ApplyConfiguration();
    }
    private bool ApplyConfiguration()
    {
        if(SrcVariety != null && AvailableModsToPerform.Length>0 && _TimeLimit > 0)
        {
            MiniGameTimeoutHandler timerComponent = GetComponent<MiniGameTimeoutHandler>();
            // - Preparar cuenta atrás
            timerComponent.CountDownT = _TimeLimit;

            if (!_ShowTutorial)
            {
                MiniGameLifecycleController lcController = GetComponent<MiniGameLifecycleController>();
                lcController.SkipTutorial();// - Preparar scripts asociado a cada pieza de modificación.
            }


            return true;
        }
        return false;
    }




}
