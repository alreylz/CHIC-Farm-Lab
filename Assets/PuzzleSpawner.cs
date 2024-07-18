using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;


[System.Serializable]
public class OnPrefabSpawned  : UnityEvent<GameObject> { };
/// <summary>
/// Simplemente crea otro objeto al chocar con algo del mundo real (por defecto, o por otra capa elegida desde el editor)
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PuzzleSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _ToSpawnPrefab;
    public GameObject ToSpawnPrefab
    {
        get => _ToSpawnPrefab;
        set => _ToSpawnPrefab = value;
    }

    [SerializeField]
    private string _CollisionLayerName = "Spatial Awareness";
    public string CollisionLayerName
    {
        get => _CollisionLayerName;
        set {
            int nuLayerID = LayerMask.NameToLayer(value);
            if (nuLayerID == -1) return;
            _CollisionLayerName = value;
        }
    }



    private readonly float minTimeBetweenSpawns = 2f;
    private float tLastSpawn = 0f;

    [SerializeField]
    private float _ScaleFactorPrefab;


    private GameObject nuInstanceReference = null;
    
    [Header("Event Subscription")]
    public OnPrefabSpawned onPrefabSpawned = new OnPrefabSpawned();

    private void OnCollisionEnter(Collision collision)
    {
        if( collision.gameObject.layer == LayerMask.NameToLayer(_CollisionLayerName))
        {
            if (Time.realtimeSinceStartup - tLastSpawn > minTimeBetweenSpawns)
            {
                nuInstanceReference = Instantiate(_ToSpawnPrefab, collision.contacts[0].point, Quaternion.Euler(Vector3.up), GameManager.GameWorld.transform);
                nuInstanceReference.transform.localScale *= _ScaleFactorPrefab;
                if (nuInstanceReference != null) onPrefabSpawned?.Invoke(nuInstanceReference);
                tLastSpawn = Time.realtimeSinceStartup;
            }
        }
    }

}
