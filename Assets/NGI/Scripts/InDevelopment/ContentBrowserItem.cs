using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentBrowserItem : MonoBehaviour
{

    [SerializeField] private Text serverUrl;
    [SerializeField] private Text description;

    [SerializeField] private Toggle toggle;

    public void SetUp(string url, string desc, ToggleGroup tg)
    {
        serverUrl.text = url;
        description.text = desc;
        toggle.group = tg;


    }


}
