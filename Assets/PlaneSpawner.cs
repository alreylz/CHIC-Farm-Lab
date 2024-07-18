using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSpawner : MonoBehaviour
{



    public List<GameObject> PrefabsToInstantiate { get; set; } = null;

    public int ToInstantiateIndex = 0;

    public SpawnPlane _SpawnPlane = SpawnPlane.XZ;
    private Vector3 _Rotation;
    private SpawnType _SpawnType = SpawnType.inUnitCircle;
    [SerializeField]
    private float _Radius;


    #region Editor-related Code 
    public void UpdateRotationFromRotationPlane()
    {
        switch (_SpawnPlane)
        {
            case SpawnPlane.XZ:
                _Rotation = new Vector3(90, 0, 0); break;
            case SpawnPlane.XY:
                _Rotation = Vector3.zero; break;
            case SpawnPlane.YZ:
                _Rotation = new Vector3(0, 90, 0); break;
            case SpawnPlane.Explicit:
                //Asignado desde el inspector directamente
                break;
        }
    }
    public void OnValidate()
    {
        UpdateRotationFromRotationPlane();
    }
    #endregion Editor-related Code 

    // Start is called before the first frame update
    void Start()
    {
        UpdateRotationFromRotationPlane();
        
        _Radius = Mathf.Min(gameObject.GetComponent<MeshRenderer>().bounds.size.x, gameObject.GetComponent<MeshRenderer>().bounds.size.z)  ;
        
    
        Gizmos.DrawWireSphere(transform.position, _Radius);
    }


    #region Spawning functions Implementation 
    // Dado un punto de origen, engendra objetos en las inmediaciones de ese punto.
    public void SpawnAll(Vector3 originPos)
    {
        Vector3 instancePosition = Vector3.zero;

        //Set spawn position constraints (Draw within sphere, within circle [plane-like in Y] or on UnitSphere )
        switch (_SpawnType)
        {
            case SpawnType.inUnitCircle:
                instancePosition = Random.insideUnitCircle; break;
            default:
                break;
        }

        Vector3 posVector = instancePosition - originPos; // v-> (direction vector desde origen a instance.position) = instance.position - origin 
        Vector3 rotatedPosVector = Quaternion.Euler(_Rotation) * posVector; // rotated direction vector given a rotation in Euler angles (x, y, z)

        instancePosition = rotatedPosVector;
        instancePosition *= _Radius; // Spawn at distance radius instead of 1 (sets modulo of the position vector) 

        Quaternion instanceRotation = Quaternion.identity;

        Debug.Log("SPAWNING STH");
        foreach (var prefab in PrefabsToInstantiate)
        {
            //GameObject nuGO = Instantiate(prefab, instancePosition, instanceRotation, transform) as GameObject;
            //nuGO.GetComponent<RectTransform>().localPosition = instancePosition;

        }
    }
    
    public void ScatterAll(Vector3 originPos)
    {

        


        
        foreach (var word in PrefabsToInstantiate)
        {
            //Generación de un punto aleatorio en el plano (usando un círculo proyectado sobre él)
            Vector3 instancePosition;
            instancePosition.x = Random.insideUnitCircle.x;
            instancePosition.z = Random.insideUnitCircle.y;


            float maxSpawnHeight = GetComponent<BoxCollider>().bounds.size.y/transform.localScale.y;
            Debug.Log("MAX SPAWN HEIGHT = " + maxSpawnHeight);
            float maxSpawnWidth = Mathf.Min(GetComponent<BoxCollider>().bounds.size.x/transform.localScale.x, GetComponent<BoxCollider>().bounds.size.z/transform.localScale.z);
            Debug.Log("MAX SPAWN WIDTH = " + maxSpawnWidth);

            float addedH = GetComponent<BoxCollider>().center.y;

            instancePosition.x *= maxSpawnWidth;
            instancePosition.z *= maxSpawnWidth;
            instancePosition.y = Random.Range(0f, maxSpawnHeight);

            //Asignación de posición sobre zona de spawning
            word.transform.localPosition = instancePosition;
        }
        

    }
    
    #endregion Spawning functions Implementation

}

