//using UnityEditor;
//using UnityEngine;
//using System.Reflection;
////[CustomEditor(typeof(LabController))]
//public class LabControllerEditor : Editor
//{

//    LabController lb;
//    private void Awake()
//    {
//       lb = target as LabController;
//    }

//    public override void OnInspectorGUI()
//    {

//        base.OnInspectorGUI();


//        /*EditorGUILayout.Separator();
//        EditorGUILayout.Foldout(true, "DEBUG - METHOD CALLS");


//        foreach (var func in lb.GetType().GetMethods()){
            
//            if (func.IsConstructor) continue;
//            if(func.DeclaringType != typeof(LabController)) continue;
//            if (func.GetParameters().Length > 0) continue;

//            if (GUILayout.Button(func.Name))
//            {
//                func.Invoke(lb, null);
//            }
            
//        }
//        */


        



//    }
//}
