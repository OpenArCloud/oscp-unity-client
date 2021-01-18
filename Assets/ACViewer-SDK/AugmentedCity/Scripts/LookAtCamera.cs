using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform target;
    public Quaternion startRotation;
    UIManager uiManager;
    void Start()
        
    {
        startRotation = this.transform.localRotation;
        target = GameObject.FindGameObjectWithTag("MainCamera").transform;
        uiManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<UIManager>();
    }

    void Update()
    {
        if (uiManager.videoLookAtUser)
        {
            transform.LookAt(target);
        }
        else {
            this.transform.localRotation = startRotation;
            this.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }
}
