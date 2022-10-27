using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

[RequireComponent(typeof(H3Manager))]
public class SSRManager : MonoBehaviour
{
    const string kDefaultSsdServerUrl = "https://ssd.orbit-lab.org";
    const string kDefaultCountryCode = "us";
    [SerializeField] string ssdServerURL = kDefaultSsdServerUrl;

    const string kDefaultScdServerUrl = "https://scd.orbit-lab.org";

    [SerializeField] GameObject listItemPrefab;

    [SerializeField] RectTransform geoPoseServiceItemsGui;
    [SerializeField] RectTransform contentDiscoveryServiceItemsGui;

    [SerializeField] H3Manager h3Manager; // TODO: rename to LocationManager?

    public static event Action<string> ServerResponseGet = null;

    private void OnEnable()
    {
        ServerResponseGet += HandleResponse;
        OAuth2Authentication.IsAuthenticated += HandleAuthenticate;
    }

    private void OnDisable()
    {
        ServerResponseGet -= HandleResponse;
        OAuth2Authentication.IsAuthenticated += HandleAuthenticate;
    }

    private void Start()
    {
        Console.WriteLine("SSRManager.Start");
        if (h3Manager == null)
        {
            h3Manager = GetComponent<H3Manager>();
        }
    }

    private void HandleResponse(string response)
    {
        CreateListItems(JSON.Parse(response));
    }

    public void CreateListItems(JSONNode response)
    {
        Console.WriteLine("SSRManager.CreateListItems");
        List<JSONNode> availableGeoPoseServices = new List<JSONNode>();
        List<JSONNode> availableContentServices = new List<JSONNode>();

        // NOTE: the SpatialServiceDiscovery returns an array of SpatialServiceRecords (SSRs),
        // and each SSR can can contain multiple services.
        int numSpatialServiceRecords = response.Count;
        Console.WriteLine("Received " + numSpatialServiceRecords.ToString() + " SSRs");
        for (int ssr_idx = 0; ssr_idx < numSpatialServiceRecords; ssr_idx++) {
            int numServicesInSsr = response[ssr_idx]["services"].Count;
            Console.WriteLine("SSR " + ssr_idx.ToString() + " contains " + numServicesInSsr.ToString() + " services");

            for (int service_idx = 0; service_idx < numServicesInSsr; service_idx++) {
                if (string.Equals(response[ssr_idx]["services"][service_idx]["type"], "geopose")) {
                    availableGeoPoseServices.Add(response[ssr_idx]["services"][service_idx]);
                    //Debug.Log(response[ssr_idx]["services"][service_idx]);
                }

                if (string.Equals(response[ssr_idx]["services"][service_idx]["type"], "content-discovery")) {
                    availableContentServices.Add(response[ssr_idx]["services"][service_idx]);
                    //Debug.Log(response[ssr_idx]["services"][service_idx]);
                }
            }
        }


        for (int i = 0; i < availableGeoPoseServices.Count; i++)
        {
            var tempObj = Instantiate(listItemPrefab, geoPoseServiceItemsGui);
            tempObj.GetComponent<SSRItem>().SetValues(availableGeoPoseServices[i]);
        }
        Console.WriteLine("availableGeoPoseServices: " + availableGeoPoseServices.Count.ToString());
#if UNITY_EDITOR
        if (availableContentServices.Count == 0)
        {
            Debug.Log("WARNING: no content services available at your location. Adding default server " + kDefaultScdServerUrl);
            availableContentServices.Add(kDefaultScdServerUrl);
        }
#endif

        for (int i = 0; i < availableContentServices.Count; i++)
        {
            var tempObj = Instantiate(listItemPrefab, contentDiscoveryServiceItemsGui);
            tempObj.GetComponent<SSRItem>().SetValues(availableContentServices[i]);
        }
        Console.WriteLine("availableContentServices: " + availableContentServices.Count.ToString());
    }

    public string GetSelectedGeoPoseService()
    {
        // Note: We have to choose one GeoPose server
        SSRItem[] ssr = geoPoseServiceItemsGui.GetComponentsInChildren<SSRItem>();

        foreach (var item in ssr)
        {
            // take the first selected for now. 
            // later we can think about how to do localization with multiple providers
            if (item.IsSelected)
            {
                return item.GetURL();
            }
        }

        return String.Empty;
    }

    public List<string> GetSelectedContentServices()
    {
        List<string> scdURLs = new List<string>();

        // Note: We can select multiple SCD servers
        SSRItem[] availableContentServices = contentDiscoveryServiceItemsGui.GetComponentsInChildren<SSRItem>();

        if (availableContentServices.Length > 0)
        {
            for (int i = 0; i < availableContentServices.Length; i++)
            {
                if (availableContentServices[i].IsSelected)
                {
                    scdURLs.Add(availableContentServices[i].GetURL());
                }
            }
        }

        return scdURLs;
    }

    private async void HandleAuthenticate(bool isAuthenticated)
    {
        Console.WriteLine("SSRManager.HandleAuthenticate");
        if (!isAuthenticated) {
            Console.WriteLine("Not authenticated. Aborting.");
            return;
        }

        string ssrs = await GetServicesInCurrentH3();
        if (String.IsNullOrEmpty(ssrs)) {
            // TODO: inform user
            Console.WriteLine("It seems no services are available at your location");
            return;
        }

        ServerResponseGet?.Invoke(ssrs);
    }

    public async Task<string> GetServicesInCurrentH3()
    {
        Console.WriteLine("SSRManager.GetServicesInCurrentH3");
#if UNITY_EDITOR
        // nothing to do
#else
        // wait until location is available
        if (Input.location.status == LocationServiceStatus.Stopped || Input.location.status == LocationServiceStatus.Failed)
        {
            h3Manager.StartLocationService();
        }

        Console.WriteLine("waiting for location services to become available... ");
        while (!h3Manager.IsLocationAvailable())
        {
            await Task.Delay(1);
        }
#endif
        float latitude = h3Manager.GetLatitude();
        float longitude = h3Manager.GetLongitude();
        Console.WriteLine("Location is available! GPS coordinates (lat, lon): " + latitude.ToString() + ", " + longitude.ToString());

        Console.WriteLine("Looking up country code...");
        string countryCode = await GetCountryCode(latitude, longitude);
        string h3Index = h3Manager.GetH3Index().ToString();

        Console.WriteLine("Searching for available spatial services...");
        string ssrs = await GetSpatialServiceRecords(countryCode, h3Index);
        return ssrs;
    }

    // Spatial Service Discovery
    public async Task<string> GetSpatialServiceRecords(string countryCode, string h3Index)
    {
        Console.WriteLine("SSRManager.GetSpatialServiceRecords");
        try {
            string requestUrl = ssdServerURL + "/" + countryCode + "/" + "ssrs?h3Index=" + h3Index;
            Console.WriteLine("SSD Request URL: " + requestUrl);
            HttpWebRequest ssdRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            ssdRequest.Method = "GET";
            // ssdRequest.Headers.Add(string.Format("Authorization: Bearer " + accessToken));
            ssdRequest.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse ssdResponse = (HttpWebResponse) (await ssdRequest.GetResponseAsync());
            if (ssdResponse.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Could not retrieve spatial services");
                return ""; // TODO: return null instead
            }

            using (StreamReader ssdResponseReader = new StreamReader(ssdResponse.GetResponseStream()))
            {
                string ssdResponseText = await ssdResponseReader.ReadToEndAsync();
                Console.WriteLine("Response from Spatial Service Discovery: " + ssdResponseText);
                return ssdResponseText;
            }
        } catch(WebException e) {
            // TODO: inform user
            Debug.Log("Exception happened:" + e.Message);
            if (e.Status == WebExceptionStatus.ProtocolError) {
                Console.WriteLine("Status Code : {0}",  ((HttpWebResponse)e.Response).StatusCode);
                Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
            }
        }
        return "";  // TODO: return null instead
    }

    // lookup from GPS to country code
    // TODO: move into H3Manager which should be renamed LocationManager
    #if UNITY_EDITOR
    [SerializeField] bool useFakeCountryCode = false;
    [SerializeField] string devCountryCode = "us";
    #endif
    public async Task<string> GetCountryCode(float latitude, float longitude) {
        Console.WriteLine("GetCountryCode");
        #if UNITY_EDITOR
            if (useFakeCountryCode) {
                return devCountryCode;
            }
        #endif
        try {
            string osmRequestUrl = "https://nominatim.openstreetmap.org/reverse?format=json"
                    + "&lat=" + latitude.ToString() + "&lon=" + longitude.ToString()
                    + "&zoom=1" + "&email=info%40michaelvogt.eu";
            Console.WriteLine("osmRequestUrl: " + osmRequestUrl);
            HttpWebRequest osmRequest = (HttpWebRequest)WebRequest.Create(osmRequestUrl);
            osmRequest.Method = "GET";

            HttpWebResponse osmResponse = (HttpWebResponse) (await osmRequest.GetResponseAsync());
            if (osmResponse.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Could not retrieve country code. Using default instead: " + kDefaultCountryCode);
                return kDefaultCountryCode;
            }

            using (StreamReader osmResponseReader = new StreamReader(osmResponse.GetResponseStream()))
            {
                string osmResponseText = await osmResponseReader.ReadToEndAsync();
                Console.WriteLine("Response from OpenStreetMap: " + osmResponseText);
                JSONNode osmResponseJson = JSON.Parse(osmResponseText);
                string countryCode = osmResponseJson["address"]["country_code"];
                Console.WriteLine("  countryCode: " + countryCode);
                return countryCode;
            }
        }
        catch(WebException e) {
            // TODO: inform user
            Debug.Log("Exception happened:" + e.Message);
            if (e.Status == WebExceptionStatus.ProtocolError) {
                Console.WriteLine("Status Code : {0}",  ((HttpWebResponse)e.Response).StatusCode);
                Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
            }
        }

        Console.WriteLine("Could not retrieve country code. Using default instead: " + kDefaultCountryCode);
        return kDefaultCountryCode;
    }

    public void LoadSceneAsync(string sceneName)
    {
        Console.WriteLine("SSRManager.LoadSceneAsync");
        //OSCPDataHolder.Instance.ClearData();
        OSCPDataHolder.Instance.contentUrls = GetSelectedContentServices();
        OSCPDataHolder.Instance.geoPoseServiceURL = GetSelectedGeoPoseService();

        //TODO: Inform the user that their selection has some errors
        if (OSCPDataHolder.Instance.CheckSelectedServices())
        {
            Console.WriteLine("Loading scene...");
            SceneManager.LoadSceneAsync(sceneName);
        }
        else
        {
            Debug.Log("Needed values missing in OSCPDataHolder, aborting scene change");
        }
    }
}
