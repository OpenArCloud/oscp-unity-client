using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LongTap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public GameObject shortTap;
    public GameObject longTap;
    public float timeForLongTap = 2f;
    float timer;
    bool tapped, upped;
    void Start()
    {
        timer = 0;
    }

    void Update()
    {
        if (tapped) {
            timer = timer + Time.deltaTime;
        }

        if ((upped)||(timer>3f)) {
            if (timer > timeForLongTap)
            {
                longTap.SetActive(true);
            }
            else { shortTap.SetActive(true); }
            upped = false;
            tapped = false;
            timer = 0;
        }
    }

    //Detect current clicks on the GameObject (the one with the script attached)
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        tapped = true;
        Debug.Log(name + "Game Object Click in Progress");
    }

    //Detect if clicks are no longer registering
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        if (tapped) upped = true;
        
        Debug.Log(name + "No longer being clicked");
    }
}
