using Newtonsoft.Json;
using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class OrbitAPI : MonoBehaviour
{
    [SerializeField] private bool isLoadingFromLocalStorage;

    [SerializeField] private OAuth2Authentication oAuth2Authentication;
    [SerializeField] private SCRManager spatialRecordManager;

    public static event Action<string> ServerResponseGet;
    public static event Action<bool, string> ServerResponse;

    List<string> contentServerUrls = new List<string>();

    private void Start()
    {
        contentServerUrls = OSCPDataHolder.Instance.ContentUrls;

#if UNITY_EDITOR
        if (contentServerUrls.Count == 0)
        {
            //hardcoded test server
            contentServerUrls.Add("https://scd.orbit-lab.org");
        }
#endif

        LoadItemsFromServer();
    }




    public void LoadItemsFromServer()
    {
        if (isLoadingFromLocalStorage)
        {

            spatialRecordManager.LoadFromJsonFile();
        }
        else
        {
            string accessToken = GetAccesToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                //TODO: Inform the user 
                return;
            }


            //TODO: Give the possibility for users to change topic
            if (spatialRecordManager.spatialServiceRecord == null || spatialRecordManager.spatialServiceRecord.Length == 0)
            {
                GetSpatialContentRecords(accessToken, "history", OSCPDataHolder.Instance.H3CurrentZone);
            }
        }

    }

    public void UpdateRecord(SCRItem sp)
    {

        string recordID = sp.id;

        string json = ConvertSCRtoString(sp);

        string accessToken = GetAccesToken();

        //TODO: Add ability to change topic. Currently hardcoded to history
        UpdateSpatialRecord(accessToken, recordID, "history", json);

    }

    public async Task<string> CreateRecord(SCRItem sp)
    {

        string json = ConvertSCRtoString(sp);

        string accessToken = GetAccesToken();

        string id = await CreateSpatialRecord(accessToken, "history", json);

        return id;

    }

    public async Task<bool> DeleteRecord(string itemID)
    {
    
        string accessToken = GetAccesToken();

        bool isDeleted = await DeleteSpatialRecord(accessToken, itemID, "history");

        return isDeleted;

    }

    public string ConvertSCRtoString(SCRItem sp)
    {

        string json = JsonConvert.SerializeObject(sp,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                });

        Debug.Log(json);

        return json;
    }


    private string GetAccesToken()
    {
        UserSessionCache userSessionCache = new UserSessionCache();
        SaveDataManager.LoadJsonData(userSessionCache);

        if (!string.IsNullOrEmpty(userSessionCache.getAccessToken()))
        {
            return userSessionCache.getAccessToken();
        }

        return null;
    }


    #region Test Methods during development


    async void GetSpatialContentRecords(string accessToken, string topic, string H3Index)
    {

        // https://scd.orbit-lab.org/scrs/history?h3Index=8808866927fffff
        output("Making API Call to read content...");
        //TODO: Ability to query multiple content servers not just the first in the list
        //sends the request
        HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(contentServerUrls[0] + "/scrs/" + topic + "?h3Index=" + H3Index);
        getRequest.Method = "GET";
        getRequest.Headers.Add(string.Format("Authorization: Bearer " + accessToken));
        getRequest.ContentType = "application/x-www-form-urlencoded";

        // gets the response
        WebResponse apiResponse = await getRequest.GetResponseAsync();
        using (StreamReader apiInforResponseReader = new StreamReader(apiResponse.GetResponseStream()))
        {
            // reads response body
            string apiInfoResponseText = await apiInforResponseReader.ReadToEndAsync();
            output("Response from scd-orbit read: " + apiInfoResponseText);

            ServerResponseGet?.Invoke(apiInfoResponseText);
        }
    }

    private async Task<string> CreateSpatialRecord(string access_token, string topic, string jsonBody)
    {
        output("Making API Call to Post content...");

        // Create POST data and convert it to a byte array.
        // string postData = "[{\"type\":\"scr\",\"content\":{\"id\":\"666\",\"type\":\"placeholder\",\"title\":\"testmodel\",\"description\":\"Thisiscratedfromtheunityapp\",\"keywords\":[\"model\",\"gltf\"],\"refs\":[{\"contentType\":\"model/gltf+json\",\"url\":\"https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Avocado/glTF-Binary/Avocado.glb\"}],\"geopose\":{\"longitude\":18.17439310285225,\"latitude\":59.16870133340334,\"ellipsoidHeight\":0,\"quaternion\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},\"size\":0,\"bbox\":\"\",\"definitions\":[{\"type\":\"unity\",\"value\":\"thisisatest\"}]}}]";



        byte[] byteArray = Encoding.UTF8.GetBytes(jsonBody);

        // sends the request
        HttpWebRequest postRequest = (HttpWebRequest)WebRequest.Create(contentServerUrls[0] + "/scrs/" + topic);
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
            // gets the response
            WebResponse putResponse = await postRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(putResponse.GetResponseStream()))
            {
                // reads response body
                string responseText = await reader.ReadToEndAsync();

                Debug.Log(responseText);

                return responseText;

                //Debug.Log(access_token);
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

    async void UpdateSpatialRecord(string access_token, string itemID, string topic, string jsonBody)
    {
        Debug.Log(jsonBody);
        output("Making API Call to update item with ID: " + itemID);


        byte[] byteArray = Encoding.UTF8.GetBytes(jsonBody);

        // sends the request
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
            // gets the response
            WebResponse putResponse = await putRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(putResponse.GetResponseStream()))
            {
                // reads response body
                string responseText = await reader.ReadToEndAsync();

                //inform the user about success

                Console.WriteLine(responseText);
                Debug.Log(responseText);

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
    async Task<bool> DeleteSpatialRecord(string access_token, string itemID, string topic)
    {
        output("Making API Call to delete item...");

        // sends the request
        HttpWebRequest deleteRequest = (HttpWebRequest)WebRequest.Create("https://scd.orbit-lab.org/scrs/" + topic + "/" + itemID);
        deleteRequest.Method = "DELETE";
        deleteRequest.Headers.Add("Authorization", "Bearer " + access_token);
        //TODO fix server. As of 2022-19-05. Server always returns a 504 timed out but record is still deleted
        //lowering timeout to 2000 seconds
        deleteRequest.Timeout = 2000;
    
        try
        {
            // gets the response
            WebResponse deleteResponse = await deleteRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(deleteResponse.GetResponseStream()))
            {
                // reads response body
                string responseText = await reader.ReadToEndAsync();
                Console.WriteLine(responseText);

                Debug.Log(responseText);
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
                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        output(responseText);
                        //TODO: inform the user about the failure
                    }
                }
            }
            return false;
        }       
    }
    #endregion

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
