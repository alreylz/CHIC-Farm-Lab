using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateTowardsWithFixedAxis : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    
        Debug.Log("POS " + transform.position);
        Debug.Log("FORWARD " +transform.forward);

        Vector3 dirTowardsCam = transform.position - Camera.main.transform.position;

        Debug.Log(Vector3.SignedAngle(transform.forward, dirTowardsCam,transform.up));
        transform.Rotate(transform.up, Vector3.SignedAngle(transform.forward, dirTowardsCam, transform.up));
      //transform.position Camera.main.transform.position 
    }
}
