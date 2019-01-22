using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class ExpressionMixingDemoPlayables : MonoBehaviour
{
    private Animator animator;
    private RuntimeAnimatorController runtimeAnimController;

    private PlayableGraph playableGraph;
    AnimationClipPlayable pInterested;
    AnimationClipPlayable pInterested2;
    AnimationClipPlayable pEntertained;
    AnimationClipPlayable pUncomfortable;
    AnimationClipPlayable pConfused;
    AnimationClipPlayable pBored;
    AnimationClipPlayable pNeutral;
    AnimationMixerPlayable mixerEmotionPlayable;

    public AvatarMask headMask;

    public AnimationClip interested;
    public AnimationClip interested2;
    public AnimationClip entertained;
    public AnimationClip uncomfortable;
    public AnimationClip confused;
    public AnimationClip bored;
    public AnimationClip neutral;

    public float weightInterested;
    public float weightInterested2;
    public float weightEntertained;
    public float weightUncomfortable;
    public float weightConfused;
    public float weightBored;
    public float weightNeutral;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        //smr = childwithSkinnedMeshRenderer.GetComponent<SkinnedMeshRenderer>();
        //blendShapeCount = smr.sharedMesh.blendShapeCount;

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
        mixerEmotionPlayable = AnimationMixerPlayable.Create(playableGraph, 10, true);
        //playableOutput.SetSourcePlayable(mixerEmotionPlayable);

        // Connect to Top Level Layer Mixer
        playableGraph.Connect(runtimeAnimControllerPlayable, 0, mixerLayerPlayable, 0);
        playableGraph.Connect(mixerEmotionPlayable, 0, mixerLayerPlayable, 1);
        mixerLayerPlayable.SetInputWeight(0, 1.0f);
        mixerLayerPlayable.SetInputWeight(1, 1.0f);
        mixerLayerPlayable.SetLayerMaskFromAvatarMask(1, headMask);

        // Wrap the clips in a playable
        pInterested = AnimationClipPlayable.Create(playableGraph, interested);
        pInterested2 = AnimationClipPlayable.Create(playableGraph, interested2);
        pEntertained = AnimationClipPlayable.Create(playableGraph, entertained);
        pUncomfortable = AnimationClipPlayable.Create(playableGraph, uncomfortable);
        pConfused = AnimationClipPlayable.Create(playableGraph, confused);
        pBored = AnimationClipPlayable.Create(playableGraph, bored);
        pNeutral = AnimationClipPlayable.Create(playableGraph, neutral);

        // Connect to Emotion Mixer 
        //mixerEmotionPlayable.SetInputCount(5); // InputCount needs to be == to the number of connected clips (for normalization purposes)
        playableGraph.Connect(pInterested, 0, mixerEmotionPlayable, 0);
        playableGraph.Connect(pInterested2, 0, mixerEmotionPlayable, 1);
        playableGraph.Connect(pEntertained, 0, mixerEmotionPlayable, 2);
        playableGraph.Connect(pUncomfortable, 0, mixerEmotionPlayable, 3);
        playableGraph.Connect(pConfused, 0, mixerEmotionPlayable, 4);
        playableGraph.Connect(pBored, 0, mixerEmotionPlayable, 5);
        playableGraph.Connect(pNeutral, 0, mixerEmotionPlayable, 6);


        // Plays the Graph
        playableGraph.Play();
    }

    // Update is called once per frame
    void Update()
    {

        mixerEmotionPlayable.SetInputWeight(0, weightInterested);
        mixerEmotionPlayable.SetInputWeight(1, weightInterested2);
        mixerEmotionPlayable.SetInputWeight(2, weightEntertained);
        mixerEmotionPlayable.SetInputWeight(3, weightUncomfortable);
        mixerEmotionPlayable.SetInputWeight(4, weightConfused);
        mixerEmotionPlayable.SetInputWeight(5, weightBored);
        mixerEmotionPlayable.SetInputWeight(6, weightNeutral);
        normalizeWeights();
    }

    // Normalize Weights in mixerEmotionPlayable
    void normalizeWeights()
    {
        int length = mixerEmotionPlayable.GetInputCount();
        float sumOfWeights = 0;
        for (int i = 0; i < length; i++)
        {
            if (mixerEmotionPlayable.GetInputWeight(i) > 0f) sumOfWeights += mixerEmotionPlayable.GetInputWeight(i);
        }
        for (int i = 0; i < length; i++)
        {
            if (mixerEmotionPlayable.GetInputWeight(i) > 0f)
            {
                mixerEmotionPlayable.SetInputWeight(i, mixerEmotionPlayable.GetInputWeight(i) / sumOfWeights);
            }
        }
        //normalize = false;
    }

    public void setToZero()
    {
        weightInterested = 0;
        weightInterested2 = 0;
        weightEntertained = 0;
        weightUncomfortable = 0;
        weightBored = 0;
        weightConfused = 0;
        weightNeutral = 0;
        normalizeWeights();
    }

    void OnDisable()
    {

        // Destroys all Playables and PlayableOutputs created by the graph.

        playableGraph.Destroy();

    }
}
