using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SSDManager;

public class SSDItem : MonoBehaviour
{

    // {"id":"ac_geopose_brandbergen","type":"geopose","url":"https://developer.augmented.city/scrs/geopose","title":"AC GeoPose Brandbergen","description":"AC GeoPose Brandbergen"}

    [SerializeField] private Text id;
    [SerializeField] private Text type;

    [SerializeField] private Text URL;
    [SerializeField] private Text title;
    [SerializeField] private Text description;


    public void SetValues(JSONNode item)
    {
        Debug.Log(item);
        id.text = item["id"];
        type.text = item["type"];
        URL.text = item["url"];
        title.text = item["title"];
        description.text = item["description"];

    }


}
