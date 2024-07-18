using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRoot : MonoBehaviour
{


    public float speed = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RotateItem3D());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RotateItem3D()
    {
        while (true)
        {
            transform.Rotate(Vector3.up,1f*speed,Space.World);
            yield return new WaitForEndOfFrame();
        }
        yield return null;

    }



}
