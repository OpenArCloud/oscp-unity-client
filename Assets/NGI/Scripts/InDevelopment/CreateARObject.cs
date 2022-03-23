using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateARObject : MonoBehaviour
{

    public GameObject[] gameobjectsAR;

    //prefab to create

    //last known geopose value

    //server to connect to

    //

    public GameObject arObjectToCreate;

    public void CreateObject()
    {

        // -at every succesful localization, we save the pair lastGlobalPose and lastLocalPose
        //- we add 1 button "Add object" that appears after successful localization
        //- when the user presses the button, the currentLocalPost(of ARCore) is saved, and the model browser opens.
        //- the users selects a model
        //-we create a geopose entry which is the lastGlobalPose (for now.Later we must add the difference between lastLocalPose and currentLocalPose.Finally, we must also add on top of this the raycast hitpoint pose with respect to currentLocalPose).
        //- we place the model's URL into an SCR, we fill in the geopose, and call POST (not much difference from the update PUT, I believe)


        //get position / geopose

        //instantiate 

        //call server and post new object with values
        //get response

        GameObject temp = Instantiate(arObjectToCreate);
        //GameObject model = Instantiate(GetComponent<ModelManager>().ABloaderNGI, placeHolderParent.transform);
        temp.transform.position = OSCPDataHolder.Instance.lastPositon;
        // temp.transform.rotation = OSCPDataHolder.Instance.lastOrientation;




    }

    public void AddObjectToList()
    {

        //FindObjectOfType<SCRManager>().AddSpatialRecord(CreateSCDItem());
    }


    //Test creation for test item
    public SCRItem CreateSCDItem()
    {
        SCRItem sp = new SCRItem();

        sp.content = new Content();
        sp.content.geopose = new GeoPosition();
        sp.content.geopose.quaternion = new Dictionary<string, float>();
        sp.content.refs = new List<Dictionary<string, string>>();
        sp.content.definitions = new List<Dictionary<string, string>>();
        sp.content.keywords = new List<string>();

        sp.content.id = "123456";
        sp.content.type = "placeholder";
        sp.content.title = "testcube";
        sp.content.description = "This was created from the unity client";

        sp.content.geopose.latitude = OSCPDataHolder.Instance.latitude;
        sp.content.geopose.longitude = OSCPDataHolder.Instance.longitude;
        sp.content.geopose.ellipsoidHeight = OSCPDataHolder.Instance.ellipsoidHeight;

        sp.id = "";
        sp.type = "scr";
        sp.tenant = "public";
        sp.timestamp = 0;

        //Dont know what these two attributes handle
        sp.content.bbox = "";
        sp.content.size = 1f;


        //Mock position Needs update when Visual Positioning System is working
        Quaternion tempQ = OSCPDataHolder.Instance.lastOrientation;

        sp.content.geopose.quaternion.Add("x", tempQ.x);
        sp.content.geopose.quaternion.Add("y", tempQ.y);
        sp.content.geopose.quaternion.Add("z", tempQ.z);
        sp.content.geopose.quaternion.Add("w", tempQ.w);

        sp.isAssetBundle = false;

        sp.content.keywords.Add("test item");


        Dictionary<string, string> keyValuePairsDefinitions = new Dictionary<string, string>();
        keyValuePairsDefinitions.Add("type", "testkey");
        keyValuePairsDefinitions.Add("valuev", "testvalue");
        sp.content.refs.Add(keyValuePairsDefinitions);

        Dictionary<string, string> keyValuePairsRefs = new Dictionary<string, string>();

        keyValuePairsRefs.Add("contentType", "model/gltf+json");
        keyValuePairsRefs.Add("url", "https://simplecloudstorageefded4746b7146beb038662f439a393602-staging.s3.eu-north-1.amazonaws.com/private/eu-north-1%3Af7c86f34-4d88-4140-8795-37e524ef076f/ngi/media/3d/glb/model%20%2827%29.glb");


        sp.content.definitions.Add(keyValuePairsRefs);
    

        return sp;

    }

}
