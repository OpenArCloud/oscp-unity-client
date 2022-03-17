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

    private SCRItem spatialObjRef;


    public static event Action<SCRItem> listItemClicked;


    public void ButtonClicked()
    {
        listItemClicked?.Invoke(spatialObjRef);
    }

    public void SetValues(SCRItem spo)
    {
        description.text = spo.content.description;
        id.text = spo.id;
        type.text = spo.type;

        spatialObjRef = spo;
    }

    public SCRItem GetSpatialObject()
    {
        return spatialObjRef;
    }


  

}
