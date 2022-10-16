using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: rename to OSCPContext or similar but generally try to get rid of this class
public class OSCPDataHolder : Singleton<OSCPDataHolder>
{

    public List<string> ContentUrls = new List<string>();

    public string GeoPoseServieURL = ""; // TODO: rename to GeoPoseServiceURL. But it should be in SSRManager

    public string H3CurrentZone = "";

    public Vector3 lastPositon;
    public Quaternion lastOrientation;

    public double latitude;
    public double longitude;
    public double ellipsoidHeight;



    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool CheckSelectedServices()
    {
        if (ContentUrls.Count == 0 || string.IsNullOrEmpty(GeoPoseServieURL) || string.IsNullOrEmpty(H3CurrentZone))
        {
            //TODO: Inform the user which is missing
            return false;
        }

        return true;

    }

    public void UpdateCoordinates(double lat, double lon, double height)
    {
        latitude = lat;
        longitude = lon;
        ellipsoidHeight = height;
    }

    public void UpdateLocation(Vector3 pos, Quaternion rotation)
    {
        lastPositon = pos;
        lastOrientation = rotation;
    }

    public void ClearURLs()
    {
        ContentUrls.Clear();
        GeoPoseServieURL = string.Empty;
    }

}
