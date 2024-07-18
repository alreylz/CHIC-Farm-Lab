using UnityEngine;
using System.Collections;

public class Fly : MonoBehaviour {
    Animator fly;
    private IEnumerator coroutine;
	// Use this for initialization
	void Start () {
        fly = GetComponent<Animator>();

        fly.SetBool("takeoff", true);
        fly.SetBool("walk", false);
        fly.SetBool("idle", false);
        StartCoroutine("flying");
        //flying();

        /*
        fly.SetBool("flyleft", true);
        fly.SetBool("flying", false);
        fly.SetBool("flyright", false);

        fly.SetBool("turnleft", true);
        fly.SetBool("turnright", false);
        fly.SetBool("idle", false);
        */
    }


    void Update()
    {

        if (Input.GetKey(KeyCode.A))
        {
            fly.SetBool("flyleft", true);
            fly.SetBool("flying", false);
            fly.SetBool("flyright", false);
        }

    }

    /*

        // Update is called once per frame
        void Update () {
            if (Input.GetKey(KeyCode.S))
            {
            Debug.Log("update 1");
            fly.SetBool("idle", true);
                fly.SetBool("walk", false);
                fly.SetBool("rubbing", false);
                fly.SetBool("turnright", false);
                fly.SetBool("turnleft", false);
            }
            if (Input.GetKey(KeyCode.W))
            {
            Debug.Log("update 2");
            fly.SetBool("walk", true);
                fly.SetBool("idle", false);
            }
            if ((Input.GetKey(KeyCode.R))||(Input.GetKey(KeyCode.Space)))
            {
            Debug.Log("update 3");
            fly.SetBool("takeoff", true);
                fly.SetBool("walk", false);
                fly.SetBool("idle", false);
                StartCoroutine("flying");
                flying();
            }
            if ((Input.GetKey(KeyCode.F))||(Input.GetKey(KeyCode.S)))
            {
            Debug.Log("update 4");
            fly.SetBool("landing", true);
                fly.SetBool("flying", false);
                fly.SetBool("flyleft", false);
                fly.SetBool("flyright", false);
                StartCoroutine("idle");
                idle();
            }
            if (Input.GetKey(KeyCode.E))
            {
            Debug.Log("update 5");
            fly.SetBool("rubbing", true);
                fly.SetBool("idle", false);
            }
            if (Input.GetKey(KeyCode.W))
            {
            Debug.Log("update 6");
            fly.SetBool("flying", true);
                fly.SetBool("flyleft", false);
                fly.SetBool("flyright", false);
            }
            if (Input.GetKey(KeyCode.A))
            {
            Debug.Log("update 7");
                fly.SetBool("flyleft", true);
                fly.SetBool("flying", false);
                fly.SetBool("flyright", false);
            }
            if (Input.GetKey(KeyCode.D))
            {
            Debug.Log("update 8");
            fly.SetBool("flyright", true);
                fly.SetBool("flying", false);
                fly.SetBool("flyleft", false);
            }
            if (Input.GetKey(KeyCode.A))
            {
            Debug.Log("update 9");
                fly.SetBool("turnleft", true);
                fly.SetBool("turnright", false);
                fly.SetBool("idle", false);
            }
            if (Input.GetKey(KeyCode.D))
            {
            Debug.Log("update 10");
                fly.SetBool("turnright", true);
                fly.SetBool("turnleft", false);
                fly.SetBool("idle", false);
            }
            if (Input.GetKey(KeyCode.Keypad0))
            {
            Debug.Log("update 11");
                fly.SetBool("die", true);
                fly.SetBool("flying", false);
            }
        }

    */

    IEnumerator flying()
    {
        yield return new WaitForSeconds(0.1f);
        fly.SetBool("takeoff", false);
        fly.SetBool("flying", true);
    }

    IEnumerator idle()
    {
        yield return new WaitForSeconds(0.1f);
        fly.SetBool("idle",true);
        fly.SetBool("flying", false);
        fly.SetBool("landing", false);
    }
}
