using System.Text;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnTimeout : UnityEvent { };
[System.Serializable]
public class OnTimeIntervalPending : UnityEvent<string> { };//Pasa el tiempo que queda en formato X:XX
public class MiniGameTimeoutHandler : MonoBehaviour
{
    [SerializeField]
    float countDownT = -1f; // --> Tiempo a contar
    public float CountDownT { set => countDownT = value; get => countDownT; }

    //Timers para obtener tiempos desde la última notificación en incrementos de  
    float t0;
    float t1;
    
    //float elapsed;
    float elapsedCumulative;


    [Header("Monitoring")]
    [SerializeField]
    bool active;
    [SerializeField]
    float pendingTime;
    [SerializeField]
    float lastNotifiedPendingTime;

    [Header("Event configuration")]
    [SerializeField]
    public uint NotifyEveryT = 1;

    

    public OnTimeout onTimeout;
    public OnTimeIntervalPending onUpdatePendingTime;

    [SerializeField]
    private UILab_CountdownPanel _DisplayPanel; // Panel de despliegue de cuenta atrás


    private void Start()
    {
        if (_DisplayPanel == null)
        {
            _DisplayPanel = CHIC.Utilities.GetFirstComponentOfTypeInChildren<UILab_CountdownPanel>(transform);
        }
        
        MiniGameLifecycleController lcController = null;

        lcController = GetComponent<MiniGameLifecycleController>();
        if(lcController!=null)
            lcController.OnGameStatusChanged += (phase) => { if (phase == PhaseMiniGame.ONGOING) Activate(); };
        else
        {
            Debug.LogError("lcController is null");
        }
    }


    public void Activate() {
        
        if (!active)
        {
            t0 = Time.realtimeSinceStartup; // --> Timer desde el que contar hasta el tiempo fijado.
            elapsedCumulative = 0; // --> Tiempo total pasado desde activación
            pendingTime = countDownT; // --> Mismo concepto pero tiempo pendiente para timeout
            lastNotifiedPendingTime = pendingTime;
            active = true;
            
            Debug.Log("TIME TRIAL STARTED : " + countDownT + " seconds left");
        }
        else
        {
            Debug.LogWarning("THERE IS A TIME TRIAL ALREADY IN PROGRESS");
        }
        if (countDownT < 0)
        {
            Debug.LogError("CANT START TIME TRIAL IF COUNTDOWN AMOUNT (countDownT) IS NOT SET");
        }
    }

    public void StopClock()
    {
        active = false;
    }

    private void Update()
    {
        if (active)
        {
            t1 = Time.realtimeSinceStartup; //Segundo timer, para controlar el tiempo desde que se inició el timer.

            elapsedCumulative = t1 - t0;
            pendingTime = countDownT - elapsedCumulative;

            Debug.Log("PENDING TIME:" +pendingTime);

            //Comprobación de Timeout
            if (elapsedCumulative > countDownT)
            {
                onTimeout?.Invoke();
                active = false;
            }

            //Evento de paso de X tiempo
            if ( (int) lastNotifiedPendingTime - (int) pendingTime == (int) NotifyEveryT)
            {

                lastNotifiedPendingTime = pendingTime;//Valor de último punto en la cuenta atrás que notificamos

                int mins =(int)(pendingTime / 60);
                int secs = (int) (pendingTime % 60 );

                string timeStr =  new StringBuilder().AppendFormat("{0}:{1}", mins.ToString().PadLeft(2,'0'), secs.ToString().PadLeft(2,'0')).ToString();
                Debug.Log("TIME PENDING --> " + timeStr);

                onUpdatePendingTime?.Invoke(timeStr);
                _DisplayPanel.Show(new CountdownTimeParams(timeStr));
                
            }

            
        }
    }


}
