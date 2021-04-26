using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Mover : MonoBehaviour
{

    private Vector3 mOffset;
    private float mZCoord;
    Transform camTrans;
    GameObject cam;
    Camera camMain;

    private float startYmouse;
    private Vector2 oldt1, oldt2, oldtmax;

    bool locked = true;
    public bool landed;      // should the model be placed to a horizontal surface
    public bool noGravity;   // should not only a vertical axis rotation of the model be applied, otherwise an azimuth angle is applied and only

    bool wasDoubleTouch, firstTouch, oneTouch;
    float timer;
    bool tapped, upped;
     
    ModelManager modelManager;
    GameObject myGO;
    [HideInInspector]
    public string modelName;
    public string objectId;
    PlaneManager pm;


    void Start()
    {
        cam = Camera.main.gameObject;
        camTrans = Camera.main.transform;
        camMain = Camera.main;
        timer = 0;
        myGO = this.gameObject;
        pm = GameObject.FindGameObjectWithTag("Manager").GetComponent<PlaneManager>();
    }

    void Update()
    {

// #if !UNITY_EDITOR
        if (!noGravity) {
            myGO.transform.eulerAngles = new Vector3(0, myGO.transform.eulerAngles.y, 0);
        }
// #endif
        if (landed && pm.yGround > -100 && locked) {
            myGO.transform.position = new Vector3(myGO.transform.position.x, pm.yGround, myGO.transform.position.z);
        }
    }

    void OnMouseDown()
    {
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        // z coordinate of game object on screen
        mousePoint.z = mZCoord;
        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
    }

    void OnMouseUp() {
    }

    public void setLocked(bool loc) {
        GameObject man = GameObject.FindGameObjectWithTag("Manager");
        modelManager = man.GetComponent<ModelManager>();
        locked = loc;
        modelManager.SetEditMode(!loc);
    }

}
