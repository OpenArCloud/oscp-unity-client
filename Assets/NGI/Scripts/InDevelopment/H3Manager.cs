using H3Lib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H3Manager : MonoBehaviour
{

    MyLocationInfo lastGpsLocation;
    const int kH3Resolution = 8;
    H3Index lastH3Index = new H3Lib.H3Index(0);
    UInt64 geoposeRequestId = 0;

    [SerializeField]float waitTimeLocationStart = 10;


    [Header("Test data Editor")]
    public float devLocationLatitude = 59.168705f;
    public float devLocationLongitude = 18.174704f;
    public float devLocationHDOP = 30.0f; // horizontal dilution of precision

    

    public enum LocalizationStatus
    {
        NotStarted,
        GetGPSData,
        NoGPSData,
        WaitForAPIAnswer,
        ServerError,
        CantLocalize,
        Ready
    }

    private void Start()
    {
        StartLocationService();
    }

    void StartLocationService()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        StartCoroutine(WaitForLoactionServiceStart());

#else

#endif

        GetH3IndexEditor(devLocationLatitude, devLocationLongitude);
    }


    IEnumerator WaitForLoactionServiceStart()
    {

        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
            yield break;

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }



        // Stops the location service if there is no need to query location updates continuously.
        Input.location.Stop();


    }

    public H3Index GetLastH3Index()
    {
        if(lastH3Index == null)
        {
            StartLocationService();
        }

        return lastH3Index;
    }


    // This method might be called publicly, for example from a Debug localizer or a separate GpsLocationService
    public void updateMyGpsLocation(LocationInfo locationInfo)
    {
        lastGpsLocation.altitude = locationInfo.altitude;
        lastGpsLocation.horizontalAccuracy = locationInfo.horizontalAccuracy;
        lastGpsLocation.latitude = locationInfo.latitude;
        lastGpsLocation.longitude = locationInfo.longitude;
        lastGpsLocation.timestamp = locationInfo.timestamp;
        lastGpsLocation.verticalAccuracy = locationInfo.verticalAccuracy;
       // hasGpsLocation = true;
      //  uim.statusDebug("Located GPS");
        Debug.Log("Updated GPS Location: "
            + " lat: " + lastGpsLocation.latitude
            + " lon: " + lastGpsLocation.longitude
            + " alt: " + lastGpsLocation.altitude
            + " hAccuracy: " + lastGpsLocation.horizontalAccuracy
            + " timestamp: " + lastGpsLocation.timestamp);

        decimal radLat = H3Lib.Api.DegsToRads((decimal)lastGpsLocation.latitude);
        decimal radLon = H3Lib.Api.DegsToRads((decimal)lastGpsLocation.longitude);
        H3Lib.GeoCoord geoCoord = new H3Lib.GeoCoord(radLat, radLon);
        lastH3Index = H3Lib.Extensions.GeoCoordExtensions.ToH3Index(geoCoord, kH3Resolution);
        Debug.Log("  H3 index (level " + kH3Resolution + "): " + lastH3Index.ToString());
    }

#if UNITY_EDITOR
    //Only for use in the editor. This takes lat lon and returns a H3 Index to console log
    public void GetH3IndexEditor(float lat, float lon)
    {

        decimal radLat = H3Lib.Api.DegsToRads((decimal)lat);
        decimal radLon = H3Lib.Api.DegsToRads((decimal)lon);
        H3Lib.GeoCoord geoCoord = new H3Lib.GeoCoord(radLat, radLon);
        lastH3Index = H3Lib.Extensions.GeoCoordExtensions.ToH3Index(geoCoord, kH3Resolution);



        Debug.Log("  H3 index (level " + kH3Resolution + "): " + lastH3Index.ToString());
    }
#endif


    struct MyLocationInfo
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public float altitude { get; set; }
        public float horizontalAccuracy { get; set; }
        public float verticalAccuracy { get; set; }
        public double timestamp { get; set; }
    }
}
