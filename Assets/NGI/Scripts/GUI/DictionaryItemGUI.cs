using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DictionaryItemGUI : MonoBehaviour
{

    [SerializeField] private Text firstEntryLabel;
    [SerializeField] private InputField firstEntry;
    
    [SerializeField] private Text secondEntryLabel;
    [SerializeField] private InputField secondEntry;


    public void SetValues(Dictionary<string,string> dict)
    {
        List<string> keys = new List<string>(dict.Keys);

        firstEntryLabel.text = keys[0];
        firstEntry.text = dict[keys[0]];

        secondEntryLabel.text = keys[1];
        secondEntry.text = dict[keys[1]];
    }

    public string[] GetKeyValuePairs()
    {
        return new string[] { firstEntryLabel.text, firstEntry.text, secondEntryLabel.text, secondEntry.text };
    }

}
