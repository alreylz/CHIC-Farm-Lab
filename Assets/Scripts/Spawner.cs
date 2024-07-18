using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum SpawnType { inUnitCircle, inUnitSphere, onUnitSphere };
public enum SpawnPlane { Explicit, XZ, XY, YZ };

/* Define las dos formas de operaci�n de esta clase */
public enum SpawningMode { WOZ_TRIGGERING, SURGE_SYSTEM }

/*[REVIEWED] Spawner: clase que se encarga de crear enemigos cercanos a las plantas */
public class Spawner : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property

    public static Spawner Instance { get; private set; } = null;

    //Número actual de Mobs en la escena
    public uint EnemiesLeft
    {
        get { return (uint)EnemiesInScene.Count; }
    }

    #region SPAWNING CONFIGURATION Fields/Properties
    [Header("Spawning Configuration")]
    /*AutoSpawnEnabled: Flag para permitir que se generen enemigos cada "SpawningInterval" si se ha acabado una oleada */
    public SpawningMode SpawnMechanism = SpawningMode.WOZ_TRIGGERING;
    public bool AutoSpawnEnabled = true;
    public uint EnemiesBeginToAppearOnDay = 10;
    /* Tupla que contiene los valores m�ximos y m�nimos en segundos de descanso entre oleadas */
    public Vector2 interWaveInterval = new Vector2();
    [SerializeField]
    private GameObject _prefabToinstantiate = null;
    public GameObject PrefabToInstantiate { get => _prefabToinstantiate; set => PrefabToInstantiate = value; } // [MAY EXTEND] : TO SUPPORT DIFFERENT KINDS OF AGENTS
    public SpawnType spawnType = SpawnType.inUnitCircle;
    public SpawnPlane spawnPlane = SpawnPlane.XZ;
    //M�ximo n�mero de Mobs simult�neos
    public int MaxMobs = 5;
    [Range(1f, 10f)] //Tiempo entre creaci�n de enemigos de la misma oleada
    public float[] SpawningInterval = new float[2];
    [Range(3f, 8f)]
    public float radius; //Distancia + rand[0,1) a la que aparece el elemento
    #endregion SPAWNING CONFIGURATION Fields/Properties

    #region MOB & SURGE MONITORING Fields/Properties
    [Header("Monitor spawnning status")]
    /* Indica si hay una oleada activa*/
    public bool isWaveActive = false;
    public int surgeCount = 1;
    //Lista de objetos spawneados que existen actualmente en la escena
    [SerializeField]
    public List<GameObject> EnemiesInScene;
    private Vector3 rotation;
    //Solamente tiene efecto si se define spawnPlane como "Explicit"
    private Vector3 Rotation { get { return rotation; } set { if (spawnPlane == SpawnPlane.Explicit) rotation = value; } }
    public Color color;

    #endregion MOB & SURGE MONITORING Fields/Properties

    // Semilla de randomizaci�n para la posici�n de los Mobs
    private int seed;
    // Identificador de enemigo �nico
    private static uint enemyID = 1;


    Action OnEnemyDefeated;


    #region Editor-related Code 
    public void UpdateRotationFromRotationPlane()
    {
        switch (spawnPlane)
        {
            case SpawnPlane.XZ:
                rotation = new Vector3(90, 0, 0); break;
            case SpawnPlane.XY:
                rotation = Vector3.zero; break;
            case SpawnPlane.YZ:
                rotation = new Vector3(0, 90, 0); break;
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

    private void Awake()
    {
        #region Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        #endregion Singleton
        EnemiesInScene = new List<GameObject>(MaxMobs);
        //Inicializamos la semilla del generador de n�meros aleatorios (usado para generar puntos de aparici�n aleatorios)
        Random.InitState(seed);
        PlayerSpeechManager.onSpeechRecognition += (string str) => { switch (str) { case "Spawn Plague": case "Crear Plaga": OnDemandSpawn(); break; default: break; } };
        PlayerSpeechManager.onSpeechRecognition += (string str) => { switch (str) { case "Kill Plague": case "Terminar Plaga": KillAll(); break; default: break; } };

        //SI EST� EN MODO CONTROL REMOTO
        if (SpawnMechanism == SpawningMode.SURGE_SYSTEM && AutoSpawnEnabled)
        {
            StartCoroutine(SpawnSurge());
        }




    }


    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Insert)) // La tecla insert crea un conjunto de Mobs on Demand (DEBUG PURPOSES)
        {
            SpawnSurgeImmediate();
        }
#endif
    }

    void DelayEnemyAppearanceTillDay(uint day)
    {
        if (day == EnemiesBeginToAppearOnDay && AutoSpawnEnabled == false)
        {
            AutoSpawnEnabled = true;
            StartCoroutine(SpawnSurge());
        }
        return;

    }











    //Notifica al usuario si ha acabado con el enemigo �ltimo
    void CheckIsLastEnemyToNotify()
    {
        if (EnemiesInScene == null || EnemiesInScene.Count == 0)
        {
            UXNotifications.Instance.SurgeCompletedNotification();
        }

    }

    public void OnDemandSpawn  ()
    {
        StartCoroutine(SpawnSurgeImmediate());
        OnEnemyDefeated += CheckIsLastEnemyToNotify;
    }


    // Genera tantos Enemigos como Máxmo de enemigos con un tiempo espaciado (PARA COMANDO REMOTO)
    private IEnumerator SpawnSurgeImmediate()
    {

        int pendingMobs = MaxMobs;
        if (pendingMobs > 0 && (EnemiesInScene == null || EnemiesLeft == 0))
        {

            if (UXNotifications.Instance != null) UXNotifications.Instance.SurgeStartNotification();
            for (; pendingMobs > 0; pendingMobs--)
            {
                SpawnFromOriginThis(); //Creamos un enemigo y posteriormente esperamos un tiempo aleatorio en el intervalo definido para crear otro
                yield return new WaitForSeconds(Random.Range((int)SpawningInterval[0], (int)SpawningInterval[1]));
            }
        }
    }



    //SI ESTAMOS USANDO OLEADAS
    IEnumerator SpawnSurge() //Corutina de manejo del ciclo de oleadas
    {

        isWaveActive = true;
        int pendingMobs = MaxMobs;

        while (true)
        {
            //[REVISIT]Comprobación de que el panel de notificaciones está activo, si no, esperar.
            while (UXNotifications.Instance == null)
            {
                if (LogsOn) Debug.Log("UX Notifications not initialised");
                yield return new WaitForSecondsRealtime(2f);
            }

            //Si hay una oleada activa, comprobamos los Mobs que quedan por crear y los vamos creando de uno en uno

            if (!AutoSpawnEnabled) //Mantiene esta Corutina sin hacer nada hasta que se activa el spawning autom�tico
            {
                yield return new WaitUntil(() => AutoSpawnEnabled);
            }
            if (isWaveActive && AutoSpawnEnabled)
            {
                if (pendingMobs > 0 && (EnemiesInScene == null || EnemiesLeft == 0))
                {
                    if (LogsOn) Debug.Log("COMENZANDO OLEADA (Spawner.cs) " + surgeCount);
                    if (UXNotifications.Instance != null) UXNotifications.Instance.SurgeStartNotification();
                    for (; pendingMobs > 0; pendingMobs--)
                    {
                        SpawnFromOriginThis(); //Creamos un enemigo y posteriormente esperamos un tiempo aleatorio en el intervalo definido para crear otro
                        //TO-DO: Add a list of possible enemies to spawn and places to spawn them
                        yield return new WaitForSeconds(Random.Range((int)SpawningInterval[0], (int)SpawningInterval[1]));
                    }
                }
                else if (pendingMobs == 0 && EnemiesLeft > 0) //Si ya se han generado todos los Mobs que tocaban pero no han sido eliminados
                {
                    if (LogsOn) Debug.Log("Esperando a que el usuario acabe con los Mobs de la oleada actual");
                    yield return new WaitForSeconds(4f); // Para evitar polling constante, se comprueba si la oleada ha terminado cada 5 segundos

                }
                else //El usuario ha vencido la plaga
                {
                    isWaveActive = false;
                    surgeCount++;
                    UXNotifications.Instance.SurgeCompletedNotification();
                }
            }

            else //Si ya se han creado todos los Mobs y ha finalizado la Oleada, entonces esperamos un intervalo aleatorio de tiempo antes de generar la nueva oleada
            {
                yield return new WaitForSeconds(Random.Range(interWaveInterval.x, interWaveInterval.y));
                isWaveActive = true;
            }

        }
    }


    public void KillAll()
    {
        StopAllCoroutines();
        foreach (var enemy in EnemiesInScene)
        {
            enemy.SetActive(false);
            Destroy(enemy);
        }
        Debug.Log("KILL PLAGUE EXECUTED");
        EnemiesInScene.Clear();
    }

    /*public async void destroyInSeparateThread(List<GameObject> enemy)
    {
        Destroy(enemy)
    }*/


    #region Spawning functions Implementation 
    //Hace aparecer un objeto relativo al origen de este objeto
    public void SpawnFromOriginThis()
    {
        SpawnRespectTo(transform); //Engendrar objetos con respecto al origen de coordenadas del objeto GameManager, seteadas a (0,0,0)
    }
    // Crea un objeto a una distancia relativa a un transform dado como argumento ( e.g. una mosca respecto a un plano de plantaci�n )
    public void SpawnRespectTo(Transform originPositionToSpawnFrom)
    {
        Spawn(originPositionToSpawnFrom.position);
    }
    // Dado un punto de origen, engendra objetos en las inmediaciones de ese punto (a un radio dado [como un campo en esta clase])
    public void Spawn(Vector3 originPos)
    {

        Vector3 instancePosition = Vector3.zero;

        //Set spawn position constraints (Draw within sphere, within circle [plane-like in Y] or on UnitSphere )
        switch (spawnType)
        {
            case SpawnType.inUnitSphere:
                instancePosition = Random.insideUnitSphere; break;
            case SpawnType.inUnitCircle:
                instancePosition = Random.insideUnitCircle; break;
            case SpawnType.onUnitSphere:
                instancePosition = Random.onUnitSphere; break;
        }

        Vector3 posVector = instancePosition - originPos; // v-> (direction vector desde origen a instance.position) = instance.position - origin 
        Vector3 rotatedPosVector = Quaternion.Euler(rotation) * posVector; // rotated direction vector given a rotation in Euler angles (x, y, z)

        instancePosition = rotatedPosVector;
        instancePosition = instancePosition.normalized;
        instancePosition *= radius; // Spawn at distance radius instead of 1 (sets modulo of the position vector) 

        Quaternion instanceRotation = Quaternion.identity;

        //instanceRotation = Quaternion.Euler(originPos - instancePosition);
        GameObject nuGO = Instantiate(PrefabToInstantiate, instancePosition, instanceRotation) as GameObject;
        nuGO.name = "MOB_" + enemyID++;
        if (LogsOn) CHIC.Utilities.Info("Nuevo Enemigo creado y añadido a la lista",this.GetType().ToString(), funcName:"Spawn");
        EnemiesInScene.Add(nuGO);
        //[REVISIT] Cambio de color del objeto para marcarlo como MOB/Enemigo
        if (LogsOn) nuGO.GetComponent<Renderer>().material.SetColor("_Color", color);
    }
    #endregion Spawning functions Implementation








    /*public async Task WaitForStaticInstanceReferenceNotNullAsync(MonoBehaviour obj, float pollFreq)
 {
     while (true)
     {
         var objNullable = obj.GetType().GetProperty("Instance");
         if (objNullable == null) await Task.Delay(TimeSpan.FromSeconds(pollFreq));
         else break;
     }
 }
     */


    /* WaitForReferenceNotNullBounded()
     {
     }*/


}