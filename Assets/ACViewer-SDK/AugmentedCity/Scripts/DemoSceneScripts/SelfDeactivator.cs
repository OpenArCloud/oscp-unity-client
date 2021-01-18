using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDeactivator : MonoBehaviour
{

    public float timeToDeactivate;

    void OnEnable()
    {
        StartCoroutine(Deactivate(timeToDeactivate));
    }

    IEnumerator Deactivate(float time) {
        yield return new WaitForSeconds(time);
        this.gameObject.SetActive(false);
    }
}
