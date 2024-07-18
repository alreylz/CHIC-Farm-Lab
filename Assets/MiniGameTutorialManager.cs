using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//METER AQUI LOGICA DE APARICION/DESAPARICION DE OBJETOS y demás. Animaciones en la propia SM.
/// <summary>
/// Implementa la gestión de aparición y desaparición de elementos en las distintas fases del mini-juego.
/// </summary>
/// 



    //RENOMBRAME
public class MiniGameTutorialManager : MonoBehaviour
{

    [SerializeField]
    Lab_MiniGameStepNotifier _LabStepNotifier;
    [SerializeField]
    MiniGameLifecycleController _lcController;



    [Header("Reference to tutorial-only Objects ")]

    [SerializeField]
    GameObject _PreviewObjsRoot;
    [SerializeField]
    GameObject _DNA_Preview;
    [SerializeField]
    GameObject _Crispr_Preview;
    [SerializeField]
    GameObject _ModChainsPreview;
    [SerializeField]
    GameObject _3Dplant;

    [Header("References to Actual Mini-game objects")]
    [SerializeField]
    GameObject _ActualObjsRoot;
    [SerializeField]
    GameObject _ActualCrispr;
    [SerializeField]
    GameObject _ActualDNAChain;
    [SerializeField]
    GameObject _ActualSetOfModifications;
    
    [Header("DISPLAY/HIDE BUTTONS ACCORDING TO WHICH HELP SCREEN IS ON")]
    [SerializeField]
    GameObject _BtnPrev;
    [SerializeField]
    GameObject _BtnNext;
    [SerializeField]
    GameObject _BtnSkipTutorial;
    [SerializeField]
    GameObject _BtnStartGame;

    private void Awake()
    {
        _LabStepNotifier = GetComponent<Lab_MiniGameStepNotifier>();
        //Suscribimos al paso deseado con las acciones en la UI que queremos.

        //Qué hacer más en paso 1 del tutorial
        _LabStepNotifier.SubscribeToNotification("Tutorial1",
            () =>
            {
                Debug.Log("Hiding Go Back button");
                SetActiveObj(_BtnPrev, false);
                SetActiveObj(_BtnStartGame, false);

            }
            );
        _LabStepNotifier.SubscribeToNotification("Tutorial2",
          () =>
          {
              SetActiveObj(_BtnPrev, true);
              SetActiveObj(_BtnNext, true);
              //SetAllPreviewObjsVisible(false);

              //SetOnPreviewObjVisible(_3Dplant);


          }
          );

        _LabStepNotifier.SubscribeToNotification("Tutorial3",
          () =>
          {
              SetActiveObj(_BtnPrev, true);
              SetActiveObj(_BtnNext, true);
              //SetAllPreviewObjsVisible(false);

              //SetOnPreviewObjVisible(_DNA_Preview);
              
          }
          );

        _LabStepNotifier.SubscribeToNotification("Tutorial5",
          () =>
          {
              SetActiveObj(_BtnPrev, true);
              SetActiveObj(_BtnNext, true);
              //SetAllPreviewObjsVisible(false);
              //SetOnPreviewObjVisible(_Crispr_Preview);

          }
          );




        _LabStepNotifier.SubscribeToNotification("TutorialEnd",
          () =>
          {
              SetActiveObj(_BtnPrev, true);
              SetActiveObj(_BtnNext, false);
              SetActiveObj(_BtnSkipTutorial, false);
              SetActiveObj(_BtnStartGame, true);
              //SetAllPreviewObjsVisible(false);
              //SetActiveObj(_BtnStartGame, true);


          }
          );








        _lcController.OnGameStatusChanged += (status) =>
        {
            //CUANDO SE INICIA EL JUEGO CON LA CUENTA ATRÁS Y DEMÁS OCULTAMOS los botones del panel de guías.
            // y mostramos todo lo asociado al mini-juego
            if (status == PhaseMiniGame.ONGOING)
            {
                SetActiveObj(_BtnSkipTutorial, false);
                SetActiveObj(_BtnPrev, false);
                SetActiveObj(_BtnNext, false);
                SetActiveObj(_BtnStartGame, false);
                SetAllPreviewObjsVisible(false);

                //Mostrar elementos del mini-juego tal cual
                SetAllActualObjectsVisible(true);


            }
            if((status == PhaseMiniGame.MOD_COMPLETED)){
                GetComponent<MiniGameTimeoutHandler>().StopClock();
                SetAllActualObjectsVisible(false);
            }
            if ((status == PhaseMiniGame.TIMED_OUT))
            {
                SetAllActualObjectsVisible(false);
            }




        };

        ///CONTINUAR CON EL RESTO CHAVAAAAAAAAAAAAAAAAAAAL
        //HEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

    }


    //Aparición de cadena de adn


    public void SetActiveObj(GameObject obj, bool isActive)
    {
        obj.SetActive(isActive);
    }

    //Muestra un único elemento de preview seleccionado.
    public void SetOnPreviewObjVisible(GameObject go)
    {
        SetAllPreviewObjsVisible(false);

        _PreviewObjsRoot.SetActive(true);
        CHIC.Utilities.SetChildrenActiveRecursively(go.transform, true);

    }

    public void SetAllPreviewObjsVisible(bool visible)
    {
        CHIC.Utilities.SetChildrenActiveRecursively(_PreviewObjsRoot.transform, visible);
    }

    public void SetAllActualObjectsVisible(bool visible)
    {
        CHIC.Utilities.SetChildrenActiveRecursively(_ActualObjsRoot.transform, visible);
    }



}
