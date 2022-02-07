using System;
using UnityEngine;
using UnityEngine.UI;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager Instance { get; private set; }
    public string deeplinkURL;
    public string accessToken = "";
    public Text Label;

    public static event Action<string> HandleDeepLink;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void onDeepLinkActivated(string url)
    {
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;  
        Debug.Log("From deeplink script: "+url);    

        Debug.Log("Invoking HandleDeepLink action...............................");
        HandleDeepLink?.Invoke(url);
    }
}