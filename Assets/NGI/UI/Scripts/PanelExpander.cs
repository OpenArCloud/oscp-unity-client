using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelExpander : MonoBehaviour
{
    // Start is called before the first frame update
    public int StartValue;
    public int EndValue;
    private Transform panel;


    public void expand(Transform panel)
    {
        RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2 (panelRectTransform.sizeDelta.x, EndValue);
    }

    public void contract(Transform panel)
    {
        RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, StartValue);
    }

    // Update is called once per frame

}
