using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteARObject : MonoBehaviour
{

    [SerializeField] private GameObject deleteButton;

    [SerializeField] private SCRItemTag currentSelectedItem;

    [SerializeField] private OrbitAPI orbitAPI;

    //onselected actiavte delete object and set current id

    //call delete objects 

    //wait for response and delete arobject

    public static event Action<SCRItemTag> Handle;


    private void OnEnable()
    {
        ARInteraction.itemSelected += HandleSelected;
        ARInteraction.itemExited += HandleExited;
    }


    private void OnDisable()
    {
        ARInteraction.itemSelected -= HandleSelected;
        ARInteraction.itemExited -= HandleExited;
    }



    public void DeleteItem()
    {
        DeleteItemAsync(currentSelectedItem.itemID);
    }

    private async void DeleteItemAsync(string itemID)
    {
        bool isSuccess = await orbitAPI.DeleteRecord(itemID);

        //TODO: fix delete response from deleteCall
        isSuccess = true;

        if(isSuccess)
        {

            Destroy(currentSelectedItem.gameObject);
            currentSelectedItem = null;
        }
        else
        {
            //TODO: inform the user
        }

    }

    private void HandleSelected(SCRItemTag obj)
    {
        currentSelectedItem = null;
        currentSelectedItem = obj;
        deleteButton.SetActive(true);
    }

    private void HandleExited()
    {
       
        deleteButton.SetActive(false);
    }

}
