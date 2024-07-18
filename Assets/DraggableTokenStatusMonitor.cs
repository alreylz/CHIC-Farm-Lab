using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Informa a otros componentes que están pendientes del token de:
// si está siendo manipulado
// si ha colisionado con algo ( a través de un evento)
[RequireComponent(typeof(Rigidbody)),RequireComponent(typeof(ManipulationHandler))]
public class DraggableTokenStatusMonitor : MonoBehaviour
{
    
    [SerializeField]
    Rigidbody _Rigidbody;
    [SerializeField]
    ManipulationHandler _ManipulationHandler;
    [SerializeField]
    Collider _TokenCollider; // Collider ( tipo derivado que se quiera para representar el espacio de colisión del objeto).
    
    
    public Action<Collision> OnTokenCollided;


    private void OnCollisionEnter(Collision collision)
    {
        OnTokenCollided?.Invoke(collision);
    }
    
    public bool IsGrabbed;

    private void Awake()
    {
        if (_Rigidbody == null) _Rigidbody = GetComponent<Rigidbody>();
        if (_ManipulationHandler == null) _ManipulationHandler = GetComponent<ManipulationHandler>();
        if (_TokenCollider == null) _TokenCollider = GetComponent<Collider>();

        DefaultConfig();
    }
    
    void DefaultConfig()
    {
        PreventFall();
        EnableTokenManipulation();
        _TokenCollider.isTrigger = false; //--> Collider can't be a trigger to work well along with Manipulation Handler script.
        _ManipulationHandler.OnManipulationStarted.AddListener( ev => IsGrabbed = true);
        _ManipulationHandler.OnManipulationEnded.AddListener( ev => IsGrabbed = false);
    }

    public void DisableTokenManipulation()
    {
        _ManipulationHandler.enabled = false;
        //[PENDING] SHOW DISABLED MESSAGE
    }

    public void EnableTokenManipulation()
    {
        _ManipulationHandler.enabled = true;
    }

    public void AllowFall()
    {
        _Rigidbody.useGravity = true;
    }

    public void PreventFall()
    {
        _Rigidbody.useGravity = false;
    }

    public void SetVelocityZero()
    {
        _Rigidbody.velocity = Vector3.zero;
        _Rigidbody.angularVelocity = Vector3.zero;
    }


}
