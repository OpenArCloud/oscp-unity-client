using Newtonsoft.Json;
using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// TODO: this class should be merged into SCRManager. 
// Orbit is just one possible deployment of SpatialContentDiscovery.
// TODO: currently the contents are queried at startup which is wrong. They should be queried after localization.
public class OrbitAPI : MonoBehaviour
{
    [SerializeField] private bool devLoadContentsFromFile;
    [SerializeField] private OAuth2Authentication oAuth2Authentication;
    [SerializeField] private SCRManager scrManager;
    
    public static event Action<string> ServerResponseGet;
    //public static event Action<bool, string> ServerResponse; // unused


    private void Start()
    {
        Console.WriteLine("OrbitAPI.Start");
    }

    // TODO: names of methods. this should be GetRecords or similar. 
    // But, ideally, GetSpatialContentRecords should be used directly
    public async Task LoadItemsFromServer()
    {
        Console.WriteLine("OrbitAPI.LoadItemsFromServer");
        if (devLoadContentsFromFile)
        {
            scrManager.LoadFromJsonFile();
            return;
        }
        
        string accessToken = GetAccesToken();
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.Log("no access token! but querying contents is still possible");
            //TODO: Inform the user
            //return;
        }

        //TODO: Give the possibility for users to change topic
        // TODO: this only loads when they were not loaded before. 
        // This is OK only as long as we don't expect the user to move to another H3 cell,
        // or we assume the contents won't change.
        if (scrManager.spatialContentRecords == null || scrManager.spatialContentRecords.Length == 0)
        {
            await GetSpatialContentRecords(accessToken, "history", OSCPDataHolder.Instance.currentH3Zone);
        }
    }

    public async Task UpdateRecord(SCRItem scr)
    {
        string json = ConvertSCRtoString(scr);
        string accessToken = GetAccesToken();
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.Log("no access token! cannot update record");
            //TODO: Inform the user
            return;
        }

        //TODO: Add ability to change topic. Currently hardcoded to history
        await UpdateSpatialContentRecord(accessToken, scr.id, "history", json);
    }

    public async Task<string> CreateRecord(SCRItem scr)
    {
        string json = ConvertSCRtoString(scr);
        string accessToken = GetAccesToken();
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.Log("no access token! cannot create record");
            //TODO: Inform the user
            return null;
        }

        string id = await CreateSpatialContentRecord(accessToken, "history", json);
        return id;
    }

    public async Task<bool> DeleteRecord(string recordID)
    {
        string accessToken = GetAccesToken();
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.Log("no access token! cannot delete record");
            //TODO: Inform the user
            return false;
        }

        bool isDeleted = await DeleteSpatialContentRecord(accessToken, recordID, "history");
        return isDeleted;
    }

    public string ConvertSCRtoString(SCRItem scr)
    {
        string json = JsonConvert.SerializeObject(scr,
                new JsonSerializerSettings(){
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });
        return json;
    }

    private string GetAccesToken()
    {
        // TODO: this assumes that the accessToken is already stored in the cache.
        // Why don't we query it from the OAuth module?
        // Can we just ask the user here again for authentication?
        Console.WriteLine("OrbitAPI.GetAccesToken");
        UserSessionCache userSessionCache = new UserSessionCache();
        SaveDataManager.LoadJsonData(userSessionCache);
        if (!string.IsNullOrEmpty(userSessionCache.getAccessToken()))
        {
            return userSessionCache.getAccessToken();
        }
        return null;
    }

    #region Test Methods during development

    public async Task GetSpatialContentRecords(string accessToken, string topic, string h3Index)
    {
        Console.WriteLine("OrbitAPI.GetSpatialContentRecords");

        // https://scd.orbit-lab.org/scrs/history?h3Index=8808866927fffff
        output("Making API Call to read content...");

        //TODO: Ability to query multiple content servers, not just the first one in the list
        string scdServerURL = OSCPDataHolder.Instance.contentUrls[0];
        HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(scdServerURL + "/scrs/" + topic + "?h3Index=" + h3Index);
        getRequest.Method = "GET";
        if (accessToken != null) {
            // accessToken is not necessary for GET, but can be submitted optionally
            getRequest.Headers.Add(string.Format("Authorization: Bearer " + accessToken));
        }
        getRequest.ContentType = "application/x-www-form-urlencoded";

        WebResponse apiResponse = await getRequest.GetResponseAsync();
        using (StreamReader apiInforResponseReader = new StreamReader(apiResponse.GetResponseStream()))
        {
            string apiInfoResponseText = await apiInforResponseReader.ReadToEndAsync();
            output("Response from Spatial Content Discovery: " + apiInfoResponseText);
            ServerResponseGet?.Invoke(apiInfoResponseText);
        }
    }

    // string testNewObjectJsonBody = "[{\"type\":\"scr\",\"content\":{\"id\":\"666\",\"type\":\"placeholder\",\"title\":\"testmodel\",\"description\":\"Thisiscratedfromtheunityapp\",\"keywords\":[\"model\",\"gltf\"],\"refs\":[{\"contentType\":\"model/gltf+json\",\"url\":\"https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Avocado/glTF-Binary/Avocado.glb\"}],\"geopose\":{\"longitude\":18.17439310285225,\"latitude\":59.16870133340334,\"ellipsoidHeight\":0,\"quaternion\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},\"size\":0,\"bbox\":\"\",\"definitions\":[{\"type\":\"unity\",\"value\":\"thisisatest\"}]}}]";
    async Task<string> CreateSpatialContentRecord(string access_token, string topic, string jsonBody)
    {
        Console.WriteLine("OrbitAPI.CreateSpatialContentRecord");
        output("Making API Call to Post content...");

        // Create POST data and convert it to a byte array.
        byte[] byteArray = Encoding.UTF8.GetBytes(jsonBody);

        // sends the request
        // TODO: this takes the first server URL, but there might be more selected!
        // TODO: Make sure to write the object only into one as intended
        string scdServerURL = OSCPDataHolder.Instance.contentUrls[0];
        HttpWebRequest postRequest = (HttpWebRequest)WebRequest.Create(scdServerURL + "/scrs/" + topic);
        postRequest.Method = "POST";
        postRequest.Headers.Add("Authorization", "Bearer " + access_token);
        postRequest.ContentType = "application/json";
        //putRequest.Accept = "application/json";
        postRequest.ContentLength = byteArray.Length;

        Stream stream = postRequest.GetRequestStream();
        await stream.WriteAsync(byteArray, 0, byteArray.Length);
        stream.Close();

        try
        {
            WebResponse putResponse = await postRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(putResponse.GetResponseStream()))
            {
                string responseText = await reader.ReadToEndAsync();
                return responseText;
            }
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    output("HTTP: " + response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        output(responseText);
                    }
                }
            }
        }
        return "";
    }

    async Task UpdateSpatialContentRecord(string access_token, string itemID, string topic, string jsonBody)
    {
        Console.WriteLine("OrbitAPI.UpdateSpatialContentRecord");
        //Debug.Log(jsonBody);
        output("Making API Call to update item with ID: " + itemID);

        byte[] byteArray = Encoding.UTF8.GetBytes(jsonBody);

        // sends the request
        // TODO: do not hardcode the URL!
        HttpWebRequest putRequest = (HttpWebRequest)WebRequest.Create("https://scd.orbit-lab.org/scrs/" + topic + "/" + itemID);
        putRequest.Method = "PUT";
        putRequest.Headers.Add("Authorization", "Bearer " + access_token);
        putRequest.ContentType = "application/json";
        // putRequest.Accept = "application/json";

        putRequest.ContentLength = byteArray.Length;
        Stream stream = putRequest.GetRequestStream();
        await stream.WriteAsync(byteArray, 0, byteArray.Length);
        stream.Close();

        try
        {
            WebResponse putResponse = await putRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(putResponse.GetResponseStream()))
            {
                string responseText = await reader.ReadToEndAsync();
                Console.WriteLine(responseText);
                //Debug.Log(responseText);
            }
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    output("HTTP: " + response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        output(responseText);
                        //inform the user about failure
                    }
                }
            }
        }
    }

    //TODO fix server. It always returns a 504 timed out but record is still deleted
    async Task<bool> DeleteSpatialContentRecord(string access_token, string itemID, string topic)
    {
        Console.WriteLine("OrbitAPI.DeleteSpatialContentRecord");
        output("Making API Call to delete item...");

        // TODO: do not hardcode the URL!
        HttpWebRequest deleteRequest = (HttpWebRequest)WebRequest.Create("https://scd.orbit-lab.org/scrs/" + topic + "/" + itemID);
        deleteRequest.Method = "DELETE";
        deleteRequest.Headers.Add("Authorization", "Bearer " + access_token);
        //TODO fix server. As of 2022-05-19. Server always returns a 504 timed out but record is still deleted
        //lowering timeout to 2000 seconds
        deleteRequest.Timeout = 2000;
    
        try
        {
            WebResponse deleteResponse = await deleteRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(deleteResponse.GetResponseStream()))
            {
                string responseText = await reader.ReadToEndAsync();
                return true;
            }
        }
        catch (WebException ex)
        {         
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    output("HTTP: " + response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseText = await reader.ReadToEndAsync();
                        output(responseText);
                    }
                }
            }
            return false;
        }       
    }

    #endregion

    // TODO: make this logging method available as a Utility for all other classes
    /// <summary>
    /// Appends the given string to the on-screen log, and the debug console.
    /// </summary>
    /// <param name="output">string to be appended</param>
    private void output(string output)
    {
        Console.WriteLine(output);
        Debug.Log(output);
    }

}
