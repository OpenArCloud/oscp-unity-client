using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectListItem : MonoBehaviour
{
    [SerializeField] Text description;

    [SerializeField] Text id;

    [SerializeField] Text type;

    private SpatialServiceRecord spatialObjRef;


    public static event Action<SpatialServiceRecord> listItemClicked;


    public void ButtonClicked()
    {
        listItemClicked?.Invoke(spatialObjRef);
    }

    public void SetValues(SpatialServiceRecord spo)
    {
        description.text = spo.content.description;
        id.text = spo.id;
        type.text = spo.type;

        spatialObjRef = spo;
    }

    public SpatialServiceRecord GetSpatialObject()
    {
        return spatialObjRef;
    }


  

}
