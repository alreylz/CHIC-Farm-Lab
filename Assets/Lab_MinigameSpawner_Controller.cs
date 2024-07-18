using UnityEngine;
using System.Collections;
using System;
using Microsoft.MixedReality.Toolkit.UI;



/// <summary>
/// Clase que controla el estado del token que crea minijuegos de modificación genética con la configuración deseada aspecto deseado.
/// También implementa la lógica de control del token que spawnea el prefab de mini-juego, 
/// </summary>
public class Lab_MinigameSpawner_Controller : MonoBehaviour
{

    [Header("Mini Game Setup")]
    public GameObject MinigamePrefabToSpawn; // --> Prefab a spawnear en la posición donde caiga el elemento
    public PlantModification[] _modsToChoose;
    public bool _showTutorial;
    public float timeLimit;
    public PlantVariety SrcVariety;

    [Header("Representación 3D a usar como Spawner (e.g. semillas)")]
    [SerializeField]
    private GameObject _3DMiniGameSpawnerPrefab;


    [SerializeField]
    private Transform _3DObjectContainer; //Contenedor de representación en UI (elemento que tiene el manipulation handler), lo que coges como tal.


    //Elemento en la escena;
    [SerializeField]
    private GameObject _Instanced3DObj;
    

    [SerializeField]
    private BroadcastChildCollider staticItemBoundaries; // Collider trigger que define límites en la pantalla.
    [SerializeField]
    private float driftPeriod = 6; //Tiempo para reposicionar elemento si no choca con nada en un buen rato.


    public Vector3 p0;
    
    [SerializeField]
    private bool _TakenOutsideBoundaries = false;

    [SerializeField]
    public bool isMiniGameActive  = false ; // CAMBIAR VALOR DE ESTO PARA QUE AUTOMÁTICAMENTE SE ACTIVE O DESACTIVE LA POSIBILIDAD DE INTERACTUAR CON EL TOKEN


    [SerializeField]
    private DraggableTokenStatusMonitor _TokenMonitor;
    



    void Start()
    {
        //Instancio elemento 3D en la jerarquía
        if (_Instanced3DObj == null)
            _Instanced3DObj = Instantiate(_3DMiniGameSpawnerPrefab, _3DObjectContainer);

        //Apunto su posición en p0
        p0 = _3DObjectContainer.transform.localPosition;
        //Accedo al contenedor de la representación 3D del spawner (el objeto que al chocar genera el mini-juego)
        //a) Obtengo el componente Rigidbody --> Control de gravedad.
        //b) Obtengo componente manipulation handler --> Manejo de iteracción drag'n drop
        //c) Suscribo a evento de Colisión con objetos, para manejar el retorno del objeto al panel del lab
        //--> Las operaciones necesarias y monitoring se realiza a traveés de un script llamado DraggableTokenStatusMonitor, 
        //    colocado en la raíz de esta representación 3D que traspasa los datos del minijuego.
        _TokenMonitor = CHIC.Utilities.GetFirstComponentOfTypeInChildren<DraggableTokenStatusMonitor>(transform, debug: true);

        _TokenMonitor.OnTokenCollided += TokenCollisionBehaviour;
        //Escucha a elemento que define los boundaries de uso del mini-game spawner
        staticItemBoundaries.TriggerExit.AddListener(WhenObjectLeavesBoundaries);

       
    }
    


    public void LoadMiniGameConfiguration(GameObject go) {
        go.GetComponent<MiniGameConfiguration>().ApplyConfiguration(SrcVariety, _modsToChoose, timeLimit, _showTutorial);

    }


    public void ConfigSrcPlantVariety(PlantVariety pv)
    {
        SrcVariety = pv;
    }


    IEnumerator InactiveMiniGameMonitorer()
    {
        while (true)
        {

            yield return new WaitUntil(() => (FindObjectsOfType<MiniGameLifecycleController>()== null));
            _TokenMonitor.EnableTokenManipulation();
        }

    }

    // Parámetro es el objeto con el que choca el token de mini-game spawning.
    void TokenCollisionBehaviour(Collision otherHitByToken)
    {

        if(otherHitByToken.gameObject.layer == LayerMask.NameToLayer("Spatial Awareness"))
        {
           
            RepositionMiniGameSpawnerInUI();
           

            if (isMiniGameActive) _TokenMonitor.DisableTokenManipulation();
        }
        
    }


    //Ejecuta cada vez que se detecta que algo sale de la bounding box reservada para el Mini-Game Spawner.
    void WhenObjectLeavesBoundaries(Collider other)
    {
        if (other.gameObject.name == _3DObjectContainer.name) {

            _TakenOutsideBoundaries = true;
            StartCoroutine(DriftCheck());
            _TokenMonitor.AllowFall();
        }
    }


   //Gestión de reposicionamniento una vez se ha utilizado el token.
   IEnumerator DriftCheck()
    {
        do
        {
            yield return new WaitForSeconds(driftPeriod);
            if (!_TokenMonitor.IsGrabbed && _TakenOutsideBoundaries)
            {
                RepositionMiniGameSpawnerInUI();
                break;
            }
        }
        while (_TokenMonitor.IsGrabbed);
        
    }
    
    public void RepositionMiniGameSpawnerInUI()
    {
        _3DObjectContainer.localPosition = p0;

        _TokenMonitor.SetVelocityZero();
        _TokenMonitor.PreventFall();
        _TakenOutsideBoundaries = false;
    }
    


}
