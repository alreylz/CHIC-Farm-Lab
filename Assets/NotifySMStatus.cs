using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnSMStateEntered : UnityEvent<string> { };
public abstract class StateMachineMonitor : MonoBehaviour
{
    [SerializeField]
    private string _CurrentStateName = "Not used yet";
    public string CurrentStateName { get => _CurrentStateName; protected set { } }
    public void UpdateCurrentStateName(string value) {
        _CurrentStateName = value;
        OnSMStateEnter?.Invoke(_CurrentStateName);
    }
    
    public UnityAction<string> OnSMStateEnter;
    
}

public class NotifySMStatus : StateMachineBehaviour
{

    public string stateNameToPropagate = "Default State";

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StateMachineMonitor smMonitor;
        smMonitor = animator.GetComponent<StateMachineMonitor>();
        smMonitor.UpdateCurrentStateName(stateNameToPropagate);

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
