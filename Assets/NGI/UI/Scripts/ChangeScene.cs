using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    // Start is called before the first frame update
    public void SceneChangerActive (GameObject Panelen)
    {
        Panelen.SetActive(true);
    }
    public void SceneChangerDeActive(GameObject Panelen)
    {
        Panelen.SetActive(false);
    }


}
