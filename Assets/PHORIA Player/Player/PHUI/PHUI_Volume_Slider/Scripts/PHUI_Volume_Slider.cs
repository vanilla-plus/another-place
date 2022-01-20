using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PHUI_Volume_Slider : MonoBehaviour
{
    public VideoPlayerVolumeControl volumeController;
    public string LabelForSlider;
    public Slider VolumeSlider;
    public TextMeshProUGUI SliderLabelText;
    public TextMeshProUGUI SliderStatusText;
    public int sliderID;

    // Start is called before the first frame update
    void Start()
    {
        SliderLabelText.text = LabelForSlider;
        SetSliderStatusText();
    }

    public void SetSliderValue(float sliderValue)
    {
        VolumeSlider.value = sliderValue;
        SetSliderStatusText();
    }

    public void SetAssociatedAudioTrackVolume()
    {
        volumeController.SetTrackVolumeFromSlider(sliderID, VolumeSlider.value);
    }

    public void SetPlayerVolumeDirectly()
    {
        volumeController.SetVideoPlayerVolumeFromSlider(VolumeSlider.value);
    }

    public void SetSliderStatusText()
    {
        SliderStatusText.text = VolumeSlider.value.ToString("#0%");
    }
}
