using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(H3Manager))]
public class SSRManager : MonoBehaviour
{
    [SerializeField] string ssdServerURL = "https://ssd.orbit-lab.org/";

    //[SerializeField] string h3Index = "ssrs?h3Index=882a13d23bfffff";
    [SerializeField] string countryCode = "us";

    [SerializeField] GameObject listItemPrefab;

    [SerializeField] RectTransform rectTransformSpawnSSR;
    [SerializeField] RectTransform rectTransformSpawnSCR;

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

    public IEnumerator GetServersInCurrentH3()
    {
#if !UNITY_EDITOR
        if (Input.location.status == LocationServiceStatus.Stopped || Input.location.status == LocationServiceStatus.Failed)
        {
            FindObjectOfType<H3Manager>().StartLocationService();
        }

        yield return new WaitUntil(() => Input.location.status == LocationServiceStatus.Running);

        GetSpatialContentRecords(countryCode, h3Manager.GetLastH3Index().ToString());
#else

        yield return null;
        GetSpatialContentRecords(countryCode, h3Manager.GetLastH3Index().ToString());
#endif
    }

    public void CreateListItems(JSONNode response)
    {
        List<JSONNode> ssrList = new List<JSONNode>();
        List<JSONNode> scrList = new List<JSONNode>();

        int length = response[0]["services"].Count;

        for (int i = 0; i < length; i++)
        {

            if (string.Equals(response[0]["services"][i]["type"], "geopose"))
            {
                ssrList.Add(response[0]["services"][i]);

                // Debug.Log(response[0]["services"][i]);
            }

            if (string.Equals(response[0]["services"][i]["type"], "content-discovery"))
            {
                scrList.Add(response[0]["services"][i]);

                // Debug.Log(response[0]["services"][i]);
            }

        }

        if (ssrList.Count > 0)
        {
            for (int i = 0; i < ssrList.Count; i++)
            {
                var tempObj = Instantiate(listItemPrefab, rectTransformSpawnSSR);
                tempObj.GetComponent<SSRItem>().SetValues(ssrList[i]);
            }
        }

        if (scrList.Count > 0)
        {
            for (int i = 0; i < scrList.Count; i++)
            {
                var tempObj = Instantiate(listItemPrefab, rectTransformSpawnSCR);
                tempObj.GetComponent<SSRItem>().SetValues(scrList[i]);
            }
        }
    }

    public string GetSelectedSSRItems(Transform parent)
    {
        //TODO: Only able to choose one
        SSRItem[] ssr = parent.GetComponentsInChildren<SSRItem>();

        foreach (var item in ssr)
        {
            if (item.IsSelected)
            {
                return ssr[0].GetURL();
            }
        }

        return String.Empty;
    }

    public List<string> GetSelectedSCDItems(Transform parent)
    {
        //TODO: Ability to selecte multiple SCD items
        SSRItem[] scd = parent.GetComponentsInChildren<SSRItem>();

        List<string> scdURLs = new List<string>();

        if (scd.Length > 0)
        {
            for (int i = 0; i < scd.Length; i++)
            {
                if (scd[i].IsSelected)
                {
                    scdURLs.Add(scd[i].GetURL());
                }
            }

            return scdURLs;
        }

        return null;
    }

    private void HandleAuthenticate(bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            StartCoroutine(GetServersInCurrentH3());
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


    public void LoadSceneAsync(string sceneName)
    {
        // OSCPDataHolder.Instance.ClearData();

        OSCPDataHolder.Instance.ContentUrls = GetSelectedSCDItems(rectTransformSpawnSCR);
        OSCPDataHolder.Instance.GeoPoseServieURL = GetSelectedSSRItems(rectTransformSpawnSSR);

        //TODO: Infor the user that their selection has some errors
        if (OSCPDataHolder.Instance.CheckSelectedServices())
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
        else
        {
            Debug.Log("Needed values missing in OSCPDataHolder, aborting scene change");
        }

    }

}
