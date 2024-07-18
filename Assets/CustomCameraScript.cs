using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit;

public class CustomCameraScript : MonoBehaviour
{


    public float maxDistanceRay = 10.0f;

    public GameObject objectToInstantiate;


    public GameObject newCursor;


    //Guarda una lista indexada de punteros que están activos en la escena
    HashSet<IMixedRealityPointer> punterosActivos = new HashSet<IMixedRealityPointer>();


    void monitorActivePointers()
    {
        //Busca entre todos los dispositivos de entrada (mando, mano, ratón...)
        foreach (var inputSource in CoreServices.InputSystem.DetectedInputSources)
        {
            //Lista los punteros asociados a cada dispositivo de entrada (Mano)
            foreach (var pt in inputSource.Pointers)
            {
                if (pt.IsInteractionEnabled && !punterosActivos.Contains(pt))
                    punterosActivos.Add(pt);
            }
        }

    }


    void printActivePointers()
    {
           foreach (var p in punterosActivos)
        {
            Debug.Log("ACTIVE POINTER => NAME:"+p.PointerName +  "Position"+  p.Position );
        }
    }



    // Start is called before the first frame update
    void Start()
    {
     /*   IMixedRealityGazeProvider gp = CoreServices.InputSystem.GazeProvider;
        Debug.Log("----------------------GAZE PROVIDER-------------------------------------");
        Debug.Log("gp.GameObjectReference: " + gp.GameObjectReference.name);
        Debug.Log("gp.GazeDirection: (" + gp.GazeDirection.x + "," + gp.GazeDirection.y + "," + gp.GazeDirection.z + ")");
        Debug.Log("gp.gazeOrigin: (" + gp.GazeOrigin.x + "," + gp.GazeOrigin.y + "," + gp.GazeOrigin.z + ")");

        Debug.Log("gp.GazeCursor: " + gp.GazeCursor.GameObjectReference.name);

        Debug.Log("----------------------HIT INFO -------------------------------------");
        MixedRealityRaycastHit rhit = gp.HitInfo;
        Debug.Log("MixedRealityRaycastHit.distance: " + rhit.distance);
        Debug.Log("MixedRealityRaycastHit.transform.name" + rhit.transform.name);
        Debug.Log("MixedRealityRaycastHit.point" + rhit.point);
        Debug.Log("MixedRealityRaycastHit.normal" + rhit.normal);
        */

    }

    // Update is called once per frame
    void Update()
    {
        monitorActivePointers();
        printActivePointers();


        /*
        IMixedRealityGazeProvider gp = CoreServices.InputSystem.GazeProvider;
        Debug.Log("----------------------HIT INFO -------------------------------------");
        MixedRealityRaycastHit rhit = gp.HitInfo;
        Debug.Log("MixedRealityRaycastHit.distance: " + rhit.distance);
        Debug.Log("MixedRealityRaycastHit.transform.name" + rhit.transform.name);
        Debug.Log("MixedRealityRaycastHit.point" + rhit.point);

        Debug.Log("MixedRealityRaycastHit.normal" + rhit.normal);
        //Debug.DrawRay(rhit.point, rhit.normal.normalized, Color.black, 20);
        Debug.DrawLine(rhit.point, rhit.point + rhit.normal.normalized * 4, Color.green);

        bool isEqual = rhit.point == gp.HitPosition ? true : false;
        Debug.Log("rhit.point == gp.HitPosition ?" + isEqual);
        */
    }

        


    private void OnDrawGizmos()
    {

        //Display components from the GazeProvider object (which implements the interface IMixedRealityGazeProvider): Proporciona información sobre la mirada, objetos siendo apuntados, distancias, normales...
        IMixedRealityGazeProvider gp = CoreServices.InputSystem?.GazeProvider;

        Gizmos.color = Color.blue;
        //Draw line from Hololens origin to the point where the ray collides with a surface
        Gizmos.DrawLine(gp.GazeOrigin, gp.HitPosition);
        float sphericalMarkersRad = 0.01f ;
        //Mark origin point with sphere
        Gizmos.DrawSphere(gp.HitPosition + (gp.HitNormal * sphericalMarkersRad), sphericalMarkersRad);
        Gizmos.DrawSphere(gp.GazeOrigin, sphericalMarkersRad);
        //Draw normal direction with vector's magnitude = 2;
        Gizmos.DrawLine(gp.HitPosition, gp.HitPosition + (gp.HitNormal.normalized * 1));
        Gizmos.DrawIcon(gp.GazeOrigin + gp.GazeDirection.normalized * gp.HitInfo.distance / 2, "gizmoIcon.png", true);


    }



   
    /* Crea un enemigo en la posición en la que se produce la colisión entre puntero y collider*/
    public void spawnEnemyAtCollissionPosition(BaseInputEventData evData)
    {


        Debug.Log("EXECUTED! --------------------------------------------");
        MixedRealityPointerEventData eventData = (MixedRealityPointerEventData) evData;
        // Ray information;
        //Objeto donde se almacena el resultado del raycast realizado por el puntero.
        FocusDetails focusDetails = eventData.Pointer.Result.Details;

        MixedRealityRaycastHit hit = focusDetails.LastRaycastHit;
        //Spawneamos un objeto en la posición donde ha chocado el rayo
        Instantiate(objectToInstantiate, /*position*/ hit.point, new Quaternion(), transform);
    }






    private void OnDrawGizmosSelected()
    {
      /*  RaycastHit rayHit;

        if(Physics.Raycast(transform.position, transform.forward, out rayHit, maxDistanceRay)){ 
        //rayHit.distance < maxDistanceRay ? rayHit.distance : maxDistanceRay
            Gizmos.color = Color.cyan;
            //On collission
        // Línea negra en el caso de que haya colisión.
         Debug.DrawLine(transform.position, transform.position + transform.forward * (rayHit.distance < maxDistanceRay ? rayHit.distance : maxDistanceRay), Color.black,  0.2f);
        }
        //Línea gris si no hay colisión
        Debug.DrawRay(transform.position, transform.position + transform.forward * (rayHit.distance < maxDistanceRay ? rayHit.distance : maxDistanceRay), Color.gray, 0.5f, false );

    */
    }

  
}
