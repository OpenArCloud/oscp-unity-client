using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SComparer : IComparer<StickerController>
{
    public int Compare(StickerController x, StickerController y)
    {
        return x.GetDistance().CompareTo(y.GetDistance());
    }
}

public class StickerManager : MonoBehaviour
{
    GetPlaceHoldersDev gph;
    int timer;

    void Start()
    {
        gph = GetComponent<GetPlaceHoldersDev>();
    }

    // Update is called once per frame
    void Update()
    {
        timer++;
        if (timer%15 == 0) SetStickers(GetVisibleStickers());
    }


    StickerController[] GetVisibleStickers()
    {
        List<GameObject> stickers = gph.GetAllStickers();
        List<StickerController> visStickers = new List<StickerController>();
        foreach (GameObject sticker in stickers)
        {
            if (sticker.GetComponent<StickerController>().PinVisiability()) {
                visStickers.Add(sticker.GetComponent<StickerController>());
            }
        }
        return visStickers.ToArray();
    }

    void SetStickers(StickerController[] stickers)
    {
        if (stickers.Length > 4)
        {
            for (int i = 0; i < stickers.Length; i++)
            {
                //Debug.Log("StickArrayN = " + i + ", distance = " + stickers[i].GetDistance());
            }

            SComparer sc = new SComparer();
            Array.Sort(stickers, sc);

            for (int i = 0; i < stickers.Length; i++)
            {
               // Debug.Log("SORTED  StickArrayN = " + i + ", distance = " + stickers[i].GetDistance());

                if (i < 5) {
                    stickers[i].SetMarker(true);
                }
                else
                    stickers[i].SetMarker(false);
            }
        }
        else {
            for (int i = 0; i < stickers.Length; i++)
            {
                //Debug.Log("NO 5 StickArrayN = " + i + ", distance = " + stickers[i].GetDistance());

                stickers[i].SetMarker(true);
            }
        }
    }
}