using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MockData : MonoBehaviour
{
    //Creates a local file from this so the app is able to be tested without logging in and connecting to servers
    [SerializeField] private TextAsset mockData;

    private void Start()
    {
        string result;

        try
        {

            var fullPath = Path.Combine(Application.persistentDataPath, "mockServerResponse.json");

            if (File.Exists(fullPath))
            {
                return;
            }

            result = mockData.text;

            File.WriteAllText(fullPath, result);

        }
        catch (Exception e)
        {
            Debug.LogWarning($"Exception {e}.");
            result = "";

        }
    }


}
