using H3Lib;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class H3Manager : MonoBehaviour
{
    // This is from AugmentedCity API. do we need it?
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

    struct MyLocationInfo
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public float altitude { get; set; }
        public float horizontalAccuracy { get; set; }
        public float verticalAccuracy { get; set; }
        public double timestamp { get; set; }
    }

    bool hasGpsLocation = false;
    MyLocationInfo lastGpsLocation; // TODO: rename to lastLocationInfo as it might not only be based on GPS
    bool hasH3Location = false;
    const int kH3Resolution = 8;
    H3Index lastH3Index = new H3Lib.H3Index(0);

    [SerializeField] int kMaxWaitTimeSecondsForLocationStart = 20;

    [Header("Test data Editor")]
    public float devLocationLatitude = 59.168705f;
    public float devLocationLongitude = 18.174704f;
    public float devLocationHDOP = 30.0f; // horizontal dilution of precision


    private void Start()
    {
#if UNITY_EDITOR
        StartLocationService();
#endif
    }

    public void StartLocationService()
    {
#if UNITY_EDITOR
        GetH3IndexEditor(devLocationLatitude, devLocationLongitude);
#elif (UNITY_ANDROID || UNITY_IOS)
        StartCoroutine(WaitForLocationServiceStart());
#endif
    }

    // Stops the location service if there is no need to query location updates continuously.
    public void StopLocationService()
    {
        StopCoroutine("UpdateLocationPeriodically");
        Input.location.Stop();
    }

    IEnumerator WaitForLocationServiceStart()
    {
        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = kMaxWaitTimeSecondsForLocationStart;
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

            updateMyGpsLocation(Input.location.lastData);
        }

        StartCoroutine("UpdateLocationPeriodically");
    }

    IEnumerator UpdateLocationPeriodically() {
        for (;;) {
            // execute block of code here
            updateMyGpsLocation(Input.location.lastData);
            yield return new WaitForSeconds(10);
        }
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
        hasGpsLocation = true;
        /*
        Debug.Log("Updated GPS Location: "
            + " lat: " + lastGpsLocation.latitude
            + " lon: " + lastGpsLocation.longitude
            + " alt: " + lastGpsLocation.altitude
            + " hAccuracy: " + lastGpsLocation.horizontalAccuracy
             + " timestamp: " + lastGpsLocation.timestamp);
        */

        decimal radLat = H3Lib.Api.DegsToRads((decimal)lastGpsLocation.latitude);
        decimal radLon = H3Lib.Api.DegsToRads((decimal)lastGpsLocation.longitude);
        H3Lib.GeoCoord geoCoord = new H3Lib.GeoCoord(radLat, radLon);
        lastH3Index = H3Lib.Extensions.GeoCoordExtensions.ToH3Index(geoCoord, kH3Resolution);
        //Debug.Log("  H3 index (level " + kH3Resolution + "): " + lastH3Index.ToString());
        OSCPDataHolder.Instance.H3CurrentZone = lastH3Index.ToString();
        hasH3Location = true;
    }

#if UNITY_EDITOR // TODO: this method should be removed as mostly redundant
    //Only for use in the editor. This takes lat lon and returns a H3 Index to console log
    public void GetH3IndexEditor(float lat, float lon)
    {
        decimal radLat = H3Lib.Api.DegsToRads((decimal)lat);
        decimal radLon = H3Lib.Api.DegsToRads((decimal)lon);
        H3Lib.GeoCoord geoCoord = new H3Lib.GeoCoord(radLat, radLon);
        lastH3Index = H3Lib.Extensions.GeoCoordExtensions.ToH3Index(geoCoord, kH3Resolution);
        //Debug.Log("  H3 index (level " + kH3Resolution + "): " + lastH3Index.ToString());
        OSCPDataHolder.Instance.H3CurrentZone = lastH3Index.ToString();

        hasGpsLocation = true;
        hasH3Location = true;
    }
#endif

    public H3Index GetH3Index()
    {
        return lastH3Index;
    }

    public float GetLatitude() {
#if UNITY_EDITOR
        return devLocationLatitude;
#else
        return lastGpsLocation.latitude;
#endif
    }

    public float GetLongitude() {
#if UNITY_EDITOR
        return devLocationLongitude;
#else
        return lastGpsLocation.longitude;
#endif
    }

    public bool IsLocationAvailable() {
        return hasGpsLocation && hasH3Location;
    }
}
