using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NGI.Api; // TODO: rename to Oscp.Api

// TODO: This class should be merged with OrbitAPI 
// because Orbit is the name of just one spatial content discovery server
// TODO: MockResponseLoad seems to be redundant with this too.
public class SCRManager : MonoBehaviour
{
    [SerializeField] private OrbitAPI orbitAPI;

    public JSONNode jsonResponseNode = null;
    public SCRItem[] spatialContentRecords = new SCRItem[0];

    public static event Action<SCRItem[]> contentsUpdatedAction = null;


    private void OnEnable()
    {
        OrbitAPI.ServerResponseGet += HandleServerResponse;
    }

    private void OnDisable()
    {
        OrbitAPI.ServerResponseGet -= HandleServerResponse;
    }

    private void Start()
    {
        Console.WriteLine("SCRManager.Start");
    }

    private void HandleServerResponse(string jsonResponse)
    {
        jsonResponseNode = JSON.Parse(jsonResponse);
        StartCoroutine(ParseSpatialContentRecords(jsonResponseNode));
    }

    public void LoadFromJsonFile()
    {
        MockResponse mockResponse = new MockResponse();
        SaveDataManager.LoadJsonData(mockResponse);

        if (!string.IsNullOrEmpty(mockResponse.mockServerResponse))
        {
            jsonResponseNode = JSON.Parse(mockResponse.mockServerResponse);
        }

        StartCoroutine(ParseSpatialContentRecords(jsonResponseNode));
    }

    public void GetSpatialRecords()
    {
        orbitAPI.LoadItemsFromServer();
    }

    // TODO: this is just parsing, does not need to run on a coroutine
    private IEnumerator ParseSpatialContentRecords(JSONNode jsonNode)
    {
        //Debug.Log(jsonNode);

        int objectAmount = jsonNode.Count;
        spatialContentRecords = new SCRItem[objectAmount];
        for (int i = 0; i < objectAmount; i++)
        {
            SCRItem sp = new SCRItem();

            sp.id = jsonNode[i]["id"];
            sp.type = jsonNode[i]["type"];
            sp.tenant = jsonNode[i]["tenant"];
            sp.timestamp = jsonNode[i]["timestamp"];

            sp.content = new Content();
            sp.content.geopose = new GeoPosition(); // TODO: rename to GeoPose
            sp.content.geopose.position = new Position();
            sp.content.geopose.quaternion = new Dictionary<string, float>();
            sp.content.refs = new List<Dictionary<string, string>>();
            sp.content.definitions = new List<Dictionary<string, string>>();
            sp.content.keywords = new List<string>();

            sp.content.id = jsonNode[i]["content"]["id"];
            sp.content.type = jsonNode[i]["content"]["type"];
            sp.content.title = jsonNode[i]["content"]["title"];
            sp.content.description = jsonNode[i]["content"]["description"];

            //TODO: Dont know what these two attributes handle
            sp.content.bbox = "";
            sp.content.size = 1f;

            //New geopose schema
            sp.content.geopose.position.lat = jsonNode[i]["content"]["geopose"]["position"]["lat"].AsDouble;
            sp.content.geopose.position.lon = jsonNode[i]["content"]["geopose"]["position"]["lon"].AsDouble;
            sp.content.geopose.position.h = jsonNode[i]["content"]["geopose"]["position"]["h"].AsFloat;
            // TODO: Mock orientation Needs update when Visual Positioning System is working
            float ox = jsonNode[i]["content"]["geopose"]["quaternion"]["x"].AsFloat;
            float oy = jsonNode[i]["content"]["geopose"]["quaternion"]["y"].AsFloat;
            float oz = jsonNode[i]["content"]["geopose"]["quaternion"]["z"].AsFloat;
            float ow = jsonNode[i]["content"]["geopose"]["quaternion"]["w"].AsFloat;
            sp.content.geopose.quaternion.Add("x", ox);
            sp.content.geopose.quaternion.Add("y", oy);
            sp.content.geopose.quaternion.Add("z", oz);
            sp.content.geopose.quaternion.Add("w", ow);

            int length = jsonNode[i]["content"]["keywords"].Count;
            for (int y = 0; y < length; y++)
            {
                sp.content.keywords.Add(jsonNode[i]["content"]["keywords"][y]);
            }

            length = jsonNode[i]["content"]["refs"].Count;
            for (int y = 0; y < length; y++)
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                foreach (var item in jsonNode[i]["content"]["refs"][y])
                {
                    keyValuePairs.Add(item.Key, item.Value);
                }
                sp.content.refs.Add(keyValuePairs);
            }

            length = jsonNode[i]["content"]["definitions"].Count;
            for (int y = 0; y < length; y++)
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                foreach (var item in jsonNode[i]["content"]["definitions"][y])
                {
                    keyValuePairs.Add(item.Key, item.Value);
                }
                sp.content.definitions.Add(keyValuePairs);
            }
            spatialContentRecords[i] = sp;
        }

        contentsUpdatedAction?.Invoke(spatialContentRecords);

        yield return null;
    }

    public void UpdateSpatialObject(SCRItem sp)
    {
        int length = spatialContentRecords.Length;
        string objID = sp.id;
        for (int i = 0; i < length; i++)
        {
            if (string.Equals(spatialContentRecords[i].id, objID))
            {
                spatialContentRecords[i] = sp;
                Debug.Log(string.Format("Updated ObjectID: {0}", spatialContentRecords[i].id));
                orbitAPI.UpdateRecord(sp);
                return;
            }
        }
        Debug.Log(string.Format("Not found: ObjectID: {0} ", objID));
    }

    public void SaveContentsToLocalStorage()
    {
        string json = JsonConvert.SerializeObject(spatialContentRecords,
                new JsonSerializerSettings(){
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });
        //Debug.Log(json);
        //TODO: where is the actual saving to file?
    }
}
