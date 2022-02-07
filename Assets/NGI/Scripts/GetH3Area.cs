using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NGI.Api;
using Newtonsoft.Json;


public class GetH3Area : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(ApiConnectionTest("https://ssd.orbit-lab.org/us/ssrs?h3Index=882a13d23bfffff"));

        StartCoroutine(ApiConnectionTest("https://scd.orbit-lab.org/scrs/history?h3Index=882a13d239fffff"));

    }

    /*
    IEnumerator PrepareC(float langitude, float latitude, Action<bool, string> getServerAnswer)
    {
        // Example: http://developer.augmented.city:5000/api/localizer/prepare?lat=59.907458f&lon=30.298400f 
        Debug.Log(apiURL + "/api/localizer/prepare?lat=" + latitude + "f&lon=" + langitude + "f");
        var w = UnityWebRequest.Get(apiURL + "/api/localizer/prepare?lat=" + latitude + "f&lon=" + langitude + "f");
        w.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
        w.SetRequestHeader("Accept", "application/vnd.myplace.v2+json");
        yield return w.SendWebRequest();
        if (w.isNetworkError || w.isHttpError)
        {
            Debug.Log(w.error);
            localizationStatus = LocalizationStatus.ServerError;
            getServerAnswer(false, w.downloadHandler.text);
        }
        else
        {
            Debug.Log("prepared API");
            Debug.Log(w.downloadHandler.text);
            getServerAnswer(true, w.downloadHandler.text);
        }
    }*/

    
    IEnumerator ApiConnectionTest(string apiURL)
    {
        
        Debug.Log(apiURL);
        UnityWebRequest webRequest = UnityWebRequest.Get(apiURL);
        
        yield return webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(webRequest.error);
            
        }
        else
        {
            
            Debug.Log(webRequest.downloadHandler.text);
            Debug.Log(webRequest.result.ToString());

            HandleResponse(webRequest.downloadHandler.text);
        }
    }
    
    void HandleResponse(string jsonResponse)
    {

        //SpatialServiceRecord record = JsonConvert.DeserializeObject<SpatialServiceRecord>(jsonResponse);



        //Debug.Log(record.type);
    }

}


