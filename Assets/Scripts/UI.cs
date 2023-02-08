using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// https://dev.qgroundcontrol.com/master/en/file_formats/plan.html
[System.Serializable]
public class MissionItem
{
    public int AMSLAltAboveTerrain; // HACK: force null
    public double Altitude;
    public int AltitudeMode;
    public bool autoContinue;
    public int command;
    public int doJumpId;
    public int frame;
    public double[] param; // HACK: params is reserved
    public string type;
}

[System.Serializable]
public class Mission
{
    public int cruiseSpeed;
    public int firmwareType;
    public int globalPlanAltitudeMode;
    public int hoverSpeed;
    public int vehicleType;
    public int version;
    public MissionItem[] items;
    public double[] plannedHomePosition;
}

[System.Serializable]
public class GeoFence
{
    public double[] circles;
    public double[] polygons;
    public int version;
}

[System.Serializable]
public class RallyPoints
{
    public double[] points;
    public int version;
}

[System.Serializable]
public class FlightPlan
{
    public string fileType;
    public GeoFence geoFence;
    public string groundStation;
    public Mission mission;
    public RallyPoints rallyPoints;
    public int version;
}

public class UI : MonoBehaviour
{
    public GameObject myPrefab;
    public GameObject[] viewPoints = new GameObject[0];
    public TextAsset planFile;
    public FlightPlan plan;
    public double lati = 47.6347922956f;
    public double longi = -122.24058493262723f;

    private void OnEnable()
    {
        Debug.Log("onEnable");
        plan = JsonUtility.FromJson<FlightPlan>(planFile.text);
        Debug.Log("fileType:" + plan.fileType);
        Debug.Log("groundStation:" + plan.groundStation);
        Debug.Log("mission.cruiseSpeed:" + plan.mission.cruiseSpeed);
        Debug.Log("mission.items:" + plan.mission.items.Length);
        Debug.Log("mission.items[0].Altitude:" + plan.mission.items[0].Altitude);
        Debug.Log("mission.items[0].AMSLAltAboveTerrain:" + plan.mission.items[0].AMSLAltAboveTerrain);

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button btnClear = root.Q<Button>("ButtonClear");
        Button btnPlan = root.Q<Button>("ButtonPlan");
        Button btnSave = root.Q<Button>("ButtonSave");
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
                    float angle = MathF.PI * 2 / (float)count * (float)i;
                    float x = MathF.Cos(angle) * radius;
                    float y = MathF.Sin(angle) * radius;
                    Quaternion q = Quaternion.LookRotation(new Vector3(-x, 2, -y));
                    viewPoints[i] = Instantiate(myPrefab, new Vector3(x, 2.5f, y), q);
                    viewPoints[count + i] = Instantiate(myPrefab, new Vector3(x * 1.2f, 3.5f, y * 1.2f), q);
                }
            }
        };
        btnSave.clicked += () =>
        {
            if (viewPoints.Length == 0)
            {
                return;
            }
            MissionItem itemTemplate = plan.mission.items[0];
            itemTemplate.AMSLAltAboveTerrain = 1122334455; // HACK
            string strItem = JsonUtility.ToJson(itemTemplate);

            plan.mission.plannedHomePosition = new double[] { lati, longi, viewPoints[0].transform.position.y };
            plan.mission.items = new MissionItem[viewPoints.Length];
            for (int i=0; i<viewPoints.Length; i++)
            {
                MissionItem item = JsonUtility.FromJson<MissionItem>(strItem);
                GameObject vp = viewPoints[i];
                Vector3 pos = vp.transform.position;
                if (i > 0)
                {
                    item.command = 16;
                }
                item.doJumpId = i + 1;
                item.param = new double[] { 0, 0, 0, 90, lati + (double)pos.x / 111000,
                                           longi + (double)pos.z / 111000 / Math.Cos(lati), pos.y };
                plan.mission.items[i] = item;
            }

            string saveFile = Application.persistentDataPath + "/generated.plan";
            Debug.Log("btnSave:" + saveFile);
            plan.groundStation = "Netdrone Planner 1";
            string jsonString = JsonUtility.ToJson(plan);
            jsonString = jsonString.Replace("param", "params"); // hack
            jsonString = jsonString.Replace("1122334455", "null"); // hack
            File.WriteAllText(saveFile, jsonString);
        };
    }

}
