using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Outdated and not used anymore, candidate to delete at all
/// </summary>
public class CoordTest : MonoBehaviour
{
    Transform camARTrans;


    public Text t1, t2, t3, textInputServerPlaceholder, textInputServer;
    public Toggle[] toggles; // toggling any of them triggers the CoordTest.SetServ() and SetToggle() methods
    // public Text[] servers;
    // WARNING: Toggle Element 5 is missing from the GUI at the moment

    // Start is called before the first frame update
    void Start()
    {
        camARTrans = Camera.main.gameObject.transform;
        if (PlayerPrefs.HasKey("Tnumber"))
        {
            //ToggleGroup a;
            toggles[PlayerPrefs.GetInt("Tnumber")].isOn = true;
            if (PlayerPrefs.GetInt("Tnumber") == 6) // WARNING: hardcoded that the custom URL must be at index 6! But in our OSCP client, there is no index 5.
            {
                textInputServer.text = PlayerPrefs.GetString("ApiUrl");
                textInputServerPlaceholder.text = PlayerPrefs.GetString("ApiUrl");
            }
        }
        else {
            // The first entry is taken as default
            toggles[0].isOn = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        t1.text = "" + camARTrans.eulerAngles.x;
        t2.text = "" + camARTrans.eulerAngles.y;
        t3.text = "" + camARTrans.eulerAngles.z;
    }

    public void SetServ(Text tt)
    {
        Debug.Log("CoordTest.SetServ: " + tt.text);
        // TODO: this method received http:// address instead of https://
        // it seems that the value came from the Settings Panels' ServerAddress Field
        // we should get the https:// address from the PlayerPrefs instead
        if (tt.text.Length > 6) // TODO: why only written when the length of the address field is larger than 6 characters? Do we want the toggle index to be here instead?
        {
            Debug.Log("New Server addr = " + tt.text);
            PlayerPrefs.SetString("ApiUrl", tt.text);
            //GetComponent<ACityAPIDev>().setApiURL(tt.text);
        }
    }

    public void SetToggle(int tog)
    {
        Debug.Log("CoordTest.SetToggle: " + tog);
        Debug.Log("Tnumber wtf = " + tog);
        PlayerPrefs.SetInt("Tnumber", tog);
    }

    public void SetUurl(string url)
    {

    }

}
