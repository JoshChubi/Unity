using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    // color event, probably material height
    // change state event? - start stop

    // data for node 
    // Net postion, state,color, etc
    private string RowPos;
    private string ColPos;
    private Material CurMaterial;
    private Global.Colors ColorSpectrum = Global.Colors.Gray;

    // Node Settings
    private float Height = 0;
    private float SpeedToPosition = 0.5f;
    private float SpeedToColor = 0.5f;
    private float ColorThreshHold = 0.05f;
    private float RangeThreshHold = 0.1f;

    // Node Variables
    private Color TargetColor;
    private Vector3 TargetPosition;
    private bool FlagIdle = true;                           // Use flag to ensure lerping in Update() can finish
    private bool FlagProgrammed = true;
    private bool FlagPosition = false;
    private bool FlagColor = false;
    private TextMesh HeightLabel;

    public void SetMaterial(Material material)
    {
        transform.GetComponent<Renderer>().material = material;
        CurMaterial = transform.GetComponent<Renderer>().material;
    }

    public void SetDisplay()
    {
        NewTargetColor();
        CurMaterial.color = TargetColor;
    }

    // To string
    public NodeConfigurationData ToNCD()
    {
        return (new NodeConfigurationData(int.Parse(this.RowPos), int.Parse(this.ColPos), this.Height));
    }

    public string ToString()
    {
        return string.Format("{0} {1} {2}", RowPos, ColPos, this.Height);
    }

    public void SetHeight(float height)
    {
        this.Height = height;
        transform.position = new Vector3(transform.position.x, this.Height, transform.position.z);
        FlagProgrammed = this.Height == 0;
    }

    public float GetHeight()
    {
        return this.Height;
    }

    // Start()
    void Start()
    {
        HeightLabel = transform.Find("HeightLabel").gameObject.GetComponent<TextMesh>();
        string[] rowCol = transform.name.Split(' ');
        RowPos = rowCol[0];
        ColPos = rowCol[1];
        //UIManager.OnChangeNodeHeight += SetHeight;
    }

    // SelectNode()
    public delegate void SelectNode(string NodeID, float height);
    public static event SelectNode OnSelectNode;
    public void Select()
    {
        SetDisplay();
        UIManager.OnChangeNodeHeight += SetHeight;
        OnSelectNode(string.Format("[{0},{1}]", RowPos, ColPos), this.Height);
    }
    public void UnSelect()
    {
        UIManager.OnChangeNodeHeight -= SetHeight;
        if(this.Height == 0)
            CurMaterial.color = new Color(1.0f, 1.0f, 1.0f);
    }
    // Register UI Events in OnEnable/OnDisable

    // Handles movement of base unit
    public bool GetProgrammed()
    {
        return FlagProgrammed;
    }

    ////////////////////////////////

    public void SetColor(Global.Colors color)
    {
        ColorSpectrum = color;
    }

    void NewTargetColor()
    {
        switch (ColorSpectrum)
        {
            case Global.Colors.Red:
                TargetColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), 0.0f, 0.0f);
                break;

            case Global.Colors.Green:
                TargetColor = new Color(0.0f, UnityEngine.Random.Range(0.0f, 1.0f), 0.0f);
                break;

            case Global.Colors.Blue:
                TargetColor = new Color(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 1.0f));
                break;

            case Global.Colors.Gray:
                float gray = UnityEngine.Random.Range(0.0f, 1.0f);
                TargetColor = new Color(gray, gray, gray);
                break;

            case Global.Colors.Random:
                TargetColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
                break;
        }
    }

    bool ReachedColor()
    {
        return ((Math.Abs(CurMaterial.color.r - TargetColor.r) < ColorThreshHold) &&
               (Math.Abs(CurMaterial.color.g - TargetColor.g) < ColorThreshHold) &&
               (Math.Abs(CurMaterial.color.b - TargetColor.b) < ColorThreshHold));
    }

    public void Reset()
    {
        SetHeight(0);
        CurMaterial.color = new Color(1.0f, 1.0f, 1.0f);
        HeightLabel.text = "";

        FlagIdle = true;
        FlagPosition = false;
        FlagColor = false;
    }

    bool ReachedPosition()
    {
        return (Math.Abs(transform.position.y - TargetPosition.y) < RangeThreshHold);
    }

    public delegate void FinishedNode(int rowPos, int colPos, float height);
    public static event FinishedNode OnFinishedNode;

    public void ChangeState(float height)
    {
        FlagIdle = !FlagIdle;
        if (FlagIdle)
        {
            TargetColor = new Color(1.0f, 1.0f, 1.0f);
            TargetPosition = new Vector3(transform.position.x, 0, transform.position.z);
        }
        else
        {
            this.Height = height;
            TargetPosition = new Vector3(transform.position.x, height, transform.position.z);
            NewTargetColor();
            FlagPosition = true;
            FlagColor = true;
        }
    }

    void Update()
    {
        /**
         * Control postion/color of node
         * Once target is wihtin threshold get new target
         * If state set to idle dont allow any lerping 
        */
        if (FlagPosition)
        {
            transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * SpeedToPosition);
            HeightLabel.text = transform.position.y.ToString("F2");
            if (ReachedPosition())
            {
                transform.position = TargetPosition;
                if (FlagIdle)
                {
                    HeightLabel.text = "";
                    FlagPosition = false;
                    OnFinishedNode(int.Parse(this.RowPos), int.Parse(this.ColPos), this.Height);
                }
                else
                    ChangeState(0);
            }
        }
        if (FlagColor)
        {
            CurMaterial.color = Color.Lerp(CurMaterial.color, TargetColor, Time.deltaTime * SpeedToColor);
            if (ReachedColor())
            {
                CurMaterial.color = TargetColor;
                if (FlagIdle)
                    FlagColor = false;
                else
                    NewTargetColor();
            }
        }
    }
}
