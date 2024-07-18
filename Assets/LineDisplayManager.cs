using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

[RequireComponent(typeof(LineRenderer))]
public class LineDisplayManager : MonoBehaviour
{

    public GazeProvider gazeProvider;
    private RaycastHit hit;
    private LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        
        //Coger referencia a gaze provider 
        if(gazeProvider == null) { FindObjectOfType(typeof(GazeProvider)); }
        if(lr == null) { lr = GetComponent<LineRenderer>(); }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Actualizar línea
        Debug.DrawRay(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f)), gazeProvider.HitPosition,Color.blue);
        if(gazeProvider.HitPosition != null) {
              
            //lr.SetPosition(0, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.7f, 0.5f)));
            //lr.SetPosition(1, gazeProvider.HitPosition );
        }
        else { //lr.SetPosition(0, Vector3.zero);
            //lr.SetPosition(1, Vector3.zero);
            }
    }
}
