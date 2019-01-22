using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableBugTest : MonoBehaviour {

    public bool GoToHappy;
    public bool GoToAngry;
    public GameObject childwithSkinnedMeshRenderer;
    public AvatarMask headMask;

    public AnimationClip happy;
    public AnimationClip angry;

    private Animator animator;
    private RuntimeAnimatorController runtimeAnimController;

    private PlayableGraph playableGraph;
    AnimationClipPlayable pHappy;
    AnimationClipPlayable pAngry;
    AnimationMixerPlayable mixerEmotionPlayable;

    SkinnedMeshRenderer smr;
    Mesh mesh;
    int blendShapeCount;



    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        smr = childwithSkinnedMeshRenderer.GetComponent<SkinnedMeshRenderer>();
        blendShapeCount = smr.sharedMesh.blendShapeCount;

        playableGraph = PlayableGraph.Create("ClairePlayableGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

        // Create Top Level Layer Mixer
        AnimationLayerMixerPlayable mixerLayerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerLayerPlayable);

        // Wrap AnimController
        runtimeAnimController = animator.runtimeAnimatorController;
        var runtimeAnimControllerPlayable = AnimatorControllerPlayable.Create(playableGraph, runtimeAnimController);

        // Create an Emotion Mixer
        mixerEmotionPlayable = AnimationMixerPlayable.Create(playableGraph, 4);
        //playableOutput.SetSourcePlayable(mixerEmotionPlayable);

        // Connect to Top Level Layer Mixer
        playableGraph.Connect(runtimeAnimControllerPlayable, 0, mixerLayerPlayable, 0);
        playableGraph.Connect(mixerEmotionPlayable, 0, mixerLayerPlayable, 1);
        mixerLayerPlayable.SetInputWeight(0, 1.0f);
        mixerLayerPlayable.SetInputWeight(1, 1.0f);
        mixerLayerPlayable.SetLayerMaskFromAvatarMask(1, headMask);

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
        animator.runtimeAnimatorController = null;
		if(GoToHappy)
        {
            for(int i=0; i < blendShapeCount; i++)
            {
                smr.SetBlendShapeWeight(i, 0f);
            }

            mixerEmotionPlayable.SetInputWeight(0, 1.0f);
            mixerEmotionPlayable.SetInputWeight(1, 0.00f);
        }
        if (GoToAngry)
        {
            for (int i = 0; i < blendShapeCount; i++)
            {
                smr.SetBlendShapeWeight(i, 0f);
            }
            mixerEmotionPlayable.SetInputWeight(0, 0.00f);
            mixerEmotionPlayable.SetInputWeight(1, 1.0f);
        }
        //Debug.Log("Happy Wieght: " + mixerEmotionPlayable.GetInputWeight(0));
        //Debug.Log("Angry Wieght: " + mixerEmotionPlayable.GetInputWeight(1));

        animator.runtimeAnimatorController = runtimeAnimController;
    }


    void OnDisable()
    {

        // Destroys all Playables and PlayableOutputs created by the graph.

        playableGraph.Destroy();

    }
}
