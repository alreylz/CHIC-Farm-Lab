using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ConstraintRotationAxis
{
    FixedX,
    FixedY,
    FixedZ,
    FixedXY,
    FixedXZ,
    FixedYZ,
    FixedAll
}


public class FollowUser : MonoBehaviour
{

    public GameObject toFollow;
    public ConstraintRotationAxis constraints;
    public bool KeepOffset = true;

    public float deltaX = 0;
    public float deltaY = 0;
    public float deltaZ = 0;

    private Vector3 lastFramePos;


    private void Awake()
    {
        lastFramePos = toFollow.transform.position;
    }
    
    void Update()
    {
        if (KeepOffset) { 
            Vector3 deltaVector = toFollow.transform.position - lastFramePos;
            deltaX = deltaVector.x;
            deltaY = deltaVector.y;
            deltaZ= deltaVector.z;
            transform.Translate(deltaVector);
        }
        else
        {
            transform.position = toFollow.transform.position; ;
        }
        lastFramePos = toFollow.transform.position;

    }
}
