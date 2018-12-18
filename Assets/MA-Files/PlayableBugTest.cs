using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableBugTest : MonoBehaviour {

    public bool GoToHappy;
    public bool GoToAngry;

    public AnimationClip happy;
    public AnimationClip angry;

    private Animator animator;
    private RuntimeAnimatorController runtimeAnimController;

    private PlayableGraph playableGraph;
    AnimationClipPlayable pHappy;
    AnimationClipPlayable pAngry;
    AnimationMixerPlayable mixerEmotionPlayable;


    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();

        playableGraph = PlayableGraph.Create("ClairePlayableGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

        // Create an Emotion Mixer
        mixerEmotionPlayable = AnimationMixerPlayable.Create(playableGraph, 4);
        playableOutput.SetSourcePlayable(mixerEmotionPlayable);

        // Wrap the clips in a playable
        pHappy = AnimationClipPlayable.Create(playableGraph, happy);
        pAngry = AnimationClipPlayable.Create(playableGraph, angry);

        // Connect to Emotion Mixer 
        //mixerEmotionPlayable.SetInputCount(5); // InputCount needs to be == to the number of connected clips (for normalization purposes)
        playableGraph.Connect(pHappy, 0, mixerEmotionPlayable, 0);
        playableGraph.Connect(pAngry, 0, mixerEmotionPlayable, 1);


        // Plays the Graph
        playableGraph.Play();
    }
	
	// Update is called once per frame
	void Update () {
		if(GoToHappy)
        {
            mixerEmotionPlayable.SetInputWeight(0, 0.7f);
            mixerEmotionPlayable.SetInputWeight(1, 0.3f);
        }
        if (GoToAngry)
        {
            mixerEmotionPlayable.SetInputWeight(0, 0.3f);
            mixerEmotionPlayable.SetInputWeight(1, 0.7f);
        }
        Debug.Log("Happy Wieght: " + mixerEmotionPlayable.GetInputWeight(0));
        Debug.Log("Angry Wieght: " + mixerEmotionPlayable.GetInputWeight(1));

    }


    void OnDisable()
    {

        // Destroys all Playables and PlayableOutputs created by the graph.

        playableGraph.Destroy();

    }
}
