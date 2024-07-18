using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleElemWithName : StateMachineBehaviour
{


    public string PathToElemToModify;
    public bool Enabled;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //}
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        GameObject gOToEnableOrDisable;
        if(PathToElemToModify == null || PathToElemToModify == ""){
            animator.gameObject.SetActive(Enabled);
            return;
        }
        try
        {
            gOToEnableOrDisable = animator.transform.Find(PathToElemToModify).gameObject as GameObject;
        
        if (gOToEnableOrDisable != null) { gOToEnableOrDisable.SetActive(Enabled); Debug.Log("HIDING " + gOToEnableOrDisable.name); }
        else { Debug.Log("BAD PATH as Input to ToggleElemWithName in State " + animatorStateInfo.nameHash); }
        }
        catch (Exception e)
        {
            Debug.Log("BAD PATH as Input to ToggleElemWithName in State " + animatorStateInfo.nameHash);
        }

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
