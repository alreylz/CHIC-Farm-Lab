using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateExample : MonoBehaviour
{



    Animator theAnim;
    // Start is called before the first frame update
    void Start()
    {
        theAnim = GetComponent<Animator>();
        StartCoroutine(PlayIdleIfNothingPlaying());
    }

   IEnumerator PlayIdleIfNothingPlaying()
    {

        theAnim.Play("IdleCube");
        yield return  new WaitForSeconds(2);

    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            theAnim.Play("JumpCube");
        }
        else
        {
            theAnim.ResetTrigger("JumpCube");
        }


    }
    public void RecomputeOrigin()
    {   
    }




}
