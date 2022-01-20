using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Vanilla.StringFormatting;

public class PHUI_Progress_Spinner : MonoBehaviour
{

    private Selection_Tile _tile;
    
    public Text ProgressText;
    public Image FillImage;


    private void Awake()
    {
        _tile = GetComponentInParent<Selection_Tile>();

//        _tile.onDownloadBytesUpdate.AddListener(bytes => ProgressText.text = bytes.AsDataSize());

        _tile.onDownloadPercentUpdate.AddListener(percent =>
                                                  {
                                                      FillImage.fillAmount = percent;
                                                      ProgressText.text    = $"{Mathf.FloorToInt(percent * 100.0f)}%";
                                                  });
    }
    
//    public void SetProgressText(string progressText)
//    {
//        ProgressText.text = progressText;
//    }
//
//    public void SetProgressValue(float progressValue)
//    {
//        FillImage.fillAmount = progressValue;
//        SetProgressText(String.Format("{0:0}", progressValue*100));
//    }

}
