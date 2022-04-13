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
    public Toggle[] toggles;
    // public Text[] servers;
    // Start is called before the first frame update

    void Start()
    {
        camARTrans = Camera.main.gameObject.transform;
        if (PlayerPrefs.HasKey("Tnumber"))
        {
            //ToggleGroup a;
            toggles[PlayerPrefs.GetInt("Tnumber")].isOn = true;
            if (PlayerPrefs.GetInt("Tnumber") == 6)
            {
                textInputServer.text = PlayerPrefs.GetString("ApiUrl");
                textInputServerPlaceholder.text = PlayerPrefs.GetString("ApiUrl");
            }
        }
        else 
        {
            if(toggles[0] != null)
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
        if (tt.text.Length > 6)
        {
            Debug.Log("New Server addr = " + tt.text);
            PlayerPrefs.SetString("ApiUrl", tt.text);
            //GetComponent<ACityAPIDev>().setApiURL(tt.text);
        }
    }

    public void SetToggle(int tog)
    {
        Debug.Log("Tnumber wtf = " + tog);
        PlayerPrefs.SetInt("Tnumber", tog);
    }
}
