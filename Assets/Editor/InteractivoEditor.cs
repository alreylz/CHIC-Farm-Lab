using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactivo))]
public class InteractivoEditor : Editor
{

    Interactivo _TheInstance;

    private void OnEnable()
    {
        _TheInstance =  target as Interactivo;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
       
        DrawDefaultInspector();

        EditorGUILayout.LabelField("Interaction Radius"+_TheInstance.interactionRadius);
        /* target object has reference to the object to be displayed (thus, allows acces to all the fields associated to a Spawn instance */

        //Information Box 
        EditorGUILayout.HelpBox(@"Interactivo:
        Este script se acopla a objetos 3D que pueden ser manipulados por parte del usuario, bien a través de 'Air Tap', 'Hold' o 'Manipulation' ", MessageType.Info);
    
        
        //Create a button in the inspetor
       /* if (GUILayout.Button("Simulate Interaction"))
        {
            _TheInstance.Interact();
        }*/


        //Mostrar campos enteros
        //sp.EnemiesLeft = EditorGUILayout.IntField("Enemies Left", (int)sp.EnemiesLeft); /* To allow assignation from inspector, we assign returned value to the field */
        //Mostrar campos no manipulables
        // EditorGUILayout.LabelField("Radius", sp.radius.ToString());
        serializedObject.ApplyModifiedProperties();
    }
}
