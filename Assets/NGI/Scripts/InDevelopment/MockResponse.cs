using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockResponse : ISaveable
{

    public string mockServerResponse;
    public MockResponse() { }


    public string FileNameToUseForData()
    {
        return "mockServerResponse.json";
    }

    public void LoadFromJson(string jsonToLoadFrom)
    {
        mockServerResponse = jsonToLoadFrom;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(mockServerResponse);
    }
}
