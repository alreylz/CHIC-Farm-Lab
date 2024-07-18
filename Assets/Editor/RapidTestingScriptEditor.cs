using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RapidTestingScript))]
public class RapidTestingScriptEditor : Editor
{

    RapidTestingScript theInstance;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        theInstance = (RapidTestingScript)target;

        if (theInstance.FunctionsToTest == null) return;
        /*var invokeList = theInstance.FunctionsToTest.GetInvocationList();
        if (invokeList == null) return;
        foreach(var method in invokeList)
        {
            if (GUILayout.Button(method.ToString()))
            {
                method.DynamicInvoke(null);

            }

        }*/
       

        if( GUILayout.Button("Execute All"))
        {
            theInstance.FunctionsToTest.Invoke();
        }

    }
}
