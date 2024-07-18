using UnityEngine;

public class SMUpdateMiniGameStatus : StateMachineBehaviour
{
    [SerializeField]
    private PhaseMiniGame _PhaseToNotify;
    private MiniGameLifecycleController _GameCycleComponent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _GameCycleComponent = animator.GetComponent<MiniGameLifecycleController>();
        if(_GameCycleComponent == null)
        {
            Debug.LogError("NO SE ESTÁ ACTUALIZANDO EL LIFECYCLE DEL JUEGO");
            return;
        }
        _GameCycleComponent.MiniGamePhase = _PhaseToNotify;
        
    }


}
