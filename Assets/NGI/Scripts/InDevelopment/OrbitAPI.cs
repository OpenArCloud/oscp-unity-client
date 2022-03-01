using Newtonsoft.Json;
using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

public class OrbitAPI : MonoBehaviour
{
    [SerializeField] private bool isLoadingFromLocalStorage;

    [SerializeField] private OAuth2Authentication oAuth2Authentication;
    [SerializeField] private SpatialRecordManager spatialRecordManager;

    public static event Action<string> ServerResponseGet;
    public static event Action<bool, string> ServerResponse;

    

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
            if (spatialRecordManager.spatialServiceRecord == null)
            {
                GetSpatialContentRecords(accessToken, "history");
            }
        }

    }

    public void UpdateItemOnServer(SpatialServiceRecord sp)
    {
        string recordID = sp.id;

        string json = JsonConvert.SerializeObject(sp,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                });


        Debug.Log(json);

        string accessToken = GetAccesToken();

        UpdateSpatialRecord(accessToken, recordID, "history", json);

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


    async void GetSpatialContentRecords(string accessToken, string topic)
    {

        output("Making API Call to read api content...");

        //sends the request
        HttpWebRequest apiInforequest = (HttpWebRequest)WebRequest.Create("https://scd.orbit-lab.org/tenant/scrs/" + topic);
        apiInforequest.Method = "GET";
        apiInforequest.Headers.Add(string.Format("Authorization: Bearer " + accessToken));
        apiInforequest.ContentType = "application/x-www-form-urlencoded";

        // gets the response
        WebResponse apiResponse = await apiInforequest.GetResponseAsync();
        using (StreamReader apiInforResponseReader = new StreamReader(apiResponse.GetResponseStream()))
        {
            // reads response body
            string apiInfoResponseText = await apiInforResponseReader.ReadToEndAsync();
            output("Response from scd-orbit read: " + apiInfoResponseText);

            ServerResponseGet?.Invoke(apiInfoResponseText);
        }
    }

    async void CreateSpatialRecord(string access_token, string jsonBody)
    {
        output("Making API Call to read api content...");

        // Create POST data and convert it to a byte array.
        string postData = "[{\"type\":\"scr\",\"content\":{\"id\":\"666\",\"type\":\"placeholder\",\"title\":\"testmodel\",\"description\":\"Thisiscratedfromtheunityapp\",\"keywords\":[\"model\",\"gltf\"],\"refs\":[{\"contentType\":\"model/gltf+json\",\"url\":\"https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Avocado/glTF-Binary/Avocado.glb\"}],\"geopose\":{\"longitude\":18.17439310285225,\"latitude\":59.16870133340334,\"ellipsoidHeight\":0,\"quaternion\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},\"size\":0,\"bbox\":\"\",\"definitions\":[{\"type\":\"unity\",\"value\":\"thisisatest\"}]}}]";
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);

        // sends the request
        HttpWebRequest putRequest = (HttpWebRequest)WebRequest.Create("https://scd.orbit-lab.org/scrs/history");
        putRequest.Method = "POST";
        putRequest.Headers.Add("Authorization", "Bearer " + access_token);
        putRequest.ContentType = "application/json";
        putRequest.Accept = "application/json";

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
                Console.WriteLine(responseText);
                //Debug.Log(responseText);
                //converts to dictionary
                //Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                Debug.Log(responseText);
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

    async void DeleteSpatialRecord(string access_token, string itemID, string jsonBody)
    {
        output("Making API Call to delete item...");

        // Create POST data and convert it to a byte array.
        string postData = "[{\"type\":\"scr\",\"content\":{\"id\":\"666\",\"type\":\"placeholder\",\"title\":\"testmodel\",\"description\":\"Thisiscratedfromtheunityapp\",\"keywords\":[\"model\",\"gltf\"],\"refs\":[{\"contentType\":\"model/gltf+json\",\"url\":\"https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/Avocado/glTF-Binary/Avocado.glb\"}],\"geopose\":{\"longitude\":18.17439310285225,\"latitude\":59.16870133340334,\"ellipsoidHeight\":0,\"quaternion\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}},\"size\":0,\"bbox\":\"\",\"definitions\":[{\"type\":\"unity\",\"value\":\"thisisatest\"}]}}]";
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);


        // sends the request
        HttpWebRequest putRequest = (HttpWebRequest)WebRequest.Create("https://scd.orbit-lab.org/scrs/history/" + itemID);
        putRequest.Method = "DELETE";
        putRequest.Headers.Add("Authorization", "Bearer " + access_token);

        try
        {
            // gets the response
            WebResponse putResponse = await putRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(putResponse.GetResponseStream()))
            {
                // reads response body
                string responseText = await reader.ReadToEndAsync();
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
                    }
                }
            }
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
