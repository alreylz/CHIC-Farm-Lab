using UnityEngine;

//Ejecuta la notificación deseada (muestra una pantalla concreta en el panel de UI del minijuego y lanza un sonido)
public class RunMiniGameStepActions : StateMachineBehaviour
{
    public string notificationToTriggerAssetName;
    private Lab_MiniGameStepNotifier notificationsController;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)

    {
        if(notificationsController==null)
            notificationsController = animator.gameObject.GetComponent<Lab_MiniGameStepNotifier>();
        
        if (notificationsController.ExecuteNotificationStep(notificationToTriggerAssetName))
        {
            Debug.Log("Executing Notification Step associated to " + notificationToTriggerAssetName);
        }
        else
        {
            Debug.LogError("NO NOTIFICATION Present in " + notificationsController.GetType().ToString()+" HAS GOT ID = " + notificationToTriggerAssetName);
        }


    }

}
