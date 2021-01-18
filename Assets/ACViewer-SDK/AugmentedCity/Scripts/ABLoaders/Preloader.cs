using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Preloader : MonoBehaviour
    {

        bool load;
        public GameObject parentPreloader;
        WWW ww;
        UnityWebRequest www1;
        Transform target;
        public Text loading;

        void Start()
        {

            target = Camera.main.gameObject.transform;

        }

        void Update()
        {
            transform.LookAt(target);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

           if ((load) && (www1 != null)) loading.text = "" + Mathf.RoundToInt(www1.downloadProgress * 100) + " %"; //Mathf.RoundToInt(ww.progress * 100) + " %";
           
        }
        public void Loading()
        {

          //  uim.aseetLoadingPanel.SetActive(true);
        }

        public void Loaded()
        {
         //   uim.aseetLoadingPanel.SetActive(false);

            Destroy(parentPreloader);
        }
        public void CantLoad()
        {
            load = false;
            loading.text = "ERROR";

        }

    public void LoadPercent(UnityWebRequest w)
        {
         www1 = w;
           // ww = w;
            load = true;
        }
    }
