using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{

    public Transform target;
    public Vector3 posCam;
    public Quaternion lastPos;
    public float turnSpeed = 20f;

    void Awake()
    {
        if(target == null)
        {
            //If target is null then target main camera 
            target = Camera.main.transform;
        }
      
    }

    void LateUpdate()
    {
        posCam = target.position - transform.position;
        posCam.y = 0;
        lastPos = Quaternion.LookRotation(-posCam);
        transform.rotation = Quaternion.Slerp(transform.rotation, lastPos, Time.deltaTime * turnSpeed);
    }
}

