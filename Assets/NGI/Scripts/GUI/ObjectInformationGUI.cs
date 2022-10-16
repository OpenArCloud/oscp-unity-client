using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInformationGUI : MonoBehaviour
{
    [Header("Item values")]
    public Text ID;
    public Text Type;
    public Text TimeStamp;
    public Text Tenant;

    public InputField ContentID;
    public InputField ContentType;
    public InputField ContentTitle;
    public InputField ContentDescription;

    public InputField Position;
    public InputField Orientation;
    public InputField Longitude;
    public InputField Latitude;
    public InputField EllipsoidHeight;

    public InputField Keywords;
    public InputField Refs;
    public InputField Definitions;


    [Header("Variables for creating prefab GUI items")]
    [SerializeField] GameObject dictionaryItemPrefab;
    [SerializeField] GameObject keywordItemPrefab;
    [SerializeReference] Transform spawnPositionRefs;
    [SerializeReference] Transform spawnPositionDefinitions;
    [SerializeReference] Transform spawnPositionKeywords;


    public void SetValues(SCRItem record)
    {
        ID.text = record.id;
        Type.text = record.type;
        TimeStamp.text = record.timestamp.ToString();
        Tenant.text = record.tenant;

        ContentID.text = record.content.id;
        ContentType.text = record.content.type;
        ContentTitle.text = record.content.title;
        ContentDescription.text = record.content.description;

        Position.text = record.Position.ToString();
        Orientation.text = record.Orientation.ToString();

        //New geopose schema
        Latitude.text = record.content.geopose.position.lat.ToString();
        Longitude.text = record.content.geopose.position.lon.ToString();
        EllipsoidHeight.text = record.content.geopose.position.h.ToString();
        
        CreateDictItems(record.content.refs, spawnPositionRefs);
        CreateDictItems(record.content.definitions, spawnPositionDefinitions);
        CreateKeywordItems(record.content.keywords, spawnPositionKeywords);
    }


    private SCRItem GetSpatialObject()
    {
        SCRItem record = new SCRItem();
        record.content = new Content();
        record.content.geopose = new GeoPosition(); // TODO: rename GeoPosition to GeoPose
        record.content.geopose.position = new Position();
        record.content.geopose.quaternion = new Dictionary<string, float>();

        record.id = ID.text;
        record.type = Type.text;
        record.timestamp = float.Parse(TimeStamp.text);
        record.tenant = Tenant.text;

        record.content.id = ContentID.text;
        record.content.title = ContentTitle.text;
        record.content.type = ContentType.text;
        record.content.description = ContentDescription.text;

        // TODO: what are these non-standards fields for?
        record.Position = StringToVector.StringToVector3(Position.text);
        record.Orientation = StringToVector.StringToVector4(Orientation.text);
     
        record.content.geopose.position.lat = double.Parse(Longitude.text);
        record.content.geopose.position.lon = double.Parse(Latitude.text);
        record.content.geopose.position.h = float.Parse(EllipsoidHeight.text);
        // TODO: can we avoid conversion to and from string?
        Vector4 vector4 = new Vector4();
        vector4 = StringToVector.StringToVector4(Orientation.text);
        record.content.geopose.quaternion.Add("x", vector4.x);
        record.content.geopose.quaternion.Add("y", vector4.y);
        record.content.geopose.quaternion.Add("z", vector4.z);
        record.content.geopose.quaternion.Add("w", vector4.w);

        record.content.keywords = GetKeywords(spawnPositionKeywords);
        record.content.refs = GetListObjects(spawnPositionRefs);
        record.content.definitions = GetListObjects(spawnPositionDefinitions);

        //Just adding values for testing remove and fix working solution
        record.content.bbox = "";
        record.content.size = 1f;
        record.content.placeKey = "";

        return record;
    }

    public void UpdateSpatialObject()
    {
        SCRManager spatialRecordManager = FindObjectOfType<SCRManager>();
        spatialRecordManager.UpdateSpatialObject(GetSpatialObject());
    }

    public void CloseInformationPanel()
    {
        ClearTextValues();
        gameObject.SetActive(false);
    }

    private void CreateDictItems(IList<Dictionary<string, string>> listObjects, Transform spawnPoint)
    {
        foreach (var item in listObjects)
        {
            GameObject temp = Instantiate(dictionaryItemPrefab, spawnPoint);

            temp.GetComponent<DictionaryItemGUI>().SetValues(item);
        }
    }

    private void CreateKeywordItems(IList<string> list, Transform spawnPoint)
    {
        foreach (var item in list)
        {
            GameObject temp = Instantiate(keywordItemPrefab, spawnPoint);
            temp.GetComponent<KeywordItemGUI>().SetValues(item);
        }
    }

    private IList<Dictionary<string, string>> GetListObjects(Transform parentObject)
    {

        DictionaryItemGUI[] dictionaryItemGUIs = parentObject.GetComponentsInChildren<DictionaryItemGUI>();
        IList<Dictionary<string, string>> keyValuePairs = new List<Dictionary<string, string>>();
        foreach (var item in dictionaryItemGUIs)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] temp = item.GetKeyValuePairs();
            dict.Add(temp[0], temp[1]);
            dict.Add(temp[2], temp[3]);
            keyValuePairs.Add(dict);
        }
        return keyValuePairs;
    }

    private IList<string> GetKeywords(Transform parentObject)
    {
        KeywordItemGUI[] keywordItems = parentObject.GetComponentsInChildren<KeywordItemGUI>();
        IList<string> stringList = new List<string>();
        foreach (var item in keywordItems)
        {
            stringList.Add(item.GetFieldInputText());
        }
        return stringList;
    }

    private void ClearTextValues()
    {
        ID.text = "";
        Type.text = "";
        TimeStamp.text = "";
        Tenant.text = "";

        ContentID.text = "";
        ContentType.text = "";
        ContentTitle.text = "";
        ContentDescription.text = "";

        Position.text = "";
        Orientation.text = "";

        Longitude.text = "";
        Latitude.text = "";
        EllipsoidHeight.text = "";

        DestroyChildren(spawnPositionRefs);
        DestroyChildren(spawnPositionDefinitions);
        DestroyChildren(spawnPositionKeywords);
    }

    private void DestroyChildren(Transform parentTransform)
    {
        if (parentTransform == null)
            return;

        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }
    }

}
