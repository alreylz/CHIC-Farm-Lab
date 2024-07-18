using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.WindowsMixedReality.SpatialAwareness;
#if (UNITY_EDITOR)
using Microsoft.MixedReality.Toolkit.SpatialObjectMeshObserver;
#endif
using Microsoft.MixedReality.Toolkit.SpatialAwareness; // -> Clases asociadas al manejo de mallas del mundo real (e.g. SpatialAwarenessMeshObject)
using CHIC;
//using SpatialAwarenessHandler = Microsoft.MixedReality.Toolkit.SpatialAwareness.IMixedRealitySpatialAwarenessObservationHandler<Microsoft.MixedReality.Toolkit.SpatialAwareness.SpatialAwarenessMeshObject>;

/* RealWorldOverlay: Clase encargada del procesado del mundo real */
public class RealWorldOverlay : Singleton<RealWorldOverlay>
{
    #region Attributes / Properties

    #region Debug Property
    [SerializeField]
    private bool _LogsOn = false;
    public bool LogsOn { get { return _LogsOn; } set { _LogsOn = value; } }
    #endregion Debug Property
    


    // Tipos de Spatial Observers existentes (Clases asociadas a la observaci�n como tal, encargadas de actualizar las mallas del mundo real)
    //1. SpatialObjectMeshObserver -> Updater de malla de test (muestra una provista como objeto 3D)
    #if (UNITY_EDITOR)
    private static Type sOtype = typeof(SpatialObjectMeshObserver);
    #endif
    //2. WindowsMixedRealitySpatialMeshObserver -> Updater de Malla del mundo real ; construida at runtimes
    private static Type WMRsOtype = typeof(WindowsMixedRealitySpatialMeshObserver);

    /* M�dulo de Spatial Awareness */
    public static IMixedRealityDataProviderAccess hololensSpatialAwarenessSystem;

    [Header("Configuration")]
    // Esta flag automatiza la configuraci�n de los observadores del mundo, para evitar tener que cambiar perfiles y dem�s continuamente
    [Tooltip("True => Use Real world mesh ; False => Use example mesh object")]
    public bool inHololens = true;
    public bool scanAtStartup = true;
    [Range(5f, 30f)]
    public float scanningPeriod = 15f;
    [Header("Status")]
    public bool scanning = false;
    public bool IsScanning {
        get{
            return scanning;
        }
        set {
            scanning = value;
            if (value == true) OnScanStart?.Invoke(); else OnScanEnd?.Invoke(); }
    }

    [Space(2f)]
    /* Provisionador de datos de malla del mundo real (o malla de ejemplo) */
    public IMixedRealityDataProvider activeDataProvider;
    /* Array de objetos de Unity que representan la malla del mundo exterior; obtenidos del data provider */
    public List<SpatialAwarenessMeshObject> meshes;
    [Header("Example Mesh to use")]
    public GameObject ExampleMeshSwap;
    [Header("Wireframe Materials (Utilizado para pasar a completa transparencia) ")]
    public Material duringScanMaterial;
    public Material inGameMaterial;

    [Header("Mesh Material Configuration")]
    public Color32 baseColor;
    public Color32 wireColor;
    public float wireWidth;

    [Header("Mesh Material Configuration")]
    public Color32 endBaseColor;
    public Color32 endWireColor;
    public float endWireWidth;

    [Header("Glowing Animation Configuration")]
    public float glowingStatus = 0;
    private float glowingStep = 0.1f;
    public float glowingSpeed = 0.5f;

    [Header("Virtual objects")]
    // Lista de elementos que han sido colocados en el mundo real (e.g. terrenos colocados en el mundo)
    public List<GameObject> OverlaidItems = new List<GameObject>();


    #region Events/Actions
    public Action OnOverlaidItemFirst;
    public Action OnNewOverlaidItem;
    public Action OnPlantAdded;
    public Action OnScanStart;
    public Action OnScanEnd;
    #endregion

    public float last_t_Spawned = 0f;
    public float spawn_period_limiter = 1f;

    #endregion Attributes / Properties

    
    #region Overlaid Objects Management 

    /* Dado un punto calculado y una rotación definida por el propio elemento a aparecer en el mundo, se instancia el mismo sobre punto indicado */
    public bool TrySpawnItem(Vector3 position, Quaternion rotation, Item itemData, int itemMask = 14)
    {
        if (Time.realtimeSinceStartup - last_t_Spawned < spawn_period_limiter) return false;

        GameObject nuGO = null;
        switch (itemData.Type)
        {
            case ItemType.Farmland:
                Farmland iData = itemData as Farmland;
                // Instanciamos prefab como hijo del objeto "GameWorld"
                nuGO  = Instantiate(iData.fieldPrefab, position, rotation, GameManager.GameWorld.transform) as GameObject;
                nuGO.transform.localScale *= 2;
                if (nuGO == null)
                {
                    Utilities.Error(" Couldn't spawn Farmland at " + position, this.GetType().ToString());
                    return false;
                }

                //Marcamos como objeto utilizado (que se ha usado desde el inventario)
                CHIC.Utilities.SetLayerRecursively(nuGO.transform, itemMask);
                
                //Creación de manejador del ciclo de vida e interacción de una plantación
                FarmlandManager fm;

                fm = nuGO.GetComponent<FarmlandManager>();
                if (fm == null)
                    fm = nuGO.GetComponentInChildren<FarmlandManager>();
                if (fm == null)
                    fm = nuGO.AddComponent<FarmlandManager>();

                bool initFarmlandOK = fm.InitFarmlandData((itemData as Farmland));
                if (!initFarmlandOK)
                {
                    Utilities.Error("Error initialising Farmland data", this.GetType().ToString(), funcName: "TrySpawnItem");
                }

                break;
        }

        if (nuGO == null) return false;
        //Añadimos a la lista de elementos situados sobre el mundo (Lista OverlaidItemes)
        OverlaidItems.Add(nuGO);
        OnNewOverlaidItem?.Invoke();

        last_t_Spawned = Time.realtimeSinceStartup;
        return true;

    }
    
    /* Devuelve un elemento cualquiera de la lista de elementos colocados sobre el mundo real */
    public GameObject getRandomItem()
    {
        if (OverlaidItems.Count == 0) return null;
        int randIndex = UnityEngine.Random.Range(0, OverlaidItems.Count);
        return OverlaidItems[randIndex];
    }

    /*Destruye todos los terrenos de plantación */
    public void DestroyAllFields()
    {
        foreach (var item in OverlaidItems)
        {
            Destroy(item, 0.5f);
        }
        OverlaidItems.Clear();
    }

    #endregion Overlaid Objects Management




    public void propagatePlantAdded(Plant p)
    {
        OnPlantAdded?.Invoke();
    }


    #if (UNITY_EDITOR)
    private void OnValidate()
    {
        // View Real-world wireframe material color in inspector
        if (duringScanMaterial != null)
        {
            baseColor = duringScanMaterial.GetColor("_BaseColor");
            wireColor = duringScanMaterial.GetColor("_WireColor");
            wireWidth = duringScanMaterial.GetFloat("_WireThickness");
        }
    }
    #endif

    private void Awake()
    {
        Instance = this; // Requisito para asegurar que sea un Singleton (clase con una sola instancia globalmente)

        //Obtenemos referencia al m�dulo de spatial awareness
        hololensSpatialAwarenessSystem = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
        //Suscripción al evento de comandos de voz detectados por el sistema
        PlayerSpeechManager.onSpeechRecognition += OnSpeechCommand;

        //inicializaci�n del proveedor de datos de malla externa
        foreach (var dataProvider in hololensSpatialAwarenessSystem.GetDataProviders())
        {
        #if UNITY_EDITOR
            //if observador de test (con la malla del lab del DEI) o configurado el uso de un testBench
            if (dataProvider.GetType() == typeof(SpatialObjectMeshObserver) && !inHololens)
            {
                SpatialObjectMeshObserver aux = dataProvider as SpatialObjectMeshObserver;
                /* Permite especificar un gameobject como malla de prueba simplificada, de forma que los objetos
                /* hijos formen parte de la malla del mundo con la que los objetos virtuales colisionan */
                if (ExampleMeshSwap != null)
                {
                    ExampleMeshSwap.SetLayerRecursively(LayerMask.NameToLayer("Spatial Awareness"));
                    if (!ExampleMeshSwap.activeInHierarchy) ExampleMeshSwap.SetActive(true);
                }
                activeDataProvider = aux;
            }
        #endif

            //if observador del mundo real es el que usan las Hololens (estamos haciendo deploy)
            if (dataProvider.GetType() == typeof(WindowsMixedRealitySpatialMeshObserver) && inHololens)
            {
                activeDataProvider = (dataProvider as WindowsMixedRealitySpatialMeshObserver);
                if (LogsOn) Debug.Log("Activedataprovider is OK? " + (activeDataProvider == null ? "NOPE" : "YES"));
                if (ExampleMeshSwap != null && ExampleMeshSwap.activeInHierarchy)
                    ExampleMeshSwap.SetActive(false);
            }

            //Si se ha configurado que se realice un escaneo al comenzar el juego y se ha detectado un proovedor de datos de malla real-world
            if (scanAtStartup && activeDataProvider != null) StartCoroutine(ScanAndStopAfterTime(scanningPeriod));

        }

    }



    /* Comenzar/Continuar procesamiento de la malla del mundo real */
    private void ResumeActiveDataProviders()
    {
        Type activeProvDatatype = activeDataProvider.GetType();
        #if (UNITY_EDITOR)
        if (activeProvDatatype == sOtype && !inHololens) //Si estamos en el editor y realizando pruebas
        {
            if (ExampleMeshSwap != null) InitStaticMesh();
            else (activeDataProvider as SpatialObjectMeshObserver).Resume();
        }
        #endif
        if (activeProvDatatype == WMRsOtype)
        {
            (activeDataProvider as WindowsMixedRealitySpatialMeshObserver).Resume();
            Debug.Log("activeDataProvider running " + (activeDataProvider as WindowsMixedRealitySpatialMeshObserver).IsRunning.ToString());
        }
        //Comenzamos el periodo de escaneo (Controlled by "scanning" variable)
        IsScanning = true;

        if (LogsOn)
        {
            Utilities.Info(" RESUMED World Observer (" + activeProvDatatype.ToString() + ")", this.GetType().ToString(), funcName:"ResumeActiveDataProviders");
            Utilities.Info(" - WORLD SCANNING PROCESS STARTED - ", this.GetType().ToString(), funcName: "ResumeActiveDataProviders");
        }

        StartCoroutine(ShowScanningProcess());
    }

    /* Dejar de procesar la malla del mundo real */
    private void SuspendActiveDataProviders()
    {
        Type activeProvDatatype = activeDataProvider.GetType();
        #if (UNITY_EDITOR)
        if (activeProvDatatype == sOtype)
        {
            (activeDataProvider as SpatialObjectMeshObserver).Suspend();
        }
        #endif
        if (activeProvDatatype == WMRsOtype)
        {
            (activeDataProvider as WindowsMixedRealitySpatialMeshObserver).Suspend();
        }
        IsScanning = false;
        if (LogsOn)
        {
            Utilities.Info(" SUSPENDED World Observer (" + activeProvDatatype.ToString() + ")", this.GetType().ToString(),funcName: "SuspendActiveDataProviders");
            Utilities.Info(" - SCANNING PROCESS SUSPENDED - ", this.GetType().ToString(), funcName: "SuspendActiveDataProviders");
        }

        //refrescamos la lista de objetos que representan la malla del mundo real (En caso de que estemos usando Hololens)
        if (inHololens) { RefreshCachedMesh(); StartCoroutine(FadeWireframeOut()); }

    }




    /* Incluye los elementos de malla de pruebas al array de mallas del mundo simulado (cache)=meshes */
    public void InitStaticMesh()
    {
        if (ExampleMeshSwap != null)
        {
            meshes = new List<SpatialAwarenessMeshObject>();
            //Añado los componentes de la malla simplificada a la lista de mallas
            meshes.Add(SpatialAwarenessMeshObject.Create(ExampleMeshSwap.GetComponent<Mesh>(), LayerMask.NameToLayer("Spatial Awareness"), "ParentTestMesh", UnityEngine.Random.Range(100, 200), ExampleMeshSwap.gameObject));

            int idchild = 0;
            foreach (Transform child in ExampleMeshSwap.transform)
            {
                meshes.Add(SpatialAwarenessMeshObject.Create(child.GetComponent<Mesh>(), LayerMask.NameToLayer("Spatial Awareness"), ("t-" + idchild), idchild, child.gameObject));
                idchild++;
                foreach (Transform childOfChild in child.transform)
                {
                    meshes.Add(SpatialAwarenessMeshObject.Create(childOfChild.GetComponent<Mesh>(), LayerMask.NameToLayer("Spatial Awareness"), ("t-u-" + idchild), idchild, childOfChild.gameObject));
                    idchild++;
                }
            }
        }
        RefreshCachedMesh();

    }


    /* Iterative Fading Wireframe material function */
    private void GlowFadeWireframeMaterial(SpatialAwarenessMeshObject meshI, float controlVar, Color32 baseColor0, Color32 baseColorT, Color32 wireColor0, Color32 wireColorT, float wireThickness0, float wireThicknessT, float maxCloseness = 1)
    {
        float interpolatingrepeating_t = Mathf.PingPong(controlVar, maxCloseness);
        meshI.Renderer.material.SetColor("_BaseColor", Color.Lerp(baseColor0, baseColorT, interpolatingrepeating_t));
        meshI.Renderer.material.SetColor("_WireColor", Color.Lerp(wireColor0, wireColorT, interpolatingrepeating_t));
        meshI.Renderer.material.SetFloat("_WireThickness", Mathf.Lerp(wireWidth, wireThicknessT, interpolatingrepeating_t));
        Debug.Log("Modifying" + meshI.Id);
        /* No olvidar incrementar la variable de control fuera de la funci�n */

    }

    /* Maneja el proceso de animación blink de la malla REAL o virtual */
    private IEnumerator GlowAnimationAllmeshes()
    {
        while (scanning)
        {
            if (inHololens)// FOR DEPLOYMENT CASES (in hololens or broadcast to hololens) --> wait for meshes to be available and then do glowing
            {
                //Si el provider a�n no ha encontrado ni una sola malla del mundo real, esperamos un intervalo de tiempo corto y volvemos a consultar
                while ((activeDataProvider as WindowsMixedRealitySpatialMeshObserver).Meshes == null /*|| meshes == null*/)
                {
                    if (LogsOn) Utilities.Warn(" Windows XR Spatial Observer has not gathered Spatial information yet.",this.GetType().ToString());
                    yield return new WaitForSeconds(0.2f);
                }
                if (LogsOn) Utilities.Info(" Windows XR Spatial Observer is SCANNING ", this.GetType().ToString(),funcName: "GlowAnimationAllmeshes");

                foreach (var m in (activeDataProvider as WindowsMixedRealitySpatialMeshObserver).Meshes.Values)
                {
                    //float interpolatingrepeating_t = mathf.pingpong(glowingstatus, 0.9f);
                    //m.Renderer.material.setcolor("_basecolor", color.lerp(basecolor, new color32(255, 255, 255, 0), interpolatingrepeating_t));
                    //m.Renderer.material.setcolor("_wirecolor", color.lerp(wirecolor, new color32(255, 255, 255, 0), interpolatingrepeating_t));
                    //m.Renderer.material.setfloat("_wirethickness", mathf.lerp(wirewidth, 0f, interpolatingrepeating_t));
                    GlowFadeWireframeMaterial(m, glowingStatus, baseColor, endBaseColor, wireColor, endWireColor, wireWidth, endWireWidth, 0.9f);
                }

            }

            #if (UNITY_EDITOR)
            if ((activeDataProvider as SpatialObjectMeshObserver) != null && !inHololens)
            {

                //Debug.Log("GLOWING SHALL HAPPEN");

                if (ExampleMeshSwap == null)
                {
                    foreach (var m in (activeDataProvider as SpatialObjectMeshObserver).Meshes.Values)
                    {
                        //Debug.Log("DOING GLOWING IN PREVIEW MESH");
                        //Debug.Log("Material" + m.Renderer.material);
                        GlowFadeWireframeMaterial(m, glowingStatus, baseColor, endBaseColor, wireColor, endWireColor, wireWidth, endWireWidth, 0.9f);
                    }
                }
                else
                {
                    foreach (var m in meshes)
                    {
                        GlowFadeWireframeMaterial(m, glowingStatus, baseColor, endBaseColor, wireColor, endWireColor, wireWidth, endWireWidth, 0.9f);
                    }
                }
            }
            #endif

            glowingStatus += glowingStep * glowingSpeed;

            yield return new WaitForSeconds(glowingStep);
        }

    }

    /* Lanza animaciones de malla y notificaciones de voz */
    private IEnumerator ShowScanningProcess()
    {
        float voiceNotificationInterval = 3f;

        while (UXNotifications.Instance == null) yield return new WaitForSecondsRealtime(2f);

        StartCoroutine(GlowAnimationAllmeshes());
        UXNotifications.Instance.ScanningNotification(1);
        while (scanning)
        {
            yield return new WaitForSeconds(voiceNotificationInterval);
            //UXNotifications.Instance.ScanningNotification(0);
        }
        //UXNotifications.Instance.EndScanningNotification();
    }

    /* Permite ejecutar un proceso de escaneado del entorno de duraci�n t */
    private IEnumerator ScanAndStopAfterTime(float t)
    {
        ResumeActiveDataProviders();
        yield return new WaitForSeconds(t);
        SuspendActiveDataProviders();
    }


    /* Recorre la lista de objetos de malla (real o de prueba y actualiza en la cach� de esta clase (objeto mesh) */
    public void RefreshCachedMesh()
    {
        if (meshes == null) meshes = new List<SpatialAwarenessMeshObject>();
        int iterator = 0;

        if (LogsOn)
            Utilities.Info("CACHING " + (inHololens ? " Real World " : " Simulated ") + "Mesh", this.GetType().ToString(),funcName:"RefreshCachedMesh");

        if (inHololens)
        {
            //Debug.Log("IN HOLOLENS REFRESHING CACHED MESH provider=" + (activeDataProvider as WindowsMixedRealitySpatialMeshObserver).Meshes.Count);
            foreach (var m in (activeDataProvider as WindowsMixedRealitySpatialMeshObserver).Meshes.Values)
            {
                meshes.Add(m);
                if (LogsOn) Debug.Log("Cached mesh " + iterator);
                iterator++;
            }
        }
        else
        {
        #if (UNITY_EDITOR)
            foreach (var m in (activeDataProvider as SpatialObjectMeshObserver).Meshes.Values)
            {
                meshes.Add(m);
                if (LogsOn) Debug.Log("Cached mesh " + iterator);
                iterator++;
            }
        #endif
        }

        if (LogsOn && meshes.Count > 0)
            Utilities.Info("Real World Mesh were CACHED SUCCESSFULLY; Count=" + meshes.Count,this.GetType().ToString());
        
    }



    
    /* Ejecuta al finalizarse el escaneo, de forma que la malla desaparezca por completo (CUANDO ESTAMOS EN HOLOLENS (usango Windows Mixed Reality Spatial Observer) */
    IEnumerator FadeWireframeOut()
    {
        float transitionstep = 0.11f;
        float currentstep = 0f;

        bool done = false;

        while (!done)
        {
            foreach (SpatialAwarenessMeshObject mesh in meshes)
            {
                //Cambio a material completamente transparente
                mesh.Renderer.material.Lerp(duringScanMaterial, inGameMaterial, currentstep);
            }
            yield return new WaitForSeconds(transitionstep);
            currentstep += transitionstep;
            if (currentstep >= 1) done = true;
        }
        if (LogsOn)
            Utilities.Info(" Mesh color is allegedly TRANSPARENT NOW ", this.GetType().ToString());
    }







    #region Speech Command Handling
    private void OnSpeechCommand(string commandDetected)
    {
        switch (commandDetected)
        {
            case "Scan Room":
            case "Escanear Habitaci�n":
                ResumeActiveDataProviders();
                break;
            case "Don't Scan Room":
            case "No escanear":
                SuspendActiveDataProviders();
                break;
        }
    }
    #endregion Speech Command Handling

}
