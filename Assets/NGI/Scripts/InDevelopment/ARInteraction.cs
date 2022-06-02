using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARInteraction : MonoBehaviour
{

    public static event Action<SCRItemTag> itemSelected;
    public static event Action itemExited;


    public void ItemSelected()
    {
        itemSelected?.Invoke(GetComponent<SCRItemTag>());
    }

    public void ItemExited()
    {
        itemExited?.Invoke();
    }

}
