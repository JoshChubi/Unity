using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    private int CurrentTypeID = 0;
    private int CurrentMaterialID = 0;
    private GameObject CurrentGhost = null;
    private GameObject ObjNormal = null;    // Keeps track of orignial object ray is hitting inorder to be able to delete it

    public Button[] TypeButtons;
    public Button[] MaterialButtons;

    public GameObject[] TypeGhost;
    public Material[] GhostMaterials;
    public Material[] Materials;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {   // For some reason C# or unity doesnt pass the value of i during the time of loop but rather refrences the variable i itself, could this cause memory to be taken up? So it keeps passing 3 instead of what i was duing itteration of loop
            int id = i; // So Decided to keep it in local variable, could possible be holding 4 variables in memory (i = 3, id =0, id = 1, and id = 2)
            TypeButtons[id].onClick.AddListener(() => SetType(id));
            MaterialButtons[id].onClick.AddListener(() => SetMaterial(id));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentGhost)
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (hit)    // Positions Ghost Object
            {
                if (hitInfo.transform.tag.Equals("Base"))
                {
                    CurrentGhost.transform.position = new Vector3(GetNearestPosition(hitInfo.point.x), hitInfo.point.y + (0.5f), GetNearestPosition(hitInfo.point.z));
                    CurrentGhost.GetComponent<MeshRenderer>().material = GhostMaterials[0];
                    ObjNormal = null;
                }

                else if (!hitInfo.transform.tag.Equals("Ghost"))
                {
                    if (hitInfo.normal == new Vector3(0, 0, 1)) // z+
                    {
                        CurrentGhost.transform.position = new Vector3(hitInfo.transform.position.x, hitInfo.transform.position.y, hitInfo.point.z + (0.5f));
                    }
                    if (hitInfo.normal == new Vector3(1, 0, 0)) // x+
                    {
                        CurrentGhost.transform.position = new Vector3(hitInfo.point.x + (0.5f), hitInfo.transform.position.y, hitInfo.transform.position.z);
                    }
                    if (hitInfo.normal == new Vector3(0, 1, 0)) // y+
                    {
                        CurrentGhost.transform.position = new Vector3(hitInfo.transform.position.x, hitInfo.point.y + (0.5f), hitInfo.transform.position.z);
                    }
                    if (hitInfo.normal == new Vector3(0, 0, -1)) // z-
                    {
                        CurrentGhost.transform.position = new Vector3(hitInfo.transform.position.x, hitInfo.transform.position.y, hitInfo.point.z - (0.5f));
                    }
                    if (hitInfo.normal == new Vector3(-1, 0, 0)) // x-
                    {
                        CurrentGhost.transform.position = new Vector3(hitInfo.point.x - (0.5f), hitInfo.transform.position.y, hitInfo.transform.position.z);
                    }
                    if (hitInfo.normal == new Vector3(0, -1, 0)) // y-
                    {
                        CurrentGhost.transform.position = new Vector3(hitInfo.transform.position.x, hitInfo.point.y - (0.5f), hitInfo.transform.position.z);
                    }
                    CurrentGhost.GetComponent<MeshRenderer>().material = GhostMaterials[1];
                    ObjNormal = hitInfo.transform.gameObject;
                }
            }
            if (!EventSystem.current.IsPointerOverGameObject()) // If Mouse is NOT over A GUI
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (CurrentGhost.transform.position.x <= 4.375f && CurrentGhost.transform.position.x >= -4.375f && CurrentGhost.transform.position.z <= 4.375f && CurrentGhost.transform.position.x >= -4.375f) // If Ghost is in boundary
                    {
                        GameObject obj = null;
                        switch (CurrentTypeID)
                        {
                            case 0:
                                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                obj.GetComponent<BoxCollider>().isTrigger = true;
                                break;
                            case 1:
                                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                obj.GetComponent<SphereCollider>().isTrigger = true;
                                break;
                            case 2:
                                obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                                obj.GetComponent<CapsuleCollider>().isTrigger = true;
                                break;
                        }

                        obj.GetComponent<MeshRenderer>().material = Materials[CurrentMaterialID];
                        obj.AddComponent<TriangleExplosion>();
                        obj.tag = "UserPlaced";
                        obj.transform.position = CurrentGhost.transform.position;
                    }
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    if (ObjNormal)
                        StartCoroutine(ObjNormal.GetComponent<TriangleExplosion>().SplitMesh(true));
                }
            }

        }
    }

    private void SetType(int typeID)
    {
        CurrentTypeID = typeID;
        if (CurrentGhost)
        {
            Destroy(CurrentGhost);
            CurrentGhost = null;
        }
        CurrentGhost = Instantiate(TypeGhost[CurrentTypeID]);
    }

    private void SetMaterial(int materialID)
    {
        CurrentMaterialID = materialID;
    }

    float GetNearestPosition(float pos) // Function to get midpoint if on baseplate
    {
        float ret = 0f;

        if (pos >= 3.75f)
            ret = 4.375f;
        else if (pos >= 2.5f)
            ret = 3.125f;
        else if (pos >= 1.25f)
            ret = 1.875f;
        else if (pos >= 0f)
            ret = 0.625f;
        else if (pos <= -3.75f)
            ret = -4.375f;
        else if (pos <= -2.5f)
            ret = -3.125f;
        else if (pos <= -1.25f)
            ret = -1.875f;
        else if (pos < 0f)
            ret = -0.625f;

        return ret;
    }
}
