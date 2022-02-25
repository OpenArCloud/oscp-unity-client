using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;

public class UIManager : MonoBehaviour
{
    GetPlaceHoldersDev gph;

    public Text placeholderRelocationTimer;
    public Text[] debugPose;

    public GameObject locateButton;
    public GameObject lowPanelButtons;
    public GameObject menuButtons;
    public GameObject loclizeProgress;
    public GameObject centerImage;
    public GameObject addButton;
    public GameObject newObjButton;
    public GameObject newObject;


    public float koefSticker;
    public float koefPin;

    public bool videoLookAtUser;
    bool sliderOn;

    public GameObject stickerPanel;
    public Text stickerText;
    public Text stickerType;
    int gloc, bloc;

    ACityAPIDev.StickerInfo stickerInfoForPanel;
    Action<bool> stickerDeActivate;
    GameObject aRcamera;


    void Start()
    {
        string ver = Application.version;
        if (PlayerPrefs.HasKey("bver"))
        {
            if (!(PlayerPrefs.GetString("bver").Equals(ver)))
            {
                UnityWebRequest.ClearCookieCache();
                Caching.ClearCache();
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetString("bver", ver);
            }
        }
        else
        {
            UnityWebRequest.ClearCookieCache();
            Caching.ClearCache();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("bver", ver);
        }

        Debug.Log("Version " + Application.version + " bver " + ver);

        gph = GetComponent<GetPlaceHoldersDev>();
        stickerInfoForPanel = null;
        stickerDeActivate = null;
        gloc = 0; bloc = 0;

        if (PlayerPrefs.HasKey("TimeForRelocation"))
        {
            float val = PlayerPrefs.GetFloat("TimeForRelocation");
            gph.setTimeForRelocation(val);
            placeholderRelocationTimer.text = "" + val;
        }
        else
        {
            PlayerPrefs.SetFloat("TimeForRelocation", gph.timeForRelocation);
        }

        aRcamera = Camera.main.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) ReloadScene("1");
    }

    public void ReloadScene(string sceneNum)
    {
        Debug.Log("Reloaded");
        AssetBundle.UnloadAllAssetBundles(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        //Application.LoadLevel("AugCityDebug");
    }

    public void setRelocationTimer(Text tt)
    {
        float val = float.Parse(tt.text);
        PlayerPrefs.SetFloat("TimeForRelocation", val);
        gph.setTimeForRelocation(val);
        placeholderRelocationTimer.text = "" + val;
    }

    public void setAnimationTimer(Text tt)
    {
        gph.setTimeForAnimation(float.Parse(tt.text));
    }

    public void setLooakAtVideo(bool vlook)
    {
        videoLookAtUser = vlook;
    }

    public void setSliderOn()
    {
        sliderOn = !sliderOn;
    }

    public void SetStickerPanel(ACityAPIDev.StickerInfo sInfo, Action<bool> stDeAct)
    {
        if (stickerDeActivate == null) { stickerDeActivate = stDeAct; }
        if ((stickerPanel.activeSelf) && sInfo.Equals(stickerInfoForPanel))
        {
            stickerPanel.SetActive(false);
            stickerDeActivate(false);
        }
        else
        {
            stickerInfoForPanel = sInfo;
            stickerPanel.SetActive(true);
            stickerText.text = sInfo.sText;
            stickerType.text = sInfo.sType;
            if (stickerDeActivate != null)
            {
                stickerDeActivate(false);
                stickerDeActivate = stDeAct;
                stickerDeActivate(true);
            }
        }
    }

    public void GoToStickerSite()
    {
        if (stickerInfoForPanel.sPath.Length > 0) {
            Application.OpenURL(stickerInfoForPanel.sPath);
        }
    }

    public void DownSwipe()
    {
        stickerPanel.SetActive(false);
        if (stickerDeActivate != null)
            stickerDeActivate(false);
    }

    public void Located()
    {
        setLowPanelButtons(true);
        setMenuButtons(true);
        setMapButtons(true);
        setLocalizeProgress(false);
        setColorCenterImage();
    }

    public void setLocateButton(bool act)
    {
        locateButton.SetActive(act);
    }
    public void setLowPanelButtons(bool act)
    {
        lowPanelButtons.SetActive(act);
    }
    public void setMapButtons(bool act)
    {
        
    }
    public void setMenuButtons(bool act)
    {
        menuButtons.SetActive(act);
    }

    public void setLocalizeProgress(bool act)
    {
        loclizeProgress.SetActive(act);
    }

    public void setColorCenterImage()
    {
        centerImage.GetComponent<Image>().color = Color.yellow;
    }

    public void SetServ(Text tt)
    {
        Debug.Log("Serv  = " + tt.text);
        PlayerPrefs.SetString("ApiUrl", tt.text);
    }

    public void CreateNewObject()
    {
        GameObject go = new GameObject("temp");
        go.transform.position = aRcamera.transform.position;
        go.transform.Translate(aRcamera.transform.forward*3);
        GameObject newObj = Instantiate(newObject);
        newObj.transform.position = go.transform.position;
    }

    public void AddToReco() {
    }

    public void setDebugPose(float xc, float yc, float zc, float xo, float yo, float zo, float wo, string recoId)
    {
        if (xc != 0)
        {
            debugPose[0].text = "xc = " + xc;
            debugPose[1].text = "yc = " + yc;
            debugPose[2].text = "zc = " + zc;
            debugPose[3].text = "xo = " + xo;
            debugPose[4].text = "yo = " + yo;
            debugPose[5].text = "zo = " + zo;
            debugPose[6].text = "wo = " + wo;
            gloc++;
        }
        else
        {
            debugPose[0].text = "Cant Localize ";
            debugPose[1].text = "yc = " + 0;
            debugPose[2].text = "zc = " + 0;
            debugPose[3].text = "xo = " + 0;
            debugPose[4].text = "yo = " + 0;
            debugPose[5].text = "zo = " + 0;
            debugPose[6].text = "wo = " + 0;
            bloc++;
        }
        debugPose[7].text = "Good loc = " + gloc;
        debugPose[8].text = "Bad loc = " + bloc;
        debugPose[9].text = "rec_id:#" + recoId;
    }

    public void gpsDebug(float lat, float lon, float hdop)
    {
        debugPose[10].text = "lat = " + lat;
        debugPose[11].text = "lon = " + lon;
        debugPose[14].text = "hdop= " + hdop;
    }

    public void planeDebug(float yplane)
    {
        debugPose[12].text = "PlaneUnderCam: " + (aRcamera.transform.position.y - yplane);
    }

    public void statusDebug(string status)
    {
        debugPose[13].text = status;
    }

    public void localizationMethodDebug(bool oscp, bool ecef, bool geo)
    {
        if (oscp)
        {
            if (!ecef && !geo) debugPose[16].text = "OSCP local";
            if (ecef)          debugPose[16].text = "ECEF";
            if (geo)           debugPose[16].text = "Geopose";
        }
        else debugPose[16].text = "Local";
    }

    public void ClearCash()
    {
        UnityWebRequest.ClearCookieCache();
        Caching.ClearCache();
    }

    public void DemoModeOff(bool mode)
    {
        menuButtons.SetActive(mode);
        lowPanelButtons.SetActive(mode);
    }

    public void Orient()
    {
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)      { Debug.Log("ScreenOrientation.LandscapeLeft");      }
        if (Input.deviceOrientation == DeviceOrientation.Portrait)           { Debug.Log("ScreenOrientation.Portrait");           }
        if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) { Debug.Log("ScreenOrientation.PortraitUpsideDown"); }
        if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)     { Debug.Log("ScreenOrientation.LandscapeRight");     }
    }

    public void DebugSet(bool onoff)
    {
        if (onoff)
            PlayerPrefs.SetInt("debug", 1);
        else
            PlayerPrefs.SetInt("debug", 0);
    }

}
