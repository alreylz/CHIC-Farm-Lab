using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    
    SerializedProperty sProp;

    private void OnEnable()
    {
      // sProp =  serializedObject.FindProperty("LookTo");
       // sProp2 = serializedObject.FindProperty("rotation");
    }

    public override void OnInspectorGUI()
    {
        //serializedObject.Update();
        
        //base.OnInspectorGUI();
       DrawDefaultInspector(); 
        /* target object has reference to the object to be displayed (thus, allows acces to all the fields associated to a Spawn instance */
     /*   Spawner sp = target as Spawner;

        //Information Box 
        EditorGUILayout.HelpBox(@"Spawner:
        Este script maneja el engendrado dinámico de objetos tales como enemigos. Por defecto los objetos se crean en paralelos al plano XZ (ejes globales) y a una distancia respecto a un origen provisto desde el inspector.", MessageType.Info);
 
        //Create a button in the inspetor
        if (GUILayout.Button("Spawn Object"))
        {
            if (Application.isPlaying == true)
                sp.SpawnFromOrigin();
            else Debug.Log("Can't Spawn objects when the game is not running !");
        }

        */
        //Mostrar campos enteros
        //sp.EnemiesLeft = EditorGUILayout.IntField("Enemies Left", (int)sp.EnemiesLeft); /* To allow assignation from inspector, we assign returned value to the field */
        //Mostrar campos no manipulables
        // EditorGUILayout.LabelField("Radius", sp.radius.ToString());
        //serializedObject.ApplyModifiedProperties();
        
    }
}
