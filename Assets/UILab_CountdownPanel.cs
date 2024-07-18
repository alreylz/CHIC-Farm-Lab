using UnityEngine;
using TMPro;
using System.Text;

public class CountdownTimeParams : IUIPanelParams
{
    public string timeToShow { get; private set; }
    public string color { get; private set; }

    public CountdownTimeParams(string label, string color="black")
    {
        timeToShow = label;
        this.color = color;
    }

}

public class UILab_CountdownPanel : MonoBehaviour, IUIPanel<IUIPanelParams>
{
    [SerializeField]
    private Transform _PanelRoot;

    [SerializeField]
    private TextMeshPro _CountdownText;
    private IUIPanelParams _Params;
    
    public IUIPanelParams GetParams()
    {
        return _Params;
    }

    public void Hide()
    {
        _PanelRoot.gameObject.SetActive(false);
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
        if (!Init(parameters)) return false;
        return Show();

        

    }

    public bool Show()
    {
        if (_Params != null)
        {
            if (_Params is CountdownTimeParams p)
            {
                string msg = new StringBuilder().AppendFormat("<color={0}>{1}</color>", p.color, p.timeToShow).ToString();

                Debug.Log("DISPLAYING UI" + msg);
                _CountdownText.SetText(msg);
                _PanelRoot.gameObject.SetActive(true);
                return true;
            }
        }
        return false;

    }
    
}
