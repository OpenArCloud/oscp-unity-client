using Newtonsoft.Json;
using NGI.Api;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCRManager : MonoBehaviour
{
    public JSONNode jsonResponseNode;
    public SCRItem[] spatialServiceRecord;

    public Vector3 mockCameraPos;
    public Vector4 mockCameraOri;

    [SerializeField] private OrbitAPI orbitAPI;

    public static event Action<SCRItem[]> UpdatedSpatialServiceRecord;


    private void OnEnable()
    {
        OrbitAPI.ServerResponseGet += HandleServerResponse;
    }

    private void OnDisable()
    {
        OrbitAPI.ServerResponseGet -= HandleServerResponse;
    }

    private void HandleServerResponse(string jsonResponse)
    {

        jsonResponseNode = JSON.Parse(jsonResponse);

        StartCoroutine(CreateSpatialRecordList(jsonResponseNode));

    }

    public void LoadFromJsonFile()
    {

        MockResponse mockResponse = new MockResponse();
        SaveDataManager.LoadJsonData(mockResponse);

        if (!string.IsNullOrEmpty(mockResponse.mockServerResponse))
        {
            jsonResponseNode = JSON.Parse(mockResponse.mockServerResponse);
        }

        StartCoroutine(CreateSpatialRecordList(jsonResponseNode));
    }

    public void GetSpatialRecords()
    {
        orbitAPI.LoadItemsFromServer();
    }

    private IEnumerator CreateSpatialRecordList(JSONNode jsonNode)
    {

        int objectAmount = jsonResponseNode.Count;

        spatialServiceRecord = new SCRItem[objectAmount];

       // Debug.Log(jsonResponseNode);

        float px, py, pz, ox, oy, oz, ow;

        for (int i = 0; i < objectAmount; i++)
        {

            SCRItem sp = new SCRItem();

            sp.content = new Content();
            sp.content.geopose = new GeoPosition();
            sp.content.geopose.quaternion = new Dictionary<string, float>();
            sp.content.refs = new List<Dictionary<string, string>>();
            sp.content.definitions = new List<Dictionary<string, string>>();
            sp.content.keywords = new List<string>();

            sp.content.id = jsonResponseNode[i]["content"]["id"];
            sp.content.type = jsonResponseNode[i]["content"]["type"];
            sp.content.title = jsonResponseNode[i]["content"]["title"];
            sp.content.description = jsonResponseNode[i]["content"]["description"];

            sp.content.geopose.latitude = jsonResponseNode[i]["content"]["geopose"]["latitude"].AsDouble;
            sp.content.geopose.longitude = jsonResponseNode[i]["content"]["geopose"]["longitude"].AsDouble;
            sp.content.geopose.ellipsoidHeight = jsonResponseNode[i]["content"]["geopose"]["ellipsoidHeight"].AsFloat;

            sp.id = jsonResponseNode[i]["id"];
            sp.type = jsonResponseNode[i]["type"];
            sp.tenant = jsonResponseNode[i]["tenant"];
            sp.timestamp = jsonResponseNode[i]["timestamp"];

            //Dont know what these two attributes handle
            sp.content.bbox = "";
            sp.content.size = 1f;


            //Mock position Needs update when Visual Positioning System is working
            ox = jsonResponseNode[i]["content"]["geopose"]["quaternion"]["x"].AsFloat;
            oy = jsonResponseNode[i]["content"]["geopose"]["quaternion"]["y"].AsFloat;
            oz = jsonResponseNode[i]["content"]["geopose"]["quaternion"]["z"].AsFloat;
            ow = 1.0f;//jsonResponseNode[i]["content"]["geopose"]["quaternion"]["w"].AsFloat;

            sp.content.geopose.quaternion.Add("x", ox);
            sp.content.geopose.quaternion.Add("y", oy);
            sp.content.geopose.quaternion.Add("z", oz);
            sp.content.geopose.quaternion.Add("w", ow);

            int length = jsonResponseNode[i]["content"]["keywords"].Count;
            for (int y = 0; y < length; y++)
            {
                sp.content.keywords.Add(jsonResponseNode[i]["content"]["keywords"][y]);
            }

            length = jsonResponseNode[i]["content"]["refs"].Count;
            for (int y = 0; y < length; y++)
            {

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                foreach (var item in jsonResponseNode[i]["content"]["refs"][y])
                {
                    keyValuePairs.Add(item.Key, item.Value);
                }

                sp.content.refs.Add(keyValuePairs);
            }

            length = jsonResponseNode[i]["content"]["definitions"].Count;
            for (int y = 0; y < length; y++)
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                foreach (var item in jsonResponseNode[i]["content"]["definitions"][y])
                {
                    keyValuePairs.Add(item.Key, item.Value);
                }

                sp.content.definitions.Add(keyValuePairs);
            }

            spatialServiceRecord[i] = sp;
        }

        UpdatedSpatialServiceRecord?.Invoke(spatialServiceRecord);
        yield return null;
    }

    public void UpdateSpatialObject(SCRItem sp)
    {

        int length = spatialServiceRecord.Length;
        string objID = sp.id;
        for (int i = 0; i < length; i++)
        {
            if (string.Equals(spatialServiceRecord[i].id, objID))
            {
                spatialServiceRecord[i] = sp;
                Debug.Log(string.Format("Updated ObjectID: {0}", spatialServiceRecord[i].id));

                orbitAPI.UpdateItemOnServer(sp);

                return;
            }
        }

        Debug.Log(string.Format("Not found: ObjectID: {0} ", objID));

    }


    public void SaveSpatialRecordListToLocalStorage()
    {

        string json = JsonConvert.SerializeObject(spatialServiceRecord,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

                });

        Debug.Log(json);

    }
}
