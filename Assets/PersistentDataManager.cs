using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine;
using CHIC;

public enum SaveFormat
{
    BINARY,
    JSON
}
public enum SavePathType
{
    DEVICE_ASSET,
    PROJECT_ASSET
}

/* PersistentDataManager: Funcionalidad públicamente accesible de carga y guardado de datos en disco */
public static class PersistentDataManager
{

    /* Save(): permite guardar (i.e. persistir) información relativa a un objeto en tiempo de ejecución. 
     * Ejemplo: guardar una variedad de planta creada durante el juego */
    public static  bool  Save(string filename, object obj_serializable, SavePathType pathBase = SavePathType.PROJECT_ASSET,  SaveFormat format = SaveFormat.BINARY)
    {
        return format == SaveFormat.BINARY ? SaveBinary(obj_serializable, filename, pathBase) : SaveJson(obj_serializable, filename, pathBase);
    }
    
    public static bool SaveBinary(object obj_serializable, string filename, SavePathType pathBase)
    {
        string fullpath = GlobalConstants.getBaseApplicationPath(pathBase) + "/" + filename ;
        return SaveBinaryFullPath(obj_serializable, fullpath);
    }
    public static bool SaveBinaryFullPath(object obj_serializable, string path)
    {
        try
        {
            using (FileStream file = File.Create(path))
            {
                new BinaryFormatter().Serialize(file, obj_serializable);
                file.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("{0} -> Binary Save Error: {1}", typeof(PersistentDataManager).ToString(), e.Message));
            return false;
        }
        Debug.Log(string.Format(" {0} -> successfully written to {0} ", obj_serializable.GetType(), path));
        return true;
    }
    
    public static object Load(string filename, SavePathType pathBase = SavePathType.PROJECT_ASSET , SaveFormat format = SaveFormat.BINARY)
    {
        string fullPath = GlobalConstants.getBaseApplicationPath(pathBase) +"/"+ filename;
        return format == SaveFormat.BINARY ? LoadBinary(fullPath) : LoadJson(fullPath);
    }
    public static object LoadFullPath(string fullPath, SaveFormat format = SaveFormat.BINARY)
    {
        return format == SaveFormat.BINARY ? LoadBinary(fullPath) : LoadJson(fullPath);
    }


    public static object LoadBinary(string fullPath)
    {
        string path = fullPath;

        if (File.Exists(path))
        {
            try
            {
                FileStream file = File.Open(path, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                object loaded = bf.Deserialize(file) as object;
                file.Close();
                return loaded;
            }
            catch(Exception e)
            {
                Debug.Log(string.Format("{0} -> error loading binary file {1} : ", typeof(PersistentDataManager).ToString(), path, e.Message));
            }
        }
        else
        {
            Debug.LogWarning(string.Format("{0} -> file at {1} does not exist!", typeof(PersistentDataManager), path));
        }
        return null;
    }

    public static bool SaveJson(object obj_serializable, string filename, SavePathType pathBase = SavePathType.PROJECT_ASSET)
    {
        Debug.LogWarningFormat("Saving {0} to JSON will only keep public fields persistent !", obj_serializable.GetType().ToString());
        try
        {
            string outJson = JsonUtility.ToJson(obj_serializable);
            string path = GlobalConstants.getBaseApplicationPath(pathBase) + "/" + filename;
            StreamWriter sWriter = new StreamWriter(path);
            sWriter.WriteLine(outJson);
            sWriter.Close();
            Debug.Log(string.Format(" {0} -> successfully written to {0} ", obj_serializable.GetType(), path));
            return true;
        }
       catch (Exception e)
        {
            Debug.LogError(string.Format("{0} -> JSON Save Error: {1}", typeof(PersistentDataManager).ToString(), e.Message));
        }
        return false;
        
    }

    public static object LoadJson(string fullpath)
    {
        object loadVar = null;
        string path = fullpath;
        string readJson = "";

        if (File.Exists(path))
        {
            try
            {
                StreamReader sReader = new StreamReader(path);
                string line;
                while ((line = sReader.ReadLine()) != null)
                {
                    readJson += line;
                }
                loadVar = JsonUtility.FromJson<object>(readJson);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("{0} -> JSON load Error while loading {1} : {2}", typeof(PersistentDataManager).ToString(), path, e.Message);
            }
        }
        else
        {
            Debug.LogWarning(string.Format("{0} -> file at {1} does not exist!", typeof(PersistentDataManager), path));
        }
        return loadVar;
    }

    public static string [] GetAllFilepathsWithExtension(string extension, SavePathType pathBase = SavePathType.PROJECT_ASSET)
    {
        if (!extension.StartsWith("*.")) extension = "*." + extension;
        string baseSearchDir = GlobalConstants.getBaseApplicationPath(pathBase);

        return Directory.GetFiles(baseSearchDir, extension,SearchOption.AllDirectories);

    }



}
