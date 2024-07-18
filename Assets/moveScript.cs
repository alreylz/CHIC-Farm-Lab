using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveScript : MonoBehaviour
{

    Rigidbody rb;
    Vector3 init_Trans ;

    public float radius = 10;
    public float w = 20;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        init_Trans = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {

        moveInCircles(transform, radius,w);



    }



    public void moveInCircles(Transform tr, float radius, float w)
    {
        /* Circular motion is given by the expression: (En el plano X-Z)
         * x = r * cos(wt);
         * a = r * sin(wt);
         */

        float x = radius * Mathf.Cos(w*Time.deltaTime);
        float z  = radius * Mathf.Sin(w * Time.deltaTime);
        
        tr.position = new Vector3(init_Trans.x + x,init_Trans.y, init_Trans.z +z);
    }

}
