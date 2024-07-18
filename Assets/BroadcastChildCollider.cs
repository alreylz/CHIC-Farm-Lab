using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnCollisionEnterChild : UnityEvent<Collision> { };
[System.Serializable]
public class OnCollisionExitChild : UnityEvent<Collision> { };
[System.Serializable]
public class OnCollisionStayChild : UnityEvent<Collision> { };
[System.Serializable]
public class OnTriggerEnterChild : UnityEvent<Collider> { };
[System.Serializable]
public class OnTriggerStayChild : UnityEvent<Collider> { };
[System.Serializable]
public class OnTriggerExitChild : UnityEvent<Collider> { };


public class BroadcastChildCollider : MonoBehaviour
{
    [SerializeField]
    new public string name = "stdBroadcastCollider";
    public Collider broadcastCollider;

    [Header("Event Subscription (Forward a parent object)")]
    public OnTriggerEnterChild TriggerEnter;
    public OnTriggerStayChild TriggerStay;
    public OnTriggerExitChild TriggerExit;
    public OnCollisionEnterChild CollisionEnter;
    public OnCollisionStayChild CollisionStay;
    public OnCollisionExitChild CollisionExit;

    private void Init()
    {
        if (broadcastCollider == null)
        {
            broadcastCollider = transform.GetComponent<Collider>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if(collision.collider == broadcastCollider)
            CollisionEnter?.Invoke(collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        //if (collision.collider == broadcastCollider)
            CollisionStay?.Invoke(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        //if (collision.collider == broadcastCollider)
            CollisionExit?.Invoke(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter?.Invoke(other);
    }
    private void OnTriggerStay(Collider other)
    {
        TriggerStay?.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        TriggerExit?.Invoke(other);
    }

}
