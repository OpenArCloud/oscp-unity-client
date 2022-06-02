using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeywordItemGUI : MonoBehaviour
{

    [SerializeField] private InputField fieldValue;

    public void SetValues(string value)
    {

        fieldValue.text = value;
    }

    public string GetFieldInputText()
    {
       return fieldValue.text;
    }

}
