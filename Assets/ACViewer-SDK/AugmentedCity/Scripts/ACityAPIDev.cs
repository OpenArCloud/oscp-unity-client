using NGI.Api;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ACityAPIDev : MonoBehaviour
{
    public class UnityPose  // class to keep pose of the camera or objects for usage in Unity with left-handed system coords
    {
        public Vector3 pos;
        public Quaternion ori;


        public UnityPose(Vector3 acpos, Quaternion acori) // convert right-handed to left-handed, by redirecting axis Y
        {
            pos = GetPosition(acpos.x, acpos.y, acpos.z);
            ori = Quaternion.Euler(-acori.eulerAngles.x, acori.eulerAngles.y, -acori.eulerAngles.z);
        }

        public static Vector3 GetPosition(float posx, float posy, float posz)
        {
            return new Vector3(posx, -posy, posz); // change the Y axis direction
        }

        public Vector4 GetOrientation()
        {
            return new Vector4(ori.x, ori.y, ori.z, ori.w);
        }

        public void SetCameraOriFromGeoPose(GameObject cam)
        {
            cam.transform.RotateAround(cam.transform.position, cam.transform.right, 90); // rotation around the X-axis to lift the Y-axis up
            cam.transform.RotateAround(cam.transform.position, cam.transform.up, 90); // rotation around the Y-axis (it looks up) by 90 so that the camera is on the Z-axis instead of X
        }

        public Vector4 SetObjectOriFromGeoPose()
        {
            GameObject temp = new GameObject();
            temp.transform.localRotation = this.ori;
            temp.transform.RotateAround(temp.transform.position, temp.transform.right, 90); // rotation around the X-axis to lift the Y-axis up
            Vector4 newori = new Vector4(temp.transform.localRotation.x, temp.transform.localRotation.y, temp.transform.localRotation.z, temp.transform.localRotation.w);
            Destroy(temp);
            return newori;
        }
    }

    public class RecoInfo
    {
        public string id;
        public float scale3dcloud;
        public Vector3 lastCamCoordinate;
        public StickerInfo[] stickerArray;
        public EcefPose zeroCamEcefPose;
        public GeoPose zeroCamGeoPose;
    }

    public class StickerInfo
    {
        public Vector3 mainPositions;
        public Vector4 orientations;

        public Vector3[] positions;
        public string sDescription;
        public string sPath;
        public string sText;
        public string sType;
        public string sSubType;
        public string SModel_scale;
        public string sTrajectoryPath;
        public string sTrajectoryOffset;
        public string sTrajectoryPeriod;
        public string sId;
        public string objectId;
        public string sAddress;
        public string sFeedbackAmount;
        public string sRating;
        public string sUrl_ta;
        public string sImage;
        public string bundleName;
        public bool grounded;
        public bool vertical;
        public string type;
        public string subType;

        //Added for external unity assetbundle that is not hosted on augmented city servers.
        public string anchorName;
        public string externalAssetUrl;
        public SCRItem spatialServiceRecord;
    }

    public class EcefPose
    {
        public double x;
        public double y;
        public double z;
    }

    public class GeoPose
    {
        public double lat;
        public double lon;
        public double h;
    }


    public enum LocalizationStatus
    {
        NotStarted,
        GetGPSData,
        NoGPSData,
        WaitForAPIAnswer,
        ServerError,
        CantLocalize,
        Ready
    }

    public string defaultApiUrl = "https://developer.augmented.city";

    public bool editorTestMode;
    public string ServerAPI = "https://developer.augmented.city";
    public GameObject devButton;

    public TextAsset bb; // TODO: this was some fake image in base64 format in the past? It is missing now.
    public string rotationDevice = "90";

    Vector3 cameraRotationInLocalization;
    Vector3 cameraPositionInLocalization;
    float cameraDistance;

    public float tempScale3d;
    public float globalTimer;
    float serverTimer = 0;
    public bool useOSCP;
    public bool ecef;
    public bool useGeopose;
    UnityPose oldUPose;

    public bool debugSaveCameraImages = false;

    ScreenOrientation ori;

    const double a = 6378137; //I think this number is for earth ellipsoid for: GPS World_Geodetic_System:_WGS_84 https://en.wikipedia.org/wiki/Earth_ellipsoid
    const double b = 6356752.3142;
    const double f = (a - b) / a;
    const double e_sq = f * (2 - f);

    GameObject ARCamera;
    ARCameraManager m_CameraManager;
    bool startedLocalization; // TODO: isn't this redundant with localizationStatus? It seems unused anyway.
    bool configurationSetted;
    LocalizationStatus localizationStatus = LocalizationStatus.NotStarted;

    bool hasGpsLocation = false;
    // This is a modifiable version of UnityEngine.LocationInfo
    struct MyLocationInfo
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public float altitude { get; set; }
        public float horizontalAccuracy { get; set; }
        public float verticalAccuracy { get; set; }
        public double timestamp { get; set; }
    }
    MyLocationInfo lastGpsLocation;
    const int kH3Resolution = 8;
    H3Lib.H3Index lastH3Index = new H3Lib.H3Index(0);
    UInt64 geoposeRequestId = 0;

    bool GPSlocation;
    string apiURL;
    Action<string, Transform, StickerInfo[]> getStickersAction;
    List<RecoInfo> recoList = new List<RecoInfo>();


    UIManager uim;

    [SerializeField] private SCRManager spatialContentManager;
    public bool useOrbitContent;

    void Start()
    {
        if(spatialContentManager == null)
        {
            spatialContentManager = FindObjectOfType<SCRManager>();
        }

       // defaultApiUrl = OSCPDataHolder.Instance.GeoPoseServieURL;

        //PlayerPrefs.DeleteAll(); // NOTE: PlayerPrefs remain stored across sessions, which we don't want.
        // TODO: But it seems the camera settings must be stored across sessions, otherwise the app cannot retrieve images from the ARCore camera.
        // this is weird but when we deleted all settings, then the CamGetImage always returned null. Even though the camera background was working.
        // TODO: we should properly initialize the camera settings in every session and not rely on magical settings from a previous run.
        PlayerPrefs.DeleteKey("ApiUrl"); // only these need to get refreshed.
        PlayerPrefs.DeleteKey("LocLoaded");

        //UnityWebRequest.ClearCookieCache(); //FixMe: aco3d has it?  // TODO: ask AC about this line.
        // PlayerPrefs.DeleteAll();
        UnityWebRequest.ClearCookieCache(); //FixMe: ACV has commented this, why?
        globalTimer = -1;
        ARCamera = Camera.main.gameObject;
        m_CameraManager = Camera.main.GetComponent<ARCameraManager>();

        // NOTE: the ApiUrl player preference may have been already set bu some other script
        // indeed, it is usually set by the Settings Panel
        if (!PlayerPrefs.HasKey("ApiUrl"))
            setApiURL(defaultApiUrl);
        else
            setApiURL(PlayerPrefs.GetString("ApiUrl"));

#if PLATFORM_ANDROID || UNITY_IOS
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif

#if UNITY_EDITOR
        editorTestMode = true;
        devButton.SetActive(true);
        AudioListener.volume = 0.1f;  // app controls an audio
#else
    editorTestMode = false;
#endif
        StartCoroutine(GetTimerC());
        Input.location.Start();
        uim = this.GetComponent<UIManager>();
        NetworkReachability nr = Application.internetReachability;
        if (nr == NetworkReachability.NotReachable) uim.statusDebug("No internet");
        if (nr == NetworkReachability.ReachableViaCarrierDataNetwork) uim.statusDebug("Mobile internet");
        if (nr == NetworkReachability.ReachableViaLocalAreaNetwork) uim.statusDebug("Wifi");
    }


    public void SetOSCPusage(bool os)
    {
        useOSCP = os;
        uim.localizationMethodDebug(useOSCP, ecef, useGeopose);
    }
    public void SetECEFusage(bool os)
    {
        ecef = os;
        uim.localizationMethodDebug(useOSCP, ecef, useGeopose);
    }
    public void SetGEOusage(bool os)
    {
        useGeopose = os;
        uim.localizationMethodDebug(useOSCP, ecef, useGeopose);
    }

    void SetCameraConfiguration()
    {
        if (!PlayerPrefs.HasKey("config"))
        {
            uim.statusDebug("Set cam frame");
            int needConfigurationNumber = 0;
#if UNITY_IOS
            using (var configurations = m_CameraManager.GetConfigurations(Allocator.Temp))
            {
                Debug.Log("configurations.Length = " + configurations.Length);
                needConfigurationNumber = 0;  // iOS's list has high resolutions first coming, get first in fail case
                for (int i = 0; i < configurations.Length; i++)
                {
                    Debug.Log("Conf: h=" + configurations[i].height + " w=" + configurations[i].width + " fr:" + configurations[i].framerate);
                    if (configurations[i].height == 1080) { needConfigurationNumber = i; }  // store the minimal resolution with the required height
                }
                Debug.Log("Config number: " + needConfigurationNumber);
                // Get that configuration by index
                var configuration = configurations[needConfigurationNumber];
                // Make it the active one
                m_CameraManager.currentConfiguration = configuration;
            }
#endif
#if PLATFORM_ANDROID
            using (var configurations = m_CameraManager.GetConfigurations(Allocator.Temp))
            {
                Debug.Log("configurations.Length = " + configurations.Length);
                bool needConfFound = false;
                needConfigurationNumber = configurations.Length - 1;  // Android's list has high resolutions last coming, get last in fail case
                for (int i = 0; i < configurations.Length; i++)
                {
                    Debug.Log("Conf: h=" + configurations[i].height + " w=" + configurations[i].width + " fr=" + configurations[i].framerate);
                    if ((configurations[i].height == 1080) && (!needConfFound))
                    {  // detect first low resolution with the required height
                        needConfigurationNumber = i; needConfFound = true;
                    }
                }
                Debug.Log("Config number: " + needConfigurationNumber);
                // Get that configuration by index
                var configuration = configurations[needConfigurationNumber];
                // Make it the active one
                m_CameraManager.currentConfiguration = configuration;
            }
#endif
            PlayerPrefs.SetInt("config", needConfigurationNumber);
        }
        configurationSetted = true;
    }

    public unsafe byte[] CamGetFrame()
    {
        uim.statusDebug("Get cam frame");
        XRCpuImage image;

        if (!m_CameraManager.TryAcquireLatestCpuImage(out image))
        {
            Debug.Log($"{Time.realtimeSinceStartup} Could not acquire cpu image.");
            return null; // unsuccessful
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 1.
            outputDimensions = new Vector2Int(image.width, image.height),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data

        image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
        Debug.Log("buffer.Length = " + buffer.Length);
        // The image was converted to RGBA32 format and written into the provided buffer
        // so we can dispose of the CameraImage. We must do this or it will leak resources.
        image.Dispose();

        // At this point, we could process the image, pass it to a computer vision algorithm, etc.
        // In this example, we'll just apply it to a texture to visualize it.

        // We've got the data; let's put it into a texture so we can visualize it.
        Texture2D m_Texture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        m_Texture.LoadRawTextureData(buffer);
        m_Texture.Apply();
        buffer.Dispose();

        byte[] bb = m_Texture.EncodeToJPG(100);
        Destroy(m_Texture);
        return bb;
    }

    public void camLocalize(string jsonanswer, bool geopose)
    {
        Debug.Log("This is the string response from AC: " + jsonanswer);

        var jsonParse = JSON.Parse(jsonanswer);
        float px, py, pz, ox, oy, oz, ow;
        int objectsAmount = -1;
        string js, sessionId;

        if (!geopose)
        {
            if (jsonParse["camera"] != null)
            {
                sessionId = jsonParse["reconstruction_id"];
                // Debug.Log("sessioID: " + sessionId);
                do
                {
                    objectsAmount++;
                    js = jsonParse["placeholders"][objectsAmount]["placeholder_id"];
                    // Debug.Log("js node [" + objectsAmount + "]  - " + js);
                } while (js != null);

                Debug.Log("nodeAmount = " + objectsAmount + ", recoArray.Len = " + recoList.Count);

                px = jsonParse["camera"]["pose"]["position"]["x"].AsFloat;
                py = jsonParse["camera"]["pose"]["position"]["y"].AsFloat;
                pz = jsonParse["camera"]["pose"]["position"]["z"].AsFloat;
                ox = jsonParse["camera"]["pose"]["orientation"]["x"].AsFloat;
                oy = jsonParse["camera"]["pose"]["orientation"]["y"].AsFloat;
                oz = jsonParse["camera"]["pose"]["orientation"]["z"].AsFloat;
                ow = jsonParse["camera"]["pose"]["orientation"]["w"].AsFloat;
                uim.setDebugPose(px, py, pz, ox, oy, oz, ow, sessionId);

                GameObject newCam = new GameObject("tempCam");
                UnityPose uPose = new UnityPose(new Vector3(px, py, pz), new Quaternion(ox, oy, oz, ow));
                newCam.transform.localPosition = uPose.pos;
                newCam.transform.localRotation = uPose.ori;

                GameObject zeroCoord = new GameObject("Zero");
                zeroCoord.transform.SetParent(newCam.transform);
                RecoInfo currentRi = checkRecoID(sessionId);
                StickerInfo[] stickers;
                stickers = null;
                if (currentRi == null)
                {
                    currentRi = new RecoInfo();
                    currentRi.id = sessionId;
                    if (objectsAmount > 0)
                    {
                        currentRi.scale3dcloud = tempScale3d;
                        stickers = new StickerInfo[objectsAmount];
                        currentRi.stickerArray = new StickerInfo[objectsAmount];
                        GameObject[,] placeHolders = new GameObject[objectsAmount, 4];

                        for (int j = 0; j < objectsAmount; j++)
                        {
                            currentRi.stickerArray[j] = new StickerInfo();
                            stickers[j] = new StickerInfo();
                            currentRi.stickerArray[j].positions = new Vector3[4];
                            px = jsonParse["placeholders"][j]["pose"]["position"]["x"].AsFloat;
                            py = jsonParse["placeholders"][j]["pose"]["position"]["y"].AsFloat;
                            pz = jsonParse["placeholders"][j]["pose"]["position"]["z"].AsFloat;
                            ox = jsonParse["placeholders"][j]["pose"]["orientation"]["x"].AsFloat;
                            oy = jsonParse["placeholders"][j]["pose"]["orientation"]["y"].AsFloat;
                            oz = jsonParse["placeholders"][j]["pose"]["orientation"]["z"].AsFloat;
                            ow = jsonParse["placeholders"][j]["pose"]["orientation"]["w"].AsFloat;

                            uPose = new UnityPose(new Vector3(px, py, pz), new Quaternion(ox, oy, oz, ow));
                            currentRi.stickerArray[j].mainPositions = uPose.pos;
                            currentRi.stickerArray[j].orientations = uPose.GetOrientation();

                            stickers[j].mainPositions = currentRi.stickerArray[j].mainPositions;
                            stickers[j].orientations = currentRi.stickerArray[j].orientations;

                            for (int i = 0; i < 4; i++)
                            {
                                float pxf = jsonParse["placeholders"][j]["frame"][i]["x"].AsFloat + px;
                                float pyf = jsonParse["placeholders"][j]["frame"][i]["y"].AsFloat + py;
                                float pzf = jsonParse["placeholders"][j]["frame"][i]["z"].AsFloat + pz;
                                placeHolders[j, i] = new GameObject("Placeholder" + j + " " + i);
                                placeHolders[j, i].transform.SetParent(newCam.transform);
                                placeHolders[j, i].transform.position = UnityPose.GetPosition(pxf, pyf, pzf);
                                currentRi.stickerArray[j].positions[i] = UnityPose.GetPosition(pxf, pyf, pzf);
                            }

                            string idnode = "" + jsonParse["placeholders"][j]["placeholder_id"];

                            for (int x = 0; x < objectsAmount; x++)
                            {
                                string idobj = "" + jsonParse["objects"][x]["sticker"]["sticker_id"];

                                if (idobj.Contains(idnode))
                                {
                                    stickers[j].sPath = "" + jsonParse["objects"][x]["sticker"]["path"];
                                    stickers[j].sText = "" + jsonParse["objects"][x]["sticker"]["sticker_text"];
                                    stickers[j].sType = "" + jsonParse["objects"][x]["sticker"]["sticker_type"];
                                    stickers[j].sSubType = "" + jsonParse["objects"][x]["sticker"]["sticker_subtype"];
                                    stickers[j].sDescription = "" + jsonParse["objects"][x]["sticker"]["description"];
                                    stickers[j].SModel_scale = "" + jsonParse["objects"][x]["sticker"]["model_scale"];
                                    stickers[j].sId = "" + jsonParse["objects"][x]["sticker"]["sticker_id"];
                                    stickers[j].objectId = "" + jsonParse["objects"][x]["placeholder"]["placeholder_id"];
                                    stickers[j].sImage = "" + jsonParse["objects"][x]["sticker"]["Image"];
                                    stickers[j].sAddress = "" + jsonParse["objects"][x]["sticker"]["Address"];
                                    stickers[j].sFeedbackAmount = "" + jsonParse["objects"][x]["sticker"]["Feedback amount"];
                                    stickers[j].sRating = "" + jsonParse["objects"][x]["sticker"]["Rating"];
                                    stickers[j].sUrl_ta = "" + jsonParse["objects"][x]["sticker"]["url_ta"];
                                    stickers[j].sTrajectoryPath = "" + jsonParse["objects"][x]["sticker"]["trajectory_path"];
                                    stickers[j].sTrajectoryOffset = "" + jsonParse["objects"][x]["sticker"]["trajectory_time_offset"];
                                    stickers[j].sTrajectoryPeriod = "" + jsonParse["objects"][x]["sticker"]["trajectory_time_period"];
                                    stickers[j].subType = "" + jsonParse["objects"][x]["sticker"]["subtype"];
                                    stickers[j].type = "" + jsonParse["objects"][x]["sticker"]["type"];
                                    stickers[j].bundleName = "" + jsonParse["objects"][x]["sticker"]["model_id"];
                                    stickers[j].anchorName = "" + jsonParse["objects"][x]["sticker"]["anchor"];
                                    stickers[j].externalAssetUrl = "" + jsonParse["objects"][x]["sticker"]["asseturl"];
                                    if (string.IsNullOrEmpty(stickers[j].bundleName))
                                    {
                                        stickers[j].bundleName = "" + jsonParse["objects"][x]["sticker"]["bundle_name"];
                                    }
                                    string groundeds = jsonParse["objects"][x]["sticker"]["grounded"];
                                    string verticals = jsonParse["objects"][x]["sticker"]["vertically_aligned"];
                                    if (groundeds != null)
                                    {
                                        if (groundeds.Contains("1")) { stickers[j].grounded = true; }
                                    }
                                    if (verticals != null)
                                    {
                                        if (verticals.Contains("1")) { stickers[j].vertical = true; }
                                    }

                                    currentRi.stickerArray[j].sPath = stickers[j].sPath;
                                    currentRi.stickerArray[j].sText = stickers[j].sText;
                                    currentRi.stickerArray[j].sType = stickers[j].sType;
                                    currentRi.stickerArray[j].sSubType = stickers[j].sSubType;
                                    currentRi.stickerArray[j].sDescription = stickers[j].sDescription;
                                    currentRi.stickerArray[j].SModel_scale = stickers[j].SModel_scale;
                                    currentRi.stickerArray[j].sId = stickers[j].sId;
                                    currentRi.stickerArray[j].sImage = stickers[j].sImage;
                                    currentRi.stickerArray[j].sAddress = stickers[j].sAddress;
                                    currentRi.stickerArray[j].sRating = stickers[j].sRating;
                                    currentRi.stickerArray[j].sUrl_ta = stickers[j].sUrl_ta;
                                    currentRi.stickerArray[j].sTrajectoryPath = stickers[j].sTrajectoryPath;
                                    currentRi.stickerArray[j].sTrajectoryOffset = stickers[j].sTrajectoryOffset;
                                    currentRi.stickerArray[j].sTrajectoryPeriod = stickers[j].sTrajectoryPeriod;
                                    currentRi.stickerArray[j].grounded = stickers[j].grounded;
                                    currentRi.stickerArray[j].vertical = stickers[j].vertical;
                                    currentRi.stickerArray[j].subType = stickers[j].subType;
                                    currentRi.stickerArray[j].type = stickers[j].type;
                                    currentRi.stickerArray[j].bundleName = stickers[j].bundleName;
                                    currentRi.stickerArray[j].anchorName = stickers[j].anchorName;
                                    currentRi.stickerArray[j].externalAssetUrl = stickers[j].externalAssetUrl;

                                }
                            }
                        }
                        recoList.Add(currentRi);
                        newCam.transform.position = cameraPositionInLocalization;
                        newCam.transform.eulerAngles = cameraRotationInLocalization;

                        for (int j = 0; j < objectsAmount; j++)
                        {
                            stickers[j].positions = new Vector3[4];
                            for (int i = 0; i < 4; i++)
                            {
                                stickers[j].positions[i] = placeHolders[j, i].transform.position;
                            }
                        }
                    }
                }
                else if (currentRi != null)
                {
                    cameraDistance = Vector3.Magnitude(currentRi.lastCamCoordinate - new Vector3(px, py, pz));
                    tempScale3d = currentRi.scale3dcloud;
                    int savedNodeLentgh = currentRi.stickerArray.Length;
                    stickers = new StickerInfo[savedNodeLentgh];
                    GameObject[,] placeHolders = new GameObject[savedNodeLentgh, 4];

                    for (int j = 0; j < savedNodeLentgh; j++)
                    {
                        stickers[j] = new StickerInfo();
                        for (int i = 0; i < 4; i++)
                        {
                            placeHolders[j, i] = new GameObject("Placeholder" + j + " " + i);
                            placeHolders[j, i].transform.SetParent(newCam.transform);
                            placeHolders[j, i].transform.position = currentRi.stickerArray[j].positions[i];
                        }
                        stickers[j].sPath = currentRi.stickerArray[j].sPath;
                        stickers[j].sText = currentRi.stickerArray[j].sText;
                        stickers[j].sType = currentRi.stickerArray[j].sType;
                        stickers[j].sSubType = currentRi.stickerArray[j].sSubType;
                        stickers[j].sDescription = currentRi.stickerArray[j].sDescription;
                        stickers[j].sId = currentRi.stickerArray[j].sId;
                        stickers[j].sImage = currentRi.stickerArray[j].sImage;

                        currentRi.stickerArray[j].sAddress = stickers[j].sAddress;
                        currentRi.stickerArray[j].sRating = stickers[j].sRating;
                        currentRi.stickerArray[j].sUrl_ta = stickers[j].sUrl_ta;
                    }
                    newCam.transform.position = cameraPositionInLocalization;
                    newCam.transform.eulerAngles = cameraRotationInLocalization;

                    for (int j = 0; j < savedNodeLentgh; j++)
                    {
                        stickers[j].positions = new Vector3[4];
                        for (int i = 0; i < 4; i++)
                        {
                            stickers[j].positions[i] = placeHolders[j, i].transform.position;
                        }
                    }
                }

                if (zeroCoord.transform.eulerAngles == Vector3.zero)
                {
                    newCam.transform.position = cameraPositionInLocalization;
                    newCam.transform.eulerAngles = cameraRotationInLocalization;
                }
                currentRi.lastCamCoordinate = new Vector3(px, py, pz);
                localizationStatus = LocalizationStatus.Ready;
                uim.statusDebug("Localized");
                getStickersAction(currentRi.id, zeroCoord.transform, stickers);
                Destroy(newCam);
            }
            else
            {
                Debug.Log("Can't localize");
                uim.statusDebug("Can't localize");

                localizationStatus = LocalizationStatus.CantLocalize;
                uim.setDebugPose(0, 0, 0, 0, 0, 0, 0, "cant loc");
                getStickersAction(null, null, null);
            }
        }
        else // (geopose)
        {
            if (jsonParse["geopose"] != null)
            {

                // Spatial Content Records (optional)
                sessionId = "1"; // jsonParse["geopose"]["reconstruction_id"];  // don't change the session as geopose SC is global
                string debugSessionId = jsonParse["geopose"]["reconstruction_id"];
                Debug.Log("sessioID: " + debugSessionId);
                do
                {
                    objectsAmount++;
                    js = jsonParse["scrs"][objectsAmount]["type"];
                    Debug.Log("js node [" + objectsAmount + "] - " + js);
                } while (js != null);
                Debug.Log("nodeAmount = " + objectsAmount + ", recoArray.Len = " + recoList.Count);

                double camLat = 0, camLon = 0, camHei = 0;
                double px0 = 0, py0 = 0, pz0 = 0;
                px = 0; py = 0; pz = 0; // reset position initially
                EcefPose zeroEcefCam = new EcefPose();
                GeoPose  zeroGeoCam  = new GeoPose();
                bool newGeoPose = false;  // flag whether we use geopose standard 1.0 with fields lat/lon/h at the position section

                RecoInfo currentRi = checkRecoID(sessionId);

                if (currentRi != null)
                {
                    zeroEcefCam = currentRi.zeroCamEcefPose;
                    zeroGeoCam = currentRi.zeroCamGeoPose;
                }

                if (ecef)
                {
                    px0 = jsonParse["geopose"]["ecefPose"]["position"]["x"].AsDouble;
                    py0 = jsonParse["geopose"]["ecefPose"]["position"]["y"].AsDouble;
                    pz0 = jsonParse["geopose"]["ecefPose"]["position"]["z"].AsDouble;
                    ox = jsonParse["geopose"]["ecefPose"]["quaternion"]["x"].AsFloat;
                    oy = jsonParse["geopose"]["ecefPose"]["quaternion"]["y"].AsFloat;
                    oz = jsonParse["geopose"]["ecefPose"]["quaternion"]["z"].AsFloat;
                    ow = jsonParse["geopose"]["ecefPose"]["quaternion"]["w"].AsFloat;
                    if (currentRi == null)
                    {
                        zeroEcefCam.x = px0;
                        zeroEcefCam.y = py0;
                        zeroEcefCam.z = pz0;
                        uim.setDebugPose(0.001f, py, pz, ox, oy, oz, ow, debugSessionId);
                    }
                    else
                    {
                        px = (float)(px0 - zeroEcefCam.x);
                        py = (float)(py0 - zeroEcefCam.y);
                        pz = (float)(pz0 - zeroEcefCam.z);
                        uim.setDebugPose(px, py, pz, ox, oy, oz, ow, debugSessionId);
                    }
                    //Debug.Log("ecef.quat = " + ox + "--" + oy + "--" + oz + "--" + ow);
                }
                else if (useGeopose)
                {

                    UInt64 timestamp = 0;
                    if (jsonParse.HasKey("timestamp")) {
                        timestamp = UInt64.Parse(jsonParse["timestamp"]);
                        Debug.Log("  timestamp:" + timestamp);
                    }
                    UInt64 id = 0;
                    if (jsonParse.HasKey("id")) {
                        id = UInt64.Parse(jsonParse["id"]);
                        Debug.Log("  id:" + id);
                    }
                    double positionAccuracy = 0.0;
                    double orientationAccuracy = 0.0;
                    if (jsonParse.HasKey("accuracy")) {
                        JSONNode jsonAccuracy = jsonParse["accuracy"];
                        positionAccuracy = jsonAccuracy["position"].AsDouble;
                        orientationAccuracy = jsonAccuracy["orientation"].AsDouble;
                        Debug.Log("  positionAccuracy:" + positionAccuracy);
                        Debug.Log("  orientationAccuracy:" + orientationAccuracy);
                    }
                    string type = "";
                    if (jsonParse.HasKey("type"))
                    {
                        type = jsonParse["type"].ToString();
                        Debug.Log("  type:" + type);
                    }

                    string checkNewGeo = jsonParse["geopose"]["position"]["lat"];
                    if (!string.IsNullOrEmpty(checkNewGeo)) { newGeoPose = true; }

                    if (newGeoPose)
                    {
                        camLat = jsonParse["geopose"]["position"]["lat"].AsDouble;
                        camLon = jsonParse["geopose"]["position"]["lon"].AsDouble;
                        camHei = jsonParse["geopose"]["position"]["h"].AsDouble;
                        Debug.Log("Cam GEO_v10 - lat = " + camLat + ", lon = " + camLon + ", h = " + camHei);

                        ox = jsonParse["geopose"]["quaternion"]["x"].AsFloat;
                        oy = jsonParse["geopose"]["quaternion"]["y"].AsFloat;
                        oz = jsonParse["geopose"]["quaternion"]["z"].AsFloat;
                        ow = jsonParse["geopose"]["quaternion"]["w"].AsFloat;
                    }
                    else
                    {
                        camLat = jsonParse["geopose"]["pose"]["latitude"].AsDouble;
                        camLon = jsonParse["geopose"]["pose"]["longitude"].AsDouble;
                        camHei = jsonParse["geopose"]["pose"]["ellipsoidHeight"].AsDouble;
                        Debug.Log("Cam GEO_v01 - lat = " + camLat + ", lon = " + camLon + ", ellH = " + camHei);

                        ox = jsonParse["geopose"]["pose"]["quaternion"]["x"].AsFloat;
                        oy = jsonParse["geopose"]["pose"]["quaternion"]["y"].AsFloat;
                        oz = jsonParse["geopose"]["pose"]["quaternion"]["z"].AsFloat;
                        ow = jsonParse["geopose"]["pose"]["quaternion"]["w"].AsFloat;
                    }

                    if (currentRi == null)
                    {
                        zeroGeoCam.lat = camLat;
                        zeroGeoCam.lon = camLon;
                        zeroGeoCam.h = camHei;
                    }
                    else
                    {
                        zeroGeoCam = currentRi.zeroCamGeoPose;
                    }
                    Vector3 enupose = EcefToEnu(GeodeticToEcef(camLat, camLon, camHei), zeroGeoCam.lat, zeroGeoCam.lon, zeroGeoCam.h);
                    OSCPDataHolder.Instance.lastPositon = enupose;
                    Debug.Log("Cam GEO enupose x = " + enupose.x + ", y = " + enupose.y + ", z = " + enupose.z);

                    px = enupose.x;
                    py = enupose.y;
                    pz = enupose.z;
                    Debug.Log("geo.quat = " + ox + "--" + oy + "--" + oz + "--" + ow);
                    if (currentRi == null)
                        uim.setDebugPose(0.001f, py, pz, ox, oy, oz, ow, debugSessionId);
                    else
                        uim.setDebugPose(px, py, pz, ox, oy, oz, ow, debugSessionId);
                }
                else // local pose (AC)
                {
                    px = jsonParse["geopose"]["localPose"]["position"]["x"].AsFloat;
                    py = jsonParse["geopose"]["localPose"]["position"]["y"].AsFloat;
                    pz = jsonParse["geopose"]["localPose"]["position"]["z"].AsFloat;
                    ox = jsonParse["geopose"]["localPose"]["orientation"]["x"].AsFloat;
                    oy = jsonParse["geopose"]["localPose"]["orientation"]["y"].AsFloat;
                    oz = jsonParse["geopose"]["localPose"]["orientation"]["z"].AsFloat;
                    ow = jsonParse["geopose"]["localPose"]["orientation"]["w"].AsFloat;
                    uim.setDebugPose(px, py, pz, ox, oy, oz, ow, debugSessionId);
                }

                GameObject newCam = new GameObject("tempCam");
                UnityPose uPose = new UnityPose(new Vector3(px, py, pz), new Quaternion(ox, oy, oz, ow));
                if (oldUPose != null) 
                {
                    Debug.Log("DbGL: " + (uPose.pos - oldUPose.pos).magnitude);
                }
                oldUPose = uPose;
                newCam.transform.localPosition = uPose.pos;
                newCam.transform.localRotation = uPose.ori;

                if (ecef)                                   // ecef pose
                {
                    uPose.SetCameraOriFromGeoPose(newCam);  // Add additional 2 rotations for camera
                }
                else if (useGeopose)                        // geopose based on ENU
                {
                    uPose.SetCameraOriFromGeoPose(newCam);  // Add additional 2 rotations for camera
                }
                Debug.Log("newCam.transform.locPos pos= "
                    + newCam.transform.localPosition.x + ", "
                    + newCam.transform.localPosition.y + ", "
                    + newCam.transform.localPosition.z);
                Debug.Log("newCam.transform.locRot ang= " + newCam.transform.localRotation.eulerAngles);


                OSCPDataHolder.Instance.UpdateCoordinates(camLat, camLon, camHei);
                OSCPDataHolder.Instance.UpdateLocation(newCam.transform.position, newCam.transform.rotation);


                GameObject zeroCoord = new GameObject("Zero");
                zeroCoord.transform.SetParent(newCam.transform);
                StickerInfo[] stickers;
                stickers = null;


                if (currentRi == null)
                {
                    currentRi = new RecoInfo();
                    currentRi.id = sessionId;
                    currentRi.zeroCamEcefPose = zeroEcefCam;
                    currentRi.zeroCamGeoPose = zeroGeoCam;

                    //Add connection to oscp-spatial-content-discovery

                    //To DO
                    //Translate SpatialRecords to StickerInfo
                    //Use already created flow

                    if (useOrbitContent)
                    {
                        Debug.Log("Number of objects from orbit: " + objectsAmount);

                        objectsAmount = spatialContentManager.spatialServiceRecord.Length;

                        
                        if (objectsAmount > 0)
                        {
                            currentRi.scale3dcloud = tempScale3d;
                            stickers = new StickerInfo[objectsAmount];
                            currentRi.stickerArray = new StickerInfo[objectsAmount];
                            GameObject[,] placeHolders = new GameObject[objectsAmount, 4];
                            for (int j = 0; j < objectsAmount; j++)
                            {
                                currentRi.stickerArray[j] = new StickerInfo();
                                stickers[j] = new StickerInfo();
                                currentRi.stickerArray[j].positions = new Vector3[4];

                                // Object's position using GeoPose

                                double tlat, tlon, thei;
                                tlat = spatialContentManager.spatialServiceRecord[j].content.geopose.latitude;
                                tlon = spatialContentManager.spatialServiceRecord[j].content.geopose.longitude;
                                thei = spatialContentManager.spatialServiceRecord[j].content.geopose.ellipsoidHeight;
                                // calc the object position relatively the recently localized camera
                                EcefPose epobj = GeodeticToEcef(tlat, tlon, thei);
                                Vector3 enupose = EcefToEnu(epobj, camLat, camLon, camHei);
                                px = enupose.x;
                                py = enupose.y;
                                pz = enupose.z;
                                ox = spatialContentManager.spatialServiceRecord[j].content.geopose.quaternion["x"];
                                oy = spatialContentManager.spatialServiceRecord[j].content.geopose.quaternion["y"];
                                oz = spatialContentManager.spatialServiceRecord[j].content.geopose.quaternion["z"];
                                ow = spatialContentManager.spatialServiceRecord[j].content.geopose.quaternion["w"];

                                //TODO: Remove this check from client, server should only return visible objects
                                //I think this means within +- 100M distance
                                double latMin = camLat - 0.001;
                                double latMax = camLat + 0.001;
                                double lonMin = camLon - 0.001;
                                double lonMax = camLon + 0.001;

                                if (!(tlat > latMin && tlat < latMax && tlon > lonMin && tlon < lonMax))
                                {
                                    spatialContentManager.spatialServiceRecord[j].isToFarAway = true;
                                }


                                //Debug.Log("scr.ecef.quat = oxo:" + ox + "--" + oy + "--" + oz + "--" + ow);

                                uPose = new UnityPose(new Vector3(px, py, pz), new Quaternion(ox, oy, oz, ow));
                                currentRi.stickerArray[j].mainPositions = uPose.pos;

                                // Update object orientation depends on the system coords
                                currentRi.stickerArray[j].orientations = uPose.SetObjectOriFromGeoPose();

                                stickers[j].mainPositions = currentRi.stickerArray[j].mainPositions;
                                stickers[j].orientations = currentRi.stickerArray[j].orientations;

                                /* Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations x" + currentRi.stickerArray[j].orientations.x + "   " + stickers[j].orientations.x);
                                   Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations y" + currentRi.stickerArray[j].orientations.y + "   " + stickers[j].orientations.y);
                                   Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations z" + currentRi.stickerArray[j].orientations.z + "   " + stickers[j].orientations.z);
                                   Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations w" + currentRi.stickerArray[j].orientations.w + "   " + stickers[j].orientations.w);
                                */

                                for (int i = 0; i < 4; i++)
                                {
                                    float pxf = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["x"].AsFloat + px;
                                    float pyf = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["y"].AsFloat + py;
                                    float pzf = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["z"].AsFloat + pz;
                                    placeHolders[j, i] = new GameObject("Placeholder" + j + " " + i);
                                    placeHolders[j, i].transform.SetParent(newCam.transform);
                                    placeHolders[j, i].transform.position = UnityPose.GetPosition(pxf, pyf, pzf);
                                    currentRi.stickerArray[j].positions[i] = UnityPose.GetPosition(pxf, pyf, pzf);
                                }

                                stickers[j].sPath = ""; //Add path to SpatialRecord
                                stickers[j].sText = "" + spatialContentManager.spatialServiceRecord[j].content.title;
                                stickers[j].sType = "" + spatialContentManager.spatialServiceRecord[j].content.type;
                                stickers[j].sSubType = "" + spatialContentManager.spatialServiceRecord[j].content.type; //Atm not using subtype
                                stickers[j].sDescription = "" + spatialContentManager.spatialServiceRecord[j].content.description;
                                stickers[j].SModel_scale = "" + spatialContentManager.spatialServiceRecord[j].content.size; //is size correct variable for scale?
                                stickers[j].sId = "" + spatialContentManager.spatialServiceRecord[j].content.id;
                                stickers[j].objectId = "" + spatialContentManager.spatialServiceRecord[j].id;
                                stickers[j].sImage = ""; //image not implemented on server side
                                stickers[j].sAddress = ""; //not implemented
                                stickers[j].sFeedbackAmount = ""; //Not implemented
                                stickers[j].sRating = ""; //+ jsonParse["scrs"][j]["content"]["custom_data"]["Rating"];
                                stickers[j].sUrl_ta = ""; //Dont know what it is used for
                                stickers[j].sTrajectoryPath = ""; // Not implemented on serverside
                                stickers[j].sTrajectoryOffset = ""; // Not implemented
                                stickers[j].sTrajectoryPeriod = ""; // Not implemented
                                stickers[j].subType = ""; // Not implemented
                                stickers[j].type = "" + spatialContentManager.spatialServiceRecord[j].type;
                                stickers[j].bundleName = ""; // Not used
                                stickers[j].anchorName = ""; //+ jsonParse["srcs"][j]["content"]["custom_data"]["anchor"];
                                stickers[j].externalAssetUrl = "";// + jsonParse["srcs"][j]["content"]["custom_data"]["externalAssetUrl"];
                                if (string.IsNullOrEmpty(stickers[j].bundleName))
                                {
                                    stickers[j].bundleName = "";// + jsonParse["scrs"][j]["content"]["custom_data"]["bundle_name"];
                                }
                                string groundeds = "1";//jsonParse["scrs"][j]["content"]["custom_data"]["grounded"];
                                string verticals = "";//jsonParse["scrs"][j]["content"]["custom_data"]["vertically_aligned"];
                                if (groundeds != null)
                                {
                                    if (groundeds.Contains("1")) { stickers[j].grounded = true; }
                                }
                                if (verticals != null)
                                {
                                    if (verticals.Contains("1")) { stickers[j].vertical = true; }
                                }

                                stickers[j].spatialServiceRecord = spatialContentManager.spatialServiceRecord[j];

                                currentRi.stickerArray[j].sPath = stickers[j].sPath;
                                currentRi.stickerArray[j].sText = stickers[j].sText;
                                currentRi.stickerArray[j].sType = stickers[j].sType;
                                currentRi.stickerArray[j].sSubType = stickers[j].sSubType;
                                currentRi.stickerArray[j].sDescription = stickers[j].sDescription;
                                currentRi.stickerArray[j].SModel_scale = stickers[j].SModel_scale;
                                currentRi.stickerArray[j].sId = stickers[j].sId;
                                currentRi.stickerArray[j].sImage = stickers[j].sImage;
                                currentRi.stickerArray[j].sAddress = stickers[j].sAddress;
                                currentRi.stickerArray[j].sRating = stickers[j].sRating;
                                currentRi.stickerArray[j].sUrl_ta = stickers[j].sUrl_ta;
                                currentRi.stickerArray[j].sTrajectoryPath = stickers[j].sTrajectoryPath;
                                currentRi.stickerArray[j].sTrajectoryOffset = stickers[j].sTrajectoryOffset;
                                currentRi.stickerArray[j].sTrajectoryPeriod = stickers[j].sTrajectoryPeriod;
                                currentRi.stickerArray[j].grounded = stickers[j].grounded;
                                currentRi.stickerArray[j].vertical = stickers[j].vertical;
                                currentRi.stickerArray[j].subType = stickers[j].subType;
                                currentRi.stickerArray[j].type = stickers[j].type;
                                currentRi.stickerArray[j].bundleName = stickers[j].bundleName;
                                currentRi.stickerArray[j].anchorName = stickers[j].anchorName;
                                currentRi.stickerArray[j].externalAssetUrl = stickers[j].externalAssetUrl;

                            }

                            recoList.Add(currentRi);
                            newCam.transform.position = cameraPositionInLocalization;
                            newCam.transform.eulerAngles = cameraRotationInLocalization;
                            for (int j = 0; j < objectsAmount; j++)
                            {
                                stickers[j].positions = new Vector3[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    stickers[j].positions[i] = placeHolders[j, i].transform.position;
                                }
                            }
                        }

                    }
                    else
                    {
                        if (objectsAmount > 0)
                        {
                            currentRi.scale3dcloud = tempScale3d;
                            stickers = new StickerInfo[objectsAmount];
                            currentRi.stickerArray = new StickerInfo[objectsAmount];
                            GameObject[,] placeHolders = new GameObject[objectsAmount, 4];
                            for (int j = 0; j < objectsAmount; j++)
                            {
                                currentRi.stickerArray[j] = new StickerInfo();
                                stickers[j] = new StickerInfo();
                                currentRi.stickerArray[j].positions = new Vector3[4];

                                // Object's position
                                if (ecef)
                                {
                                    double posTX = jsonParse["scrs"][j]["content"]["ecefPose"]["position"]["x"].AsDouble;
                                    double posTY = jsonParse["scrs"][j]["content"]["ecefPose"]["position"]["y"].AsDouble;
                                    double posTZ = jsonParse["scrs"][j]["content"]["ecefPose"]["position"]["z"].AsDouble;
                                    // calc the object position relatively the recently localized camera
                                    px = (float)(posTX - px0);
                                    py = (float)(posTY - py0);
                                    pz = (float)(posTZ - pz0);
                                    ox = jsonParse["scrs"][j]["content"]["ecefPose"]["quaternion"]["x"].AsFloat;
                                    oy = jsonParse["scrs"][j]["content"]["ecefPose"]["quaternion"]["y"].AsFloat;
                                    oz = jsonParse["scrs"][j]["content"]["ecefPose"]["quaternion"]["z"].AsFloat;
                                    ow = jsonParse["scrs"][j]["content"]["ecefPose"]["quaternion"]["w"].AsFloat;
                                }
                                else if (useGeopose)
                                {
                                    double tlat, tlon, thei;
                                    if (newGeoPose)
                                    {
                                        tlat = jsonParse["scrs"][j]["content"]["geopose"]["position"]["lat"].AsDouble;
                                        tlon = jsonParse["scrs"][j]["content"]["geopose"]["position"]["lon"].AsDouble;
                                        thei = jsonParse["scrs"][j]["content"]["geopose"]["position"]["h"].AsDouble;
                                    }
                                    else
                                    {
                                        tlat = jsonParse["scrs"][j]["content"]["geopose"]["latitude"].AsDouble;
                                        tlon = jsonParse["scrs"][j]["content"]["geopose"]["longitude"].AsDouble;
                                        thei = jsonParse["scrs"][j]["content"]["geopose"]["ellipsoidHeight"].AsDouble;
                                    }
                                    
                                    // calc the object position relatively the recently localized camera
                                    EcefPose epobj = GeodeticToEcef(tlat, tlon, thei);
                                    Vector3 enupose = EcefToEnu(epobj, camLat, camLon, camHei);
                                    px = enupose.x;
                                    py = enupose.y;
                                    pz = enupose.z;
                                    ox = jsonParse["scrs"][j]["content"]["geopose"]["quaternion"]["x"].AsFloat;
                                    oy = jsonParse["scrs"][j]["content"]["geopose"]["quaternion"]["y"].AsFloat;
                                    oz = jsonParse["scrs"][j]["content"]["geopose"]["quaternion"]["z"].AsFloat;
                                    ow = jsonParse["scrs"][j]["content"]["geopose"]["quaternion"]["w"].AsFloat;

                                    //Debug.Log("scr.ecef.quat = oxo:" + ox + "--" + oy + "--" + oz + "--" + ow);
                                }
                                else  // object local pose
                                {
                                    px = jsonParse["scrs"][j]["content"]["geopose"]["local"]["position"]["x"].AsFloat;
                                    py = jsonParse["scrs"][j]["content"]["geopose"]["local"]["position"]["y"].AsFloat;
                                    pz = jsonParse["scrs"][j]["content"]["geopose"]["local"]["position"]["z"].AsFloat;
                                    ox = jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["x"].AsFloat;
                                    oy = jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["y"].AsFloat;
                                    oz = jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["z"].AsFloat;
                                    ow = jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["w"].AsFloat;
                                }

                                uPose = new UnityPose(new Vector3(px, py, pz), new Quaternion(ox, oy, oz, ow));
                                currentRi.stickerArray[j].mainPositions = uPose.pos;

                                // Update object orientation depends on the system coords
                                if (ecef)
                                {
                                    currentRi.stickerArray[j].orientations = uPose.SetObjectOriFromGeoPose();
                                }
                                else if (useGeopose)
                                {
                                    currentRi.stickerArray[j].orientations = uPose.SetObjectOriFromGeoPose();
                                }
                                else // local pose
                                {
                                    currentRi.stickerArray[j].orientations = uPose.GetOrientation();
                                }

                                stickers[j].mainPositions = currentRi.stickerArray[j].mainPositions;
                                stickers[j].orientations = currentRi.stickerArray[j].orientations;

                                /* Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations x" + currentRi.stickerArray[j].orientations.x + "   " + stickers[j].orientations.x);
                                   Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations y" + currentRi.stickerArray[j].orientations.y + "   " + stickers[j].orientations.y);
                                   Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations z" + currentRi.stickerArray[j].orientations.z + "   " + stickers[j].orientations.z);
                                   Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations w" + currentRi.stickerArray[j].orientations.w + "   " + stickers[j].orientations.w);
                                */
                                for (int i = 0; i < 4; i++)
                                {
                                    float pxf = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["x"].AsFloat + px;
                                    float pyf = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["y"].AsFloat + py;
                                    float pzf = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["z"].AsFloat + pz;
                                    placeHolders[j, i] = new GameObject("Placeholder" + j + " " + i);
                                    placeHolders[j, i].transform.SetParent(newCam.transform);
                                    placeHolders[j, i].transform.position = UnityPose.GetPosition(pxf, pyf, pzf);
                                    currentRi.stickerArray[j].positions[i] = UnityPose.GetPosition(pxf, pyf, pzf);
                                }

                                stickers[j].sPath = "" + jsonParse["scrs"][j]["content"]["custom_data"]["path"];
                                stickers[j].sText = "" + jsonParse["scrs"][j]["content"]["custom_data"]["sticker_text"];
                                stickers[j].sType = "" + jsonParse["scrs"][j]["content"]["custom_data"]["sticker_type"];
                                stickers[j].sSubType = "" + jsonParse["scrs"][j]["content"]["custom_data"]["sticker_subtype"];
                                stickers[j].sDescription = "" + jsonParse["scrs"][j]["content"]["custom_data"]["description"];
                                stickers[j].SModel_scale = "" + jsonParse["scrs"][j]["content"]["custom_data"]["model_scale"];
                                stickers[j].sId = "" + jsonParse["scrs"][j]["content"]["custom_data"]["sticker_id"];
                                stickers[j].objectId = "" + jsonParse["scrs"][j]["content"]["custom_data"]["placeholder_id"];
                                stickers[j].sImage = "" + jsonParse["scrs"][j]["content"]["custom_data"]["Image"];
                                stickers[j].sAddress = "" + jsonParse["scrs"][j]["content"]["custom_data"]["Address"];
                                stickers[j].sFeedbackAmount = "" + jsonParse["scrs"][j]["content"]["custom_data"]["Feedback amount"];
                                stickers[j].sRating = "" + jsonParse["scrs"][j]["content"]["custom_data"]["Rating"];
                                stickers[j].sUrl_ta = "" + jsonParse["scrs"][j]["content"]["custom_data"]["url_ta"];
                                stickers[j].sTrajectoryPath = "" + jsonParse["scrs"][j]["content"]["custom_data"]["trajectory_path"];
                                stickers[j].sTrajectoryOffset = "" + jsonParse["scrs"][j]["content"]["custom_data"]["trajectory_time_offset"];
                                stickers[j].sTrajectoryPeriod = "" + jsonParse["scrs"][j]["content"]["custom_data"]["trajectory_time_period"];
                                stickers[j].subType = "" + jsonParse["scrs"][j]["content"]["custom_data"]["subtype"];
                                stickers[j].type = "" + jsonParse["scrs"][j]["content"]["custom_data"]["type"];
                                stickers[j].bundleName = "" + jsonParse["scrs"][j]["content"]["custom_data"]["model_id"];
                                stickers[j].anchorName = "" + jsonParse["srcs"][j]["content"]["custom_data"]["anchor"];
                                stickers[j].externalAssetUrl = "" + jsonParse["srcs"][j]["content"]["custom_data"]["externalAssetUrl"];
                                if (string.IsNullOrEmpty(stickers[j].bundleName))
                                {
                                    stickers[j].bundleName = "" + jsonParse["scrs"][j]["content"]["custom_data"]["bundle_name"];
                                }
                                string groundeds = jsonParse["scrs"][j]["content"]["custom_data"]["grounded"];
                                string verticals = jsonParse["scrs"][j]["content"]["custom_data"]["vertically_aligned"];
                                if (groundeds != null)
                                {
                                    if (groundeds.Contains("1")) { stickers[j].grounded = true; }
                                }
                                if (verticals != null)
                                {
                                    if (verticals.Contains("1")) { stickers[j].vertical = true; }
                                }

                                currentRi.stickerArray[j].sPath = stickers[j].sPath;
                                currentRi.stickerArray[j].sText = stickers[j].sText;
                                currentRi.stickerArray[j].sType = stickers[j].sType;
                                currentRi.stickerArray[j].sSubType = stickers[j].sSubType;
                                currentRi.stickerArray[j].sDescription = stickers[j].sDescription;
                                currentRi.stickerArray[j].SModel_scale = stickers[j].SModel_scale;
                                currentRi.stickerArray[j].sId = stickers[j].sId;
                                currentRi.stickerArray[j].sImage = stickers[j].sImage;
                                currentRi.stickerArray[j].sAddress = stickers[j].sAddress;
                                currentRi.stickerArray[j].sRating = stickers[j].sRating;
                                currentRi.stickerArray[j].sUrl_ta = stickers[j].sUrl_ta;
                                currentRi.stickerArray[j].sTrajectoryPath = stickers[j].sTrajectoryPath;
                                currentRi.stickerArray[j].sTrajectoryOffset = stickers[j].sTrajectoryOffset;
                                currentRi.stickerArray[j].sTrajectoryPeriod = stickers[j].sTrajectoryPeriod;
                                currentRi.stickerArray[j].grounded = stickers[j].grounded;
                                currentRi.stickerArray[j].vertical = stickers[j].vertical;
                                currentRi.stickerArray[j].subType = stickers[j].subType;
                                currentRi.stickerArray[j].type = stickers[j].type;
                                currentRi.stickerArray[j].bundleName = stickers[j].bundleName;
                                currentRi.stickerArray[j].anchorName = stickers[j].anchorName;
                                currentRi.stickerArray[j].externalAssetUrl = stickers[j].externalAssetUrl;
                            }

                            recoList.Add(currentRi);
                            newCam.transform.position = cameraPositionInLocalization;
                            newCam.transform.eulerAngles = cameraRotationInLocalization;
                            for (int j = 0; j < objectsAmount; j++)
                            {
                                stickers[j].positions = new Vector3[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    stickers[j].positions[i] = placeHolders[j, i].transform.position;
                                }
                            }
                        }
                    }
                }
                else if (currentRi != null)
                {


                    cameraDistance = Vector3.Magnitude(currentRi.lastCamCoordinate - new Vector3(px, py, pz));
                    tempScale3d = currentRi.scale3dcloud;
                    int savedNodeLentgh = currentRi.stickerArray.Length;
                    stickers = new StickerInfo[savedNodeLentgh];
                    GameObject[,] placeHolders = new GameObject[savedNodeLentgh, 4];

                    for (int j = 0; j < savedNodeLentgh; j++)
                    {
                        stickers[j] = new StickerInfo();
                        for (int i = 0; i < 4; i++)
                        {
                            placeHolders[j, i] = new GameObject("Placeholder" + j + " " + i);
                            placeHolders[j, i].transform.SetParent(newCam.transform);
                            placeHolders[j, i].transform.position = currentRi.stickerArray[j].positions[i];
                        }
                        stickers[j].sPath = currentRi.stickerArray[j].sPath;
                        stickers[j].sText = currentRi.stickerArray[j].sText;
                        stickers[j].sType = currentRi.stickerArray[j].sType;
                        stickers[j].sSubType = currentRi.stickerArray[j].sSubType;
                        stickers[j].sDescription = currentRi.stickerArray[j].sDescription;
                        stickers[j].sId = currentRi.stickerArray[j].sId;
                        stickers[j].sImage = currentRi.stickerArray[j].sImage;

                        currentRi.stickerArray[j].sAddress = stickers[j].sAddress;
                        currentRi.stickerArray[j].sRating = stickers[j].sRating;
                        currentRi.stickerArray[j].sUrl_ta = stickers[j].sUrl_ta;




                    }
                    newCam.transform.position = cameraPositionInLocalization;
                    newCam.transform.eulerAngles = cameraRotationInLocalization;

                    for (int j = 0; j < savedNodeLentgh; j++)
                    {
                        stickers[j].positions = new Vector3[4];
                        for (int i = 0; i < 4; i++)
                        {
                            stickers[j].positions[i] = placeHolders[j, i].transform.position;
                        }
                    }
                }

                if (zeroCoord.transform.eulerAngles == Vector3.zero)
                {
                    newCam.transform.position = cameraPositionInLocalization;
                    newCam.transform.eulerAngles = cameraRotationInLocalization;
                }
                currentRi.lastCamCoordinate = new Vector3(px, py, pz);
                localizationStatus = LocalizationStatus.Ready;
                uim.statusDebug("Localized");
                getStickersAction(currentRi.id, zeroCoord.transform, stickers);

                OSCPDataHolder.Instance.UpdateCoordinates(camLat, camLon, camHei);
                OSCPDataHolder.Instance.UpdateLocation(ARCamera.transform.position, ARCamera.transform.rotation);

                Destroy(newCam);
            }
            else
            {
                Debug.Log("Cant localize");
                uim.statusDebug("Cant localize");
                localizationStatus = LocalizationStatus.CantLocalize;
                uim.setDebugPose(0, 0, 0, 0, 0, 0, 0, "cant loc");
                getStickersAction(null, null, null);
            }
        }
    }

    public void ARLocation(Action<string, Transform, StickerInfo[]> getStickers)
    {
        if (!configurationSetted)
        {
            SetCameraConfiguration();
        }

        getStickersAction = getStickers;

        if (!hasGpsLocation) //FixMe: ???
        {
            // determine the coarse (GPS) location first, and then query the VPS
            System.Action onFinishedAction = new System.Action(() => {
                Debug.Log("ARLocation Locate callback...");
                firstLocalization(lastGpsLocation.longitude, lastGpsLocation.latitude, lastGpsLocation.horizontalAccuracy, null, null);
            });
            StartCoroutine(Locate(onFinishedAction));
        }
        else
        {
            // go directly to VPS
            firstLocalization(lastGpsLocation.longitude, lastGpsLocation.latitude, lastGpsLocation.horizontalAccuracy, null, null);
        }

    }

    // This method might be called publicly, for example from a Debug localizer or a separate GpsLocationService
    public void updateMyGpsLocation(LocationInfo locationInfo)
    {
        lastGpsLocation.altitude = locationInfo.altitude;
        lastGpsLocation.horizontalAccuracy = locationInfo.horizontalAccuracy;
        lastGpsLocation.latitude = locationInfo.latitude;
        lastGpsLocation.longitude = locationInfo.longitude;
        lastGpsLocation.timestamp = locationInfo.timestamp;
        lastGpsLocation.verticalAccuracy = locationInfo.verticalAccuracy;
        hasGpsLocation = true;
        uim.statusDebug("Located GPS");
        Debug.Log("Updated GPS Location: "
            + " lat: " + lastGpsLocation.latitude
            + " lon: " + lastGpsLocation.longitude
            + " alt: " + lastGpsLocation.altitude
            + " hAccuracy: " + lastGpsLocation.horizontalAccuracy
            + " timestamp: " + lastGpsLocation.timestamp);

        decimal radLat = H3Lib.Api.DegsToRads((decimal)lastGpsLocation.latitude);
        decimal radLon = H3Lib.Api.DegsToRads((decimal)lastGpsLocation.longitude);
        H3Lib.GeoCoord geoCoord = new H3Lib.GeoCoord(radLat, radLon);
        lastH3Index = H3Lib.Extensions.GeoCoordExtensions.ToH3Index(geoCoord, kH3Resolution);
        Debug.Log("  H3 index (level " + kH3Resolution + "): " + lastH3Index.ToString());
        OSCPDataHolder.Instance.H3CurrentZone = lastH3Index.ToString();
    }

#if UNITY_EDITOR
    //Only for use in the editor. This takes lat lon and returns a H3 Index to console log and saves to OSCPDataHolder
    public void GetH3IndexEditor(float lat, float lon)
    {

        decimal radLat = H3Lib.Api.DegsToRads((decimal)lat);
        decimal radLon = H3Lib.Api.DegsToRads((decimal)lon);
        H3Lib.GeoCoord geoCoord = new H3Lib.GeoCoord(radLat, radLon);
        lastH3Index = H3Lib.Extensions.GeoCoordExtensions.ToH3Index(geoCoord, kH3Resolution);
        Debug.Log("  H3 index (level " + kH3Resolution + "): " + lastH3Index.ToString());
        OSCPDataHolder.Instance.H3CurrentZone = lastH3Index.ToString();
    }
#endif

    public void firstLocalization(float longitude, float latitude, float hdop, string path, Action<string, Transform, StickerInfo[]> getStickers)
    {
        Debug.Log("firstLocalization: " + "lat: " + latitude + ", lon: " + longitude + ", hdop: " + hdop + ", path: " + path);
        byte[] bjpg;
        string framePath;
        if (editorTestMode)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            framePath = path;
            bjpg = File.ReadAllBytes(framePath);
        }
        else
        {
            bjpg = CamGetFrame();
        }

        if (bjpg == null)
        {
            //bjpg = bb.bytes; // TODO: what is this fake image? abort instead!
            Debug.Log("Frame is null! Aborting localization");
            return;
        }

        if (getStickers != null) getStickersAction = getStickers;

        // save current local pose
        cameraRotationInLocalization = ARCamera.transform.rotation.eulerAngles;
        cameraPositionInLocalization = ARCamera.transform.position;

        if (debugSaveCameraImages)
        {
            
           // string debugCameraImagePath = Path.Combine(Application.persistentDataPath, System.DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss--fff") + ".jpg");
           // Debug.Log("DEBUG saving camera image to " + debugCameraImagePath);
           // File.WriteAllBytes(debugCameraImagePath, bjpg);
            Debug.Log($"ACityAPIDev::firstLocalization has apiURL = {apiURL}");
            uploadFrame(bjpg, apiURL, longitude, latitude, hdop, camLocalize);
        }

        // TODO: at this point, the apiURL must be set properly and we should not overwrite it again. So we can remove the lines below.
        // if there is no apiURL, then we should not try to uploadFrame() anyway.
        //if (PlayerPrefs.HasKey("ApiUrl"))
        //{
        //    apiURL = PlayerPrefs.GetString("ApiUrl");
        //}
        uploadFrame(bjpg, apiURL, longitude, latitude, hdop, camLocalize);
    }

    public void setApiURL(string url)
    {
        apiURL = url;
        PlayerPrefs.SetString("ApiUrl", apiURL);
    }

    RecoInfo checkRecoID(string newId)
    {
        RecoInfo rinfo = null;
        if (newId != null)
        {
            foreach (RecoInfo ri in recoList)
            {
                //Debug.Log(ri.id);
                //Debug.Log(newId);
                if (ri.id.Contains(newId))
                {
                    rinfo = ri;
                }
            }
        }
        return rinfo;
    }

    public void uploadFrame(byte[] bytes, string apiURL, float longitude, float latitude, float hdop, Action<string, bool> getJsonCameraObjects)
    {
        if (!useOSCP) {
            StartCoroutine(UploadJPGwithGPS(bytes, apiURL, longitude, latitude, hdop, getJsonCameraObjects));
        } else {
            StartCoroutine(UploadJPGwithGPSOSCP(bytes, apiURL, longitude, latitude, hdop, getJsonCameraObjects));
        }
    }

    IEnumerator UploadJPGwithGPSOSCP(byte[] bytes, string baseURL, float longitude, float latitude, float hdop, Action<string, bool> getJsonCameraObjects)
    {
        Debug.Log("UploadJPGwithGPSOSCP...");

        localizationStatus = LocalizationStatus.WaitForAPIAnswer;
        //  byte[] bytes = File.ReadAllBytes(filePath);
        Debug.Log("nBytes: " + bytes.Length);
        rotationDevice = "90";
        if (!editorTestMode)
        {
            rotationDevice = "270";  // Default value
            if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) {
               rotationDevice = "90";
            }
        }

        //  string shot = System.Text.Encoding.UTF8.GetString(bytes);
        string shot = Convert.ToBase64String(bytes);

        // TODO: we should not use hardcoded timestamp as before
        // we should use the real capture timestamp of the image, and of the last GPS measurements
        const string timestampExample = "2020-11-11T11:56:21+00:00"; // we want this format based on the example:
        string timestampLocal = System.DateTime.UtcNow.ToString(); // not good: 3/15/2022 7:45:50 PM
        string timestamp = System.DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture); // almost good 2022-03-15T19:45:50

        // TODO: we should not use hardcoded request_id as before
        const string requestIdExample = "9089876676575754";
        //geoposeRequestId++;
        string requestId = geoposeRequestId.ToString();

        // TODO: altitude is hardcoded to 0
        // TODO: mirrored is hardcoded to false
        // TODO: sequenceNumber is harcoded to 0
        string finalJson = "{" +
            "\"id\":\"" + requestId + "\"," +
            "\"timestamp\":\""+ timestamp + "\"," +
            "\"type\":\"geopose\"," +
            "\"sensors\":[" +
                "{\"id\":\"0\",\"type\":\"camera\"}," +
                "{\"id\":\"1\",\"type\":\"geolocation\"}" +
            "]," +
            "\"sensorReadings\":[" +
                "{" +
                "\"timestamp\":\"" + timestamp + "\"," +
                "\"sensorId\":\"0\"," +
                "\"reading\":{" +
                    "\"sequenceNumber\":0," +
                    "\"imageFormat\":\"JPG\"," +
                    "\"imageOrientation\":{\"mirrored\":false,\"rotation\":"+ rotationDevice +"}," +
                    "\"imageBytes\":\"" + shot + "\"}" +
                "}, {" +
                "\"timestamp\":\"" + timestamp + "\"," +
                "\"sensorId\":\"1\"," +
                "\"reading\":{" +
                    "\"latitude\":" + latitude + "," +
                    "\"longitude\":" + longitude + "," +
                    "\"altitude\":0" + "," +
                    "\"accuracy\":" + hdop + "}" +
                "}" +
            "]" +
        "}";
        Debug.Log("finalJson OSCP = " + finalJson);

        // WARNING: there has been some changes in the URL ending over the past year:
        //string finalUrl = baseURL + "/scrs/geopose_objs_local";  // this returns camera pose and all objects in the neighborhood
        //string finalUrl = baseURL + "/scrs/geopose_objs"; // this is obsolete and should never be used
        string finalUrl = baseURL + "/scrs/geopose"; // this returns camera pose only
        Debug.Log("finalUrl: " + finalUrl);

        // Debug.Log("Uploading Screenshot started...");
        var request = new UnityWebRequest(finalUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(finalJson);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
        request.SetRequestHeader("Accept", "application/vnd.myplace.v2+json");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler.contentType = "application/json";
        uim.statusDebug("Waiting response");
        request.timeout = 50;
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
            localizationStatus = LocalizationStatus.ServerError;
        }
        else
        {
            Debug.Log("Finished Uploading Screenshot");
        }
        Debug.Log(request.downloadHandler.text);
        var jsonParse = JSON.Parse(request.downloadHandler.text);
        string sid = null;
        if (jsonParse["camera"] != null)
        {
            sid = jsonParse["reconstruction_id"];   //FixMe: can be null?
            Debug.Log("answer: rec-id:" + sid);
        }
        tempScale3d = 1;
        getJsonCameraObjects(request.downloadHandler.text, true);
    }

    IEnumerator UploadJPGwithGPS(byte[] bytes, string apiURL, float longitude, float latitude, float hdop, Action<string, bool> getJsonCameraObjects)
    {
        Debug.Log("UploadJPGwithGPS...");

        localizationStatus = LocalizationStatus.WaitForAPIAnswer;
        Debug.Log("bytes length = " + bytes.Length);
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        rotationDevice = "0";
        if (!editorTestMode)
        {
            rotationDevice = "270";  // Default value
            if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) { rotationDevice = "90"; }
        }

        // TODO: do not hardcode 'mirrored'
        string jsona = "{\"gps\":{\"latitude\":" + latitude + ",\"longitude\":" + longitude + ",\"hdop\":" + hdop + "},\"rotation\": " + rotationDevice + ",\"mirrored\": false}";
        uim.gpsDebug(latitude, longitude, hdop);
        Debug.Log("" + jsona);
        form.Add(new MultipartFormFileSection("image", bytes, "test.jpg", "image/jpeg"));
        form.Add(new MultipartFormDataSection("description", jsona));

        byte[] boundary = UnityWebRequest.GenerateBoundary();
        string targetURL = apiURL + "/api/localizer/localize";
        Debug.Log(targetURL);
        var w = UnityWebRequest.Post(apiURL + "/api/localizer/localize", form, boundary);
        //w.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");  //FixMe: commented in aco3d???
        w.SetRequestHeader("Accept", "application/vnd.myplace.v2+json");
        w.SetRequestHeader("user-agent", "Unity AC-Viewer based app, name: " + Application.productName + ", Device: " + SystemInfo.deviceModel);

        Debug.Log("Uploading Screenshot started...");
        uim.statusDebug("Waiting response");
        w.timeout = 50;
        yield return w.SendWebRequest();
        if (w.result == UnityWebRequest.Result.ConnectionError || w.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"UploadJPGwithGPS: error on connection to {targetURL} error:'{w.error}' "); 
            localizationStatus = LocalizationStatus.ServerError;
        }
        else
        {
            Debug.Log("Finished Uploading Screenshot");
        }
        Debug.Log(w.downloadHandler.text);
        var jsonParse = JSON.Parse(w.downloadHandler.text);
        string sid = null;
        if (jsonParse["camera"] != null)
        {
            sid = jsonParse["reconstruction_id"];
        }
        tempScale3d = 1;

        getJsonCameraObjects(w.downloadHandler.text, false);
    }


    public void prepareSession(Action<bool, string> getServerAnswer)
    {
        Debug.Log("prepareSession...");
        if (editorTestMode)
        { 
            // nothing to do when testing in editor
            return;
        }

        //if (Input.location.status != LocationServiceStatus.Running)
        //{
        //    Debug.Log("LocationService is not running yet! Cannot prepare AC API service.");
        //}
        //Input.location.Start();
        // TODO: wait here until it really starts
        // --> Locate() initiates the GPS query and waits until a measurement is available
        System.Action onFinishedAction = new System.Action(() => {
            Debug.Log("prepareSession Locate callback...");
            // NOTE: prepareC is a coroutine and must be called like this:
            StartCoroutine(prepareC(lastGpsLocation.longitude, lastGpsLocation.latitude, getServerAnswer));
        });
        StartCoroutine(Locate(onFinishedAction));
    }

    IEnumerator prepareC(float longitude, float latitude, Action<bool, string> getServerAnswer)
    {
        // Example: https://developer.augmented.city/api/localizer/prepare?lat=59.907458f&lon=30.298400f
        //
        string req = apiURL + "/api/localizer/prepare?lat=" + latitude + "f&lon=" + longitude + "f";
        Debug.Log(req);
        var w = UnityWebRequest.Get(req);
        w.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
        w.SetRequestHeader("Accept", "application/vnd.myplace.v2+json");

        yield return w.SendWebRequest();
        if (w.isNetworkError || w.isHttpError)
        {
            Debug.Log(w.error);
            localizationStatus = LocalizationStatus.ServerError;
            getServerAnswer(false, w.downloadHandler.text);
        }
        else
        {
            Debug.Log("prepareC Success: " + w.downloadHandler.text);
            getServerAnswer.Invoke(true, w.downloadHandler.text);
        }
    }


    //IEnumerator Locate(Action<float, float, float, string, Action<string, Transform, StickerInfo[]>> onGpsLocationUpdated)
    IEnumerator Locate(Action onGpsLocationUpdatedCallback = null)
    {
        Debug.Log("Started Locate GPS");
        if (uim == null)
        {
            Debug.LogError("uim is null at this point!"); // TODO: this is likely to happen when this is called during Start()
        }
        else
        {
            uim.statusDebug("Locating GPS");
        }

        localizationStatus = LocalizationStatus.GetGPSData;
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("geo not enabled"); PlayerPrefs.SetInt("LocLoaded", -1);
            yield break;
        }
        // Start service before querying location
        Input.location.Start();
        // Wait until service is initializing
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service hasn't initialized for 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            localizationStatus = LocalizationStatus.NoGPSData;
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            PlayerPrefs.SetInt("LocLoaded", -2);
            localizationStatus = LocalizationStatus.NoGPSData;
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            updateMyGpsLocation(Input.location.lastData);
            // TODO: should we set a value for localizationStatus here?

            if (onGpsLocationUpdatedCallback != null)
            {
                Debug.Log("Locate invoking callback...");
                onGpsLocationUpdatedCallback.Invoke();
            }
            Debug.Log("Location: " + Input.location.lastData.latitude + " "
                                   + Input.location.lastData.longitude + " "
                                   + Input.location.lastData.altitude + " "
                                   + Input.location.lastData.horizontalAccuracy + " "
                                   + Input.location.lastData.timestamp);

           // getLocData(Input.location.lastData.latitude, Input.location.lastData.longitude, Input.location.lastData.horizontalAccuracy, null, null);
            GPSlocation = true;
           // longitude  = Input.location.lastData.longitude;     //FixMe: mix them up twice
           // latitude   = Input.location.lastData.latitude;
           // hdop       = Input.location.lastData.horizontalAccuracy;
            uim.statusDebug("Located GPS");
        }

        // Stop service if there is no need to query location updates continuously
        //Input.location.Stop();
        Debug.Log("Finished Locate GPS");
    }

    public LocalizationStatus getLocalizationStatus() { return localizationStatus; }
    public float getApiCameraDistance()
    {
        return cameraDistance;
    }


    void FixedUpdate()
    {
        globalTimer = serverTimer + Time.timeSinceLevelLoad;
    }

    void SetTimer(double timer)
    {
        Debug.Log("(float)timer  % 100000= " + (float)(timer % 100000));
        serverTimer = (float)(timer % 100000);
        Debug.Log("serverTimer = " + serverTimer);
    }

    IEnumerator GetTimerC()
    {
        Debug.Log("Getting server time...");
        // TODO: do not use hardcoded API URL. But this method is currently called before setApiUrl()
        var sw = UnityWebRequest.Get("https://developer.augmented.city/api/v2/server_timestamp");
        yield return sw.SendWebRequest();
        if (sw.result == UnityWebRequest.Result.ConnectionError || sw.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("GetTimerC error: " + sw.error);
        }
        else
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Debug.Log("GetTimerC success: " + sw.downloadHandler.text);
            double timer = double.Parse(sw.downloadHandler.text);
            SetTimer(timer);
        }
    }

    public void TimerShow()
    {
        StartCoroutine(GetTimerC());
        Debug.Log("globalTimer = " + globalTimer);
    }


    public EcefPose GeodeticToEcef(double lat, double lon, double h)
    {
        double lamb, phi, s, N;
        lamb = lat * Mathf.Deg2Rad;
        phi = lon * Mathf.Deg2Rad;
        s = Math.Sin(lamb);
        N = a / Math.Sqrt(1 - e_sq * s * s);

        double sin_lambda, cos_lambda, sin_phi, cos_phi;
        sin_lambda = Math.Sin(lamb);
        cos_lambda = Math.Cos(lamb);
        sin_phi = Math.Sin(phi);
        cos_phi = Math.Cos(phi);

        double x, y, z;
        x = (h + N) * cos_lambda * cos_phi;
        y = (h + N) * cos_lambda * sin_phi;
        z = (h + (1 - e_sq) * N) * sin_lambda;

        EcefPose ep = new EcefPose();
        ep.x = x;
        ep.y = y;
        ep.z = z;
        return ep;
    }

    public Vector3 EcefToEnu(EcefPose ep, double lat_ref, double lon_ref, double h_ref)
    {
        double lamb, phi, s, N;
        lamb = lat_ref * Mathf.Deg2Rad;
        phi = lon_ref * Mathf.Deg2Rad;
        s = Math.Sin(lamb);
        N = a / Math.Sqrt(1 - e_sq * s * s);

        double sin_lambda, cos_lambda, sin_phi, cos_phi;
        sin_lambda = Math.Sin(lamb);
        cos_lambda = Math.Cos(lamb);
        sin_phi = Math.Sin(phi);
        cos_phi = Math.Cos(phi);

        double x0, y0, z0;
        x0 = (h_ref + N) * cos_lambda * cos_phi;
        y0 = (h_ref + N) * cos_lambda * sin_phi;
        z0 = (h_ref + (1 - e_sq) * N) * sin_lambda;

        Debug.Log("ep.x = " + ep.x + ", ep.y = " + ep.y + ",ep.z = " + ep.z);

        double xd, yd, zd;
        xd = ep.x - x0;
        yd = ep.y - y0;
        zd = ep.z - z0;
        Debug.Log("xd= " + xd + ", yd = " + yd + ",zd = " + zd);

        double xEast, yNorth, zUp;
        xEast = -sin_phi * xd + cos_phi * yd;
        yNorth = -cos_phi * sin_lambda * xd - sin_lambda * sin_phi * yd + cos_lambda * zd;
        zUp = cos_lambda * cos_phi * xd + cos_lambda * sin_phi * yd + sin_lambda * zd;

        //Debug.Log("xEast = "+ xEast + ",yNorth " + yNorth+ ",zUp" + zUp);

        return new Vector3((float)xEast, (float)yNorth, (float)zUp);
    }

}
