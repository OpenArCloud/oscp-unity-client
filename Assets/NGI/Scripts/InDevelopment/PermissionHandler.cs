using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.iOS;
using UnityEngine.SceneManagement;

public class PermissionHandler : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(WaitForPermission());
    }

    private IEnumerator WaitForPermission()
    {
        //TODO: Change while loop to a list of permissions, ask the user about each specific permission and remember users choice.
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) || !Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
#if PLATFORM_ANDROID || UNITY_IOS
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                if (Application.isFocused)
                {
                    Permission.RequestUserPermission(Permission.FineLocation);
                }
            }
            else if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                if (Application.isFocused)
                {
                    Permission.RequestUserPermission(Permission.Camera);
                }

            }
#endif
            yield return null;

        }

       // SceneManager.LoadSceneAsync("AugCityToNGI_Dev_Interaction");

    }


    private void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string obj)
    {
        throw new NotImplementedException();
    }

    private void PermissionCallbacks_PermissionGranted(string obj)
    {
 
        Debug.Log("Permission was granted: " + obj);
        // throw new NotImplementedException();
    }

    private void PermissionCallbacks_PermissionDenied(string obj)
    {
        throw new NotImplementedException();
    }

}
