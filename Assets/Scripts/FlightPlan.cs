
// https://dev.qgroundcontrol.com/master/en/file_formats/plan.html

[System.Serializable]
public class MissionItem
{
    public string type;    // "SimpleItem"
    public double AMSLAltAboveTerrain; // Altitude value shown to the user. HACK: force null
    public double Altitude;
    public int AltitudeMode;  // 1:??
    public bool autoContinue; // true
    public int command;       // MAV_CMD (16:MAV_CMD_NAV_WAYPOINT, 22:MAV_CMD_NAV_TAKEOFF)
    public int doJumpId;      // The target id, auto-numbered from 1.
    public int frame;         // MAV_FRAME (3:MAV_FRAME_GLOBAL_RELATIVE_ALT)
    public double[] param;    // HACK: params is reserved
}

[System.Serializable]
public class Mission
{
    public int version;      // Current version is 2
    public int firmwareType; // 12 = MAV_AUTOPILOT_PX4
    public int vehicleType;  // 2 = MAV_TYPE_QUADROTOR
    public int cruiseSpeed;  // The default forward speed for Fixed wing or VTOL vehicles
    public float hoverSpeed; // The default forward speed for multi-rotor vehicles.
    public MissionItem[] items;
    public double[] plannedHomePosition; // latitude, longitude and AMSL altitude
    public int globalPlanAltitudeMode;
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
    public string fileType;      // Must be "Plan"
    public string groundStation; // The name of the ground station which created this file
    public int version;          // Current version is 1
    public Mission mission;
    public GeoFence geoFence;
    public RallyPoints rallyPoints;
}