using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visiability : MonoBehaviour
{
    bool visible;

    void OnBecameVisible()
    {
        visible = true;
    }

    void OnBecameInvisible()
    {
        visible = false;
    }

    public bool VisibleA() {
        return visible;
    }
}
