using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public const double NULL_VALUE = 112233.44f; // HACK to force null in json file

    public GameObject myPrefab;
    public GameObject[] viewPoints = new GameObject[0];
    public TextAsset planFile;
    public FlightPlan plan;
    public double latitude = 47.7028601;
    public double longitude = -122.1398441;
    public int count = 16;
    public float radius1 = 3;
    public float radius2 = 3.5f;
    public bool twoRows = true;
    public float height1 = 2.5f;
    public float height2 = 3.5f;
    public float hold = 2.0f;
    public float speed = 0.22352f;
    public bool specifyYaw = true;

    private void clear()
    {
        for (int i = 0; i < viewPoints.Length; i++)
        {
            Destroy(viewPoints[i]);
        }
        viewPoints = new GameObject[0];
    }

    private void createPlan()
    {
        Debug.Log("creating a plan");
        clear();
        viewPoints = new GameObject[count * (twoRows ? 2 : 1)];
        for (int i = 0; i < count; i++)
        {
            float angle = 2 * MathF.PI / 2 - MathF.PI * 2 / (float)count * (float)i; // Start from South
            float x = MathF.Sin(angle);
            float y = MathF.Cos(angle);
            Quaternion q = Quaternion.LookRotation(new Vector3(-x, 1, -y));
            viewPoints[i] = Instantiate(myPrefab, new Vector3(x * radius1, height1, y * radius1), q);
            if (twoRows)
            {
                viewPoints[count + i] = Instantiate(myPrefab, new Vector3(x * radius2, height2, y * radius2), q);
            }
        }
    }

    private void save()
    {
        if (viewPoints.Length == 0)
        {
            return;
        }

        GeoLocation center = new GeoLocation(latitude, longitude);

        MissionItem itemTemplate = plan.mission.items[0];
        itemTemplate.AMSLAltAboveTerrain = NULL_VALUE;
        string strItem = JsonUtility.ToJson(itemTemplate);

        Vector3 homePos = viewPoints[0].transform.position;
        GeoLocation home = new GeoLocation(center, homePos.x * 1.5f, homePos.z * 1.5f);
        plan.mission.plannedHomePosition = new double[] { home.lati, home.longi, homePos.y };
        plan.mission.hoverSpeed = speed;
        plan.mission.items = new MissionItem[viewPoints.Length + 1];

        // NAV_TAKEOFF
        MissionItem item = JsonUtility.FromJson<MissionItem>(strItem);
        item.command = 22;
        itemTemplate.Altitude = homePos.y;
        item.param = new double[] { hold, 0, 0, NULL_VALUE, home.lati, home.longi, homePos.y };
        item.doJumpId = 1;
        item.Altitude = homePos.y;
        plan.mission.items[0] = item;

        /*
        // CHANGE_SPEED
        item = JsonUtility.FromJson<MissionItem>(strItem);
        item.command = 178;
        itemTemplate.Altitude = homePos.y;
        item.param = new double[] { 1, speed, -1, 0, 0, 0 };
        item.doJumpId = 2;
        */

        for (int i = 0; i < viewPoints.Length; i++)
        {
            item = JsonUtility.FromJson<MissionItem>(strItem);
            GameObject vp = viewPoints[i];
            Vector3 angles = vp.transform.rotation.eulerAngles;
            double yaw = specifyYaw ? (int)(angles.y + 90.5) % 360 : NULL_VALUE;
            // Debug.Log("angle" + angles.x + ":" + angles.y + ":" + angles.z);
            Vector3 pos = vp.transform.position;
            GeoLocation loc = new GeoLocation(center, pos.x, pos.z);
            // NAV_WAYPOINT
            item.command = 16;
            item.doJumpId = i + 2;
            item.Altitude = pos.y;
            item.param = new double[] { hold, 0, 0, yaw, loc.lati, loc.longi, pos.y };
            plan.mission.items[i + 1] = item;
        }

        string saveFile = Application.persistentDataPath + "/generated.plan";
        Debug.Log("btnSave:" + saveFile);
        plan.groundStation = "Netdrones Flight Planner";
        string jsonString = JsonUtility.ToJson(plan);
        jsonString = jsonString.Replace("param", "params"); // hack
        jsonString = jsonString.Replace(NULL_VALUE.ToString(), "null");
        File.WriteAllText(saveFile, jsonString);
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
        btnClear.clicked += clear;
        btnPlan.clicked += createPlan;
        btnSave.clicked += save;
    }
}
