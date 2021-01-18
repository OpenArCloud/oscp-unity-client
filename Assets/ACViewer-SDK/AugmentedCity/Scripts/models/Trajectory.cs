using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;

public class Trajectory : MonoBehaviour
{


    GameObject pivotOrigin;
    GameObject tempGO;
    GameObject tempGO2;
    GameObject tempParent;
    Transform movingTransform;
    Quaternion startRotationTemp;
    int moveFrames;
    float frameCounter;
    Vector3 firstPointObjectPosition, secondPointObjectPosition;
    Quaternion targetRotation, startRotation;
    bool nextTrajectory, notFirstStart;
    float timer = 1f;
    int counter, starter;
     List<Vector3> coords = new List<Vector3>();
    Vector3[] v3;
    int obhodTime;

    [HideInInspector]
    public bool go;
    public bool kv = false;
    public string sTrajectory;
    public string sOffset;
    public string sTimePeriod;
    public ACityAPIDev acapi;
    void Start()
    {

        if (go)
        {
            int i =-1;
           //  string kaplya = "{[[0, 0], [0.01291947, 0.78263626], [0.05049832, 1.52446679], [0.11099, 2.22556198], [0.19267831, 2.88603913], [0.29387745, 3.50606247], [0.41293199, 4.08584313], [0.54821687, 4.6256392],[0.69813741, 5.12575564],[0.86112928, 5.58654438],[1.03565857, 6.00840424],[1.22022171, 6.39178097],[1.41334552, 6.73716724],[1.61358719, 7.04510265], [1.81953428, 7.31617371], [2.02980474, 7.55101386], [2.24304688, 7.75030344], [2.45793939, 7.91476974], [2.67319133, 8.04518696], [2.88754216, 8.14237622], [3.09976167, 8.20720555], [3.30865007, 8.24058991], [3.51303792, 8.2434912], [3.71178615, 8.21691822], [3.90378609, 8.16192668], [4.08795942, 8.07961923], [4.2632582, 7.97114545], [4.42866489, 7.83770182], [4.58319229, 7.68053175], [4.72588358, 7.50092557], [4.85581235, 7.30022053], [4.97208252, 7.07980081], [5.07382842, 6.8410975], [5.16021472, 6.5855886], [5.2304365, 6.31479907], [5.28371919, 6.03030076], [5.3193186, 5.73371243], [5.33652094, 5.42669981], [5.33464276, 5.11097549], [5.31303099, 4.78829904], [5.27106296, 4.4604769], [5.20814636, 4.12936246], [5.12371924, 3.79685603], [5.01725004, 3.46490484], [4.88823759, 3.13550302], [4.73621106, 2.81069165], [4.56073002, 2.49255872], [4.36138442, 2.18323914], [4.13779455, 1.88491474], [3.88961112, 1.59981427], [3.61651518, 1.3302134], [3.31821817, 1.07843474], [2.99446191, 0.84684779], [2.64501859, 0.637869], [2.26969076, 0.45396172], [1.86831136, 0.29763623], [1.44074371, 0.17144973], [0.9868815, 0.07800634], [0.50664878, 0.01995711]]}";
          //  string kvadrat = "{[[0, 0], [0.6433333333333333, 0.013333333333333334], [1.2866666666666666, 0.02666666666666667], [1.93, 0.04], [2.5733333333333333, 0.05333333333333334], [3.216666666666667, 0.06666666666666667], [3.86, 0.08], [4.503333333333333, 0.09333333333333334], [5.1466666666666665, 0.10666666666666667], [5.79, 0.12000000000000001], [6.433333333333334, 0.13333333333333333], [7.076666666666666, 0.14666666666666667], [7.72, 0.16], [8.363333333333333, 0.17333333333333334], [9.006666666666666, 0.18666666666666668], [9.65, 0.2], [9.602, 1.0133333333333334], [9.554, 1.8266666666666667], [9.506, 2.64], [9.458, 3.4533333333333336], [9.41, 4.266666666666667], [9.362, 5.08], [9.314, 5.8933333333333335], [9.266, 6.706666666666667], [9.218, 7.5200000000000005], [9.17, 8.333333333333332], [9.122, 9.146666666666667], [9.074, 9.959999999999999], [9.026, 10.773333333333333], [8.978, 11.586666666666666], [8.93, 12.4], [8.354666666666667, 12.420666666666667], [7.779333333333334, 12.441333333333334], [7.204, 12.462], [6.628666666666667, 12.482666666666667], [6.053333333333334, 12.503333333333334], [5.478, 12.524000000000001], [4.902666666666667, 12.544666666666668], [4.327333333333334, 12.565333333333333], [3.7520000000000007, 12.586], [3.1766666666666676, 12.606666666666667], [2.6013333333333337, 12.627333333333334], [2.0260000000000007, 12.648000000000001], [1.4506666666666677, 12.668666666666667], [0.8753333333333337, 12.689333333333334], [0.3, 12.71], [0.27999999999999997, 11.862666666666668], [0.26, 11.015333333333334], [0.24, 10.168000000000001], [0.21999999999999997, 9.320666666666668], [0.19999999999999998, 8.473333333333333], [0.18, 7.626], [0.15999999999999998, 6.778666666666667], [0.13999999999999999, 5.931333333333334], [0.12, 5.0840000000000005], [0.09999999999999998, 4.236666666666666], [0.07999999999999999, 3.389333333333333], [0.06, 2.542], [0.03999999999999998, 1.6946666666666665], [0.019999999999999962, 0.8473333333333333]]}";
            string jsonj = "{" + sTrajectory+ "}";
        //    if (kv) jsonj = kvadrat; else jsonj = kaplya;
            var jsonParse = JSON.Parse(jsonj);
            string js1, js2,js3;
            do
            {
                  i++;
                  js1 =jsonParse[0][i][0];
                  js2 ="" + jsonParse[0][i][1];
                  js3= "" + jsonParse[0][i][2];
                if (js1 != null) {
                    if (js3.Length > 0) { coords.Add(new Vector3(jsonParse[0][i][0].AsFloat, jsonParse[0][i][2].AsFloat, jsonParse[0][i][1].AsFloat)); }
                    else coords.Add(new Vector3(jsonParse[0][i][0].AsFloat, 0, jsonParse[0][i][1].AsFloat));
                }
            } while (js1 != null);
            v3 = coords.ToArray();
            counter = v3.Length;
            int offset = 0;
      //      if (sOffset.Length > 0) offset = int.Parse(sOffset); else offset = 0;
            if (sTimePeriod.Length > 0) obhodTime = int.Parse(sTimePeriod); else obhodTime = counter;
            moveFrames = (50 * obhodTime) / counter;
            Debug.Log("moveFrames = " + moveFrames + ", obhodTime First = " + obhodTime);
            obhodTime = (counter * moveFrames) / 50;
            Debug.Log("counter = " + counter + ", obhodTime CEL = " + obhodTime);

            /*  timer = obhodTime / counter;
              timer = (Mathf.Round(timer / Time.fixedDeltaTime)) * Time.fixedDeltaTime;
              obhodTime = timer * counter;
              float timerOffset = (acapi.globalTimer% obhodTime) + offset * timer;
              if (timerOffset >= obhodTime) timerOffset = timerOffset - obhodTime;*/
            if (acapi.globalTimer > 0) starter = (int)Mathf.Round(acapi.globalTimer%counter); //(timerOffset/timer); 
            else starter = 0;
            Debug.Log("timer = " + timer + ", starter = " + starter + ", counter = " + counter + ", obhodTime =" + obhodTime);//+ ", timerOffset = " + timerOffset + ", offset = " + offset);
            if (starter > counter - 1) { starter = 0; }
            pivotOrigin = new GameObject("TraektoryForObject");
            pivotOrigin.transform.SetParent(this.gameObject.transform.parent);
            pivotOrigin.transform.position = this.gameObject.transform.position;
            pivotOrigin.transform.rotation = this.gameObject.transform.rotation;
            this.gameObject.transform.SetParent(pivotOrigin.transform);
            tempParent = new GameObject("tempParent");
            tempParent.transform.position = pivotOrigin.transform.position;
            tempParent.transform.SetParent(pivotOrigin.transform);
            startRotationTemp = tempParent.transform.rotation;
            tempGO = new GameObject("tempForTraektory");
            tempGO.transform.SetParent(tempParent.transform);
            tempGO.transform.localPosition = v3[starter + 1];
            tempGO2 = new GameObject("tempForTraektory");
            tempGO2.transform.SetParent(tempParent.transform);
            tempGO2.transform.localPosition = v3[starter];
            tempParent.transform.rotation = pivotOrigin.transform.rotation;
            this.transform.position = tempGO2.transform.position;
            this.transform.LookAt(tempGO.transform);
          //  Destroy(tempGO); Destroy(tempGO2); Destroy(tempParent);


            nextTrajectory = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {/*
        if (starter % 50 == 0) {
            if (sTimePeriod.Length > 0) obhodTime = float.Parse(sTimePeriod); else obhodTime = counter;
            float timerOffset = (acapi.globalTimer % obhodTime);
            float newstarter = (int)Mathf.Round(timerOffset / timer);
            if (newstarter  >= counter - 1) { newstarter = 0; }
            if (timerOffset >= obhodTime) timerOffset = timerOffset - obhodTime;
            Debug.Log("timer = " + timer + ", starter old = " + starter + ", starter new = " + newstarter + ", obhodTime =" + obhodTime + ", timerOffset = " + timerOffset + ", moveFrames = " + moveFrames);
        }
        */
        if (go)
        {
            if (!nextTrajectory) TranslationMover();
            else
            {
                tempParent.transform.rotation = startRotationTemp;
                tempGO.transform.SetParent(tempParent.transform);
                tempGO.transform.localPosition = v3[starter + 1];
                tempParent.transform.rotation = pivotOrigin.transform.rotation;
                tempGO.transform.SetParent(pivotOrigin.transform);
                tempGO2.transform.SetParent(pivotOrigin.transform);
                tempGO2.transform.position = this.gameObject.transform.position;
                tempGO2.transform.LookAt(tempGO.transform);
                tempGO.transform.rotation = tempGO2.transform.rotation;
                Translocation(this.gameObject.transform, tempGO.transform, timer);
                starter++;
                if (starter > (counter - 2)) starter = -1;
                nextTrajectory = false;
            }
        }
    }

    void Translocation(Transform firstPointObjectTransform, Transform secondPointObjectTransform, float time)
    {
        movingTransform = firstPointObjectTransform;
    //    moveFrames = int(int(time) / int( Time.fixedDeltaTime));
        frameCounter = moveFrames;
        firstPointObjectPosition = movingTransform.localPosition;
        secondPointObjectPosition = secondPointObjectTransform.localPosition;
        targetRotation = secondPointObjectTransform.rotation;
        startRotation = movingTransform.rotation;
    }


    void TranslationMover()
    {
        movingTransform.localPosition = Vector3.Lerp(firstPointObjectPosition, secondPointObjectPosition, (moveFrames - frameCounter) / moveFrames);
        movingTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, (moveFrames - frameCounter) / moveFrames);
        frameCounter--;
        if (frameCounter < 0) {nextTrajectory = true;

           // Debug.Log("pos = " + this.gameObject.transform.position);
        }
    }


}
