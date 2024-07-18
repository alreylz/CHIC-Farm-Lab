using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.UI;

/* Estructura de solo datos que guarda los datos de un MOB (representa a un enemigo en el juego) */
[System.Serializable]
public class MobData
{ 
    public new string name;
    private static uint _EnemyCounter = 0;
    private static uint EnemyCounter { get => _EnemyCounter++; } 
    public float Health { get; private set; }
    public float BaseHealth { get; private set; }
    public float Damage { get; private set; }
  
    [Range(0,0.7f)]
    public float toughness=1; 

    public void HitMob( float ToApplyDamage)
    {
        Health -=  toughness * ToApplyDamage;
    }
    
    /* Constructor para creación de MobData sin necesidad de un asset (recurso persistente de Unity) */
    public MobData(string name,float health, float damage)
    {
        this.name = name + EnemyCounter.ToString();
        Health = health;
        BaseHealth = Health;
        Damage = damage;
    }


    /* Constructor a partir de un Scriptable Object modelando las tipologías de enemigos */
    public MobData(EnemyBlueprint blueprint )
    {
        this.name = blueprint.name + EnemyCounter.ToString() ;
        this.Health = blueprint.health;
        this.Damage = blueprint.HitDamage;
    }

}

public class MobBehavior : MonoBehaviour
{
    #region Debug Property
    [SerializeField]
    private bool _logsOn = false;
    public bool LogsOn { get { return _logsOn; } set { _logsOn = value; } }
    #endregion Debug Property

    public MobData mobData; /* Datos básicos de un enemigo para control de su ciclo de vida */
    public GameObject objective; /* Objeto hacia el que se va a dirigir un enemigo para atacar */

    [Header("Mob Mesh collider/Trigger monitoring")]
    public ChildColliderAndTrigger actualMobAnimated;
    
    
    public bool keepStatic = true;
    public Rigidbody rb;

    public float speed = 0.02f; /* Velocidad de movimiento */

    #region Manejo de Collisiones (Daño a plantas, usuario, etc. ; y daño al enemigo mismo) 

    /* Veces por segundo que un enemigo puede hacer daño a un objeto al colisionar con él (usado a la hora de infringir daño a las plantas) */
    protected float collisionThreshold = 0.5f;
    
    /* Timers para controlar el tiempo que pasó desde que ocurre la última colisión */
    protected float timer;
    protected float lastCollissionTime;

    #endregion

    //Evento a la que elementos interesados en los datos del enemigo concreto pueden suscribirse
    public Action<MobData> MobDataChanged;

    public SkinnedMeshRenderer ActualMobRenderer ;

    /* Color por defecto, guardado para crear una animación básica al ser golpeados */
    [SerializeField]
    protected Color defaultColor;

    bool moving = true;

    public AudioSource audioSrc;
    public AudioSource audioDamage;


    public Animator flyAnimator;


    /* Guarda la dirección hacia la que se mueve el enemigo */
    public Vector3 DirectionVector {get; private set; }

    public Material firstMaterial;


    private void TriggerEnter(Collider other)
    {
        if (other.tag == "MainCamera") // este esel player
        {
            //Debug.Log("I " + gameObject.name + " hit the player!");
            //GameManager.Instance.ThePlayer.gameObject.GetComponent<BleedBehavior>().Attack();

            //Move far away from user in direction
            //transform.Translate(DirectionVector * UnityEngine.Random.Range(2, UnityEngine.Random.Range(4,5)));
        }
        else
        {
            //Debug.Log("ENTERING TRIGGER FLY!! " + other.gameObject.name);
            FarmlandManager fm = other.gameObject.GetComponent<FarmlandManager>();
            if (fm == null) return;
            //opcion 1

            fm.DamagePlant(mobData.Damage);
            //Debug.Log("HURTING PLANT");
        }
        lastCollissionTime = Time.realtimeSinceStartup;
    }

    private void TriggerStay(Collider other)
    {
        timer = Time.realtimeSinceStartup;

        if ((timer - lastCollissionTime) > 1/collisionThreshold )
        {
            TriggerEnter(other);
        }


    }


        IEnumerator MakeSoundFromTimeToTime()
    {
        audioSrc.loop = false;
        while (true)
        {
            if (!audioSrc.isPlaying) audioSrc.Play();
            yield return new WaitForSecondsRealtime(4f);
            audioSrc.Pause();
            yield return new WaitForSecondsRealtime(20f);

        }
    }
    
    public void Awake()
    {
        // [PROVISIONAL] Crea un objeto de datos con la información relativa al ciclo de vida de un enemigo ; de forma que no necesitemos crear un blueprint y asignarlo desde el inspector
        mobData = new MobData("Fly", 300f, 20f);
        // Obtiene el objetivo hacia el que dirigirse para infligir daño
        GetMyObjective();

        ActualMobRenderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();
        
        timer = 0;
        defaultColor = ActualMobRenderer.material.GetColor("_Color");

        if(flyAnimator == null) { flyAnimator = transform.Find("Fly").gameObject.GetComponent<Animator>(); } 


        if (audioSrc == null) audioSrc = gameObject.GetComponents<AudioSource>()[0];
        if (audioDamage == null) audioDamage = gameObject.GetComponents<AudioSource>()[1];

        flyAnimator.SetTrigger("startFlying");
        audioDamage.loop = false;
        audioDamage.playOnAwake = false;
        
        StartCoroutine(MakeSoundFromTimeToTime());

        
        //SUBSCRIPTION TO EVENTS OF MOB CHILD
        actualMobAnimated.collisionEnterEvent += CollisionEnter;
        actualMobAnimated.collisionExitEvent += CollisionExit;
        
        actualMobAnimated.triggerEnterEvent += TriggerEnter;
        actualMobAnimated.triggerStayEvent += TriggerStay;


    }

    IEnumerator HurtAnimation()
    {
        ActualMobRenderer.material.SetColor("_Color",Color.red);
        yield return new WaitForSeconds(2f);
        ActualMobRenderer.material.SetColor("_Color", defaultColor);
    }



    /* Ejecutor de daño por parte del usuario */
    public void OnUserShoot()
    {
        //DAÑO POR DEFECTO
        float damage = 60.0f;
        audioDamage.Play();

        //Aplicamos fuerza para simular golpe a Mob
        rb.AddForce(0, 0, 6);

        mobData.HitMob(damage);
        StartCoroutine(HurtAnimation());

        if (mobData.Health <= 0)
        { //Animación de muerte cutre máx


            flyAnimator.SetTrigger("dieNow");
            rb.mass = 200;
            moving = false;
            rb.useGravity = true;
            
            Spawner.Instance.EnemiesInScene.Remove(gameObject);
            audioSrc.Stop();
            Destroy(gameObject, 2f);
            
        }
        MobDataChanged.Invoke(mobData);
        
    }
    





    //A IMPLEMENTAR:
    //Sufrir daño si algún objeto de la capa "Proyectile" golpea al enemigo (a este objeto); necesario un Collider
    private void CollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Players")
        {
            moving = false;
            //Debug.Log("Player encountered");
            
            //HURT PLAYER
        }
        else
        {
            Debug.Log("COLLIDING WITH STH FLY!! " + collision.gameObject.name);
            


            //FarmlandManager fm = collision.gameObject.GetComponent<FarmlandManager>();
            
            //fm.DamagePlant(mobData.Damage);
            //Debug.Log("HURTING PLANT");

        }
        //Debug.Log("COLLIDING WITH STH FLY!! " + collision.gameObject.name);

    }

    private void CollisionExit(Collision collision)
    {
        //if(collision.gameObject.tag == "Players")  moving = true;
    }


    public  void  ShowMatabichos()
    {
        StartCoroutine(ShowMatabichosFor());
    }


    private IEnumerator ShowMatabichosFor()
    {

        GameObject steam = GameManager.Instance.ThePlayer.transform.Find("PressurisedSteam").gameObject as GameObject;

        steam.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);

        steam.SetActive(false);

    }

    
    // Obtiene un elemento hacia el que enemigo se va a dirigir para atacar
    public void GetMyObjective()
    {
        if (objective == null) objective = RealWorldOverlay.Instance.getRandomItem() as GameObject;
        if (objective == null) { objective = GameManager.Instance.ThePlayer.gameObject; Debug.Log("Enemy assigned to attack Player"); }

        transform.GetComponent<Billboard>().TargetTransform = objective.transform;

    }

    // TZ Funcion que quita el objetivo. Se llama cuando ese objetivo se va a borrar de la escena
    public void RemoveObjective()
    {
        objective = null;
        transform.GetComponent<Billboard>().TargetTransform = null;

    }

    public void moveTowardsObjective()
    {
        //Calculamos vector de dirección Enemigo->Objetivo = Objetivo.position - Enemigo.position
        Vector3 dirVector = objective.transform.position - transform.position;
        DirectionVector = dirVector;
        float stopDistance = 0.2f; //Distancia del objeto objetivo a la que se tiene parar
        if (Vector3.Magnitude(dirVector) >= stopDistance)
        {
            //Puede expandirse para que haya un poco de movimiento random
            transform.Translate(dirVector * Time.deltaTime * speed);
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (objective == null)
        {
            // Si el objetivo esta a nulo porque la plata o el terreno han desaparecido que ataque al jugador
            objective = GameManager.Instance.ThePlayer.gameObject; Debug.Log("Enemy assigned to attack Player");
            transform.GetComponent<Billboard>().TargetTransform = objective.transform;
        }

        if (!keepStatic && moving) moveTowardsObjective();
    }

}
