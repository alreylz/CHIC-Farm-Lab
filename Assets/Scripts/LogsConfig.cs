using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHIC;

using Object = UnityEngine.Object;


/* Clase que indexa todos los objetos activos del juego para controlar si crean logs o no */
[ExecuteAlways]
public class LogsConfig : MonoBehaviour , ISerializationCallbackReceiver /*Requerido para implementar la serialización de un diccionario */
{
    //Diccionario de referencias a Objetos con instancias existentes de una determinada clase
    public Dictionary<string, List<Object>> LivingObjectsIndex;
    public Dictionary<string, bool> LivingObjectsConfiguration;


   /* private void Update()
    {
        refreshConfigurationData();
    }
    */
    private void OnValidate()
    {
        if(gameObject.activeSelf)
        refreshConfigurationData();
    }




    private bool neglectNonCustomInstances(string classType)
    {
        if (classType.StartsWith("Microsoft")) return true;
        if (classType.StartsWith("TMPro")) return true;
        if (classType.StartsWith("UnityEngine")) return true;
        return false;
    }

    private void refreshConfigurationData()
    {
        foreach (var instance in Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour)))
        { //Obtiene todos los scripts que derivan de MonoBehaviour
            string classType = instance.GetType().ToString();
            if (!LivingObjectsIndex.ContainsKey(instance.GetType().ToString())) //Si ya se ha encontrado otra instancia previamente
            {
                //Descartar cosas con prefijos indeseados (MRTK components, TMPro... etc) =>  No interesa que aparezcan los Scripts no Custom
                if (neglectNonCustomInstances(classType)) continue;
                //Añade primera instancia a la lista de Objetos al diccionario <Nombre de clase, Lista de instancias >
                LivingObjectsIndex.Add(instance.GetType().ToString(), new List<Object> { instance});
                
            }
            else
            {
                //Descartar cosas con prefijos indeseados (MRTK components, TMPro... etc) =>  No interesa que aparezcan los Scripts no Custom
                if (neglectNonCustomInstances(classType)) continue;
                LivingObjectsIndex[instance.GetType().ToString()].Add(instance);  
            }

            if (!LivingObjectsConfiguration.ContainsKey(instance.GetType().ToString()))
            {
                //Obtiene configuración actual de la instancia (si están activos los logs o no) y lo añade a la muestra
                LivingObjectsConfiguration.Add(instance.GetType().ToString(), getLogConfiguration(instance));
            }
            else LivingObjectsConfiguration[instance.GetType().ToString()] = getLogConfiguration(instance);

        }            
        
    }

    public bool getLogConfiguration(Object instance)
    {
        NullableOutValue<bool> hasLogsOnNullable;
        if (!Utilities.getPropertyByName<Object, NullableOutValue<bool>, bool>(instance, "LogsOn", out hasLogsOnNullable))
        {
            Debug.LogWarning(instance.name + " has NO LogsOn property");
            return false;
        }
        else
        {
            Debug.Log(instance.name + " has got LogsOn property; `value = " + hasLogsOnNullable.ToString());
            return hasLogsOnNullable.Value;
        }
    }


        
    #region Dictionary Serialization

    public List<string> _classNames = new List<string> { "ClassA", "ClassB" };
    public List<List<Object>> _instances = new List<List<Object>>();
    public List<bool> _debugValues = new List<bool> { true, false };

        //Convertimos datos a listas antes de mostrar en el inspector
        public void OnBeforeSerialize()
        {
            _classNames.Clear();
            _instances.Clear();
            _debugValues.Clear();

            //Añadimos los componentes del diccionario en las dos primeras listas
            foreach (var kvp in LivingObjectsIndex)
            {
                _classNames.Add(kvp.Key); // Añadimos Nombres de clases encontradas
                _instances.Add(kvp.Value);
            }
            //Añadimos la configuración de los logs a la tercera lista
            foreach (var kvp in LivingObjectsConfiguration)
            {
                _debugValues.Add(kvp.Value);
            }
        }
        //Repoblación de Diccionarios al deserializar (pasar de datos guardados en disco a datos en inspector)
        public void OnAfterDeserialize()
        {
            LivingObjectsIndex = new Dictionary<string, List<Object>>();
            LivingObjectsConfiguration = new Dictionary<string, bool>();

            //Volvemos a poblar los diccionarios desde las listas
            for (int i = 0; i != Math.Min(_classNames.Count, _instances.Count); i++)
                LivingObjectsIndex.Add(_classNames[i], _instances[i]);
            for (int i = 0; i != Math.Min(_classNames.Count, _debugValues.Count); i++)
                LivingObjectsConfiguration.Add(_classNames[i],_debugValues[i]);

    }

        void OnGUI()
        {
       /*     foreach (var kvp in LivingObjectsIndex)
                GUILayout.Label("Type name: " + kvp.Key + " Objects: " + kvp.Value);
            foreach (var kvp in LivingObjectsConfiguration)
                GUILayout.Label("Type name " + kvp.Key + "logsActive " + kvp.Value);
                */
        }
    #endregion Dictionary Serialization




}
