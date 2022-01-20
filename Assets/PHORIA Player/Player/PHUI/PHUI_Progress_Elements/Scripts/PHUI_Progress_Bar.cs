using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PHUI_Progress_Bar : MonoBehaviour
{

    public TextMeshProUGUI ProgressText;
    public Image FillImage;
    public bool UseMarker;
    public GameObject ProgessMarker;
    private RectTransform ProgessMarkerTransform;

    private void Awake()
    {
        if (!UseMarker)
        {
            ProgessMarker.SetActive(false);
        }
        else
        {
            ProgessMarkerTransform = ProgessMarker.GetComponent<RectTransform>();
        }
    }
    public void SetProgressText(string progressText)
    {
        ProgressText.text = progressText;
    }

    public void SetProgressValue(float progressValue)
    {
        FillImage.fillAmount = progressValue;
        if (UseMarker && progressValue > 0f)
        {
            ProgessMarkerTransform.anchorMin = new Vector2(progressValue, 0f);
            ProgessMarkerTransform.anchorMax = new Vector2(progressValue, 1f);
            ProgessMarkerTransform.anchoredPosition = Vector2.zero;
        }
    }

}
