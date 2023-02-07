using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[System.Serializable]
public class Mission
{
    public int cruiseSpeed;
    public int firmwareType;
    public int globalPlanAltitudeMode;
    public int hoverSpeed;
    public int vehicleType;
    public int version;
}

[System.Serializable]
public class FlightPlan
{
    public string fileType;
    public string groundStation;
    public Mission mission;
    public int version;
}

public class UI : MonoBehaviour
{
    public GameObject myPrefab;
    public GameObject[] viewPoints = new GameObject[0];
    public TextAsset planFile;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        Debug.Log("onEnable");
        FlightPlan plan = JsonUtility.FromJson<FlightPlan>(planFile.text);
        Debug.Log("fileType:" + plan.fileType);
        Debug.Log("groundStation:" + plan.groundStation);
        Debug.Log("mission.cruiseSpeed:" + plan.mission.cruiseSpeed);

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button btnClear = root.Q<Button>("ButtonClear");
        Button btnPlan = root.Q<Button>("ButtonPlan");
        btnClear.clicked += () =>
        {
            Debug.Log("btnClear.clicked");
            if (viewPoints.Length > 0)
            {
                for (int i=0; i<viewPoints.Length; i++)
                {
                    Destroy(viewPoints[i]);
                }
                viewPoints = new GameObject[0];
            }
        };
        btnPlan.clicked += () =>
        {
            if (viewPoints.Length == 0)
            {
                Debug.Log("btnPlan.clicked");
                int count = 24;
                float radius = 3;
                viewPoints = new GameObject[count * 2];
                for (int i=0; i<count; i++)
                {
                    float angle = Mathf.PI * 2 / (float)count * (float)i;
                    float x = Mathf.Cos(angle) * radius;
                    float y = Mathf.Sin(angle) * radius;
                    Quaternion q = Quaternion.LookRotation(new Vector3(-x, 2, -y));
                    viewPoints[i] = Instantiate(myPrefab, new Vector3(x, (float)2.5, y), q);
                    viewPoints[count + i] = Instantiate(myPrefab, new Vector3(x, (float)3.5, y), q);
                }
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
