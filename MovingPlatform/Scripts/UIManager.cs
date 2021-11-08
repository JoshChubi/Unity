using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Function handles all button events
    public void ButtonClicked(Button b)
    {
        switch (b.name)
        {
            case "Main Menu":
                ClickedChangeState(Global.States.MainMenu);
                break;
            case "Configuration":
                ClickedChangeState(Global.States.Configuration);
                break;
            case "Programming":
                ClickedChangeState(Global.States.Program);
                break;
            case "Simulation":
                ClickedChangeState(Global.States.Simulation);
                break;
            case "Exit":
                ClickedExit();
                break;
            case "Build Platform":
                ClickedBuildPlatform();
                break;
            case "Save Platform":
                ClickedSavePlatform();
                break;
            case "Load Platform":
                ClickedLoadPlatform();
                break;
            case "StartPause":
                ClickedStartStop(b.GetComponentInChildren<Text>());
                break;
        }
    }

    // When A ChangeState button is activated, update UI, And let Platform Manager Know.
    public delegate void ChangeState(Global.States state);
    public static event ChangeState OnChangeState;
    public GameObject[] States = new GameObject[] { };
    public GameObject Top;
    void ClickedChangeState(Global.States state)
    {
        Top.SetActive(state != Global.States.MainMenu);
        foreach (GameObject objState in States)
            objState.SetActive(false);
        States[(int)state].SetActive(true);
        OnChangeState(state);
    }

    // Exit - Made it an event so the core function of exit is in Platform Manager, and keyboard input can fire it
    public delegate void Exit();
    public static event Exit OnExit;
    void ClickedExit()
    {
        OnExit();
    }

    // Build Platform - Take information from config UI scene and send it to platform manger, Update UI
    public delegate void BuildPlatformClicked(PlatformConfigurationData pcd);//(int row, int col, float deltaSpace, float yRange, Global.Colors colorSpectrum);
    public static event BuildPlatformClicked OnBuildPlatformClicked;
    void ClickedBuildPlatform()
    {
        int row = ConfigurationRow.text != "" ? int.Parse(ConfigurationRow.text) : -1;
        int col = ConfigurationCol.text != "" ? int.Parse(ConfigurationCol.text) : -1;
        if (row > 0 && col > 0)
        {
            float deltaSpace = ConfigurationDeltaSpace.value / 10;
            float yRange = ConfigurationYSlider.value;
            Global.Colors colorSpectrum = (Global.Colors)ConfigurationColor.value;

            PlatformConfigurationData pcd = new PlatformConfigurationData(row, col, deltaSpace, yRange, colorSpectrum);

            OnBuildPlatformClicked(pcd);
        }
    }

    // Save Platform - Fire Event to let Platform Manager to save current platform
    public delegate void SavePlatform();
    public static event SavePlatform OnSavePlatform;
    void ClickedSavePlatform() // saves the program to a file L
    {
        OnSavePlatform();
    }

    // Load Platform - Fire Event to let Platform Manager to load platform
    public delegate void LoadPlatform(string fileName);
    public static event LoadPlatform OnLoadPlatform;
    void ClickedLoadPlatform()
    {
        OnLoadPlatform("WriteLines.txt");   // Replace hard coded string with input from text UI
    }

    // StartStop Simulation - Fire Event to let Platform Manager to start simulation
    public delegate void StartStop(bool startStop); // if true then start it, else stop
    public static event StartStop OnStartStop;
    void ClickedStartStop(Text startStop)
    {
        startStop.text = (startStop.text == "Stop" ?"Start":"Stop");
        OnStartStop(startStop.text == "Stop"); // if its pause then that means we can start
    }

    // UI Elements:
    // Top
    public Text TopSize;
    public Text TopSpace;
    public Text TopYRange;
    // Configuration
    public Text     ConfigurationRow;
    public Text     ConfigurationCol;
    public Slider   ConfigurationDeltaSpace;
    public Slider   ConfigurationYSlider;
    public Dropdown ConfigurationColor;
    // Programming
    public Text     ProgramNodeID;
    public Text     ProgramHeight;
    public Slider   ProgramHeightSlider;
    // Simulation
    public Dropdown SimulationColor;

    // UI Methods
    void UpdatePCDUI(PlatformConfigurationData pcd)
    {
        //Top
        TopSize.text = string.Format("{0}x{1}", pcd.Row, pcd.Col);  // Platform Size
        TopSpace.text = (pcd.DeltaSpace).ToString("0.0") + "f";     // Space
        TopYRange.text = string.Format("{0}", pcd.YRange);          // Y Range

        // Configuration Pannel
        ConfigurationDeltaSpace.value = pcd.DeltaSpace * 10;    // Delta Slider
        ConfigurationYSlider.value = pcd.YRange;                // YRange slider
        ConfigurationColor.value = (int)pcd.ColorSpectrum;      // Color

        // Program Pannel
        ProgramNodeID.text = "";
        ProgramHeight.text = "0.00f";
        ProgramHeightSlider.maxValue = 10 * pcd.YRange; // Set max height for yRange

        // Simulation
        SimulationColor.value = (int)pcd.ColorSpectrum; // Update Color
    }
    void SetNode(string NodeID, float height)
    {
        ProgramNodeID.text = NodeID;
        ProgramHeightSlider.value = height * 10;
    }
    // Fired on UI.ValueChanged Event
    public delegate void ChangeNodeHeight(float height);
    public static event ChangeNodeHeight OnChangeNodeHeight;
    public void SetNodeHeight()
    {
        if (OnChangeNodeHeight != null) // if a node is selected (currently listing to event)
        {
            ProgramHeight.text = string.Format("{0:0.00}f", ProgramHeightSlider.value / 10);
            OnChangeNodeHeight(ProgramHeightSlider.value / 10);
        }
    }
    public void SetYRange()
    {
        TopYRange.text = string.Format("{0}", ConfigurationYSlider.value);
    }
    public void SetSpace()
    {
        TopSpace.text = (ConfigurationDeltaSpace.value / 10).ToString("0.0") + "f";
    }

    // Allows for UI Manager to persist along scene changes
    private static GameObject uiManager;
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        if (uiManager == null)
            uiManager = transform.gameObject;
        else
            Destroy(transform.gameObject);
    }

    // Connect Events
    void Start()
    {
        // Platform Manager
        PlatformManager.OnUpdatePlatformUI += UpdatePCDUI;

        // Program
        Node.OnSelectNode += SetNode;
    }

    // Change Color during simulation runtime
    public delegate void UpdateColorSpectrum(Global.Colors ColorSpectrum);
    public static event UpdateColorSpectrum OnUpdateColor;
    public void UpdateColor()
    {
        OnUpdateColor((Global.Colors)SimulationColor.value);
    }
}
