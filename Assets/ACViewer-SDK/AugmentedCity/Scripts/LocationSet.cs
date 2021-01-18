using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class LocationSet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       /* PlayerPrefs.SetInt("LocLoaded", 0);
        PlayerPrefs.SetFloat("Latitude", 59.877427f);
        PlayerPrefs.SetFloat("Longitude", 30.318510f);*/
        PlayerPrefs.SetString("Latitud", "59.832010");
        PlayerPrefs.SetString("Longitud", "30.332837");

    #if PLATFORM_ANDROID || UNITY_IOS
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
            }
        #endif
        StartCoroutine(Locate());



    }

    public void UpdateLocation() {
        StartCoroutine(Locate());
    }


    IEnumerator Locate()
    {
        Debug.Log("Started");
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser) { Debug.Log("geo not enabled"); PlayerPrefs.SetInt("LocLoaded", -1);
            yield break; }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            PlayerPrefs.SetInt("LocLoaded", -2);

            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            PlayerPrefs.SetFloat("Latitude", Input.location.lastData.latitude);
            PlayerPrefs.SetFloat("Longitude", Input.location.lastData.longitude);
            PlayerPrefs.SetInt("LocLoaded", 1);



        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }

}
