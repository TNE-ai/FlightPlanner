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

public struct GeoLocation
{
    public double lati;
    public double longi;
    public GeoLocation(double _lati, double _longi)
    {
        lati = _lati;
        longi = _longi;
    }

    public GeoLocation(GeoLocation anchor, float x, float y)
    {
        lati = anchor.lati + (double)y / 111000;
        longi = anchor.longi + (double)x / 111000 / Math.Cos(Math.PI * anchor.lati / 180);
    }
}

public class UI : MonoBehaviour
{
    public GameObject myPrefab;
    public GameObject[] viewPoints = new GameObject[0];
    public TextAsset planFile;
    public FlightPlan plan;
    public double latitude = 47.7028601;
    public double longitude = -122.1398441;
    public int count = 32;
    public float radius1 = 3;
    public float radius2 = 3.5f;
    public float height1 = 2.5f;
    public float height2 = 3.5f;
    public float hold = 2.0f;

    private void clear()
    {
        for (int i = 0; i < viewPoints.Length; i++)
        {
            Destroy(viewPoints[i]);
        }
        viewPoints = new GameObject[0];
    }
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
            clear();
        };
        btnPlan.clicked += () =>
        {
            Debug.Log("btnPlan.clicked");
            clear();
            viewPoints = new GameObject[count * 2];
            for (int i=0; i<count; i++)
            {
                float angle = 2 * MathF.PI / 2 - MathF.PI * 2 / (float)count * (float)i; // Start from South
                float x = MathF.Sin(angle);
                float y = MathF.Cos(angle);
                Quaternion q = Quaternion.LookRotation(new Vector3(-x, 1, -y));
                viewPoints[i] = Instantiate(myPrefab, new Vector3(x * radius1, height1, y * radius1), q);
                viewPoints[count + i] = Instantiate(myPrefab, new Vector3(x * radius2, height2, y * radius2), q);
            }
        };
        btnSave.clicked += () =>
        {
            if (viewPoints.Length == 0)
            {
                return;
            }

            GeoLocation center = new GeoLocation(latitude, longitude);

            MissionItem itemTemplate = plan.mission.items[0];
            itemTemplate.AMSLAltAboveTerrain = 1122334455; // HACK
            string strItem = JsonUtility.ToJson(itemTemplate);

            Vector3 homePos = viewPoints[0].transform.position;
            GeoLocation home = new GeoLocation(center, homePos.x * 1.5f, homePos.z * 1.5f);
            plan.mission.plannedHomePosition = new double[] { home.lati, home.longi, homePos.y };
            plan.mission.items = new MissionItem[viewPoints.Length];
            for (int i=0; i<viewPoints.Length; i++)
            {
                MissionItem item = JsonUtility.FromJson<MissionItem>(strItem);
                GameObject vp = viewPoints[i];
                Vector3 angles = vp.transform.rotation.eulerAngles;
                int yaw = (int)(angles.y + 0.5);
                // Debug.Log("angle" + angles.x + ":" + angles.y + ":" + angles.z);
                Vector3 pos = vp.transform.position;
                GeoLocation loc = new GeoLocation(center, pos.x, pos.z);
                if (i > 0)
                {
                    item.command = 16;
                }
                item.doJumpId = i + 1;
                item.Altitude = pos.y;
                item.param = new double[] { hold, 0, 0, yaw, loc.lati, loc.longi, pos.y };
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
