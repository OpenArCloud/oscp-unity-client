using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Swiper : MonoBehaviour
{
    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;
    UIManager uim;


    void Start()
    {
        uim = GameObject.FindGameObjectWithTag("Manager").GetComponent<UIManager>();
    }

    void Update()
    {
        Swipe();
    }


    public void LeftSwipe()
    {
    }

    public void RightSwipe()
    {
    }

    public void DownSwipe()
    {
        uim.DownSwipe();
    }

    public void Swipe()
    {
        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                // save began touch 2d point
                firstPressPos = new Vector2(t.position.x, t.position.y);
            }
            if (t.phase == TouchPhase.Ended)
            {
                // save ended touch 2d point
                secondPressPos = new Vector2(t.position.x, t.position.y);
                if (Mathf.Abs(secondPressPos.x - firstPressPos.x) > 150 ||
                    Mathf.Abs(secondPressPos.y - firstPressPos.y) > 150)
                {
                    // create vector from the two points
                    currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

                    // normalize the 2d vector
                    currentSwipe.Normalize();

                    // swipe upwards
                    if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                    {
                        //Debug.Log("up swipe");
                    }
                    // swipe down
                    if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
                    {
                        //Debug.Log("down swipe, y pos = " + firstPressPos.y);
                        DownSwipe();
                    }
                    // swipe left
                    if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                    {
                        LeftSwipe();
                        //Debug.Log("left swipe");
                    }
                    // swipe right
                    if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
                    {
                        RightSwipe();
                        //Debug.Log("right swipe");
                    }
                }
            }
        }
    }
}
