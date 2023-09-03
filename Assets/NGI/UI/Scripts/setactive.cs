using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setactive : MonoBehaviour
{


    private GameObject Main;

    public GameObject Element1;
    public GameObject Element2;
    public GameObject Element3;
    public GameObject Element4;
    //private bool active1 = true; //unused
    private int value;

    // Start is called before the first frame update
    public void activate(int value)
    {
        if (value == 1)
        {
            if (Element1.activeInHierarchy == false)
            {
                Element1.SetActive(true);
                Element2.SetActive(false);
                Element3.SetActive(false);
                Element4.SetActive(false);
            }
            else { 
                Element1.SetActive(false);
                Element2.SetActive(false);
                Element3.SetActive(false);
                Element4.SetActive(false);
            }

        }
        else if (value == 2)
        {
            if (Element2.activeInHierarchy == false)
            {
                Element1.SetActive(false);
                Element2.SetActive(true);
                Element3.SetActive(false);
                Element4.SetActive(false);
            }
            else
            {
                Element1.SetActive(false);
                Element2.SetActive(false);
                Element3.SetActive(false);
                Element4.SetActive(false);

            }
        }
        else if (value == 3)
            {
                if (Element3.activeInHierarchy == false)
                {
                    Element1.SetActive(false);
                    Element2.SetActive(false);
                    Element3.SetActive(true);
                    Element4.SetActive(false);
                }
                else
                {
                    Element1.SetActive(false);
                    Element2.SetActive(false);
                    Element3.SetActive(false);
                    Element4.SetActive(false);
                }
        }
        else if (value == 4)
                {
                    if (Element4.activeInHierarchy == false)
                    {
                        Element1.SetActive(false);
                        Element2.SetActive(false);
                        Element3.SetActive(false);
                        Element4.SetActive(true);
                    }
                    else
                    {
                        Element1.SetActive(false);
                        Element2.SetActive(false);
                        Element3.SetActive(false);
                        Element4.SetActive(false);
                    }


                }



    }
}
