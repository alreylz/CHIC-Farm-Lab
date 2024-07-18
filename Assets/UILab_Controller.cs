using UnityEngine;
using UnityEngine.UI;

public class UILab_Controller : UIScreen
{    
    [SerializeField]
    LabController controller;

    [SerializeField]
    Image AnchorOrFollowButton;

    [SerializeField]
    Sprite _AnchoredLabSprite;
    [SerializeField]
    Sprite _FollowLabSprite;

    [SerializeField]
    GameObject _GoPrevScreenButton;


    private void Awake()
    {

        if (controller == null)
        {
            controller  = GetComponent<LabController>();
        }
        if (_GoPrevScreenButton == null)
        {
            Debug.LogError(" Remember to set the reference to the 'goBack' button for the UILab_Controller to manage it properly");
        }


            //Actualización de botón anchor o follow
            controller.OnLabModeIsUpdated += (active) =>
        {
            if (active)
            {
                AnchorOrFollowButton.sprite = _FollowLabSprite;
            }
            else
            {
                AnchorOrFollowButton.sprite = _AnchoredLabSprite;
            }
        };
        //Ocultación o muestra del laboratorio al completo (el menú flotante)
        controller.OnLabActiveIsUpdated += (active) =>
        {
            if (active) { Show(); }
            else { Hide(); }
        };

        //Muestra u ocultación de botón de Atrás según el contexto.
        controller.OnUpdateGoBackButtonDisplayState += (active) => {
            _GoPrevScreenButton.SetActive(active);
        };




    }



    public override void Show()
    {
        base.Show();

    }

    public override void Show<T>(T screenParams)
    {
        throw new System.NotImplementedException();
    }



}
