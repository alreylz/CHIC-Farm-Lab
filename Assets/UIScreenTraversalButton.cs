using UnityEngine;
using UnityEngine.Events;

//Genera un evento que hace forward de un on-click notmal para indicar a qué pantalla hay que dirigirse.
public class UIScreenTraversalButton : MonoBehaviour
{
    [SerializeField]
    UIScreen _destScreen; //--> Ref. a script de pantalla a la que se quiere ir.    
    public UIScreen DestinationScreen { get => _destScreen; }

    public void GenOnClick() {
        OnClick?.Invoke(_destScreen);
    }

    public UnityAction<UIScreen> OnClick; 


}
