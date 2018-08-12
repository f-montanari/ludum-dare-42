using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHandler : MonoBehaviour
{

    private enum MouseMode
    {
        PLACING,
        ERASING,
        SELECTING
    }

    // TODO: Place these in their respective classes
    /******/
    [Header("Temporary")]    
    public GameObject satisfierPrefab;
    public GameObject employeePrefab;
    public Transform employeeSpawnPoint;    
    /******/

    [Space]
    public LayerMask snapPointLayer;
    public int currentFloor = 0;

    private UIUpdater updater;
    private MouseMode currentMouseMode = MouseMode.SELECTING;
    private GameObject currentPrefab;
    private Plane zeroPlane;
    private Camera cam;
    private float floorHeight = 2f;
    private Employee currentEmployee;
    private bool isLocating = false;
    private bool isSatisfier = false;

    public void SetPrefab(GameObject prefab)
    {
        satisfierPrefab = prefab;
        currentMouseMode = MouseMode.PLACING;
        SpawnSatisfier();
    }

    public void SetEraseMode()
    {
        currentMouseMode = MouseMode.ERASING;
        updater.ShowHint("Currently in Erase Mode \n Press Escape to exit Erase Mode" );
    }

    // Use this for initialization
    void Start ()
    {
                
        cam = Camera.main;

        if(cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }
        updater = FindObjectOfType<UIUpdater>();

    }
	
	// Update is called once per frame
	void Update ()
    {

        if(currentMouseMode != MouseMode.SELECTING)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1f;
        }

        zeroPlane = new Plane(Vector3.up, new Vector3(0, floorHeight * currentFloor, 0));
        InputUpdate();
        if(currentPrefab == null)
        {
            return;
        }
        PrefabUpdate();
	}

    private void PrefabUpdate()
    {
        Time.timeScale = 0;
        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        if (zeroPlane.Raycast(r, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = r.GetPoint(enter);            
            Collider[] cols;            

            cols = Physics.OverlapSphere(hitPoint, 0.5f, snapPointLayer);
            if(cols.Length > 0)
            {                
                currentPrefab.transform.position = cols[0].transform.position;

            }
            else
            {
                currentPrefab.transform.position = hitPoint;
            }                                                
        }
    }

    private void InputUpdate()
    {   
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CancelOperation();
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            CancelOperation();
            currentMouseMode = MouseMode.PLACING;            
            SpawnSatisfier();
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            SetEraseMode();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            RotatePrefab();
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            isLocating = !isLocating;
        }
        if(Input.GetMouseButtonDown(0))
        {
            if(EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if(currentMouseMode == MouseMode.ERASING)
            {
                EraseObject();
                return;
            }

            if(isLocating)
            {
                MoveEmployee();
                return;
            }
            PlacePrefab();
            currentMouseMode = MouseMode.SELECTING;
        }
    }

    private void EraseObject()
    {
        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        if (zeroPlane.Raycast(r, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = r.GetPoint(enter);
            Collider[] cols;
            cols = Physics.OverlapSphere(hitPoint, 0.5f);
            if (cols.Length > 0)
            {
                for (int i = 0; i < cols.Length; i++)
                {
                    Debug.Log(cols[i].name);                    
                    NeedSatisfier ns = cols[i].GetComponentInParent<NeedSatisfier>();
                    if(ns == null)
                    {
                        continue;
                    }
                    if(ns.Need == Employee.SatisfactionStatus.WORK)
                    {
                        Destroy(ns.owner.gameObject);
                    }

                    GameManager.Instance.AddMoney(ns.Value);
                    Destroy(ns.gameObject);
                    break;
                }
            }            
        }
    }

    private void CancelOperation()
    {
        if(currentPrefab != null)
        {
            Destroy(currentPrefab);
            currentPrefab = null;
        }
        currentMouseMode = MouseMode.SELECTING;
        updater.HideHint();
    }

    private void RotatePrefab()
    {
        if(currentPrefab == null)
        {
            return;
        }
        currentPrefab.transform.Rotate(Vector3.up, 90f);
    }

    private void SpawnSatisfier()
    {
        currentPrefab = Instantiate(satisfierPrefab);
        for (int i = 0; i < currentPrefab.transform.childCount; i++)
        {
            Transform child = currentPrefab.transform.GetChild(i);
            if (child.CompareTag("SnapPoint"))
            {
                child.gameObject.SetActive(false);
            }
        }
        NeedSatisfier ns = currentPrefab.GetComponent<NeedSatisfier>();
        updater.ShowHint("Placing " + ns.ObjectName + " ($" + ns.Value + ") \n Press R to rotate, press S to place again");
        isSatisfier = true;
    }

    private void PlacePrefab()
    {        
        if(isSatisfier)
        {
            isSatisfier = false;
            NeedSatisfier ns = currentPrefab.GetComponent<NeedSatisfier>();

            try
            {
                GameManager.Instance.RemoveMoney(ns.Value);
            }
            catch (Exception)
            {
                Debug.LogError("Not Enough Money!");                            
                CancelOperation();
                updater.ShowHint("Not Enough Money!");
                return;
            }

            SatisfierLocationDictionary.Instance.AddSatisfier(ns.Need, ns);
            if ( ns.Need == Employee.SatisfactionStatus.WORK )
            {
                SpawnEmployee(ns);
            }

            // Enable snappoints
            for (int i = 0; i < currentPrefab.transform.childCount; i++)
            {
                Transform child = currentPrefab.transform.GetChild(i);
                if (child.CompareTag("SnapPoint"))
                {
                    child.gameObject.SetActive(true);
                }
            }            

        }
        updater.HideHint();
        currentPrefab = null;        
    }

    private void SpawnEmployee(NeedSatisfier ns)
    {
        GameObject go = Instantiate(employeePrefab,employeeSpawnPoint.position,employeeSpawnPoint.rotation);
        Employee employee = go.GetComponent<Employee>();

        employee.BathroomSatisfactionFactor = UnityEngine.Random.Range(1, 5);
        employee.BathroomSatisfactionThreshold = UnityEngine.Random.Range(30, 60);

        employee.FoodSatisfactionFactor = UnityEngine.Random.Range(1, 5);
        employee.FoodSatisfactionThreshold = UnityEngine.Random.Range(30, 60);

        // TODO: Set when entertainment satisfiers are added.
        employee.EntertainmentFactor = 0;
        employee.EntertainmentThreshold = UnityEngine.Random.Range(30, 60);

        employee.SetWorkstation(ns);
        ns.owner = employee;
    }

    private void MoveEmployee()
    {
        if(currentEmployee == null)
        {
            return;
        }

        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        if (zeroPlane.Raycast(r, out enter))
        {            
            //Get the point that is clicked
            Vector3 hitPoint = r.GetPoint(enter);
            Vector3 dest = new Vector3(hitPoint.x, currentFloor * floorHeight, hitPoint.z);
            currentEmployee.MoveTo(dest);
        }
    }    
}
