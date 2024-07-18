using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
[CustomEditor(typeof(FarmlandManager))]
public class FarmlandManagerEditor : Editor
{

    //Permite  mostrar y ocultar todos los elementos de una instancia, no tan solo campos públicos.
    FarmlandManager farmlandObj;
    SerializedProperty plantsInField;


    private void OnEnable()
    {
        farmlandObj = target as FarmlandManager;
        plantsInField = serializedObject.FindProperty("instantiatedPlantGOs");

    }

    public override void OnInspectorGUI()
    {
       serializedObject.Update(); // Antes de imprimir el inspector, obtiene los últimos valores en el objeto a mostrar

        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Has plant growing?", farmlandObj.IsGrowingPlant ? "YES" : "NO");

        if (farmlandObj.IsGrowingPlant)
        {
            List<GameObject> plant3DList = farmlandObj.getAllGrowingPlants();
            EditorGUILayout.LabelField("Plant count: ", plant3DList.Count.ToString());

            EditorGUILayout.BeginVertical();
            foreach (var p in plant3DList)
            {
                EditorGUILayout.LabelField(p.transform.position.ToString(), p.name);
            }
            EditorGUILayout.EndVertical();

        }


        serializedObject.ApplyModifiedProperties(); // Aplicar cambios 
        
    }
}
