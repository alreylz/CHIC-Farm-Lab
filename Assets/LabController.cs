using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

/// <summary>
/// Controla el estado del laboratorio a grandes rasgos, si está activo,  su configuración general y provee de la lógica principal
/// </summary>
public class LabController : MonoBehaviour
{

    [SerializeField]
    bool _IsActive;
    [SerializeField]
    bool _IsFollowingUsr;
    [SerializeField]
    List<UIScreen> _ListInnerLvls; //--> Lista de subniveles de este menú (laboratorio), que a su vez contienen los paneles y dialogs que muestran contenido como tal.

    [SerializeField]
    UIScreen _ActiveScreen;
    [SerializeField]
    UIScreen _PrevScreen;



    public Action<UIScreen> OnActiveScreenUpdated; //--> Cuando se actualiza la pantalla
    //Cuando se desactiva, generar desactivación del panel del lab al completo, junto con la desactivación del componente Bounding Box
    public Action<bool> OnLabActiveIsUpdated;
    // Cuando se hace posible que siga al usuario, se genera otro evento para notificar al componente de ui.
    public Action<bool> OnLabModeIsUpdated;
    public Action<bool> OnUpdateGoBackButtonDisplayState; // --> Control de si tiene que estar visible un botón de "Atrás" 
                                                    // o no según si la pantalla anterior es la pantalla en la que estamos. 
    
    [Header("Components that allow scaling and rotating on demand")]
    [SerializeField]
    BoundingBox _BboxWholeLab;

    [Header("Components that enable user follow functionality")]
    [SerializeField]
    RadialView _UsrFollower;
    [SerializeField]
    SolverHandler _UsrFollowerSolvHandler;

    private void Awake()
    {

        _ListInnerLvls = CHIC.Utilities.GetAllComponentsOfTypeInChildren<UIScreen>(transform);

        Player playerData = FindObjectOfType<Player>(); // Cojo objeto jugador para ver a quién está enfocando.
        //Suscripción a comando de voz
        PlayerSpeechManager.onSpeechRecognition += (word) => { if (word == "Open Lab") SetActiveLab(true); };
        PlayerSpeechManager.onSpeechRecognition += (word) => { if (word == "Close Lab") SetActiveLab(false); };

        //Follow 
        PlayerSpeechManager.onSpeechRecognition += (word) =>
        {
            if (word == "Follow Me" && playerData.ObjectOnFocus == gameObject)
            {
                SetFollowingUsr(true);
            }
        };
        //Don't Follow
        PlayerSpeechManager.onSpeechRecognition += (word) =>
        {
            if (word == "Don't follow me" && playerData.ObjectOnFocus == gameObject)
            {
                SetFollowingUsr(false);
            }
        };

        _ActiveScreen = _ListInnerLvls[1]; // -> Por defecto es la pantalla root
        _PrevScreen = _ActiveScreen;
        SetActiveScreen(_ListInnerLvls[1]);

        //SUSCRIPCIÓN A TODOS LOS BOTONES QUE EXISTAN DE CAMBIO DE PANTALLA
        List<UIScreenTraversalButton> allButtons = CHIC.Utilities.GetAllComponentsOfTypeInChildren<UIScreenTraversalButton>(transform);
        foreach(var btn in allButtons){

            btn.OnClick +=(screen) => SetActiveScreen(screen);
        }

        OnLabActiveIsUpdated += (active) =>_BboxWholeLab.enabled = active; //Activación/Desactivación de ENable/disable
        OnLabModeIsUpdated += (active) =>
        {
            _UsrFollower.enabled = active;
            _UsrFollowerSolvHandler.enabled = active;
        };
        // Activación /desactivación de follow user

        CHIC.Utilities.DebugList(_ListInnerLvls,this.GetType().ToString());

        //Configuración por defecto de inventario
        SetFollowingUsr(true); //--> Lab sigue a usuario
        



    }

      
    //Cuando cambia la pantalla activa --> Mostrar la nueva
    public bool SetActiveScreen(UIScreen uiScreen)
    {

        if (_ListInnerLvls.Contains(uiScreen))
        {
            _PrevScreen = _ActiveScreen;
            _PrevScreen.Hide();
            _ActiveScreen = uiScreen;
            _ActiveScreen.Show();
            OnActiveScreenUpdated?.Invoke(_ActiveScreen);

            //Evento de si existe pantalla previa a la que volver o no.
            if (_ActiveScreen == _ListInnerLvls[1])
                OnUpdateGoBackButtonDisplayState?.Invoke(false);
            else
                OnUpdateGoBackButtonDisplayState?.Invoke(true);

            return true;
        }
        return false;


    }


    public void TryGoBackScreen()
    {
        if (!GoBackScreen())
        {
            Debug.LogWarning("CANT GO BACK MORE SCREENS");
        }
    }


    public void SetNOTMode()
    {
        SetFollowingUsr(!_IsFollowingUsr,false);

    }

    //Permitir volver atrás.
    public bool GoBackScreen()
    {
        if (_PrevScreen!= null &&  _ListInnerLvls.Contains(_PrevScreen))
        {
            return SetActiveScreen(_PrevScreen);
        }
        return false;
    }
    
    public void SetFollowingUsr(bool isFollowing, bool triggerEvents = true)
    {
        _IsFollowingUsr = isFollowing;
        OnLabModeIsUpdated?.Invoke(_IsFollowingUsr);


    }
    public void SetActiveLab(bool isActive, bool triggerEvents = true)
    {
        _IsActive = isActive;
        OnLabActiveIsUpdated?.Invoke(_IsActive);
    }


    //QUEDA ASIGNAR LOS DIFERENTES BOTONES Y HACER UN MONOBEHAV QUE PASE AL 
    //NIVEL SUPERIOR LA REFERENCIA A LA PANTALLA A LA QUE PERTENECE PARA SETEAR LA PANTALLA ASOCIADA


    







}
