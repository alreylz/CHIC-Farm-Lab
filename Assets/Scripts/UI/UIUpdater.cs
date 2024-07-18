using UnityEngine;
using System;
using TMPro;

/* A ELIMINAR -->  HAY QUE REEMPLAZAR LA FUNCIONALIDAD */


/* Se encarga de actualizar el HUD para que el usuario vea el estado global del juego */
[RequireComponent(typeof(CanvasGroup))]
public class UIUpdater : MonoBehaviour
{
    public Color textColor;

    private CanvasGroup _CanvasGroup;
    
    public TextMeshProUGUI CoinText;
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI HourText;


    private void Start()
    {
        GameManager _GlobalData = GameManager.Instance;
        _GlobalData.subscribeToChanges(onMoneyChanged);
        _GlobalData.subscribeToChanges(onTimeChanged);

        PlayerSpeechManager.onSpeechRecognition += onSpeechActionRecognized;
    }

    private void onMoneyChanged( GameManager obj)
    {
        CoinText.text = obj.GetStringMoney;
    }
    
    private void onTimeChanged (GameManager obj)
    {
        DayText.text = " " +obj.CurrentDay+((obj.CurrentDay>1)?" DAYS":"DAY") ;
    }


    /* onSpeechActionRecognized: Handler encargado de reaccionar a comandos de voz*/
    void onSpeechActionRecognized(string recognized)
    {
        switch (recognized)
        {
            case "Ocultar Estado":
            case "Hide Status":
                HideUI();
                break;
            case "Show Status":
            case "Mostrar Estado":
                ShowUI();
                break;
        }
    }

    private void HideUI()
    {
        _CanvasGroup.interactable = false;
        _CanvasGroup.alpha = 0;
        _CanvasGroup.blocksRaycasts = false;
    }
    private void ShowUI()
    {
        _CanvasGroup.interactable = true;
        _CanvasGroup.alpha = 1;
        _CanvasGroup.blocksRaycasts = true;
    }






}
