using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(H3Manager))]
public class SSRManager : MonoBehaviour
{
    [SerializeField] string ssdServerURL = "https://ssd.orbit-lab.org/";

    [SerializeField] string h3Index = "ssrs?h3Index=882a13d23bfffff";
    [SerializeField] string countryCode = "us";

    [SerializeField] GameObject listItemPrefab;

    [SerializeField] RectTransform rectTransformSpawn;

    [SerializeField] H3Manager h3Manager;


    public static event Action<string> ServerResponseGet;

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
        if (h3Manager == null)
        {
            h3Manager = GetComponent<H3Manager>();
        }
    }

    private void HandleResponse(string response)
    {
        CreateListItems(JSON.Parse(response));
    }

    public void GetServicesInH3()
    {
        GetSpatialContentRecords(countryCode, h3Manager.GetLastH3Index().ToString());
    }

    public void CreateListItems(JSONNode response)
    {
        List<JSONNode> listJSONNode = new List<JSONNode>();

        int length = response[0]["services"].Count;

        for (int i = 0; i < length; i++)
        {

            if (string.Equals(response[0]["services"][i]["type"], "geopose"))
            {
                listJSONNode.Add(response[0]["services"][i]);

                Debug.Log(response[0]["services"][i]);
            }

        }

        if (listJSONNode.Count > 0)
        {
            for (int i = 0; i < listJSONNode.Count; i++)
            {
                var tempObj = Instantiate(listItemPrefab, rectTransformSpawn);
                tempObj.GetComponent<SSRItem>().SetValues(listJSONNode[i]);
            }          
        }
    }

    private void HandleAuthenticate(bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            GetServicesInH3();
        }
    }

    async void GetSpatialContentRecords(string countryCode, string h3Index)
    {

        //output("Making Call to read SSD services...");

        //sends the request
        HttpWebRequest apiInforequest = (HttpWebRequest)WebRequest.Create(ssdServerURL + countryCode + "/ssrs?h3Index=" + h3Index);
        apiInforequest.Method = "GET";
        // apiInforequest.Headers.Add(string.Format("Authorization: Bearer " + accessToken));
        apiInforequest.ContentType = "application/x-www-form-urlencoded";

        // gets the response
        WebResponse apiResponse = await apiInforequest.GetResponseAsync();
        using (StreamReader apiInforResponseReader = new StreamReader(apiResponse.GetResponseStream()))
        {
            // reads response body
            string apiInfoResponseText = await apiInforResponseReader.ReadToEndAsync();
            Debug.Log("Response from scd-orbit read: " + apiInfoResponseText);

            ServerResponseGet?.Invoke(apiInfoResponseText);
        }
    }
}
