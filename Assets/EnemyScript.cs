using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
/* Representa un tipo de datos no modificable, se pasa por valor */
public struct MobInfo{

    public string name;
    public float baseHealth;
    public float health;
    public  string damage;
         
    public MobInfo( string name, float baseHealth, float health, float damage)
    {
        this.name = name;
        this.baseHealth = baseHealth;
        this.health = health;
        this.damage = damage.ToString();
    }

}

//public class EnemyScript : //Interactivo, IMixedRealityPointerHandler
//{
//    /*Representa el tipo y los atributos de inicialización del enemigo*/
//    public EnemyBlueprint enemyPreset;

//    protected string _name;
//    protected float baseHealth;
//    protected float healthPoints;
//    protected float enemyDamage;

//    /*propiedad que devuelve de por si la información del Mob*/
//    private MobInfo GetMobInfo
//    {
//        get { return new MobInfo(_name, baseHealth, healthPoints, enemyDamage); }
//    }


//    public delegate void dataChanged(MobInfo info);
//    public dataChanged onDataChanged;


//    /* Color por defecto, guardado para crear una animación básica al ser golpeados */
//    [SerializeField]
//    protected Color defaultColor ;

//    /*Veces por segundo que un enemigo puede hacer daño a un objeto al colisionar con él (usado a la hora de infringir daño a las plantas) */
//    protected float collisionThreshold = 2;
//    /*Timers para controlar el tiempo que pasó desde que ocurre la última colisión*/
//    protected float timer;
//    protected float lastCollissionTime;

//    //Contador de colisiones en el intervalo de tiempo
//    protected float numCurrCollisions = 0;
    


//    //Controlador del sonido del enemigo
//    public AudioSource audioSrc;
//    public AudioSource audioDamage;


//    public Material firstMaterial ;

//    public override void Awake()
//    {
//        base.Awake();

//        //if(enemyPreset == null ) enemyPreset = Resources.Load("ScriptableObjects/DefaultEnemy") as EnemyBlueprint;
//        //Inicializamos el enemigo con los datos que vienen dados por el preset utilizado (e.g. Mosca)
//        this._name = enemyPreset.name;
//        this.baseHealth = enemyPreset.health;
//        this.healthPoints = enemyPreset.health;
//        this.enemyDamage = enemyPreset.HitDamage;
//        timer = 0;
//        defaultColor = gameObject.GetComponentInChildren<Renderer>().material.GetColor("_Color");
//        firstMaterial = gameObject.GetComponentInChildren<Renderer>().material;
//        //interactionCollider.isTrigger = false;
//        if(audioSrc==null) audioSrc = gameObject.GetComponents<AudioSource>()[0];
//        if (audioDamage == null) audioDamage = gameObject.GetComponents<AudioSource>()[1];
//        audioDamage.loop = false;
//        audioDamage.playOnAwake = false;

//    }

//   /* public override void Interact() {

//        gameObject.GetComponentInChildren<Renderer>().material.SetColor("_Color", Color.red);
//        this.healthPoints -= 30;
//        audioDamage.Play();
//        onDataChanged.Invoke(GetMobInfo);
//        if (healthPoints <= 0) { //Animación de muerte cutre máx
//            GetComponent<Rigidbody>().useGravity = true;
//            GetComponent<Rigidbody>().mass = 200;
//            Destroy(gameObject, 0.5f);
//            audioSrc.Stop();
//        }


//    }

//        */

//public void OnPointerClicked(MixedRealityPointerEventData eventData)
//    {
//        //Debug.Log("Pointer clicked");
//    }

//    public void OnPointerDown(MixedRealityPointerEventData eventData)
//    {
        
//        //Debug.Log("Pointer down");
//        //gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
//        // healthPoints -= 50;
//        //Debug.Log("ENEMY: <b>" +this.name +"</b> WAS HIT");
//        //this.healthPoints -= 30;
//        //los datos han cambiado en el enemigo, lo notificamos.
//        //onDataChanged.Invoke(GetMobInfo);
//        //if (healthPoints <= 0) { //Animación de muerte cutre máx
//         //   GetComponent<Rigidbody>().useGravity = true;
//         //   Destroy(gameObject, 2);
//       // }
        

//    }

//    public void OnPointerDragged(MixedRealityPointerEventData eventData)
//    {

//       // Debug.Log("Pointer dragged");
//    }

//    public void OnPointerUp(MixedRealityPointerEventData eventData)
//    {
//        Debug.Log("POINTER UP !");
//        gameObject.GetComponentInChildren<Renderer>().material.SetColor("_Color", defaultColor);
//    }



//    /* public void applyDamage ( float damage)
//     {
//         healthPoints -= damage;
//         gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
//     }
//     */


//    void Update()
//    {
//        if(gameObject.GetComponentInChildren<Renderer>().material.GetColor("_Color") != defaultColor) gameObject.GetComponentInChildren<Renderer>().material.SetColor("_Color", defaultColor);
//        if (!audioSrc.isPlaying)
//        {
//            if (healthPoints <= 0) //si ya no tiene vida, no hacemos nada
//            {
               
//            }
//            else
//            {
//                //
//                //audioDamage.Stop();
//                audioSrc.UnPause();
//            }
            
//        }
//    }




//    // Update is called once per frame
//    /*   void Update()
//       {
//           //Material con renderer
//           gameObject.GetComponent<Renderer>().material = firstMaterial;

//           //Debug.Log("Timer:" + timer);
//           //Debug.Log("Last Collission timer" + lastCollissionTime);
//           //Debug.Log("Elapsed since last collission" + (timer - lastCollissionTime));


//           if(gameObject.GetComponent<SphereCollider>() != null) { 
//               timer = Time.time;

//               //Check if the list of colliders overlapping the sphere is non-empty (Si pueden hacerse más colisiones en este segundo o si ya ha pasado tiempo suficiente para colisionar de nuevo...
//              if( numCurrCollisions < collisionThreshold  || (timer - lastCollissionTime > 1)) {
//                   //Hacemos el overlap esférico para ver  qué colliders se cruzan con el de nuestro objeto (solamente si choca con un jugador [Player layer])

//                   Collider [] colls = Physics.OverlapSphere(transform.position, transform.GetComponent<SphereCollider>().radius, LayerMask.GetMask("Plants"));
//                   if (!(colls.Length <= 0))
//                   {
//                       if (timer - lastCollissionTime > 0) { numCurrCollisions = 0; }
//                       Debug.Log("-------------------------"+transform.gameObject.name + "'s Collission detected with "+ colls[0].gameObject.name);
//                       foreach (Collider col in colls)
//                       {
//                           col.gameObject.SendMessage("applyDamage", enemyDamage);
//                       }
//                       lastCollissionTime = Time.time;
//                       timer = lastCollissionTime;

//                       numCurrCollisions++;
//                   }


//               }

//           }


//       }
//       */





//}
