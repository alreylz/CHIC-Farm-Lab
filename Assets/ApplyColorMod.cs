using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyColorMod : StateMachineBehaviour
{
    [Range(1,3)]
    public int modChosen;
    public string chainToReplace;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
   override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        List<Material> mList = new List<Material>(animator.transform.Find(chainToReplace).GetComponent<MeshRenderer>().materials);

        Color modColor = Color.black;
        switch (modChosen)
        {
            case 1:
                modColor = Color.green;
                break;
            case 2:
                modColor = Color.blue;
                break;
            case 3:
                modColor = Color.yellow;
                break;

        }
        foreach (var m in mList)
        {
            m.SetColor("_Color", modColor);
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
