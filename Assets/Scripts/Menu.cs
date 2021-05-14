#define city
#define BackPropagation
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

public class Menu : MonoBehaviour
{
    // Menu Settings
    private float menuXpos = 20;
    //private float menuYpos = 20;
    MenuBase menu = new MenuBase();
    #region DO NOT TOUCH
    private float menuItemWidth = 160f;
    private float menuHeight = 0f;
    #endregion

    #region Objects and Variables
    // GameObjects
    GameObject speedoMeter;
    GameObject nnDrawer;
    GameObject joyStick;
    GameObject cameraObject;
    GameObject car;

    // Objects
    RockSpawner rockSpawner;
    GeneticManager geneticManager;
    DistanceSensorArray distanceSensorArray;
    FitnessEvaluator fitnessEvaluator;
    TrainingDataRecorder trainingDataRecorder;
    VideoInput videoInput;
    Transform wheelsHubs;
    Vector3 wheelsHubsOriginal;
    CarController carController;
    CarInput carInput;
    DistanceNNInterface distanceNNInterface;


    sub currentSub = 0;
    enum sub : int
    {
        main = 0,
        raysettings,
        fitness,
        guimenu,
        nnmanager,
        nnsettings,
        filehandler,
        enviroment,
        autoexit,
        exit,
        datarecorder,
        vehicleSettings,
        spawn,
        vehicleInfo,
        location,
        closed
    }

    // GUI
    public static bool drawInfoPanel = true;
    public static bool autoStart = false;
    bool running = false;
    bool displayFPS = false;
    bool speedoMeterButtonValue;
    bool nnDrawerButtonValue;
    bool joyStickButtonValue;
    bool cameraButtonValue;
    bool crazyStonesHasBeenGenerated = false;
    bool showNNFitnesses = false;
    string forceNextNetworkField = "";


    // GUI Error Message
    static float errorMsgStartTime;
    static bool dislayErrorMsg = false;
    static string errorMsg = "";
    static float errorMsgDuration = 15f;

    // Layers
    public static int numberOfRays = 11;
    public static int hiddenLayers = 2;
    public static List<int> hiddenLayerSizes = new List<int>();

    // InfoPanel
    short infoPanelUpdateFreq = 15;
    short infoPanelUpdateCounter = 15;
    string infoPanelData;

    // Other, used by Update()

    public static int
    updateTimeScaleWhenDistance = 0,
    updateTimeScaleWhenFitness = 0,
    updateTimeScaleWhenIRLTime = 0,
    updateTimeScaleWhenSimTime = 0;
    public static int updatedTimeScale = 1;
    bool timeScaleHasBeenUpdated = false;
    short timeScaleCheckCounter = 0;

    short updatesBetweenFPSDisplayUpdate = 30;
    short updateCounterFPS = 30;

    bool carFrozen = false;

    // Other
    int fps;
    int fpsTarget = 60;
    public static DistanceNNInterface.InputMode inputMode = DistanceNNInterface.InputMode.line;

    #endregion


    #region info panel +info panel setup
    struct sMenu
    {
        public float xPos;
        public float yPos;
        public float height;
        public float width;
    }

    sMenu infoPanel;

    void DrawInfoPanel()
    {
        if (infoPanelUpdateCounter++ > infoPanelUpdateFreq)
        {
            infoPanelUpdateCounter = 0;
            infoPanelData =
                "<size=16>" +
                $"<b>Real Time:</b> {System.TimeSpan.FromSeconds((uint)(Time.realtimeSinceStartup + FileHandler.timeFromFileIRL))}\n" +
                $"<b>Simulated Time:</b> {System.TimeSpan.FromSeconds((uint)(Time.time + FileHandler.timeFromFileSim))}\n" +
                $"<b>Time Scale:</b> {Time.timeScale}\n" +
                $"<b>Network:</b> {geneticManager.currentNetwork}\n" +
                $"<b>Generation:</b> {geneticManager.currentGeneration}\n" +
                $"<b>Fitness:</b> {System.Math.Round(fitnessEvaluator.fitness, 2)}\n" +
                $"<b>Elapsed Time:</b> {System.TimeSpan.FromSeconds((uint)fitnessEvaluator.elapsedTime)}\n" +
                $"<b>Time Limit:</b> {System.TimeSpan.FromSeconds((uint)fitnessEvaluator.timeLimit)}\n" +
                $"<b>Distance:</b> {System.Math.Round(fitnessEvaluator.distanceTravelled, 2)}\n" +
                $"<b>Fitness Highscore:</b> {System.Math.Round(geneticManager.fitnessHighScore, 2)}\n" +
                $"<b>Distance Highscore:</b> {System.Math.Round(geneticManager.distanceHighScore, 0)}\n" +
                $"<b>Ends by Time:</b> {geneticManager.endCauseTimeCount}\n" +
                $"<b>Ends by Wall:</b> {geneticManager.endCauseWallCount}\n" +
                $"<b>Ends by Rock:</b> {geneticManager.endCauseRockCount}\n" +
                $"<b>Filename:</b>\n<color=red>{FileHandler.saveFile}</color>\n" +
                "</size>";
        }
        menu.DrawMenuBackground(menu.infoPanel, infoPanelData);

    }
    #endregion
    #region helper functions
    void SetUpWalls(bool ignoreRayCast, bool visible)
    {
        for (int i = 1; i <= 4; i++)
        {
            if (GameObject.Find($"HelperWalls{i}") != null)
            {
                GameObject.Find($"HelperWalls{i}").layer = ignoreRayCast ? 2 : 9;
                GameObject.Find($"HelperWalls{i}").GetComponent<MeshRenderer>().enabled = visible;
            }

        }
    }
    
    void InitObjectRef(string objectName, ref GameObject gameObject, ref bool gameObjectValue)
    {
        gameObject = GameObject.Find(objectName);
        if (gameObject != null)
            gameObjectValue = gameObject.activeSelf;
    }

    bool GameObjectExists(string name)
    {
        if (GameObject.Find(name) != null)
            return true;
        else
            return false;
    }
    #endregion

    void SubMenuSwitcher()
    {
        switch (currentSub)
        {
            case sub.main:            SubMainMenu();        break;
            case sub.guimenu:         SubGUIMenu();         break;
            case sub.nnmanager:       SubNNManager();       break;
            case sub.nnsettings:      SubNNSettings();      break;
            case sub.filehandler:     SubFileHandler();     break;
            case sub.fitness:         SubFitnessSettings(); break;
            case sub.enviroment:      SubEnvironment();     break;
            case sub.autoexit:        SubAutoExit();        break;
            case sub.datarecorder:    SubDataRecorder();    break;
            case sub.exit:            SubExit();            break;
            case sub.vehicleSettings: SubVehicleMenu();     break;
            case sub.raysettings:     SubRaySettings();     break;
            case sub.location:        SubLocation();        break;
            case sub.closed:          SubClosed();          break;
            default: currentSub = sub.main;                 break;

        }

    }

    #region SubMenus

    void SubNNManager()
    {
        menu.title = "NN Manager";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        menu.AddToggle("Never End Test", ref geneticManager.neverEndTest);
        menu.AddButton("Force End Test", ref fitnessEvaluator.forceEndTest);
        menu.AddLabel($"<b>Current Network:</b> {geneticManager.currentNetwork}");
        menu.AddLabel($"<b>Current Generation:</b> {geneticManager.currentGeneration}");
        menu.AddLabel($"<b>Current Fitness:</b> {System.Math.Round(fitnessEvaluator.fitness, 2)}");
        menu.AddButton("Fitnesses", ref showNNFitnesses);
        if (showNNFitnesses)
        {
            for (int i = 0; i < geneticManager.fitnesses.Length; i++)
                menu.AddLabel($"Network {i}: {geneticManager.fitnesses[i]}");
        }
    }

    void SubEnvironment()
    {
        menu.title = "Enviroment";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        if (!crazyStonesHasBeenGenerated)
        {            
            if (menu.AddBoolButton("Generate Crazy Stones"))
            {
                rockSpawner.GenerateCrazyStones();
                RockSpawner.crazyStonesHasBeenGenerated = true;
            }
        }
        else
        {
            if(menu.AddBoolButton("Remove all stones"))
            {
                rockSpawner.RemoveAllStones();
                RockSpawner.crazyStonesHasBeenGenerated = false;
            }

        }



    }

    void SubAutoExit()
    {
        menu.title = "Auto Exit";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        menu.AddTextFieldWithLabelAndToggle("Distance Travelled Limit", ref geneticManager.distanceTravelledLimit, "Distance Limit", ref geneticManager.distanceTravelledLimitEnabled);
        menu.AddTextFieldWithLabelAndToggle("Fitness Limit",            ref geneticManager.fitnessLimit, "Fitness Limit", ref geneticManager.fitnessLimitEnabled);
    }

    void SubFitnessSettings()
    {
        menu.title = "Fitness Settings";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        menu.AddToggle("Rock Collision End", ref fitnessEvaluator.stoneCollisionEndsTest);
        menu.AddToggle("Collision Ends Test", ref fitnessEvaluator.collisionEndsTest);
        menu.AddSliderWithLabel("Default Time Limit", ref fitnessEvaluator.defaultTimeLimit, 0f, 200f, 0);
        menu.AddSliderWithLabel("Extra Time after Distance", ref fitnessEvaluator.distanceExtraTime, 0f, 200f, 0);
    }

    void SubRaySettings()
    {
        menu.title = "Ray Settings";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;
        menu.AddSliderWithLabel("Rays Range", ref distanceSensorArray.fieldOfViewDeg, 0f, 200f, 0);
        menu.AddSliderWithLabel("Rays Range", ref distanceSensorArray.maxDistance, 0f, 200f, 0);
        menu.AddToggle("Fix Distance Bug", ref distanceSensorArray.fixMaxDistanceBug);
         
    }

    void SubNNSettings()
    {
        menu.title = "NN Settings";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;
        float timescale = Time.timeScale;
        menu.AddSliderWithLabel("Time Scale", ref timescale, 0f, 10f, 1);
        Time.timeScale = timescale;
        menu.AddToggle("Random Networks", ref geneticManager.randomNetworks);
        if (geneticManager.randomNetworks)
            menu.AddSliderWithLabel("Random Networks",       ref geneticManager.newRandomsCount, 1, geneticManager.populationSize);
        menu.AddSliderWithLabel("Population Size",           ref geneticManager.populationSize, 10, 100);
        menu.AddSliderWithLabel("Population Winner Percent", ref geneticManager.populationWinnerPercentage, 0.01f, 0.99f);
        menu.AddSliderWithLabel("Mutation Probability",      ref geneticManager.mutationProbability, 0.01f, 0.99f, 2);
        menu.AddSliderWithLabel("Mutation Factor",           ref geneticManager.mutationFactor, 0.01f, 0.5f, 2);

        if (menu.AddTextFieldWithLabelAndBoolButton("Force Next Network", ref forceNextNetworkField, "Force") )
        {
            if( int.TryParse(forceNextNetworkField, out int nextNetwork))
            {
                geneticManager.currentGeneration = nextNetwork - 1;
                geneticManager.EndTest();
            }
        }
    }

    void SubFileHandler()
    {
        menu.title = "File Handler";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        menu.AddToggle("Auto Save", ref geneticManager.autoSave);      
        if (menu.AddTextFieldWithLabelAndBoolButton("Save File", ref FileHandler.saveFile, "Save"))
            FileHandler.SaveNNToXML(geneticManager);
        menu.AddTextFieldWithLabel("File Comment", ref FileHandler.fileComment);

    }
    

    void SubDataRecorder()
    {
        menu.title = "Data Recorder";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;
        if(trainingDataRecorder != null)
        {
            menu.AddTextFieldWithLabel("Filename", ref trainingDataRecorder.filename);
            menu.AddToggle("Record Data", ref trainingDataRecorder.recordData);
            menu.AddButton("Save Data", ref trainingDataRecorder.save);
        }
        else
        {
            menu.AddLabel("Data Recorder is null");
        }
    }
    

    void SubVehicleMenu()
    {
        menu.title = "GUI Settings";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        if (menu.AddBoolButton("Vehicle Info"))
            currentSub = sub.vehicleInfo;

        menu.AddSliderWithLabel("Extra Torque", ref carController.extraTorque, 0f, 10000f);
        menu.AddTextFieldWithLabel("Extra Torque", ref carController.extraTorque);

        menu.Add2RowLabel("<b>Extra Torque</b>\n "      + System.Math.Round(carController.extraTorque, 5));
        menu.Add2RowLabel("<b>Motor Torque</b>\n "      + System.Math.Round(carController.motorTorque, 5));
        menu.Add2RowLabel("<b>Current Torque</b>\n "    + System.Math.Round(carController.m_CurrentTorque, 5));
        menu.Add2RowLabel("<b>Break Torque</b>\n"       + System.Math.Round(carController.brakeTorque, 5));
        menu.Add2RowLabel("<b>Steering:</b>\n"          + System.Math.Round(carInput.steering, 5));
        menu.Add2RowLabel("<b>Accel</b>\n"              + System.Math.Round(carInput.acceleration, 5));


    }


    void SubGUIMenu()
    {
        menu.title = "GUI Settings";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        menu.AddToggle("Info Panel", ref drawInfoPanel);
        menu.AddGameObjectToggle("Camera", ref cameraObject, ref cameraButtonValue);
        menu.AddGameObjectToggle("Speedometer", ref speedoMeter, ref speedoMeterButtonValue);
        menu.AddGameObjectToggle("NN Drawer", ref nnDrawer, ref nnDrawerButtonValue);
        menu.AddGameObjectToggle("Joy Stick", ref joyStick, ref joyStickButtonValue);
        if (videoInput != null)
            menu.AddToggle("Draw Video Input", ref videoInput.drawVideoInput);
        menu.AddToggle("Display FPS", ref displayFPS);
        menu.AddTextFieldWithLabel("FPS Target", ref fpsTarget);

    }

    void SubExit()
    {
        menu.title = "Exit";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;

        if (menu.AddBoolButton("Save and Exit"))
        {
            FileHandler.SaveNNToXML(geneticManager);
            Application.Quit();

        }
        if(menu.AddBoolButton("Exit without saving"))
            Application.Quit();

        
    }

    
    void SubLocation()
    {
        menu.title = "Select Location";
        if (menu.AddBoolButton("← Main Menu"))
            currentSub = sub.main;
        
        if (menu.AddBoolButton("Go to Windridge City"))
        {
            try
            {
                Transform temp = GameObject.Find("Spawn_City").GetComponent<Transform>();
                fitnessEvaluator.spawnPosition = temp.position;
                fitnessEvaluator.spawnRotation = temp.rotation;
                fitnessEvaluator.forceEndTest = true;
            }
            catch (Exception e) { ErrorMsg("Failed to go to Windridge City\n" + e.Message); }

        }
        if(menu.AddBoolButton("Go to Race Track"))
        {
            try
            {
                Transform temp = GameObject.Find("Spawn_RaceTrack").GetComponent<Transform>();
                fitnessEvaluator.spawnPosition = temp.position;
                fitnessEvaluator.spawnRotation = temp.rotation;
                fitnessEvaluator.forceEndTest = true;
            }
            catch (Exception e) { ErrorMsg("Failed to go to Race Track\n" + e.Message); }
        }
    }

    void SubClosed()
    {
        menuHeight = 0f;
        if (menu.AddBoolButton("Open Menu"))
        {
            currentSub = sub.main;
            infoPanel.xPos = menuXpos + menuItemWidth + 15;
            infoPanel.yPos = 20;
        }
        menu.AddToggle("Info Panel", ref drawInfoPanel);
    }

    void SubMainMenu()
    {
        menu.title = "Main Menu";

        if (menu.AddBoolButton("Hide Menu"))
        {
            currentSub = sub.closed;
            infoPanel.xPos = 1;// menuXpos;// + menuItemWidth + 15;
            infoPanel.yPos = 90;
            menu.DrawMenuBackground();
        }

        if (menu.AddBoolButton("GUI Settings"))
            currentSub = sub.guimenu;

        if (menu.AddBoolButton("Change Location"))
            currentSub = sub.location;

        if (menu.AddBoolButton("Vehicle Settings"))
            currentSub = sub.vehicleSettings;
        
        if (distanceSensorArray != null)
        {
            if (menu.AddBoolButton("Ray Settings")) // distanceSensorArray
                currentSub = sub.raysettings;
        }
        
        if(geneticManager != null)
        {
            if (menu.AddBoolButton("NN Manager"))
                currentSub = sub.nnmanager;

            if (menu.AddBoolButton("NN Settings")) // geneticManager
                currentSub = sub.nnsettings;

            if (menu.AddBoolButton("File Handler")) // geneticManager
                currentSub = sub.filehandler;

            if (menu.AddBoolButton("Auto Exit")) // geneticManager
                currentSub = sub.autoexit;

            if (fitnessEvaluator != null && menu.AddBoolButton("Fitness Settings")) // geneticManager fitnessEvaluator
                currentSub = sub.fitness;
        }

        if(rockSpawner != null)
        {
            if (menu.AddBoolButton("Environment")) // rockspawner
                currentSub = sub.enviroment;
        }

        if (trainingDataRecorder != null)
        {
            if (menu.AddBoolButton("Data Recorder"))
                currentSub = sub.datarecorder;
        }

        if (menu.AddBoolButton("Exit"))
            currentSub = sub.exit;
    }
    
    #region Pre-Start menu
    void LoadFromFileMenu()
    {
        menu.AddTextFieldWithLabel("Filename", ref FileHandler.loadFile);
        Color tempColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.blue;
        string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\NetworkGenerations\\");
        foreach (string file in files)
        {
            if (file.Contains(".xml"))
            {
                string temp = file.Replace(Directory.GetCurrentDirectory() + "\\NetworkGenerations\\", "");
                if (menu.AddBoolButton("<color=orange>" + temp + "</color>"))
                    FileHandler.loadFile = "\\" + temp;
            }

        }
        GUI.backgroundColor = tempColor;
    }
    void NotLoadFromFileMenu()
    {
        string inputModeStringValue = "";
        switch (inputMode)
        {
            case DistanceNNInterface.InputMode.combo: inputModeStringValue = "Combo"; break;
            case DistanceNNInterface.InputMode.distance: inputModeStringValue = "Distance Sensors"; break;
            case DistanceNNInterface.InputMode.line: inputModeStringValue = "Line Detection"; break;
        }
        int _inputMode = (int)inputMode;
        menu.AddSwitcher("Input Mode", ref inputModeStringValue, ref _inputMode, 0, 2);
        inputMode = (DistanceNNInterface.InputMode)_inputMode;

        if (inputMode == DistanceNNInterface.InputMode.distance || inputMode == DistanceNNInterface.InputMode.combo)
        {
            menu.AddTextFieldWithLabel("Input Rays:", ref numberOfRays);
            menu.AddSliderWithLabel("Rays FOV:", ref distanceSensorArray.fieldOfViewDeg, 0f, 360f, 0);
            menu.AddSliderWithLabel("Rays Lenght:", ref distanceSensorArray.maxDistance, 0f, 100f, 0);
        }
        menu.AddSliderWithLabel("Hidden Layers", ref hiddenLayers, 1, 10);
        for (int i = 0; i < hiddenLayers; i++)
        {
            if (i > hiddenLayerSizes.Count - 1)
                hiddenLayerSizes.Add(5);

            int value = hiddenLayerSizes[i];
            menu.AddTextFieldWithLabel($"Hidden Layer {i}", ref value);
            hiddenLayerSizes[i] = value;
        }
    }
    void PreStartMenu()
    {
 
        menu.title = "Start Up";
        bool startPressed = false;
        menu.AddButton("Start Test", ref startPressed);
        menu.AddToggle("Fix Distance Bug", ref distanceSensorArray.fixMaxDistanceBug);

        menu.AddToggle("Load from File", ref geneticManager.loadFileOnStartup);
        if (geneticManager.loadFileOnStartup)
            LoadFromFileMenu();
        else
            NotLoadFromFileMenu();

        if (rockSpawner != null && !crazyStonesHasBeenGenerated && menu.AddBoolButton("Generate Crazy Stones"))
        {
            rockSpawner.GenerateCrazyStones();
            crazyStonesHasBeenGenerated = true;
        }

        if (startPressed || autoStart)
            StartUp();
    }
    #endregion

    #endregion

    /// <summary>
    /// Starts up the Test/Traning/Simulation
    /// </summary>
    void StartUp()
    {
        switch (inputMode)
        {
            case DistanceNNInterface.InputMode.distance:
                geneticManager.inputSize = numberOfRays;
                distanceNNInterface.SetupInputs(inputMode, numberOfRays);
                SetUpWalls(ignoreRayCast: false, visible: false);
                break;
            case DistanceNNInterface.InputMode.line:
                geneticManager.inputSize = 2;
                distanceNNInterface.SetupInputs(inputMode);
                SetUpWalls(ignoreRayCast: true, visible: false);
                break;
            case DistanceNNInterface.InputMode.combo:
                geneticManager.inputSize = numberOfRays + 2;
                distanceNNInterface.SetupInputs(inputMode, numberOfRays);
                SetUpWalls(ignoreRayCast: true, visible: false);
                break;
        }

        if (geneticManager != null)
        {
            geneticManager.hiddenLayerSizes = new int[hiddenLayers];
            for (int i = 0; i < hiddenLayers; i++)
                geneticManager.hiddenLayerSizes[i] = hiddenLayerSizes[i];
        }

        car.SetActive(true);

        if (geneticManager != null)
        {
            geneticManager.enabled = true;
            geneticManager.StartTests();
        }

        if (nnDrawer != null)
            nnDrawer.SetActive(nnDrawerButtonValue);

        running = true;
        hiddenLayerSizes.Clear();
    }


    public static void ErrorMsg(string msg)
    {
        errorMsgStartTime = Time.time;
        dislayErrorMsg = true;
        errorMsg = msg;
    }
    void OnGUI()
    {
        if (dislayErrorMsg)
        {
            if (Time.time - errorMsgStartTime > errorMsgDuration)
                dislayErrorMsg = false;
            int errorMsgXPos = (int)menuItemWidth + (int)menuXpos + 20;
           if( drawInfoPanel)
                errorMsgXPos += (int)infoPanel.width ;
            GUI.Box(new Rect(x: errorMsgXPos, y: 30, width: 400, height: 100),
                    $"<size=16><color=red><b>Error:\n</b></color>{errorMsg}</size>");
        }
        
        if (currentSub != sub.closed)
            menu.DrawMenuBackground();
       menu.menuHeight = 30f; // Reset the menu height every frame

        if (!running)
        {
            PreStartMenu();
        }
        else
        {
            SubMenuSwitcher();

            if (drawInfoPanel && geneticManager != null)
                DrawInfoPanel();

            if (displayFPS)
                GUI.Label(new Rect(x: 20, y: 1, width: 60, height: 40),
                $"<size=16><color=orange><b>{fps} FPS</b></color></size>");

            if (carFrozen)
                GUI.Label(new Rect(x: 20, y: menuHeight + 30, width: 400, height: 100),
                    $"<size=50><color=red><b>CAR FROZEN</b></color></size>");
        }
        menuHeight -= 10f;
    }

    void Update()
    {
        // unnecessarily to check this every update.
        if (!timeScaleHasBeenUpdated && timeScaleCheckCounter++ > 300) 
        {
            if (geneticManager.distanceHighScore > updateTimeScaleWhenDistance &&
                geneticManager.fitnessHighScore > updateTimeScaleWhenFitness &&
                Time.realtimeSinceStartup > updateTimeScaleWhenIRLTime &&
                Time.time > updateTimeScaleWhenSimTime)
            {
                Time.timeScale = updatedTimeScale;
                timeScaleHasBeenUpdated = true;
            }
        }

        
        // Updating the FPS every frame makes the fps label unreadable
        if (displayFPS && updateCounterFPS++ > updatesBetweenFPSDisplayUpdate) //
        {
            updateCounterFPS = 0;
            fps = (int)(1f / Time.unscaledDeltaTime);
        }

        if (Input.GetKeyDown("space")) // just for debuging, should'nt be here
        {
            RigidbodyConstraints rigidbodyConstraints = car.GetComponent<Rigidbody>().constraints;
            if (rigidbodyConstraints == RigidbodyConstraints.FreezeAll)
            {
                carFrozen = false;
                car.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }

            else
            {
                carFrozen = true;
                car.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
        }

    }

    void Start()
    {
        Application.targetFrameRate = 30;

        InitObjectRef("Speedometer", ref speedoMeter, ref speedoMeterButtonValue);
        InitObjectRef("NN Drawer", ref nnDrawer, ref nnDrawerButtonValue);
        InitObjectRef("Camera Rig", ref cameraObject, ref cameraButtonValue);
        InitObjectRef("Joystick", ref joyStick, ref cameraButtonValue);

        car = GameObject.Find("Toyota GT86");
        wheelsHubs = car.transform.Find("Graphics").Find("SportCar20_WheelsHubs");
        wheelsHubsOriginal = wheelsHubs.position;
        carController = car.GetComponent<CarController>();
        carInput = car.GetComponent<CarInput>();
        distanceNNInterface = car.GetComponent<DistanceNNInterface>();

        if (GameObjectExists("NN Manager"))
        {
            geneticManager = GameObject.Find("NN Manager").GetComponent<GeneticManager>();
        }


        if (GameObjectExists("Distance Sensor"))
            distanceSensorArray = GameObject.Find("Distance Sensor").GetComponent<DistanceSensorArray>();

        if (GameObjectExists("RockSpawner"))
            rockSpawner = GameObject.Find("RockSpawner").GetComponent<RockSpawner>();

        if (GameObjectExists("Video Input"))
            videoInput = GameObject.Find("Video Input").GetComponent<VideoInput>();

        fitnessEvaluator = car.GetComponent<FitnessEvaluator>();
        trainingDataRecorder = car.GetComponent<TrainingDataRecorder>();

        if (car != null)
            car.SetActive(false);

        if (nnDrawer != null)
            nnDrawer.SetActive(false);

        if (geneticManager != null)
        {
            geneticManager.enabled = false;
            drawInfoPanel = false;
        }

        for (int i = 0; i < hiddenLayers; i++)
            if (i > hiddenLayerSizes.Count - 1)
                hiddenLayerSizes.Add(5);

        //infoPanel.xPos = Screen.width - 240;
        infoPanel.xPos = menuXpos + menuItemWidth + 15;
        infoPanel.yPos = 20;
        infoPanel.height = 275;//260;//250;//190;//130; //öka med typ 20 per rad
        infoPanel.width = 210;

        FileHandler.LoadXMLSettings(geneticManager, fitnessEvaluator, distanceSensorArray);
    }

}
