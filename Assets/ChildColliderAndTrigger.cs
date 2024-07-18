using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider)),RequireComponent(typeof(SphereCollider))]
public class ChildColliderAndTrigger : MonoBehaviour
{

    public Action<Collision> collisionEnterEvent;
    public Action<Collision> collisionExitEvent;
    public Action<Collision> collisionStayEvent;
    public Action<Collider> triggerEnterEvent;
    public Action<Collider> triggerStayEvent;
    public Action<Collider> triggerExitEvent;



    private void OnCollisionEnter(Collision collision)
    {
        collisionEnterEvent?.Invoke(collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        collisionStayEvent?.Invoke(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        collisionExitEvent?.Invoke(collision);
    }
   

    private void OnTriggerExit(Collider other)
    {
        triggerExitEvent?.Invoke(other);
    }
    private void OnTriggerEnter(Collider other)
    {
        triggerEnterEvent?.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        triggerStayEvent?.Invoke(other);
    }

   
}
