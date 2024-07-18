using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCube : MonoBehaviour
{
    public float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.O))
        {
            Debug.Log("MOVING OBJ");
            gameObject.transform.Translate(new Vector3(0,0,1) * Time.deltaTime * speed);
        }
    }
}
