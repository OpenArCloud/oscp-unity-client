using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SimpleJSON;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Android;
using System.Text;



public class ACityAPIDev : MonoBehaviour
{

    public class RecoInfo {
        public string id;
        public float scale3dcloud;
        public Vector3 lastCamCoordinate;
        public StickerInfo[] stickerArray;

    }

    public class StickerInfo {
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

    public bool editorTestMode;
    public string ServerAPI = "http://developer.augmented.city";
    public GameObject devButton;


    public TextAsset bb;
    Vector3 cameraRotationInLocalization;
    Vector3 cameraPositionInLocalization;
    float cameraDistance;
    float longitude, latitude;
    public float tempScale3d;
    public float globalTimer;
    float serverTimer;
    public bool useOSCP;

    ScreenOrientation ori;


    GameObject ARCamera;
    ARCameraManager m_CameraManager;
    bool startedLocalization;
    bool configurationSetted;
    bool GPSlocation;
    string apiURL = "http://developer.augmented.city";
    Action<string, Transform, StickerInfo[]> getStickersAction;
    List<RecoInfo> recoList = new List<RecoInfo>();

    LocalizationStatus localizationStatus = LocalizationStatus.NotStarted;
    UIManager uim;

    void Start()
    {
        // PlayerPrefs.DeleteAll();
        globalTimer = -1;
         ARCamera = Camera.main.gameObject;
        m_CameraManager = Camera.main.GetComponent<ARCameraManager>();
        if (!PlayerPrefs.HasKey("ApiUrl")) setApiURL(ServerAPI);
        else setApiURL(PlayerPrefs.GetString("ApiUrl"));
        #if PLATFORM_ANDROID || UNITY_IOS
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
            }
#endif

#if UNITY_EDITOR 

        editorTestMode = true;
        devButton.SetActive(true);
#endif

        StartCoroutine(GetTimerC());
        Input.location.Start();
        uim = this.GetComponent<UIManager>();

    }

    public void SetOSCPusage(bool os)
    {
        useOSCP = os;
    }


    void SetCameraConfiguration()
    {
        
        #if UNITY_IOS
        using (var configurations = m_CameraManager.GetConfigurations(Allocator.Temp))
        {
            Debug.Log("configurations.Length =   " + configurations.Length);

            int needConfingurationNumber = 0;
            for (int i = 0; i < configurations.Length; i++)
            {
                Debug.Log("Conf.height = " + configurations[i].height + ";  Conf.width = " + configurations[i].width + ";  conf.framerate = " + configurations[i].framerate);
                if (configurations[i].height == 720) { needConfingurationNumber = i;}
            }
            Debug.Log("Config number: " + needConfingurationNumber);
            // Get that configuration by index
            var configuration = configurations[needConfingurationNumber];   
            // Make it the active one
            m_CameraManager.currentConfiguration = configuration;
        }

        #endif

        #if PLATFORM_ANDROID
            using (var configurations = m_CameraManager.GetConfigurations(Allocator.Temp))
            {
                Debug.Log("configurations.Length =   " + configurations.Length);
                bool needConfFounded = false;
                int needConfingurationNumber = configurations.Length - 1;

                for (int i = 0; i < configurations.Length; i++)
                {
                    Debug.Log("Conf.height = " + configurations[i].height + ";  Conf.width = " + configurations[i].width + ";  conf.framerate = " + configurations[i].framerate);
                    if ((configurations[i].height == 1080)&&(!needConfFounded)) { needConfingurationNumber = i; needConfFounded = true; }
                }
                Debug.Log("Config number: " + needConfingurationNumber);
                // Get that configuration by index
                var configuration = configurations[needConfingurationNumber];
                // Make it the active one
                m_CameraManager.currentConfiguration = configuration;
            }
        #endif
            configurationSetted = true;
        }

    public unsafe byte[] CamGetFrame()
    {
        XRCpuImage image;
        if (m_CameraManager.TryAcquireLatestCpuImage(out image))
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                // Get the entire image.
                inputRect = new RectInt(0, 0, image.width, image.height),

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
            Debug.Log("buffer.Length" + buffer.Length);
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

            Texture2D normTex = m_Texture;

            byte[] bb = normTex.EncodeToJPG(100);
            return bb;
        }
        return null;
    }

    public void camLocalize(string jsonanswer, bool geopose)
    {
        var jsonParse = JSON.Parse(jsonanswer);
        float px, py, pz, ox, oy, oz, ow;
        int objectsAmount = -1; string js, sessionId;
        if (!geopose)
        {
            if (jsonParse["camera"] != null)
            {
                sessionId = jsonParse["reconstruction_id"];

                do
                {
                    objectsAmount++;
                    js = jsonParse["placeholders"][objectsAmount]["placeholder_id"];
                } while (js != null);

                Debug.Log("nodeAmount =   " + objectsAmount + ", RecoArray Length = " + recoList.Count);

                px = jsonParse["camera"]["pose"]["position"]["x"].AsFloat;
                py = jsonParse["camera"]["pose"]["position"]["y"].AsFloat;
                pz = jsonParse["camera"]["pose"]["position"]["z"].AsFloat;
                ox = jsonParse["camera"]["pose"]["orientation"]["x"].AsFloat;
                oy = jsonParse["camera"]["pose"]["orientation"]["y"].AsFloat;
                oz = jsonParse["camera"]["pose"]["orientation"]["z"].AsFloat;
                ow = jsonParse["camera"]["pose"]["orientation"]["w"].AsFloat;
                uim.setDebugPose(px, py, pz, ox, oy, oz, ow, sessionId);
                GameObject newCam = new GameObject("tempCam");
                newCam.transform.localPosition = new Vector3(px, py, pz);
                newCam.transform.localRotation = new Quaternion(ox, oy, oz, ow);
                newCam.transform.localPosition = new Vector3(px, -newCam.transform.localPosition.y, pz);
                //   Debug.Log("Camera new: " + newCam.transform.localPosition.x + ", " + newCam.transform.localPosition.y + ", " + newCam.transform.localPosition.z);
                newCam.transform.localRotation = Quaternion.Euler(-newCam.transform.localRotation.eulerAngles.x, newCam.transform.localRotation.eulerAngles.y, -newCam.transform.localRotation.eulerAngles.z);
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
                            float positionObjX = jsonParse["placeholders"][j]["pose"]["position"]["x"].AsFloat;
                            float positionObjY = jsonParse["placeholders"][j]["pose"]["position"]["y"].AsFloat;
                            float positionObjZ = jsonParse["placeholders"][j]["pose"]["position"]["z"].AsFloat;
                            currentRi.stickerArray[j].mainPositions = new Vector3(positionObjX, -positionObjY, positionObjZ);
                            //    Debug.Log("currentRi.stickerArray[j].mainPositions = " + currentRi.stickerArray[j].mainPositions);
                            stickers[j].mainPositions = new Vector3(positionObjX, -positionObjY, positionObjZ);
                            currentRi.stickerArray[j].orientations = new Vector4(jsonParse["placeholders"][j]["pose"]["orientation"]["x"].AsFloat, jsonParse["placeholders"][j]["pose"]["orientation"]["y"].AsFloat, jsonParse["placeholders"][j]["pose"]["orientation"]["z"].AsFloat, jsonParse["placeholders"][j]["pose"]["orientation"]["w"].AsFloat);
                            stickers[j].orientations = currentRi.stickerArray[j].orientations;
                            //   Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations" + currentRi.stickerArray[j].orientations);
                            for (int i = 0; i < 4; i++)
                            {
                                px = jsonParse["placeholders"][j]["frame"][i]["x"].AsFloat + positionObjX;
                                py = jsonParse["placeholders"][j]["frame"][i]["y"].AsFloat + positionObjY;
                                pz = jsonParse["placeholders"][j]["frame"][i]["z"].AsFloat + positionObjZ;
                                placeHolders[j, i] = new GameObject("Placeholder" + j + " " + i);
                                placeHolders[j, i].transform.SetParent(newCam.transform);
                                py = -py;
                                placeHolders[j, i].transform.position = new Vector3(px, py, pz);
                                currentRi.stickerArray[j].positions[i] = new Vector3(px, py, pz);
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
                                }
                            }
                        }
                        recoList.Add(currentRi);
                        newCam.transform.position = cameraPositionInLocalization;
                        newCam.transform.eulerAngles = cameraRotationInLocalization;
                        newCam.transform.RotateAround(newCam.transform.position, newCam.transform.forward, 90);
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
                    newCam.transform.RotateAround(newCam.transform.position, newCam.transform.forward, 90);

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
                    newCam.transform.RotateAround(newCam.transform.position, newCam.transform.forward, 90);
                }
                currentRi.lastCamCoordinate = new Vector3(px, py, pz);
                localizationStatus = LocalizationStatus.Ready;
                getStickersAction(currentRi.id, zeroCoord.transform, stickers);
                Destroy(newCam);
            }
            else
            {
                Debug.Log("Cant localize");

                localizationStatus = LocalizationStatus.CantLocalize;
                uim.setDebugPose(0, 0, 0, 0, 0, 0, 0, "cant loc");
                getStickersAction(null, null, null);
            }
        }
        // -- if -- GEOPOSE
        else
        {
            if (jsonParse["geopose"] != null)
            {
                sessionId = jsonParse["geopose"]["reconstruction_id"];

                Debug.Log("sessioID: " + sessionId);
                do
                {
                    objectsAmount++;
                    js = jsonParse["scrs"][objectsAmount]["type"];
                    // Debug.Log("js node [" + objectsAmount + "]  - " + js);
                } while (js != null);

                Debug.Log("nodeAmount =   " + objectsAmount + ", RecoArray Length = " + recoList.Count);

                px = jsonParse["geopose"]["local"]["position"]["x"].AsFloat;
                py = jsonParse["geopose"]["local"]["position"]["y"].AsFloat;
                pz = jsonParse["geopose"]["local"]["position"]["z"].AsFloat;
                ox = jsonParse["geopose"]["local"]["orientation"]["x"].AsFloat;
                oy = jsonParse["geopose"]["local"]["orientation"]["y"].AsFloat;
                oz = jsonParse["geopose"]["local"]["orientation"]["z"].AsFloat;
                ow = jsonParse["geopose"]["local"]["orientation"]["w"].AsFloat;
                uim.setDebugPose(px, py, pz, ox, oy, oz, ow, sessionId);
                GameObject newCam = new GameObject("tempCam");
                newCam.transform.localPosition = new Vector3(px, py, pz);
                newCam.transform.localRotation = new Quaternion(ox, oy, oz, ow);
                newCam.transform.localPosition = new Vector3(px, -newCam.transform.localPosition.y, pz);
                // Debug.Log("Camera new: " + newCam.transform.localPosition.x + ", " + newCam.transform.localPosition.y + ", " + newCam.transform.localPosition.z);



                newCam.transform.localRotation = Quaternion.Euler(-newCam.transform.localRotation.eulerAngles.x, newCam.transform.localRotation.eulerAngles.y, -newCam.transform.localRotation.eulerAngles.z);
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
                            float positionObjX = jsonParse["scrs"][j]["content"]["geopose"]["local"]["position"]["x"].AsFloat;
                            float positionObjY = jsonParse["scrs"][j]["content"]["geopose"]["local"]["position"]["y"].AsFloat;
                            float positionObjZ = jsonParse["scrs"][j]["content"]["geopose"]["local"]["position"]["z"].AsFloat;
                            currentRi.stickerArray[j].mainPositions = new Vector3(positionObjX, -positionObjY, positionObjZ);
                            //  Debug.Log("currentRi.stickerArray[j].mainPositions = " + currentRi.stickerArray[j].mainPositions);
                            stickers[j].mainPositions = new Vector3(positionObjX, -positionObjY, positionObjZ);
                            currentRi.stickerArray[j].orientations = new Vector4(jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["x"].AsFloat, jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["y"].AsFloat, jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["z"].AsFloat, jsonParse["scrs"][j]["content"]["geopose"]["local"]["orientation"]["w"].AsFloat);
                            stickers[j].orientations = currentRi.stickerArray[j].orientations;
                            Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations x" + currentRi.stickerArray[j].orientations.x + "   " + stickers[j].orientations.x);
                            Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations y" + currentRi.stickerArray[j].orientations.y + "   " + stickers[j].orientations.y);
                            Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations z" + currentRi.stickerArray[j].orientations.z + "   " + stickers[j].orientations.z);
                            Debug.Log("!!!!! currentRi.stickerArray[" + j + "].orientations w" + currentRi.stickerArray[j].orientations.w + "   " + stickers[j].orientations.w);

                            for (int i = 0; i < 4; i++)
                            {
                                px = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["x"].AsFloat + positionObjX;
                                py = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["y"].AsFloat + positionObjY;
                                pz = jsonParse["scrs"][j]["content"]["geopose"]["local"]["frame"][i]["z"].AsFloat + positionObjZ;
                                placeHolders[j, i] = new GameObject("Placeholder" + j + " " + i);
                                placeHolders[j, i].transform.SetParent(newCam.transform);
                                py = -py;
                                placeHolders[j, i].transform.position = new Vector3(px, py, pz);
                                currentRi.stickerArray[j].positions[i] = new Vector3(px, py, pz);
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


                        }
                        recoList.Add(currentRi);
                        newCam.transform.position = cameraPositionInLocalization;
                        newCam.transform.eulerAngles = cameraRotationInLocalization;
                        newCam.transform.RotateAround(newCam.transform.position, newCam.transform.forward, 90);
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
                    newCam.transform.RotateAround(newCam.transform.position, newCam.transform.forward, 90);

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
                    newCam.transform.RotateAround(newCam.transform.position, newCam.transform.forward, 90);
                }
                currentRi.lastCamCoordinate = new Vector3(px, py, pz);
                localizationStatus = LocalizationStatus.Ready;
                getStickersAction(currentRi.id, zeroCoord.transform, stickers);
                Destroy(newCam);

            }
            else
            {
                Debug.Log("Cant localize");

                localizationStatus = LocalizationStatus.CantLocalize;
                uim.setDebugPose(0, 0, 0, 0, 0, 0, 0, "cant loc");
                getStickersAction(null, null, null);
            }
        }
    }

    public void ARLocation(Action<string, Transform, StickerInfo[]> getStickers)
    {
        if (!configurationSetted) SetCameraConfiguration();
        getStickersAction = getStickers;

        if (!GPSlocation)
        {
            StartCoroutine(Locate(firstLocalization));
        }
        else firstLocalization(latitude, longitude, null, null);
    }

    public void firstLocalization(float langitude, float latitude, string path, Action<string, Transform, StickerInfo[]> getStickers) {
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
            if (bjpg == null)
            {
                Debug.Log("FRame getted NULL !!!");
                bjpg = bb.bytes;
            }
        }

        if (getStickers !=null) getStickersAction = getStickers;
        cameraRotationInLocalization = ARCamera.transform.rotation.eulerAngles;
        cameraPositionInLocalization = ARCamera.transform.position;
        if (bjpg != null) {
            if (PlayerPrefs.HasKey("ApiUrl")) apiURL = PlayerPrefs.GetString("ApiUrl");
            uploadFrame(bjpg, apiURL, langitude, latitude, camLocalize);
        }
    }

    public void setApiURL(string url) {
        apiURL = url;
    }

    RecoInfo checkRecoID(string newId)
    {
        RecoInfo rinfo = null;
        if (newId != null)
        {
            foreach (RecoInfo ri in recoList)
            {
                Debug.Log(ri.id);
                Debug.Log(newId);
                if (ri.id.Contains(newId))
                {
                    rinfo = ri;
                }
            }
        }
        return rinfo;
    }

    public void uploadFrame(byte[] bytes, string apiURL, float langitude, float latitude, Action<string, bool> getJsonCameraObjects)
    {
        if (!useOSCP)
        {
            StartCoroutine(UploadJPGwithGPS(bytes, apiURL, langitude, latitude, getJsonCameraObjects));
        }
        else StartCoroutine(UploadJPGwithGPSOSCP(bytes, apiURL, langitude, latitude, getJsonCameraObjects));
    }

    IEnumerator UploadJPGwithGPSOSCP(byte[] bytes, string apiURL, float langitude, float latitude, Action<string, bool> getJsonCameraObjects)
    {
        localizationStatus = LocalizationStatus.WaitForAPIAnswer;
        //  byte[] bytes = File.ReadAllBytes(filePath);
        Debug.Log("bytes length = " + bytes.Length);
        string rotationDevice = "180";
        if (!editorTestMode)
        {
            if (Input.deviceOrientation == DeviceOrientation.Portrait) { rotationDevice = "0"; }
            if (Input.deviceOrientation == DeviceOrientation.LandscapeRight) { rotationDevice = "90"; }
            if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) { rotationDevice = "180"; }
            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft) { rotationDevice = "270"; }
        }

        //  string shot = System.Text.Encoding.UTF8.GetString(bytes);
        string shot = Convert.ToBase64String(bytes);
        // Debug.Log("Uploading Screenshot started...");

        string finalJson = "{\"id\":\"9089876676575754\",\"timestamp\":\"2020-11-11T11:56:21+00:00\",\"type\":\"geopose\",\"sensors\":[{\"id\":\"0\",\"type\":\"camera\"},{\"id\":\"1\",\"type\":\"geolocation\"}],\"sensorReadings\":[{\"timestamp\":\"2020-11-11T11:56:21+00:00\",\"sensorId\":\"0\",\"reading\":{\"sequenceNumber\":0,\"imageFormat\":\"JPG\",\"imageOrientation\":{\"mirrored\":false,\"rotation\":" + rotationDevice + "},\"imageBytes\":\"" + shot + "\"}},{\"timestamp\":\"2020-11-11T11:56:21+00:00\",\"sensorId\":\"1\",\"reading\":{\"latitude\":" + langitude + ",\"longitude\":" + latitude + ",\"altitude\":0}}]}";
        Debug.Log("finalJson OSCP = " + finalJson);
        Debug.Log(apiURL + "/scrs/geopose_objs_local");
        /*  string path = "request.json";
        //  File.Create(path);
          StreamWriter writer = new StreamWriter(path, true, Encoding.UTF8, 1000000);
          writer.WriteLine(finalJson);
          writer.Close();*/

        var request = new UnityWebRequest(apiURL + "/scrs/geopose_objs_local", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(finalJson);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
        request.SetRequestHeader("Accept", "application/vnd.myplace.v2+json");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler.contentType = "application/json";
        request.timeout = 50;
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error); localizationStatus = LocalizationStatus.ServerError;

        }
        else
        {
            //  Debug.Log("Finished Uploading Screenshot");
        }
        Debug.Log(request.downloadHandler.text);
        var jsonParse = JSON.Parse(request.downloadHandler.text);
        string sid = null;
        if (jsonParse["camera"] != null)
        {
            sid = jsonParse["reconstruction_id"];
        }
        tempScale3d = 1;
        getJsonCameraObjects(request.downloadHandler.text, true);
    }

    IEnumerator UploadJPGwithGPS(byte[] bytes, string apiURL, float langitude, float latitude, Action<string, bool> getJsonCameraObjects)
    {
        localizationStatus = LocalizationStatus.WaitForAPIAnswer;
        Debug.Log("bytes = " + bytes.Length);
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        string rotationDevice = "90";
        if (!editorTestMode)
        {
            if (Input.deviceOrientation == DeviceOrientation.Portrait) { rotationDevice = "0"; }
            if (Input.deviceOrientation == DeviceOrientation.LandscapeRight) { rotationDevice = "90"; }
            if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) { rotationDevice = "180"; }
            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft) { rotationDevice = "270"; }
        }

        string jsona = "{\"gps\":{\"latitude\":" + langitude + ",\"longitude\":" + latitude + "},\"rotation\": " + rotationDevice + ",\"mirrored\": false}";
        uim.gpsDebug(langitude, latitude);
        Debug.Log("" + jsona);
        form.Add(new MultipartFormFileSection("image", bytes, "test.jpg", "image/jpeg"));
        form.Add(new MultipartFormDataSection("description", jsona));
        byte[] boundary = UnityWebRequest.GenerateBoundary();
        Debug.Log(apiURL + "/api/localizer/localize");
        var w = UnityWebRequest.Post(apiURL + "/api/localizer/localize", form, boundary);
        w.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
        w.SetRequestHeader("Accept", "application/vnd.myplace.v2+json");
        w.SetRequestHeader("user-agent", "Unity AC-viewer based app, name: " + Application.productName + ", Device: " + SystemInfo.deviceModel);

        Debug.Log("Uploading Screenshot started...");

        yield return w.SendWebRequest();
        if (w.isNetworkError || w.isHttpError) { Debug.Log(w.error); localizationStatus = LocalizationStatus.ServerError; }
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


    public void prepareSession(Action<bool, string> getServerAnswer) {
        if (!editorTestMode)
        {
            Input.location.Start();
            StartCoroutine(prepareC(Input.location.lastData.longitude, Input.location.lastData.latitude, getServerAnswer));
        }
    }

    IEnumerator prepareC (float langitude, float latitude, Action<bool, string> getServerAnswer)
    {
        Debug.Log(apiURL + "/api/localizer/prepare?lat=" + latitude + "f&lon=" + langitude + "f");
        var w = UnityWebRequest.Get(apiURL + "/api/localizer/prepare?lat=" + latitude + "f&lon=" + langitude + "f");
        w.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
        w.SetRequestHeader("Accept", "application/vnd.myplace.v2+json");
        yield return w.SendWebRequest();
        if (w.isNetworkError || w.isHttpError) { Debug.Log(w.error); localizationStatus = LocalizationStatus.ServerError;
            getServerAnswer(false, w.downloadHandler.text);

        }
        else
        {
            Debug.Log("prepared API");
            Debug.Log(w.downloadHandler.text);
            getServerAnswer(true, w.downloadHandler.text);
        }
    }


    IEnumerator Locate(Action<float, float, string, Action<string, Transform, StickerInfo[]>> getLocData)
    {
        Debug.Log("Started Locate GPS");
        localizationStatus = LocalizationStatus.GetGPSData;
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("geo not enabled"); PlayerPrefs.SetInt("LocLoaded", -1);
            yield break;
        }
        // Start service before querying location
        Input.location.Start();
        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
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
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            getLocData(Input.location.lastData.latitude, Input.location.lastData.longitude, null, null);
            GPSlocation = true;
            longitude = Input.location.lastData.longitude;
            latitude = Input.location.lastData.latitude;
            
        }


    }

    public LocalizationStatus getLocalizationStatus() { return localizationStatus; }
    public float getApiCameraDistance() {
        return cameraDistance;
    }


    void FixedUpdate() { 
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
        var sw = UnityWebRequest.Get("http://developer.augmented.city:15000/api/v2/server_timestamp");
        yield return sw.SendWebRequest();
        if (sw.isNetworkError || sw.isHttpError)
        {
            Debug.Log(sw.error);
        }
        else
        {
            Debug.Log("timer loaded");
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            double timer = double.Parse(sw.downloadHandler.text);
            Debug.Log("Timer = " + timer);
            SetTimer(timer);
        }
    }


    public void TimerShow()
    {

        StartCoroutine(GetTimerC());
        Debug.Log("globalTimer = " + globalTimer);

    }


}
