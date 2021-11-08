using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCameraControl : MonoBehaviour
{
    public float centerX = 0f;
    public float centerZ = 0f;

    public float speed = 10.0f;

    protected float fDistance = 1;
    protected float fSpeed = 1;

    Vector3 targetPosition;

    private void OnEnable()
    {
        UIManager.OnBuildPlatformClicked += UIManager_BuildPlatformOnClicked;
        //UIManager.OnUpdateComeraPosition += UIManager_BuildPlatformOnClicked;
    }

    private void UIManager_BuildPlatformOnClicked(PlatformConfigurationData pcd)
    {
        centerX = pcd.Row / 2;
        centerZ = pcd.Col / 2;

        transform.position = new Vector3(pcd.Row, 10, pcd.Col);
        targetPosition = new Vector3(centerX, 3, centerZ);
    }

    private void OnDisable()
    {
        UIManager.OnBuildPlatformClicked -= UIManager_BuildPlatformOnClicked;
        //UIManager.OnUpdateComeraPosition -= UIManager_BuildPlatformOnClicked;
    }

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(targetPosition);

        #region ZOOM IN/OUT
        if (Input.GetKey(KeyCode.LeftShift))
            if(Input.GetKey(KeyCode.UpArrow))
                transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftShift))
            if (Input.GetKey(KeyCode.DownArrow))
                transform.Translate(-Vector3.forward * speed * Time.deltaTime);
        #endregion

        #region ROTATE VERTICAL/HORIZONTAL
        if (Input.GetKey(KeyCode.RightArrow))
            transform.RotateAround(targetPosition, -Vector3.up, Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.RotateAround(targetPosition, Vector3.up, Time.deltaTime * speed);


        if (Input.GetKey(KeyCode.UpArrow))
            transform.RotateAround(targetPosition, Vector3.right, Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.DownArrow))
            transform.RotateAround(targetPosition, -Vector3.right, Time.deltaTime * speed);
        #endregion
    }

    private static GameObject uiManager;
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        if (uiManager == null)
            uiManager = transform.gameObject;
        else
            Destroy(transform.gameObject);
    }
}
