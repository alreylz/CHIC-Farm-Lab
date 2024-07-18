using UnityEngine;

public class BroadcastChildCollider_Test : MonoBehaviour
{
    public void Collider_TestEnter(Collision collision)
    {
        Debug.Log("Child Collider Enter");
    }
    public void Collider_TestStay(Collision collision)
    {
        Debug.Log("Child Collider Stay");
    }
    public void Collider_TestExit(Collision collision){
        Debug.Log("Child Collider Exit");
    }


    public void Trigger_TestEnter(Collider collider)
    {
        Debug.Log("Child Trigger Enter");
    }
    public void Trigger_TestStay(Collider collider)
    {
        Debug.Log("Child Trigger Stay");
    }
    public void Trigger_TestExit(Collider collider)
    {
        Debug.Log("Child Trigger Exit");
    }
}
