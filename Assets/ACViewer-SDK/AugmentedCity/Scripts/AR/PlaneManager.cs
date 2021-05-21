using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneManager : MonoBehaviour
{
    public ARPlaneManager arplaner;
    public float yGround;
    int planetimer = 0;
    List<GameObject> planes;
    GameObject aRcamera;
    
    void Start()
    {
        aRcamera = Camera.main.gameObject;
        yGround = -1000;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        planetimer++;
        if (planetimer > 100) { getPlaneY(); }
    }

    public void getPlaneY()
    {
        planes = new List<GameObject>();
        var go = arplaner.trackables;
        foreach (var g in go) {
            planes.Add(g.gameObject);
        }
        //Debug.Log("planes length = " + planes.Count);
        float minY = 10;
        foreach (GameObject pl in planes) {
            // search plane on distance >0.8m from the camera
            if (aRcamera.transform.position.y - pl.transform.position.y > 0.8f) {
                if (pl.transform.position.y < minY)
                    minY = pl.transform.position.y;  // search for the minimal Y coord - closest to the camera
            }
            planetimer = -50;  // make 1sec delay in the plane obtaining procedure
        }
        if (minY < 10)  // if it's found
        {
            yGround = minY;  // found closest plane to the camera on dist >0.8m
            if (aRcamera.transform.position.y - yGround > 1.7f) {
                yGround = aRcamera.transform.position.y - 1.7f;  // assume that a ground couldn't more far than 1.7m
            }
            // store found plane coord by Y axis
            GetComponent<UIManager>().planeDebug(yGround);
        }
        else {
            yGround = aRcamera.transform.position.y - 1.5f;  // there're no found planes, assume the ground is in 1.5m
        }

        if (yGround < -100) {
            planetimer = 0;  // force to recalculate plane proc 
        }
    }

}
