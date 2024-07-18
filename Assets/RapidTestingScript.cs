using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class RapidTestingScript : MonoBehaviour
{

    public UnityEvent FunctionsToTest;

   
    public void InvokeFunctions()
    {
        FunctionsToTest?.Invoke();
    }

}
