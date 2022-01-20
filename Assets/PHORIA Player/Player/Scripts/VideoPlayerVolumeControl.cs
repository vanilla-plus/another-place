using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using RenderHeads.Media.AVProVideo;

public class VideoPlayerVolumeControl : MonoBehaviour
{
    //public MediaPlayer VideoPlayerComponent; //the video component to use
    public PHUI_Volume_Slider VolumeSliderComponent;

    public MediaPlayer mediaPlayer;
    public VideoController VideoPlayerController;
    private Slider VolumeSliderController;

    public GameObject audioSliderPrefab;
    public GameObject audioSliderLayoutGroup;

    public List<AudioSource> audioSources;
    public int sliderIndex;
    public List<GameObject> audioSourceSliders;

    // Start is called before the first frame update
    void Start()
    {
        //VideoPlayerController = VideoPlayerComponent.GetComponent<VideoController>();

        VolumeSliderController = VolumeSliderComponent.GetComponentInChildren<Slider>();

        //VolumeSliderComponent.SetSliderValue(VideoPlayerController.GetVideoVolume());
    }

    // Update is called once per frame
    public void SetTrackVolumeFromSlider(int sliderTrackTargedID, float volume)
    {
        
        VideoPlayerController.SetVideoVolume(sliderTrackTargedID, volume);
    }

    public void SetVideoPlayerVolumeFromSlider(float volume)
    {
        VideoPlayerController.SetPlayerVolume(volume);
    }

    

    public void AddAudioTrackToControl(AudioSource audioSource, string label)
    {
        Debug.Log("Audio Track adding hunt stage 2 -  now we're setting up the slider");

        GameObject newSlider = Instantiate(audioSliderPrefab);
        PHUI_Volume_Slider sliderHandler = newSlider.GetComponent<PHUI_Volume_Slider>();
        //Debug.Log("AddAudioTrackToControl : setting label: " + label + " and source: "+ audioSource);
        sliderHandler.LabelForSlider = label;
        sliderHandler.volumeController = this;
        newSlider.transform.SetParent(audioSliderLayoutGroup.transform);
        newSlider.transform.localPosition = Vector3.zero;
        newSlider.transform.localScale = Vector3.one;
        sliderHandler.sliderID = sliderIndex;
        sliderHandler.SetSliderValue(AppManager.Instance.VideoController.audioTracks[sliderIndex].volume);
        audioSourceSliders.Add(newSlider);

        sliderIndex++;

    }

    public void ClearAudioTrackSliders()
    {
        //Debug.Log("I've been summoned to clear the sliders, by my healthy friend");
        //Debug.Log("Audio Track adding hunt stage 4 -  and now I'm removing the sliders");

        foreach (GameObject slider in audioSourceSliders)
        {

            //Debug.Log("Audio Track adding hunt stage 4- -  a slider is being removed");

            Destroy(slider);
        }


        sliderIndex = 0;
        audioSourceSliders.Clear();
    }

}

// add slider for every audio track
// remove sliders and audio tracks on stop/go home