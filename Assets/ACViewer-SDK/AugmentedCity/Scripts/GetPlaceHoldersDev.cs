using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class GetPlaceHoldersDev : MonoBehaviour
{
    public GameObject dot;
    public GameObject linePrefab;
    public GameObject cantLocalizeImage;
    public GameObject localizedImage;
    public GameObject videoPref;
    public GameObject stickerPref;
    public GameObject stickerFood;
    public GameObject stickerPlace;
    public GameObject stickerShop;

    public Material devCamMat;
    public string devImagePath;

    List<GameObject> recos = new List<GameObject>();
    List<GameObject> videoDemos = new List<GameObject>();
    List<GameObject> stickerObjects = new List<GameObject>();
    List<GameObject> videoURLs = new List<GameObject>();
    List<GameObject> placeHoldersDotsLines = new List<GameObject>();
    List<GameObject> plyObjects = new List<GameObject>();
    List<GameObject> models = new List<GameObject>();

    ACityAPIDev acapi;
    Vector3 deltaTranslateVector, deltaRotateVector;
    public bool needScaling;
    bool translateAction;
    Transform movingTransform;
    float moveFrames;
    float frameCounter;
    Vector3 arCamCoordinates, pastArCamCoordinates;
    Quaternion targetRotation, startRotation;
    GameObject aRcamera;
    string lastLocalizedRecoId;
    float timerRelocation;
    float animationTime = 2f;
    UIManager uim;
    GameObject activeReco, modelToServer;


    bool ARStarted, relocationCompleted, toShowPlaceHolders, videoDemosTurn, toShowStickers;
    [HideInInspector]
    public float timeForRelocation = 2f;
    void Start()
    {
        acapi = GetComponent<ACityAPIDev>();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        aRcamera = Camera.main.gameObject;
        relocationCompleted = true;
        toShowStickers = true;
        uim = this.GetComponent<UIManager>();
        acapi.prepareSession(preparationCheck);
    }

    public void setTimeForRelocation(float tfr) {
        timeForRelocation = tfr;
        timerRelocation = tfr;
    }

    public void setTimeForAnimation(float tfa)
    {
        animationTime = tfa;
    }

    void preparationCheck(bool b, string ans) {
        Debug.Log(ans+"CLIENT");
    }

    public void startDevLocation() { // Test loacalization from Unity Editor
        if (acapi.editorTestMode) timeForRelocation = 100f;
        pastArCamCoordinates = arCamCoordinates;
        arCamCoordinates = new Vector3(aRcamera.transform.position.x, aRcamera.transform.position.y, aRcamera.transform.position.z);

        byte[] bytes = File.ReadAllBytes(devImagePath);
        Debug.Log("bytes = " + bytes.Length);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        devCamMat.mainTexture = tex;
        // acapi.firstLocalization(59.934320f, 30.272606f, devImagePath, showPlaceHolders); // 59.934320f, 30.272606f - ВО офис, двор
       //  acapi.firstLocalization(41.1224f, 16.8684f, devImagePath, showPlaceHolders); //Bari cafe lat=41.1224f, long=16.8684f
         acapi.firstLocalization(43.40529f, 39.95574f, devImagePath, showPlaceHolders); // Sochi - 43.404521f, 39.954741f / / 43.404080, 39.954735// 43.404769, 39.954042//43.40529, 39.95574
        timerRelocation = timeForRelocation;
        ARStarted = true;
        relocationCompleted = false;
    }


    public void startLocalization() {
        pastArCamCoordinates = arCamCoordinates;
        arCamCoordinates = new Vector3(aRcamera.transform.position.x, aRcamera.transform.position.y, aRcamera.transform.position.z);
        Debug.Log("ARcam x = " + aRcamera.transform.position.x);
        acapi.ARLocation(showPlaceHolders);
        timerRelocation = timeForRelocation;
        ARStarted = true;
        relocationCompleted = false;
    }


    void showPlaceHolders(string id, Transform zeroP, ACityAPIDev.StickerInfo[] stickers) {
        if (id != null)
        {
            Debug.Log("zero = " + zeroP.position.x + "    " + zeroP.position.y + "    " + zeroP.position.z);
            Debug.Log("zeroeulerAngles = " + zeroP.eulerAngles.x + "    " + zeroP.eulerAngles.y + "    " + zeroP.eulerAngles.z);

            GameObject placeHolderParent;
            placeHolderParent = checkSavedID(id);
            if (placeHolderParent == null)
            {
                if (stickers != null)
                {
                    GameObject scaleParent = new GameObject("CamParent-" + id);
                    scaleParent.transform.position = arCamCoordinates;
                    placeHolderParent = new GameObject(id);
                    placeHolderParent.transform.position = zeroP.position;
                    placeHolderParent.transform.rotation = zeroP.rotation;
                    activeReco = placeHolderParent;   

                    for (int j = 0; j < stickers.Length; j++)
                    {
                        //Placeholders
                        for (int i = 0; i < 4; i++)
                        {
                            GameObject go = Instantiate(dot, placeHolderParent.transform);
                            go.transform.position = stickers[j].positions[i];
                            placeHoldersDotsLines.Add(go);
                        }
                        //Lines
                        GameObject lineHolder = Instantiate(linePrefab);
                        LineRenderer lr = lineHolder.GetComponent<LineRenderer>();
                        lr.positionCount = 4;
                        lr.SetPositions(stickers[j].positions);
                        lineHolder.transform.SetParent(placeHolderParent.transform);
                        lr.useWorldSpace = false;
                        placeHoldersDotsLines.Add(lineHolder);

                        //---VideoPlayer
                        GameObject temp1 = Instantiate(dot, placeHolderParent.transform);
                        temp1.transform.position = stickers[j].positions[0];
                        GameObject temp2 = Instantiate(dot, placeHolderParent.transform);
                        temp2.transform.position = new Vector3(stickers[j].positions[1].x, stickers[j].positions[0].y, stickers[j].positions[1].z);
                        GameObject temp3 = Instantiate(dot, placeHolderParent.transform);
                        temp3.transform.position = stickers[j].positions[2];
                        Vector3 raznp = (stickers[j].positions[0] - stickers[j].positions[2]) / 2;
                        GameObject vp = Instantiate(videoPref, placeHolderParent.transform);
                        vp.transform.position = temp1.transform.position;
                        vp.transform.SetParent(temp1.transform);
                        temp1.transform.LookAt(temp2.transform);
                        vp.transform.position = stickers[j].positions[0] - raznp;
                        vp.transform.localEulerAngles = new Vector3(vp.transform.localEulerAngles.x, vp.transform.localEulerAngles.y + 90, vp.transform.localEulerAngles.z);
                        vp.transform.SetParent(placeHolderParent.transform);
                        vp.transform.localEulerAngles = new Vector3(0, vp.transform.localEulerAngles.y, 0);
                        vp.transform.localScale = (vp.transform.localScale * Vector3.Magnitude(stickers[j].positions[0] - stickers[j].positions[1]));
                        videoDemos.Add(vp);

                        if (stickers[j].sPath.Contains("mp4"))
                        {
                            GameObject urlVid = Instantiate(vp, placeHolderParent.transform);
                            VideoPlayer vidos = urlVid.GetComponentInChildren<VideoPlayer>();
                            vidos.source = VideoSource.Url;
                            vidos.url = stickers[j].sPath;
                            videoURLs.Add(urlVid);
                        }
                        else if (stickers[j].sSubType.Contains("3dobject")||stickers[j].sPath.Contains("3dobject")) {
                            GameObject model = Instantiate(GetComponent<ModelManager>().ABloader, placeHolderParent.transform);
                            string bundleName = stickers[j].sText.ToLower();
                            model.GetComponent<AssetLoader>().ABName = bundleName;
                            model.GetComponent<AssetLoader>().BundleFullURL = GetComponent<ModelManager>().modelPath;
                            model.transform.localPosition = stickers[j].mainPositions;// * acapi.tempScale3d;
                            model.transform.localPosition = new Vector3(model.transform.localPosition.x,-model.transform.localPosition.y,model.transform.localPosition.z);
                            model.transform.localRotation = new Quaternion(stickers[j].orientations.x, stickers[j].orientations.y, stickers[j].orientations.z, stickers[j].orientations.w);

                               if (stickers[j].sTrajectoryPath.Length > 1) {
                                   Trajectory tr = model.GetComponent<Trajectory>();
                                   tr.go = true;
                                   tr.acapi = acapi;
                                   tr.sTrajectory = stickers[j].sTrajectoryPath;
                                   tr.sTimePeriod = stickers[j].sTrajectoryPeriod;
                                   tr.sOffset = stickers[j].sTrajectoryOffset;
                               }

                            Debug.Log(stickers[j].sTrajectoryPath);

                            // TEMP
                            Mover mover = model.GetComponent<Mover>();
                            mover.setLocked(true);
                            mover.objectId = stickers[j].objectId;
                            if (bundleName.Contains("quar") || bundleName.Contains("santa") || bundleName.Contains("pavel") || bundleName.Contains("gard"))
                            {
                                mover.landed = true;
                            }

                            Debug.Log(j+". 3dmodel "+ stickers[j].sText + " = " + model.transform.localPosition + " ROT Quaternion = " + model.transform.localRotation + " stickers[j].orientations = " + stickers[j].orientations);
                           
                            if (stickers[j].SModel_scale.Length>0) model.transform.localScale = new Vector3(float.Parse(stickers[j].SModel_scale), float.Parse(stickers[j].SModel_scale), float.Parse(stickers[j].SModel_scale));
                            models.Add(model);
                        }
                        else
                        {
                            GameObject newSticker = null;
                            string checkType = stickers[j].sType.ToLower();
                            if (checkType.Contains("food") || checkType.Contains("restaurant")) newSticker = Instantiate(stickerFood, placeHolderParent.transform);
                            else if (checkType.Contains("place")) newSticker = Instantiate(stickerPlace, placeHolderParent.transform);
                            else if (checkType.Contains("shop")) newSticker = Instantiate(stickerShop, placeHolderParent.transform);
                            else newSticker = Instantiate(stickerPref, placeHolderParent.transform);
                            if (newSticker != null)
                            {
                                newSticker.transform.position = stickers[j].positions[0] - raznp;
                                StickerController sc = newSticker.GetComponent<StickerController>();
                                sc.setStickerInfo(stickers[j]);
                                stickerObjects.Add(newSticker);
                            }
                        }
                        Destroy(temp1); Destroy(temp2);
                        Destroy(temp3);
                        relocationCompleted = true;

                    }
                    GameObject id3d = get3dFromLocal(id);
                    if (id3d != null)
                    {
                        GameObject model = Instantiate(GetComponent<ModelManager>().ABloader, placeHolderParent.transform);
                        model.GetComponent<AssetLoader>().ABName = id3d.name;
                        model.GetComponent<AssetLoader>().BundleFullURL = GetComponent<ModelManager>().modelPath;
                        model.transform.localPosition = id3d.transform.position;
                        model.transform.localRotation = id3d.transform.rotation;
                        model.GetComponent<Mover>().setLocked(true);
                        Debug.Log("Loaded pos = " + id3d.transform.position + ", ori = " + id3d.transform.rotation);
                        models.Add(model);
                    }


                    turnOffVideoDemos(videoDemosTurn);
                    turnOffPlaceHolders(toShowPlaceHolders);
                    turnOffStickers(toShowStickers);

                    localizedImage.SetActive(true);
                    placeHolderParent.transform.SetParent(scaleParent.transform);
                    recos.Add(placeHolderParent);
                    uim.Located();
                }
                else
                {
                    Debug.Log("No stickers");

                    CantLocalize();
                }
            }
            else
            {
                Transform scaleParentTransform = placeHolderParent.transform.root;
                placeHolderParent.SetActive(true);
                GameObject tempScaler = new GameObject("TempScaler");
                tempScaler.transform.position = arCamCoordinates;
                GameObject tempBiasVector = new GameObject("TempBiasVector");
                tempBiasVector.transform.position = zeroP.position;
                tempBiasVector.transform.eulerAngles = zeroP.eulerAngles;

                tempBiasVector.transform.SetParent(tempScaler.transform);
                tempScaler.transform.localScale = scaleParentTransform.localScale;


                Translocation(placeHolderParent, tempBiasVector.transform, animationTime);
                Destroy(tempScaler); Destroy(tempBiasVector);
            }

            lastLocalizedRecoId = id;
        }
        else
        {
            CantLocalize();
        }
    }


    void Translocation(GameObject transObject, Transform targetTransform, float time)
    {
        movingTransform = transObject.transform;
        moveFrames = time / Time.fixedDeltaTime;
        frameCounter = moveFrames;
        deltaTranslateVector = movingTransform.position;
        deltaRotateVector = targetTransform.position;
        translateAction = true;
        targetRotation = targetTransform.rotation;
        startRotation = movingTransform.rotation;
    }

    void TranslationMover() {
        movingTransform.position = Vector3.Lerp(deltaTranslateVector, deltaRotateVector, (moveFrames - frameCounter) / moveFrames);
        movingTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, (moveFrames-frameCounter)/ moveFrames);
        frameCounter--;
        if (frameCounter <= 0) { translateAction = false; relocationCompleted = true; }
    }

    void FixedUpdate()
    {
        if (translateAction) TranslationMover();
        if (ARStarted) {
            timerRelocation = timerRelocation - Time.fixedDeltaTime;
            if ((timerRelocation < 0) && (relocationCompleted)) {
               if (acapi.editorTestMode) startDevLocation();
                else startLocalization();
                timerRelocation = timeForRelocation;
            }
        }
    }


    public List<GameObject> GetAllStickers() {
        return stickerObjects;
    }

    void CantLocalize() {
        Debug.Log("Can't localize client or no stickers");
        ACityAPIDev.LocalizationStatus ls = acapi.getLocalizationStatus();
            timerRelocation = 1.1f;
        relocationCompleted = true;
    }

    public void SetRecoScale(float scale) {
        Transform llrt = checkSavedID(lastLocalizedRecoId).transform.root;
        if (llrt != null) { llrt.localScale = new Vector3(scale, scale, scale); }
    }

    GameObject checkSavedID(string id)
    {
        GameObject reco = null;
        foreach (GameObject go in recos)
        {
            if (go.name.Contains(id)) { reco = go; }
            else go.SetActive(false);
        }
        return reco;
    }

    public bool GetRelocationState() {
        return relocationCompleted;
    }

    public void turnOffVideoDemos(bool setup)
    {
        videoDemosTurn = setup;
        if (videoDemos != null)
        {
            foreach (GameObject go in videoDemos)
            {
                go.SetActive(setup);
            }

            foreach (GameObject go in videoURLs)
            {
                go.SetActive(!setup);
            }
        }
    }

    public void turnOffVideoURL(bool setup)
    {
        foreach (GameObject go in videoURLs)
        {
            go.SetActive(setup);
        }
    }

    public void turnOffPlaceHolders(bool onOff) {
        toShowPlaceHolders = onOff;
        foreach (GameObject p in placeHoldersDotsLines) {
            p.SetActive(onOff);
        }
    }

    public void turnOffStickers(bool onOff)
    {
        toShowStickers = onOff;
        foreach (GameObject sticker in stickerObjects)
        {
            sticker.SetActive(onOff);
        }
    }

    public void turnOffModels(bool setup)
    {
        foreach (GameObject go in models)
        {
            go.SetActive(setup);
        }
    }


    public void setNewModelObjectId(string objectParams) {
        if (modelToServer != null) {
            modelToServer.GetComponent<Mover>().objectId = objectParams;
        }
    }

    public void set3dToLocal(string id, string name, Vector3 coords, Quaternion orientation) {
        PlayerPrefs.SetString(id, name);
        PlayerPrefs.SetFloat(id+"coordx", coords.x);
        PlayerPrefs.SetFloat(id + "coordy", coords.y);
        PlayerPrefs.SetFloat(id + "coordz", coords.z);
        PlayerPrefs.SetFloat(id + "orix", orientation.x);
        PlayerPrefs.SetFloat(id + "oriy", orientation.y);
        PlayerPrefs.SetFloat(id + "oriz", orientation.z);
        PlayerPrefs.SetFloat(id + "oriw", orientation.w);
        PlayerPrefs.Save();
        Debug.Log("saved pos = " + coords + ", ori = " + orientation);
    }

    public GameObject get3dFromLocal(string id) {
        GameObject temp = null;
        if (PlayerPrefs.HasKey(id)) {
            temp = new GameObject(PlayerPrefs.GetString(id));
            temp.transform.position = new Vector3(PlayerPrefs.GetFloat(id + "coordx"), PlayerPrefs.GetFloat(id + "coordy"), PlayerPrefs.GetFloat(id + "coordz"));
            temp.transform.rotation = new Quaternion(PlayerPrefs.GetFloat(id + "orix"), PlayerPrefs.GetFloat(id + "oriy"), PlayerPrefs.GetFloat(id + "oriz"), PlayerPrefs.GetFloat(id + "oriw"));
        }
        return temp;
    }

    public string getCurrentRecoId() {
        return lastLocalizedRecoId;
    }
}
