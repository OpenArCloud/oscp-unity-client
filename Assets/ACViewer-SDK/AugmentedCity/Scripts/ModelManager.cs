using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelManager : MonoBehaviour
{

    string bundleName;
    public GameObject ABloader;
    string modelPath;
    GameObject activeModel;
    GetPlaceHoldersDev gph;
    UIManager uim;
    public GameObject activeButtonImage;
    public GameObject groundPlane;
    public GameObject planeForObjects;
    public GameObject shadowForObjects;

    public Color gk;
    public Image actImage;
    public List<AssetBundle> bundles = new List<AssetBundle>();
    public List<string> loadingBunles = new List<string>();
    bool editModeOn;
    public float timeForLongTap = 2f;
    [HideInInspector]
    public GameObject pl;
    [HideInInspector]
    public GameObject shadowObj;

    void Start()
    {
        bundleName = "trans2tank";
        gph = GetComponent<GetPlaceHoldersDev>();
        uim = GetComponent<UIManager>();
    }

    void Update()
    {
    }

    public void setModel(string bName)
    {
        bundleName = bName;
        Debug.Log(bundleName);
    }


    public void firstButton(GameObject act) {
        activeButtonImage = act;
        activeButtonImage.SetActive(true);
        actImage = activeButtonImage.GetComponentInChildren<Image>();
        setActiveButton(activeButtonImage);
        setColorImage(actImage);

    }

    public void setActiveButton(GameObject ImageObj)
    {
        activeButtonImage.SetActive(false);
        ImageObj.SetActive(true);
        activeButtonImage = ImageObj;
    }

    public void setColorImage(Image imag)
    {
        actImage.color = gk;
        imag.color = Color.white;
        actImage = imag;
    }


    public bool GetEditMode()
    {
        return editModeOn;
    }

    public void SetEditMode(bool mode)
    {
        editModeOn = mode;
    }



}