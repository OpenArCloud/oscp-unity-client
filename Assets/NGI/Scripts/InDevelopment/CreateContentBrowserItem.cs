using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateContentBrowserItem : MonoBehaviour
{

    [SerializeField] private GameObject contentHolder;
    [SerializeField] private GameObject contentPrefab;

    [SerializeField] private ToggleGroup toggleGroup;


    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            CreateItems();
        }
    }


    public void CreateItems()
    {

        GameObject temp = Instantiate(contentPrefab);
        temp.transform.SetParent(contentHolder.transform);

        ContentBrowserItem item = temp.GetComponent<ContentBrowserItem>();

        item.SetUp("100.1.1.1:5000", "test description", toggleGroup);
     

        Debug.Log("Item created");
    }




}
