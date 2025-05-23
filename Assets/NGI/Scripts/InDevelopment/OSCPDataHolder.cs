using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: rename to OSCPContext or similar but generally try to get rid of this class
public class OSCPDataHolder : Singleton<OSCPDataHolder>
{

    public List<string> contentUrls = new List<string>();
    public string geoPoseServiceURL = "";
    public string currentH3Zone = "";

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
        if (contentUrls.Count == 0 || string.IsNullOrEmpty(geoPoseServiceURL) || string.IsNullOrEmpty(currentH3Zone))
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
        contentUrls.Clear();
        geoPoseServiceURL = string.Empty;
    }

}
