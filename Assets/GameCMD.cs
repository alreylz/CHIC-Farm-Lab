using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameCMD : MonoBehaviour
{

    public bool useTerminal;
    
    public int originX;
    public int originY;
    public int width;
    public int height;
    private string terminalContent = "Enter a command here";


    private void Start()
    {
        PlayerSpeechManager.onSpeechRecognition += OnSpeechCommand;

    }
    

    #region Terminal GUI
    private void OnGUI()
    {
        if (useTerminal)
            GUI.Window(0, new Rect(new Vector2(originX, originY), new Vector2(width, height)), DebugWindowContent, "Debug window");
    }
    private void DebugWindowContent(int winID)
    {
        //GUILayout.Label("Object onFocus: " + objectOnFocus);
        
        if (!(terminalContent = GUILayout.TextField(terminalContent)).StartsWith("!"))
        {
            terminalContent = "Enter a command here";
        }
        else
        {
        }
        if (GUILayout.Button("Run") || Input.GetKeyDown(KeyCode.Return))
        {
            switch (terminalContent)
            {
                case "!openInventory":
                    //  MR_InputSystem.RaiseSpeechCommandRecognized(MR_InputSystem.DetectedInputSources.ToReadOnlyCollection()[0], Microsoft.MixedReality.Toolkit.Utilities.RecognitionConfidenceLevel.High, new System.TimeSpan(20), new System.DateTime(), new SpeechCommands("Open Inventory", KeyCode.None, MixedRealityInputAction.None));
                    break;
                case "!killAll":
                    PlagueAnnihilation();
                    break;
                case "!spawnPlague":
                    Debug.Log("COMMAND SPAWN PLAGUE");
                    SpawnPlague();
                    break;
                case "!reset":
                    Reset();
                    break;
            }
        }
    }
    #endregion Terminal GUI


    //COMMANDS:
    #region Commands 
    //Inventary Commands
    public static bool OpenInventory()
    {
        FindObjectOfType<InventoryUI>().OpenInventory();
        return true;
    }
    public static bool CloseInventory()
    {
        FindObjectOfType<InventoryUI>().CloseInventory();
        return true;
    }
    public static void InventoryFollowUser(bool setOn)
    {
        FindObjectOfType<InventoryUI>().SetFollowUser(false, setOn);
    }
    public static void InventoryStatic() { InventoryFollowUser(false); }
    //Shopping Commands
    public static void OpenShop() => FindObjectOfType<ShopUI>().OpenShop();
    public static void CloseShop() => FindObjectOfType<ShopUI>().CloseShop();
    public static void AddMoney(int amount) => GameManager.Instance.AddMoney(amount);
    public static void SubstractMoney(int amount) => GameManager.Instance.AddMoney(amount);
    //Minigame (DNA Editing)
    public static bool StartMiniGame() {
        GameObject lookingAt = FindObjectOfType<Player>().ObjectOnFocus;
        try
        {
            Instantiate(Resources.Load("Prefabs/DNA-MiniGame"), FindObjectOfType<Player>().ImpactPoint, Quaternion.identity, GameManager.GameWorld.transform);
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
    }
    //Eventualities
    public static bool SpawnPlague()
    {
        FindObjectOfType<Spawner>().OnDemandSpawn();
        return true;
    }
    public static void PlagueAnnihilation()
    {
        if (Spawner.Instance.EnemiesLeft == 0) return;
        Spawner.Instance.KillAll();
    }
    //Ancillary
    public static void Reset()
    {
        //Matar moscas
        PlagueAnnihilation();
        //Eliminar terrenos
        RealWorldOverlay.Instance.DestroyAllFields();
        //Quitar sol y nubes
        try
        {
            ShowHideWeather("Sun", "hide");
            ShowHideWeather("CloudStorm", "hide");
        }catch(Exception e)
        {

        }
        //Retornar dinero a inicial
        GameManager.Instance.ResetGameManager();
        //Cerrar inventario y tienda
        CloseInventory();
        CloseShop();
        //Reset StarterPack
        ItemInventory.Instance.ResetToInitial();
    }
    public static bool PlayAudioInst(string instId)
    {
        UXNotifications.Instance.PlayAudioInstructions(instId);

        return true;
    }


    public static bool ShowHideWeather(string tipo, string status)
    {

        if (status == "show")
        {
            GameObject weatherObject = (GameObject)(GameManager.Instance.weatherDict[tipo]);
            weatherObject.SetActive(true);

            // Intentamos coger un terreno 
            GameObject terrain = RealWorldOverlay.Instance.getRandomItem();
            if (terrain != null)
            {
                weatherObject.transform.position = new Vector3(terrain.transform.localPosition.x, terrain.transform.localPosition.y + 2f, terrain.transform.localPosition.z);
            }
            else
            {
                weatherObject.transform.position = GameManager.Instance.ThePlayer.transform.forward * 2f;
            }

        }

        if (status == "hide")
        {
            ((GameObject)(GameManager.Instance.weatherDict[tipo])).SetActive(false);
        }

        return true;
    }


    //FUTURE
    public static bool ScanEnvironment(bool setOn) { return false; }
    // Ocultar puzzle
    public static bool BuyShopElem(int id) { return false; }
    

    public static int getMobsInScene() { return 0; }


    #endregion Commands




    #region SpeechCommandListener

    public void OnSpeechCommand(string cmd)
    {
        switch (cmd)
        {
            case "Open Puzzle":
            case "Abrir Puzzle":
                StartMiniGame();
                break;
            case "Open Sun":
                ShowHideWeather("Sun", "show");
                break;
            case "Close Sun":
                ShowHideWeather("Sun", "hide");
                break;
            case "Open Cloud":
                ShowHideWeather("CloudStorm", "show");
                break;
            case "Close Cloud":
                ShowHideWeather("CloudStorm", "hide");
                break;
        }
    }

    #endregion SpeechCommandListener



}