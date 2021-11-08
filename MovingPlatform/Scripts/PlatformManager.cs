using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.IO;

public class PlatformManager : PlatformGenericSinglton<PlatformManager>
{
    public GameObject HeightLabel;

    Global.States CurrentState = Global.States.MainMenu;
    PlatformConfigurationData PlatformData = null;
    Node[,] Nodes = null;
    List<NodeConfigurationData> NodesProgrammed = new List<NodeConfigurationData>();

    Node CurNode = null;
    public Material NodeMaterial;

    Node CreateNode(string name, Vector3 position, Vector3 scale)
    {
        var node = GameObject.CreatePrimitive(PrimitiveType.Cube);

        var tempLabel = Instantiate(HeightLabel);
        tempLabel.name = "HeightLabel";
        tempLabel.transform.parent = node.transform;

        node.transform.position = position;
        node.transform.localScale = scale;
        node.name = name;

        node.AddComponent<Node>();
        Node nodeComp = node.GetComponent<Node>();
        nodeComp.SetHeight(position.y);

        node.transform.SetParent(transform);    // Stores it in platform so it can change scenes
        return nodeComp;
    }

    public delegate void UpdatePlatformUI(PlatformConfigurationData pcd);
    public static event UpdatePlatformUI OnUpdatePlatformUI;

    void CleanUpCurNode()
    {
        if (CurNode != null)
        {
            CurNode.UnSelect();
            CurNode = null;
        }
    }

    void CleanUpPlatformNodes()
    {
        if(Nodes != null)
        {
            foreach (Node node in Nodes)
                Destroy(node.gameObject);
            Nodes = null;
        }
    }

    void SetPlatformData(PlatformConfigurationData pcd)
    {
        if (PlatformData != null)
            PlatformData = null;
        PlatformData = new PlatformConfigurationData(pcd);
    }

    void ManagePlatform(PlatformConfigurationData pcd)
    {
        CleanUpCurNode();
        CleanUpPlatformNodes();
        SetPlatformData(pcd);
        Nodes = new Node[PlatformData.Row, PlatformData.Col];
    }

    // Creates platform gameObjects
    void BuildPlatform(PlatformConfigurationData pcd)
    {
        ManagePlatform(pcd);
        for(int x = 0; x < PlatformData.Row; x++)
            for(int z = 0; z < PlatformData.Col; z++)
            {
                Nodes[x, z] = CreateNode(string.Format("{0} {1}", x, z),
                                         new Vector3(x + (x * PlatformData.DeltaSpace), 0, z + (z * PlatformData.DeltaSpace)),
                                         new Vector3(1, 0.1f, 1));
                Nodes[x, z].SetColor(pcd.ColorSpectrum);
                Nodes[x, z].SetMaterial(NodeMaterial);
            }
        OnUpdatePlatformUI(pcd);
    }

    // Change state
    void ChangeState(Global.States state)
    {
        CurrentState = state;
        SceneManager.LoadScene((int)state);
    }

    // Updates platform gameObjects

    void CacheProgrammedNodes()
    {
        NodesProgrammed.Clear();
        foreach (Node node in Nodes)
            if (node.GetHeight() != 0)
                NodesProgrammed.Add(node.ToNCD());
    }

    void SavePlatform()
    {
        Debug.Log("Saving Program To File");

        CacheProgrammedNodes();

        using (StreamWriter outputFile =new StreamWriter("WriteLines.txt"))//new StreamWriter(Path.Combine(Application.dataPath, "WriteLines.txt")))
        {
            outputFile.WriteLine(PlatformData.ToString());
            foreach (NodeConfigurationData ncd in NodesProgrammed)
                outputFile.WriteLine(ncd.ToString());
        }
    }

    void LoadPlatform(string fileName)
    {
        Debug.Log("Loading Program From File");
        using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
        {
            string[] stringData = (sr.ReadLine()).Split(' ');
            BuildPlatform( new PlatformConfigurationData(int.Parse(stringData[0]), int.Parse(stringData[1]), float.Parse(stringData[2]), float.Parse(stringData[3]), (Global.Colors)int.Parse(stringData[4])) );

            string temp;
            while( (temp = sr.ReadLine())  != null) // Set nodes that are suppose to move to their height
            {
                stringData = temp.Split(' ');
                float h = float.Parse(stringData[2]);
                var tempNode = Nodes[int.Parse(stringData[0]), int.Parse(stringData[1])];
                tempNode.SetHeight(float.Parse(stringData[2]));
                if(tempNode.GetHeight() != 0)
                    tempNode.SetDisplay();
            }

            OnUpdatePlatformUI(PlatformData);   // Fires event for UIManager to update UI;
        }
    }

    /////////////////////////////////////////////////////////////////////


    void SelectNode()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100.0f))
            if (hit.transform != null)
            {
                CleanUpCurNode();
                CurNode = hit.transform.GetComponent<Node>();
                CurNode.Select();
            }
    }

    void Exit()
    {
        Application.Quit();
    }

    void Update()
    {
        if (CurrentState == Global.States.Program)
        {
            // handle mouse
            if (Input.GetMouseButtonDown(0))
                if (!EventSystem.current.IsPointerOverGameObject())
                    SelectNode();
        }
    }

    void NextNode(int rowPos, int colPos, float height)
    {
        Debug.Log(string.Format("{0} {1} {2}", rowPos, colPos, height));
        Nodes[(rowPos + 1 == PlatformData.Row? 0 :rowPos + 1 ), colPos].ChangeState(height);
    }

    void Simulate(bool startStop)
    {
        if (startStop)  // If text == stop then that means we are in start mode
        {
            CacheProgrammedNodes();

            foreach (NodeConfigurationData ncd in NodesProgrammed)  // Set all there heights
                Nodes[ncd.Row, ncd.Col].SetHeight(ncd.Height);

            foreach (NodeConfigurationData ncd in NodesProgrammed)  // Start the simulation
                Nodes[ncd.Row, ncd.Col].ChangeState(ncd.Height);
            Debug.Log("Starting");
        }
        else
        {
            foreach(Node node in Nodes)
                node.Reset();
            foreach (NodeConfigurationData ncd in NodesProgrammed)
            {
                Nodes[ncd.Row, ncd.Col].SetHeight(ncd.Height);
                Nodes[ncd.Row, ncd.Col].SetDisplay();
            }
            Debug.Log("Stopping");
        }

    }

    void UpdateColor(Global.Colors ColorSpectrum)
    {
        PlatformData.ColorSpectrum = ColorSpectrum;
        foreach (Node node in Nodes)
            node.SetColor(ColorSpectrum);
    }

    void Start()
    {
        // Core
        UIManager.OnChangeState += ChangeState;
        UIManager.OnExit += Exit;

        // Configuration
        UIManager.OnBuildPlatformClicked += BuildPlatform;

        // Program
        UIManager.OnSavePlatform += SavePlatform;
        UIManager.OnLoadPlatform += LoadPlatform; // maybe is under simulation

        // Simulation
        UIManager.OnUpdateColor += UpdateColor;
        UIManager.OnStartStop += Simulate;
        Node.OnFinishedNode += NextNode;
    }
}
