﻿using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(anchorTool))]
class anchorToolEditor : Editor
{
    void OnSceneGUI()
    {
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            anchorTool myTarget = (anchorTool)target;
            myTarget.StopDrag();
        }
    }
}

//This script must be placed in a folder called "Editor" in the root of the "Assets"
//Otherwise the script will not work as intended
