using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class LipSyncDemoSceneStartStop : MonoBehaviour
{
    public GameObject claireEnergy;
    public GameObject clairePhoneme;
    public GameObject claireMoCap;
    public GameObject objWithVideo;

    private AudioSource energyAudio;
    private AudioSource phonemeAudio;
    private VideoPlayer videoPlayer;
    private Animator moCapAnimator;

    // Start is called before the first frame update
    void Start()
    {
        energyAudio = claireEnergy.GetComponent<AudioSource>();
        phonemeAudio = clairePhoneme.GetComponent<AudioSource>();
        moCapAnimator = claireMoCap.GetComponent<Animator>();
        videoPlayer = objWithVideo.GetComponent<VideoPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayPause()
    {
        Debug.Log("PlayPause triggered");
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            energyAudio.Pause();
            phonemeAudio.Pause();
            moCapAnimator.enabled = false;
        }
        else
        {
            videoPlayer.Play();
            energyAudio.Play();
            phonemeAudio.Play();
            moCapAnimator.enabled = true;
        }
    }
}
