using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

using System.Text;
namespace CHIC
{

    public interface IPersistentObject<T> where T : class 
    {
        bool Save(string filename, SaveFormat format = SaveFormat.BINARY);
    }


    public class Singleton<T> : MonoBehaviour where T : class
    {
        protected static T _instance;
        public static T Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                if (_instance != null && _instance != value)
                {
                    Debug.LogWarning(typeof(T).ToString() + " : You tried to create two instances of a Singleton Object, so I removed the last one");
                    Destroy(value as UnityEngine.Object);
                }
                else
                {
                    _instance = value;
                }
            }
        }
    }


    #region Interval Class
    [System.Serializable]
    /*Clase genérica para declaración de intervalos en el juego (e.g. temperaturas soportadas por las plantas ) */
    public class Interval<T>
    {
        public T Min { get => min; }
        [SerializeField]
        private T min;
        public T Max { get => max; }
        [SerializeField]
        private T max;

        public Interval(T min, T max, bool minIncluded = true, bool maxIncluded = true)
        {
            this.min = min;
            this.max = max;
        }
    }

    /* Unity no permite serialización de generics; de ahí que haya tenido que crear estas clases derivadas no genéricas. 
     * De otra manera se pierde la capacidad editar los intervalos (son por tanto null); 
     * y esto impide usar los Scriptable Objects como blueprints de variedades de plantas o cualquier clase que quiera usar la clase Interval*/
    [System.Serializable]
    public class Interval_UInt : Interval<uint>
    {
        public Interval_UInt(uint min, uint max, bool minIncluded = true, bool maxIncluded = true) : base(min, max) { }
    }
    [System.Serializable]
    public class Interval_Float : Interval<float>
    {
        public Interval_Float(float min, float max, bool minIncluded = true, bool maxIncluded = true) : base(min, max) { }
    }
    #endregion Interval Class




    public static class GlobalConstants
    {
        
        /* Resuelve la ruta de guardado de datos persistentes según se esté trabajando en Unity o ejecutando en el dispositivo final */
        public static string getBaseApplicationPath(SavePathType pathType)
        {
            string outPath = "";
            switch (pathType)
            {
                case SavePathType.DEVICE_ASSET:
                    outPath = Application.persistentDataPath;
                    break;
                case SavePathType.PROJECT_ASSET:
                    outPath = Application.dataPath;
                    break;
            }
            return outPath;

        }



    }

    /*Clase utilizada para encapsular cualquier valor que puede ser conceptualmente nulo; de forma que se pueda usar con generics */
    public class NullableOutValue<T>
    {
        public bool IsNull = true;
        private T _value;
        
        public T Value { get { return _value; } set { IsNull = false; _value = value; } }

        public NullableOutValue(T valor)
        {
            IsNull = false;
            this._value = valor;
        }

        public NullableOutValue()
        {
        }
        public override string ToString()
        {
            if (IsNull)
            {
                return "null";
            }
            else
            {
                return _value.ToString();
            }
        }
    }

    public static class Utilities
    {

#if (UNITY_EDITOR)
        #region Console clear
        static MethodInfo _clearConsoleMethodRef;
        static MethodInfo clearConsoleMethodRef
        {
            get
            {
                if (_clearConsoleMethodRef == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                    Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                    _clearConsoleMethodRef = logEntries.GetMethod("Clear");
                }
                return _clearConsoleMethodRef;
            }
        }

        public static void clearUnityConsole()
        {
            clearConsoleMethodRef.Invoke(new object(), null);
        }
        #endregion Console clear
#endif

        /* T: instancia de la que se quiere saber el valor de la propiedad 
         * U: instancia de la clase nullableOutValue que encapsula el valor final de la propiedad
         * V: clase o tipo cuyo valor se quiere obtener */
        public static bool getPropertyByName<T, U, V>(T instance, string propertyName, out U value) where T : class where U : NullableOutValue<V>
        {
            foreach (var property in instance.GetType().GetProperties())
            {
                if (property.Name.ToLower() == propertyName.ToLower()) //Si encuentra una propiedad con el nombre sin tener en cuenta CAPS, asigna el valor y devuelve true porque lo ha encontrado
                {
                    try
                    {
                        V castAttempt = (V)property.GetValue(instance);
                        value = (U)new NullableOutValue<V>(castAttempt);
                        return true;
                    }
                    catch (InvalidCastException e)
                    {
                        value = (U)new NullableOutValue<V>();
                        return false;
                    }

                }
            }
            value = (U)new NullableOutValue<V>();
            return false;
        }


        /// <summary>
        /// ChooseRandomlyFromList: Dada una lista de elementos, obtiene un de los elementos de forma aleatoria 
        /// y devuelve la posición del elemento devuelto
        /// </summary>
        /// <typeparam name="T"> Tipo de los elementos de la lista </typeparam>
        /// <param name="listToChoose1From"> Lista de la que obtener 1 elemento</param>
        /// <param name="index"> parámetro de salida para la posición del elemento devuelto </param>
        /// <returns> Elemento de la lista obtenido aleatoriamente</returns>
        public static T ChooseRandomlyFromList<T>(List<T> listToChoose1From, out int index) where T : class
        {
            //Check the number of elements or is null
            if (listToChoose1From == null || listToChoose1From.Count < 1) { index = -1; return null; }
            //Choose one randonmy and return it
            index = UnityEngine.Random.Range(0, listToChoose1From.Count);
            return listToChoose1From[index];
        }


        public static void SetChildrenActiveRecursively(Transform parent, bool isActive, string hierarchy = "", bool debug = false)
        {
        
            hierarchy += "/" + parent.name;

            parent.gameObject.SetActive(isActive);

            foreach(Transform child in parent.transform)
            {
                SetChildrenActiveRecursively(child, isActive, hierarchy, debug);
            }
            
        }









        /// <summary>
        /// GetAllComponentsOfTypeInChildren: obtiene todos los componentes de un tipo especificado 
        /// en la jerarquía dada como origen a través del parámetro parent
        /// </summary>
        /// <typeparam name="T"> Tipo de componentes a encontrar y retornar </typeparam>
        /// <param name="parent"> Componente Transform desde el que partir para la búsqueda recursiva</param>
        /// <param name="allComponentsFound"> (Opcional) Lista sobre la que ir añadiendo los resultados</param>
        /// <param name="hierarchy"> (Opcional) Cadena identificadora de kerarquía recorrida </param>
        /// <returns></returns>
        public static List<T> GetAllComponentsOfTypeInChildren<T>(Transform parent, List<T> allComponentsFound = null, string hierarchy = "", bool debug=false)
        {
            if (allComponentsFound == null)
            {
                allComponentsFound = new List<T>();
            }

            //1.Busco en el padre y añado todos los componentes encontrados en el mismo
            T[] inParentComponents = parent.GetComponents<T>();
            hierarchy += "/" + parent.name;
            if (inParentComponents.Length > 0 && debug) {
                Debug.Log("GetAllComponentsOfTypeInChildren: In " + parent.name + " found " + inParentComponents.Length + " " + inParentComponents[0].GetType().ToString());
            }
            foreach (T comp in inParentComponents)
            {
                allComponentsFound.Add(comp);
            }
            //2. Recursividad, realizo el mismo proceso en los hijos
            foreach (Transform child in parent)
            {
                GetAllComponentsOfTypeInChildren<T>(child, allComponentsFound, hierarchy);
            }

            return allComponentsFound;
        }


        public static T GetFirstComponentOfTypeInChildren<T>(Transform parent, string hierarchy="", bool debug=false)
        {

            T found = default(T) ;

            //1.Busco en el padre y añado todos los componentes encontrados en el mismo a un array
            T[] inParentComponents = parent.GetComponents<T>();
            hierarchy += "/" + parent.name;
            //Si encuentra al menos 1 --> return
            if (inParentComponents.Length > 0)
            {
                if (debug)
                    Debug.Log("GetFirstComponentOfTypeInChildren: In " + parent.name + " found " + inParentComponents.Length + " " + inParentComponents[0].GetType().ToString());
                found = inParentComponents[0];
                return found ;
            }
            else
            {
                //2. Recursividad, realizo el mismo proceso en los hijos
                foreach (Transform child in parent)
                {
                     if( GetFirstComponentOfTypeInChildren<T>(child, hierarchy) is T sthFound)
                    {
                            return sthFound;
                    }
                }
                
            }

            return found;

        }


        public static void DebugList<T>(List<T> listToPrint, string scriptName, GameObject relatedContext = null, string funcName = "") where T : class 
        {
            StringBuilder strOut = new StringBuilder("");

            int index = 0;
            foreach( var el in listToPrint)
            {
                
                strOut.AppendFormat("[{0}]: {1}\n", index.ToString(), el.ToString());
                index++;
            }
            Info(strOut.ToString(), scriptName, relatedContext, funcName);

        }




        #region MRTK Helper Functions

        private static MixedRealityInputSystem _MRTK_Input;
        public static MixedRealityInputSystem MRTK_Input
        {
            get
            {
                MixedRealityServiceRegistry.TryGetService<MixedRealityInputSystem>(out _MRTK_Input);
                return _MRTK_Input;
            }
        }

        /* Función auxiliar para obtener todos los tipos de Input actions definidas en la configuración del MRTK */
        public static bool TryGetInputActions(out string[] descriptionsArray)
        {
            if (!MixedRealityToolkit.ConfirmInitialized() || !MixedRealityToolkit.Instance.HasActiveProfile)
            {
                descriptionsArray = null;
                return false;
            }

            MixedRealityInputAction[] actions = MRTK_Input.InputSystemProfile.InputActionsProfile.InputActions;

            descriptionsArray = new string[actions.Length];
            for (int i = 0; i < actions.Length; i++)
            {
                descriptionsArray[i] = actions[i].Description;
            }

            return true;
        }

        public static void printInputActions()
        {
            string[] availableInputActions;
            TryGetInputActions(out availableInputActions);

            if (availableInputActions != null ? true : false || availableInputActions.Length <= 0)
            {
                Debug.LogError("No input actions were found!");
            }
            else
            {
                foreach (string inputAction in availableInputActions)
                {
                    Debug.LogError("Input action: " + inputAction + " found");
                }
            }
        }
        #endregion MRTKHelperFunctions



        public static void SetLayerRecursively(Transform tGameObject, int layer)
        {
            tGameObject.gameObject.layer = layer;
            foreach (Transform child in tGameObject)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child, layer);
            }

        }

        
        #region Console code
        public static void Warn( string msgInfo, string scriptName, GameObject relatedContext=null, string funcName="") { Log(LogType.Warning, msgInfo,scriptName,relatedContext,funcName); }
        public static void Error(string msgInfo, string scriptName, GameObject relatedContext = null, string funcName = "") { Log(LogType.Error, msgInfo, scriptName, relatedContext, funcName); }
        public static void Info(string msgInfo, string scriptName, GameObject relatedContext = null, string funcName = "") { Log(LogType.Log, msgInfo, scriptName, relatedContext, funcName); }

        public static void Log(LogType logType, string msgInfo , string scriptName, GameObject related=null, string funcName="" ) {
            
            string optionalInfo = (related != null ? related.name : "") + "<i>" + funcName + "</i>";

            //COLORES UTILZIADOS EN LOS LOGS:
            string scriptNameDisplayColor = ColorUtility.ToHtmlStringRGBA(Color.magenta);
            string errorColor = ColorUtility.ToHtmlStringRGBA(Color.red);
            string logColor = ColorUtility.ToHtmlStringRGBA(Color.black);
            string warnColor = ColorUtility.ToHtmlStringRGBA(Color.yellow);

            switch (logType)
            {
                case (LogType.Error):
                    Debug.LogErrorFormat("<b><color={0}>[ERROR]</color></b> {1}. Check <color={2}><b>{3}.cs</b></color> {4}", errorColor , msgInfo, scriptNameDisplayColor, scriptName, optionalInfo);
                    break;
                case (LogType.Log):
                    Debug.LogFormat("<b><color={0}>[INFO]</color></b> {1}. Check <color={2}><b>{3}.cs</b></color> {4}", errorColor, msgInfo, scriptNameDisplayColor, scriptName, optionalInfo);
                    break;
                case (LogType.Warning):
                    Debug.LogWarningFormat("<b><color={0}>[WARN]</color></b> {1}. Check <color={2}><b>{3}.cs</b></color> {4}", errorColor, msgInfo, scriptNameDisplayColor, scriptName, optionalInfo);
                    break;
            }
        }
        #endregion Console code
    }
}