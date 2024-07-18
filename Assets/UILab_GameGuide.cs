using UnityEngine;
using UnityEngine.UI;



public class BasicHelpScreenParams : IUIPanelParams
{
    public Sprite BackgroundImage { get; private set; }
    public Sprite ForegroundImage { get; private set; }

    public BasicHelpScreenParams(Sprite bg, Sprite fg=null)
    {
        BackgroundImage = bg;
        ForegroundImage = fg;
    }
}



//ESTE ES UN PANEL ÚNICO PERO DINÁMICO ( Se le pasa lo que tiene que mostrar por parámetros, teniendo varios tipos delayouts diferentes pero estándar)
public class UILab_GameGuide : MonoBehaviour, IUIPanel<IUIPanelParams>
{

    IUIPanelParams _Params;

    [SerializeField]
    private Transform rootCanvas;

    [Header("References to Layout Components in the scene")]
    public Image backgroundPanel;
    public Image foregroundImage;


    public IUIPanelParams GetParams()
    {
        return _Params;
    }

    public void Hide()
    {
        rootCanvas.gameObject.SetActive(false);
    }

    public bool Init(IUIPanelParams parameters)
    {
        if (parameters != null)
        {
            _Params = parameters;
            return true;
        }
        return false;
    }

    public bool Show(IUIPanelParams parameters)
    {


        Init(parameters);
       

        if ( parameters is BasicHelpScreenParams inputParams)
        {
            backgroundPanel.sprite = inputParams.BackgroundImage;
            if(inputParams.ForegroundImage != null)
                foregroundImage.sprite = inputParams.ForegroundImage;
        }


        rootCanvas.gameObject.SetActive(true);
        return true;

    }

    public bool Show()
    {
        if (Init(GetParams()))
        {
            return Show(_Params);
        }
        Debug.LogError("UILab Guide couldn't be shown because no params were passed to it: "+ GetParams());
        return false;
    }

}
