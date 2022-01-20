using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioManagerSimple : MonoBehaviour
{

    public AudioSource sfxSource;
    public AudioSource menuMusic;
    public float menuMax;
    public float menuMin;
    bool lerping;
    public IEnumerator LerpVolume;

    public AudioClip[] sfx;

    public static AudioManagerSimple i;

    public UnityEvent LerpStart = new UnityEvent();
    public UnityEvent LerpFinished = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        i = this;
        menuMax = menuMusic.volume;

    }



    public void PlayOneShot(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
;    }

    public void PlayOneShotInt(int ID)
    {
        sfxSource.PlayOneShot(sfx[ID]);
    }

    public void PlayMenuMusic()
    {
        HandleLerp(true, menuMax);
        menuMusic.Play();
    }

    public void StopMenuMusic()
    {
        menuMusic.Stop();
        LerpFinished.RemoveAllListeners();

    }

    public void RaiseMenuMusic()
    {
        HandleLerp(true, menuMax);
    }

    public void LowerMenuMusic()
    {
        HandleLerp(false, menuMin);
    }

    public void FadeMenuMusic()
    {
        HandleLerp(false, 0f);
        LerpFinished.AddListener(StopMenuMusic);
    }

    public void HandleLerp(bool up, float volValue)
    {
        if (lerping) StopCoroutine(LerpVolume);
        lerping = false;
        LerpVolume = LerpMenuVolume(up, volValue);
        StartCoroutine(LerpVolume);
    }

    public IEnumerator LerpMenuVolume(bool up, float volValue)
    {
        LerpStart.Invoke();
        lerping = true;
        float normal = 0;
        float currentVolume = menuMusic.volume;
        while (normal < 1)
        {
            normal += Time.deltaTime / 2;
            if (up)
            {
                menuMusic.volume = Mathf.Lerp(0, volValue, normal);
            }
            else
            {
                menuMusic.volume = Mathf.Lerp(currentVolume, volValue, normal);
            }

            yield return null;
        }
        LerpFinished.Invoke();
        lerping = false;
    }
}
