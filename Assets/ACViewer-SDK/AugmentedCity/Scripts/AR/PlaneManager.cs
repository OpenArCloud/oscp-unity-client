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
    public void getPlaneY() {
        planes = new List<GameObject>();
        var go = arplaner.trackables;
        foreach (var g in go) {
            planes.Add(g.gameObject);
        }
        Debug.Log("planes length = " + planes.Count);
        float minY = 100;
        foreach (GameObject pl in planes) {
            if (aRcamera.transform.position.y - pl.transform.position.y > 0.8f) {
                if (pl.transform.position.y < minY) minY = pl.transform.position.y;
            }
            planetimer = -50;
        }
        if (minY < 10) { Debug.Log("New Plane Founded y = " + minY);
            yGround = minY;
            GetComponent<UIManager>().planeDebug(yGround);
        }

        if (yGround < -100) { planetimer = 0; }
    }

}
