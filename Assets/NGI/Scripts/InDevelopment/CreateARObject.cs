using NGI.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateARObject : MonoBehaviour
{

    public GameObject[] gameobjectsAR;

    [SerializeField] OrbitAPI orbitAPI;

    [SerializeField] GameObject inputPanel;
    [SerializeField] GameObject itemsPanel;

    [SerializeField] private InputField inputTitle;
    [SerializeField] private InputField inputDescription;

    [SerializeField] string[] urls;

    [SerializeField] string selectedUrl;

    //prefab to create

    //last known geopose value

    //server to connect to

    //

    public GameObject arObjectToCreate;

    private void Start()
    {
        
    }


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

        SCRItem item = CreateSCDItem();

        //GameObject temp = Instantiate(arObjectToCreate);

        //orbitAPI.CreateSpatialRecord(item);
        CreateObjectOnServer(item);

        //Toggle active panels
        OpenItemsPanel();
        OpenInputPanel();
    }

    public async void CreateObjectOnServer(SCRItem item)
    {
        string id = await orbitAPI.CreateSpatialRecord(item);

        if(!string.IsNullOrEmpty(id))
        {
             GameObject model = Instantiate(arObjectToCreate);

            model.transform.position = OSCPDataHolder.Instance.lastPositon;
            model.transform.rotation = OSCPDataHolder.Instance.lastOrientation;

            var gltf = model.AddComponent<GLTFast.GltfAsset>();
            gltf.url = selectedUrl;

            model.AddComponent<SCRItemTag>();

            model.GetComponent<SCRItemTag>().itemID = id;

        }

    }

    public void SelectURL(int selection)
    {
        selectedUrl = urls[selection];
    }

    public void OpenItemsPanel()
    {
        itemsPanel.SetActive(!itemsPanel.activeSelf);
    }

    public void OpenInputPanel()
    {
        inputPanel.SetActive(!inputPanel.activeSelf);
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
      
        sp.type = "scr";
        sp.tenant = "public";
        sp.timestamp = 0;

        sp.content.id = "123456";
        sp.content.type = "placeholder";
        sp.content.title = inputTitle.text;
        sp.content.description = inputDescription.text;

        sp.content.geopose.latitude = OSCPDataHolder.Instance.latitude;
        sp.content.geopose.longitude = OSCPDataHolder.Instance.longitude;

        //TODO: Evil hack to get Webxr client to show models on the floor that is why we are adding - 1.5 height
        sp.content.geopose.ellipsoidHeight = OSCPDataHolder.Instance.ellipsoidHeight - 1.5;

        //Dont know what these two attributes handle
        sp.content.bbox = "0";
        sp.content.size = 1f;


        //Mock position Needs update when Visual Positioning System is working
        Quaternion tempQ = OSCPDataHolder.Instance.lastOrientation;

        sp.content.geopose.quaternion.Add("x", 0); //tempQ.x);
        sp.content.geopose.quaternion.Add("y", 0);//tempQ.y);
        sp.content.geopose.quaternion.Add("z", 0);//tempQ.z);
        sp.content.geopose.quaternion.Add("w", 1);//tempQ.w);

        sp.isAssetBundle = false;

        sp.content.keywords.Add("test item");


        Dictionary<string, string> keyValuePairsDefinitions = new Dictionary<string, string>();
        keyValuePairsDefinitions.Add("type", "testkey");
        keyValuePairsDefinitions.Add("value", "testvalue");
        sp.content.definitions.Add(keyValuePairsDefinitions);

        Dictionary<string, string> keyValuePairsRefs = new Dictionary<string, string>();
        keyValuePairsRefs.Add("contentType", "model/gltf+json");
        keyValuePairsRefs.Add("url", selectedUrl);
        sp.content.refs.Add(keyValuePairsRefs);
    
        return sp;

    }

}
