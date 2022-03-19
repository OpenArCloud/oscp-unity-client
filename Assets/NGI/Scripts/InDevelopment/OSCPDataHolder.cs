using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCPDataHolder : Singleton<OSCPDataHolder>
{

    public List<string> ContentUrls = new List<string>();

    public string GeoPoseServieURL = "";

    public string H3CurrentZone = "";


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

    public void ClearData()
    {
        ContentUrls.Clear();
        GeoPoseServieURL = string.Empty;
    }

}