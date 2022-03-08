using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARInteractionManager : MonoBehaviour
{

    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] Toggle toggle;


    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(toggle);
        });
    }

    private void OnDisable()
    {
        toggle.onValueChanged.RemoveAllListeners();
    }


    void ToggleValueChanged(Toggle change)
    {
        raycastManager.enabled = change.isOn;
    }


}
