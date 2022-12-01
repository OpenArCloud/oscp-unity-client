using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
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

    //59.934320f,  30.272610f, 30, devImagePath, showPlaceHolders); // Spb VO-yard
    //41.122400f,  16.868400f, 30, devImagePath, showPlaceHolders); // Bari cafe (lat=41.1224f, lon=16.8684f)
    //43.405290f,  39.955740f, 30, devImagePath, showPlaceHolders); // Sochi 43.404521f,39.954741f 43.404080,39.954735 43.404769,39.954042 43.40529,39.95574
    //59.91467f, 30.30398f, 30, devImagePath, showPlaceHolders); // Новый дом, 8я красноармейская 59.914639, 30.304093 59.91462671296234, 30.304159752060876// 59.9131102286, 30.303762554748754
    //59.9145560f, 30.304109f, 30, devImagePath, showPlaceHolders);
    //59.168705f, 18.174704f, 30, path, showPlaceHolders); //Brandbergen Sweden
    public float devLocationLatitude = 59.168705f;
    public float devLocationLongitude = 18.174704f;
    public float devLocationHDOP = 30.0f; // horizontal dilution of precision

    List<GameObject> recos = new List<GameObject>();        // cache of created scene of gameobjects by entry 'id'
    List<GameObject> videoDemos = new List<GameObject>();
    List<GameObject> stickerObjects = new List<GameObject>();
    List<GameObject> videoURLs = new List<GameObject>();
    List<GameObject> placeHoldersDotsLines = new List<GameObject>();
    List<GameObject> plyObjects = new List<GameObject>();
    List<GameObject> models = new List<GameObject>();

    ACityAPIDev acapi;
    //ACityNGI acapi;
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
    public float timeForRelocation = 20f;  // set reloc time to 20 secs by default


    private OrbitAPI orbitAPI;

    void Start()
    {
        orbitAPI = FindObjectOfType<OrbitAPI>();

        acapi = GetComponent<ACityAPIDev>();
        //acapi = GetComponent<ACityNGI>();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        aRcamera = Camera.main.gameObject;
        relocationCompleted = true;
        toShowStickers = true;
        uim = this.GetComponent<UIManager>();
        acapi.prepareSession(preparationCheck); //FixMe: in aco3d it's off
    }

    public void setTimeForRelocation(float tfr)
    {
        timeForRelocation = tfr;
        timerRelocation = tfr;
    }

    public void setTimeForAnimation(float tfa)
    {
        animationTime = tfa;
    }

    void preparationCheck(bool b, string ans)
    {
        Debug.Log(ans + "CLIENT");
    }

    public void startDevLocation()   // Test localization from Unity Editor
    {
        Debug.Log("startDevLocalization");
        if (acapi.editorTestMode)
        {
            timeForRelocation = 20f;
            PlayerPrefs.SetFloat("TimeForRelocation", timeForRelocation);
        }
        pastArCamCoordinates = arCamCoordinates;
        arCamCoordinates = new Vector3(aRcamera.transform.position.x, aRcamera.transform.position.y, aRcamera.transform.position.z);
        string path = Application.streamingAssetsPath + "/" + devImagePath;
        Debug.Log(path);
        byte[] bytes = File.ReadAllBytes(path);
        //byte[] bytes = File.ReadAllBytes(devImagePath);
        Debug.Log("bytes = " + bytes.Length);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        devCamMat.mainTexture = tex;  //FixMe: ???

        acapi.firstLocalization(devLocationLongitude, devLocationLatitude, devLocationHDOP, path, showPlaceHolders);
        timerRelocation = timeForRelocation;
        ARStarted = true;
        relocationCompleted = false;

#if UNITY_EDITOR
        //This uses the location set in editor, editor testing only
        acapi.GetH3IndexEditor(devLocationLatitude, devLocationLongitude);
#endif
    }

    public void startLocalization()
    {
        Console.WriteLine("startLocalization");
        pastArCamCoordinates = arCamCoordinates;
        arCamCoordinates = new Vector3(aRcamera.transform.position.x, aRcamera.transform.position.y, aRcamera.transform.position.z);
        Console.WriteLine("ARcam position\n  x = " + aRcamera.transform.position.x + "\n  y = " + aRcamera.transform.position.y + "\n  z = " + aRcamera.transform.position.z);
        acapi.ARLocation(showPlaceHolders);
        timerRelocation = timeForRelocation;
        ARStarted = true;
        relocationCompleted = false;
    }

    //NGI addition
    // TODO: unify with SCRManager.GetSpatialRecords
    // TODO: apparently never used
    public async Task GetOrbitContent()
    {
        if (acapi.useOrbitContent)
        {
            await orbitAPI.LoadItemsFromServer();
        }
    }

    // Get the normal to a triangle from the three corner points, a, b and c.
    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        // Find vectors corresponding to two of the sides of the triangle.
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        // Cross the vectors to get a perpendicular vector, then normalize it.
        return Vector3.Cross(side1, side2).normalized;
    }

    bool checkVideoOrientation(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        bool isReversedVideo = false;
        //1. Calculate vector product (1-2) and (2-3) that's the plane normal
        //2. Check the sign of Z normal component => if positive (parallel to Z axis), it's forward - it's reversed; otherwise - it's ok.
        Vector3 normal = GetNormal(point1, point2, point3);
        Console.WriteLine("normal: x: " + normal.x + " y: " + normal.y + " z: " + normal.z);
        if (normal.z > 0) {
            isReversedVideo = true;
        }
        return isReversedVideo;
    }

    // TODO: this method is specific to AugmentedCity. We should instead write an OSCP-standard method
    void showPlaceHolders(string id, Transform zeroP, ACityAPIDev.StickerInfo[] stickers)
    {
        Console.WriteLine("Enterd showplaceholders");

        if (id == null) {
            Debug.Log("AC reconstruction id is null!");
            id = "0";
            // This is not a problem anymore. The reconstruction_id is only returned in AC responses 
            // but is not part of the GeoPoseProtocol
            //CantLocalize();
            //return;
        }

        lastLocalizedRecoId = id;
        setTimeForRelocation(PlayerPrefs.GetFloat("TimeForRelocation"));

        /*
        //Debug.Log("zeroPpos = " + zeroP.position.x + "    " + zeroP.position.y + "    " + zeroP.position.z);
        //Debug.Log("zeroPori = " + zeroP.eulerAngles.x + "    " + zeroP.eulerAngles.y + "    " + zeroP.eulerAngles.z);
        if (pcloud == null) {
            pcloud = GetComponent<plyreader>().GetPointCloud();
            if (pcloud != null) {
                pcloud.transform.position = zeroP.position;
                pcloud.transform.rotation = zeroP.rotation;
            }
        }
        */

        GameObject placeHolderParent = checkSavedID(id);

        // If there is already a scene, we may slightly adjust it based on the latest localization
        if (placeHolderParent != null) {
            Transform scaleParentTransform = placeHolderParent.transform.root;
            //if (needScaling && lastLocalizedRecoId.Contains(id)) {}
            placeHolderParent.SetActive(true);
            GameObject tempScaler = new GameObject("TempScaler");
            tempScaler.transform.position = arCamCoordinates;
            GameObject tempBiasVector = new GameObject("TempBiasVector");
            tempBiasVector.transform.position = zeroP.position;
            tempBiasVector.transform.eulerAngles = zeroP.eulerAngles;

            tempBiasVector.transform.SetParent(tempScaler.transform);
            tempScaler.transform.localScale = scaleParentTransform.localScale;

            //Translocation(placeHolderParent, tempBiasVector.transform, animationTime);
            Destroy(tempScaler);
            Destroy(tempBiasVector);
            return;
        }

        // There is no scene yet, we need to generate a new scene

        if (stickers == null) {
            // But there is nothing to do in an empty scene, no objects to create
            Debug.Log("No stickers -- aborting");
            CantLocalize();
            return;
        }

        Console.WriteLine("Retrieved " + stickers.Length + " stickers. Creating the scene...");


        GameObject scaleParent = new GameObject("CamParent-" + id);  // add 'id' into the name
        scaleParent.transform.position = arCamCoordinates;
        placeHolderParent = new GameObject(id);
        placeHolderParent.transform.position = zeroP.position;
        placeHolderParent.transform.rotation = zeroP.rotation;
        activeReco = placeHolderParent;

        for (int j = 0; j < stickers.Length; j++)
        {
            Console.WriteLine("*** Creating sticker #" + j);
            if (stickers[j] == null) // invalid sticker
            {
                Debug.Log("Sticker #" + j + " was invalid!");
                continue;
            }

            //skips current spatial item if its to far away
            if (stickers[j].spatialContentRecord.isTooFarAway)
            {
                Console.WriteLine("Sticker #" + j + " is too far away -- skipping it");
                continue;
            }

            // Placeholders
            for (int i = 0; i < 4; i++)
            {
                GameObject go = Instantiate(dot, placeHolderParent.transform);
                go.transform.position = stickers[j].positions[i];
                placeHoldersDotsLines.Add(go);
            }

            // Lines
            GameObject lineHolder = Instantiate(linePrefab);
            LineRenderer lr = lineHolder.GetComponent<LineRenderer>();
            lr.positionCount = 4;
            lr.SetPositions(stickers[j].positions);
            lineHolder.transform.SetParent(placeHolderParent.transform);
            lr.useWorldSpace = false;
            placeHoldersDotsLines.Add(lineHolder);

            // VideoPlayer
            GameObject temp1 = Instantiate(dot, placeHolderParent.transform);
            temp1.transform.position = stickers[j].positions[0];
            //Debug.Log("temp1 -> x: " + temp1.transform.position.x + "y: " + temp1.transform.position.y + " z: " + temp1.transform.position.z);
            GameObject temp2 = Instantiate(dot, placeHolderParent.transform);
            temp2.transform.position = new Vector3(stickers[j].positions[1].x, stickers[j].positions[1].y, stickers[j].positions[1].z); 
            //Debug.Log("temp2 -> x: " + temp2.transform.position.x + "y: " + temp2.transform.position.y + " z: " + temp2.transform.position.z);
            GameObject temp3 = Instantiate(dot, placeHolderParent.transform);
            temp3.transform.position = stickers[j].positions[2];
            //Debug.Log("temp3 -> x: " + temp3.transform.position.x + "y: " + temp3.transform.position.y + " z: " + temp3.transform.position.z);
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
            //fix the reversed video checking the plane orientation
            bool isReversedVideoSticker = checkVideoOrientation(temp1.transform.position, temp2.transform.position, temp3.transform.position);
            //Debug.Log("isReversedVideoSticker: " + isReversedVideoSticker);
            if (isReversedVideoSticker)
            {
                vp.transform.localEulerAngles = new Vector3(0, vp.transform.localEulerAngles.y + 180, 0);
            }
            videoDemos.Add(vp);
            Destroy(temp1);
            Destroy(temp2);
            Destroy(temp3);


            bool isVideoSticker =
                stickers[j].sPath != null &&
                stickers[j].sPath.Contains(".mp4");

            bool is3dModel = !isVideoSticker &&
                (stickers[j].type.ToLower().Contains("3d") ||   // new 3d object format
                    stickers[j].sSubType.Contains("3dobject") ||   // old 3d object format
                    (stickers[j].sPath != null &&
                    stickers[j].sPath.Contains("3dobject"))       // oldest 3d object format
                );

            bool is3dModelTransfer =
                stickers[j].sDescription.ToLower().Contains("transfer") ||
                stickers[j].subType.ToLower().Contains("transfer");

            // NGI:
            bool isSpatialContentRecord = (stickers[j].spatialContentRecord != null);
            if (isSpatialContentRecord)
            {
                // TODO: move the whole SCR placement out from this method, separate OSCP and AC objects!
                Console.WriteLine("--- This is a SpatialContentRecord ---");
                Console.WriteLine(stickers[j].spatialContentRecord.content.title);

                GameObject model = Instantiate(GetComponent<ModelManager>().ABloaderNGI, placeHolderParent.transform);
                model.AddComponent<SCRItemTag>();
                model.GetComponent<SCRItemTag>().itemID = stickers[j].spatialContentRecord.id;

                Console.WriteLine("refs: " +  stickers[j].spatialContentRecord.content.refs.Count);
                // TODO: AC objects do not have the refs field filled, but use "url" instead
                // we handle this here but they should fix it on their server
                
                if (stickers[j].spatialContentRecord.content.refs.Count > 0) {
                    if (string.Equals(stickers[j].spatialContentRecord.content.refs[0]["contentType"], "assetbundle"))  //(stickers[j].spatialContentRecord.content.refs[0].ContainsKey("assetbundle"))
                    {
                        //TODO: Fix so it supports more than one refs entry
                        Console.WriteLine("This is an assetbundle");
                        string assetbundleName = "noAsset";
                        string assetbundleUrl = stickers[j].spatialContentRecord.content.refs[0]["url"];
                        for (int i = 0; i < stickers[j].spatialContentRecord.content.definitions.Count; i++)
                        {
                            if (string.Equals(stickers[j].spatialContentRecord.content.definitions[0]["type"], "assetbundleName"))
                            {
                                assetbundleName = stickers[j].spatialContentRecord.content.definitions[0]["value"];
                            }
                        }

                        Console.WriteLine("Assetbundle name is: " + assetbundleName);
                        model.GetComponent<AssetLoaderNGI>().ABName = assetbundleName.ToLower();
                        model.GetComponent<AssetLoaderNGI>().customUrl = assetbundleUrl.ToLower();
                    }
                    else // 3D model
                    {  
                        model.GetComponent<AssetLoaderNGI>().enabled = false;
                        var gltf = model.AddComponent<GLTFast.GltfAsset>();
                        gltf.url = stickers[j].spatialContentRecord.content.refs[0]["url"];
                    }
                } else if (!string.IsNullOrEmpty(stickers[j].sPath)) { // stickers[j].spatialContentRecord.content.refs.Count == 0
                    // TODO: ideally this should not happen, but AC objects do not have their 'refs' filled (Oct 2022)
                    Debug.Log("Sticker #" + j + " had no Refs! Trying to use URL field: " + stickers[j].sPath);
                    model.GetComponent<AssetLoaderNGI>().enabled = false;
                    var gltf = model.AddComponent<GLTFast.GltfAsset>();
                    gltf.url = stickers[j].sPath;
                } else {
                    // no refs and no url, ignore this object
                    Debug.Log("Sticker #" + j + " had no Refs and no URL!");
                    continue;
                }


                model.transform.localPosition = stickers[j].mainPositions; // * acapi.tempScale3d;
                model.transform.localRotation = new Quaternion(
                    stickers[j].orientations.x,
                    stickers[j].orientations.y,
                    stickers[j].orientations.z,
                    stickers[j].orientations.w);

                if (stickers[j].sTrajectoryPath.Length > 1)
                {
                    Trajectory tr = model.GetComponent<Trajectory>();
                    tr.go = true;
                    tr.acapi = acapi;
                    tr.sTrajectory = stickers[j].sTrajectoryPath;
                    tr.sTimePeriod = stickers[j].sTrajectoryPeriod;
                    tr.sOffset = stickers[j].sTrajectoryOffset;
                }

                //Debug.Log(stickers[j].sTrajectoryPath);

                //TODO: Fix: The mover scripts is always returning flying
                Mover mover = model.GetComponent<Mover>();
                mover.setLocked(true);
                mover.objectId = stickers[j].objectId;

                if (!stickers[j].vertical)
                {
                    //mover.noGravity = true;
                    mover.noGravity = false;
                }

                if (stickers[j].grounded)
                {
                    mover.landed = true;
                }

                /*
                Debug.Log(j + ". 3dmodel " + stickers[j].sText
                    + " = " + model.transform.localPosition
                    + " model.rot = " + model.transform.localRotation
                    + " stick.ori = " + stickers[j].orientations);
                */

                if (stickers[j].SModel_scale.Length > 0)
                {
                    float scale = float.Parse(stickers[j].SModel_scale);
                    model.transform.localScale = new Vector3(scale, scale, scale);
                }

                models.Add(model);                      // store the new just created model
            }
            else if (isVideoSticker)                         // if it's a video-sticker
            {
                GameObject urlVid = Instantiate(vp, placeHolderParent.transform);
                VideoPlayer vidos = urlVid.GetComponentInChildren<VideoPlayer>();
                vidos.source = VideoSource.Url;
                vidos.url = stickers[j].sPath;
                videoURLs.Add(urlVid);
            }
            else if (is3dModel || is3dModelTransfer)    // 3d object or special navi object
            {
                GameObject model = Instantiate(GetComponent<ModelManager>().ABloader, placeHolderParent.transform);
                string bundleName = stickers[j].sText.ToLower();
                if (stickers[j].type.ToLower().Contains("3d"))      // is it new format
                {
                    bundleName = stickers[j].bundleName.ToLower();
                    if (string.IsNullOrEmpty(bundleName))
                    {
                        bundleName = stickers[j].sText.ToLower();  // return back to default bundle name as the 'name'
                    }
                }
                model.GetComponent<AssetLoader>().ABName = bundleName;
                model.transform.localPosition = stickers[j].mainPositions; // * acapi.tempScale3d;
                model.transform.localRotation = new Quaternion(
                    stickers[j].orientations.x,
                    stickers[j].orientations.y,
                    stickers[j].orientations.z,
                    stickers[j].orientations.w);

                if (stickers[j].sTrajectoryPath.Length > 1)
                {
                    Trajectory tr = model.GetComponent<Trajectory>();
                    tr.go = true;
                    tr.acapi = acapi;
                    tr.sTrajectory = stickers[j].sTrajectoryPath;
                    tr.sTimePeriod = stickers[j].sTrajectoryPeriod;
                    tr.sOffset = stickers[j].sTrajectoryOffset;
                }

                Mover mover = model.GetComponent<Mover>();
                mover.setLocked(true);
                mover.objectId = stickers[j].objectId;

                if (!stickers[j].vertical ||
                    bundleName.Contains("nograv"))
                {
                    mover.noGravity = true;
                }

                if (stickers[j].grounded ||
                    bundleName.Contains("quar") ||
                    bundleName.Contains("santa") ||
                    bundleName.Contains("pavel") ||
                    bundleName.Contains("gard"))
                {
                    mover.landed = true;
                }


                /*Debug.Log(j + ". 3dmodel " + stickers[j].sText
                    + " = " + model.transform.localPosition
                    + " model.rot = " + model.transform.localRotation
                    + " stick.ori = " + stickers[j].orientations);*/

                if (stickers[j].SModel_scale.Length > 0)
                {
                    float scale = float.Parse(stickers[j].SModel_scale);
                    model.transform.localScale = new Vector3(scale, scale, scale);
                }

                models.Add(model);                      // store the new just created model
            }
            else                                        // other types of objects - info-stickers
            {
                GameObject newSticker = null;
                string checkType = stickers[j].sType.ToLower();
                if (checkType.Contains("food") || checkType.Contains("restaurant"))
                {
                    newSticker = Instantiate(stickerFood, placeHolderParent.transform);
                }
                else if (checkType.Contains("place"))
                {
                    newSticker = Instantiate(stickerPlace, placeHolderParent.transform);
                }
                else if (checkType.Contains("shop"))
                {
                    newSticker = Instantiate(stickerShop, placeHolderParent.transform);
                }
                else
                {
                    newSticker = Instantiate(stickerPref, placeHolderParent.transform);
                }
                if (newSticker != null)
                {
                    newSticker.transform.position = stickers[j].positions[0] - raznp;
                    StickerController sc = newSticker.GetComponent<StickerController>();
                    sc.setStickerInfo(stickers[j]);

                    stickerObjects.Add(newSticker);     // store the new just created info-sticker
                }

            }
        } // foreach stickers

        turnOffVideoDemos(videoDemosTurn);
        turnOffPlaceHolders(toShowPlaceHolders);
        turnOffStickers(toShowStickers);

        localizedImage.SetActive(true);
        placeHolderParent.transform.SetParent(scaleParent.transform);
        recos.Add(placeHolderParent);  // store processed scene into the cache
        uim.Located();

        relocationCompleted = true;
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

    void TranslationMover()
    {
        movingTransform.position = Vector3.Lerp(deltaTranslateVector, deltaRotateVector, (moveFrames - frameCounter) / moveFrames);
        movingTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, (moveFrames - frameCounter) / moveFrames);
        frameCounter--;
        if (frameCounter <= 0)
        {
            translateAction = false;
            relocationCompleted = true;
        }
    }

    void FixedUpdate()
    {
        if (translateAction)
        {
            TranslationMover();
        }
        if (ARStarted)
        {
            timerRelocation = timerRelocation - Time.fixedDeltaTime;
            if ((timerRelocation < 0) && relocationCompleted)
            {
                if (acapi.editorTestMode)
                {
                    startDevLocation();
                }
                else
                {
                    startLocalization();
                }
                timerRelocation = timeForRelocation;
            }
        }
    }

    public List<GameObject> GetAllStickers()
    {
        return stickerObjects;
    }

    void CantLocalize()
    {
        Debug.Log("Can't localize client or no stickers");
        ACityAPIDev.LocalizationStatus ls = acapi.getLocalizationStatus();
        /*if (recos.Count > 0) timerRelocation = timeForRelocation;
        else */
        timerRelocation = 1.1f;
        relocationCompleted = true;
    }

    public void SetRecoScale(float scale)
    {
        Transform llrt = checkSavedID(lastLocalizedRecoId).transform.root;
        if (llrt != null)
        {
            llrt.localScale = new Vector3(scale, scale, scale);
        }
    }

    GameObject checkSavedID(string id)  // extract from the cache the gameobject by 'id'
    {
        GameObject reco = null;
        foreach (GameObject go in recos)
        {
            if (go.name.Contains(id))
            {
                reco = go;
            }
            else
            {
                go.SetActive(false);
            }
        }
        return reco;
    }

    public bool GetRelocationState()
    {
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

    public void turnOffPlaceHolders(bool onOff)
    {
        toShowPlaceHolders = onOff;
        foreach (GameObject p in placeHoldersDotsLines)
        {
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


    public void setNewModelObjectId(string objectParams)
    {
        if (modelToServer != null)
        {
            modelToServer.GetComponent<Mover>().objectId = objectParams;
        }
    }

    public void set3dToLocal(string id, string name, Vector3 coords, Quaternion orientation)
    {
        PlayerPrefs.SetString(id, name);
        PlayerPrefs.SetFloat(id + "coordx", coords.x);
        PlayerPrefs.SetFloat(id + "coordy", coords.y);
        PlayerPrefs.SetFloat(id + "coordz", coords.z);
        PlayerPrefs.SetFloat(id + "orix", orientation.x);
        PlayerPrefs.SetFloat(id + "oriy", orientation.y);
        PlayerPrefs.SetFloat(id + "oriz", orientation.z);
        PlayerPrefs.SetFloat(id + "oriw", orientation.w);
        PlayerPrefs.Save();
        Debug.Log("saved pos = " + coords + ", ori = " + orientation);
    }


    public string getCurrentRecoId()
    {
        return lastLocalizedRecoId;
    }
}
