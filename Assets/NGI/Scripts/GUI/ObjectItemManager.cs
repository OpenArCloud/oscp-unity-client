using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectItemManager : MonoBehaviour
{

    [SerializeField] Transform contentHolder;

    [SerializeField] GameObject prefabToSpawn;

    [SerializeField] SpatialRecordManager spatialRecordManager;

    [SerializeField] ObjectInformationGUI informationPanel;


    private void OnEnable()
    {
        ObjectListItem.listItemClicked += HandleClickedItem;
        SpatialRecordManager.UpdatedSpatialServiceRecord += HandleUpdatedSpatialRecords;
    }

    private void OnDisable()
    {
        ObjectListItem.listItemClicked -= HandleClickedItem;
        SpatialRecordManager.UpdatedSpatialServiceRecord -= HandleUpdatedSpatialRecords;
    }

    private void HandleUpdatedSpatialRecords(SpatialServiceRecord[] spatialRecords)
    {
        CreateObjectItems(spatialRecords);
    }

    private void HandleClickedItem(SpatialServiceRecord obj)
    {

        Debug.Log("Clicked item: " + obj.id);
        informationPanel.SetValues(obj);
        informationPanel.gameObject.SetActive(true);

    }

    public void CreateObjectItems(SpatialServiceRecord[] spatialItems)
    {
        int itemCount = spatialItems.Length;
        for (int i = 0; i < itemCount; i++)
        {
            GameObject temp = Instantiate(prefabToSpawn);

            temp.GetComponent<ObjectListItem>().SetValues(spatialItems[i]);

            temp.transform.SetParent(contentHolder);
        }
    }

    public void GetObjects()
    {
        
        //Destroying all objects in GUI
        foreach (Transform child in contentHolder)
        {
            Destroy(child.gameObject);
        }

        //CreateObjectItems(mockResponse.spatialServiceRecord);
        spatialRecordManager.GetSpatialRecords();

    }

}
