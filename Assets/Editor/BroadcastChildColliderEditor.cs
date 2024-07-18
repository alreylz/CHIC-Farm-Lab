using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BroadcastChildCollider))]
public class BroadcastChildColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = target as BroadcastChildCollider;
        if (obj.broadcastCollider == null)
        {
            EditorGUILayout.LabelField("Collider: ", "Set broadcastCollider to sth!");
        }
        else
        {


            EditorGUILayout.LabelField("Collider type", obj.broadcastCollider.GetType().ToString());
            EditorGUILayout.LabelField("isTrigger", obj.broadcastCollider.isTrigger.ToString());
        }

        base.OnInspectorGUI();
        
    }
}
