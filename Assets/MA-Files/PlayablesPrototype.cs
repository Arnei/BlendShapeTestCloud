using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class PlayablesPrototype : MonoBehaviour {

    public bool GoToHappy;
    public bool GoToAngry;

    public AnimationClip tPose;
    public AnimationClip happy;
    public AnimationClip happyTransitionIn;
    public AnimationClip angry;
    public AnimationClip angryTransitionIn;

    public AvatarMask headMask;


    private bool GoToHappyStart = true;
    private bool canPlayHappyMain;
    private bool canPlayAngryMain;
    private bool normalize = false;

    private Animator animator;
    private RuntimeAnimatorController runtimeAnimController;

    private PlayableGraph playableGraph;

    AnimationClipPlayable pTPose;
    AnimationClipPlayable pHappy;
    AnimationClipPlayable pHappyTransitionIn;
    AnimationClipPlayable pAngry;
    AnimationClipPlayable pAngryTransitionIn;
    AnimationMixerPlayable mixerEmotionPlayable;

    private float lerpBlendDuration = 0.5f;
    private string currentlyPlaying = "TPose";
    private float currentTime = 0f;


    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();

        playableGraph = PlayableGraph.Create("ClairePlayableGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

        // Create Top Level Layer Mixer
        AnimationLayerMixerPlayable mixerLayerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerLayerPlayable);

        // Create an Emotion Mixer
        mixerEmotionPlayable = AnimationMixerPlayable.Create(playableGraph, 4);

        // Wrap AnimController
        runtimeAnimController = animator.runtimeAnimatorController;
        var runtimeAnimControllerPlayable = AnimatorControllerPlayable.Create(playableGraph, runtimeAnimController);

        // Connect to Top Level Layer Mixer
        playableGraph.Connect(runtimeAnimControllerPlayable, 0, mixerLayerPlayable, 0);
        playableGraph.Connect(mixerEmotionPlayable, 0, mixerLayerPlayable, 1);
        mixerLayerPlayable.SetInputWeight(0, 1.0f);
        mixerLayerPlayable.SetInputWeight(1, 1.0f);
        mixerLayerPlayable.SetLayerMaskFromAvatarMask(1, headMask);

        // Wrap the clips in a playable
        pHappy = AnimationClipPlayable.Create(playableGraph, happy);
        pHappyTransitionIn = AnimationClipPlayable.Create(playableGraph, happyTransitionIn);
        pAngry = AnimationClipPlayable.Create(playableGraph, angry);
        pAngryTransitionIn = AnimationClipPlayable.Create(playableGraph, angryTransitionIn);
        pTPose = AnimationClipPlayable.Create(playableGraph, tPose);

        // Setup Durations for IsDone Checks
        pHappyTransitionIn.SetDuration(happyTransitionIn.length);
        pAngryTransitionIn.SetDuration(angryTransitionIn.length);

        // Connect to Emotion Mixer 
        mixerEmotionPlayable.SetInputCount(5); // InputCount needs to be == to the number of connected clips (for normalization purposes)
        playableGraph.Connect(pHappy, 0, mixerEmotionPlayable, 0);
        playableGraph.Connect(pHappyTransitionIn, 0, mixerEmotionPlayable, 1);
        playableGraph.Connect(pAngry, 0, mixerEmotionPlayable, 2);
        playableGraph.Connect(pAngryTransitionIn, 0, mixerEmotionPlayable, 3);
        playableGraph.Connect(pTPose, 0, mixerEmotionPlayable, 4);

        Debug.Log("INputLength: " + mixerEmotionPlayable.GetInputCount());



        // Activate T Pose
        mixerEmotionPlayable.SetInputWeight(4, 1.0f);

        Debug.Log("Happy Transition Time: " + pHappyTransitionIn.GetDuration());

        // Plays the Graph
        playableGraph.Play();
    }

    private void LateUpdate()
    {
        mixerEmotionPlayable.SetInputWeight(4, 0.0f); // Set TPose to 0

        if (GoToHappy)
        {
            if(GoToHappyStart)
            {
                mixerEmotionPlayable.GetInput(1).SetTime(0f);
                mixerEmotionPlayable.GetInput(1).SetDone(false);
                GoToHappyStart = false;
            }

            currentTime += Time.deltaTime;
            float upcomingBlendWeight = Mathf.Lerp(0, 1, currentTime / lerpBlendDuration);
            mixerEmotionPlayable.SetInputWeight(1, upcomingBlendWeight);


            if (currentlyPlaying == "Happy")
            {
                mixerEmotionPlayable.SetInputWeight(0, 1f - upcomingBlendWeight);   
            }
            else if(currentlyPlaying == "Angry")
            {
                mixerEmotionPlayable.SetInputWeight(2, (1f - upcomingBlendWeight));
            }


            if (currentTime >= lerpBlendDuration)
            {
                GoToHappy = false;
                GoToHappyStart = true;
                canPlayHappyMain = true;
                currentlyPlaying = "Happy";
                currentTime = 0;
            }
            normalize = true;
        }
        if (canPlayHappyMain && ((mixerEmotionPlayable.GetInput(1).GetDuration() - mixerEmotionPlayable.GetInput(1).GetTime()) < 0.1)) //(mixerEmotionPlayable.GetInput(1).GetTime() >= mixerEmotionPlayable.GetInput(1).GetDuration())
        {
            canPlayHappyMain = false;
            mixerEmotionPlayable.SetInputWeight(1, 0.0f);   // Deactivate Transition
            mixerEmotionPlayable.SetInputWeight(0, 1.0f);   // Active Main
            mixerEmotionPlayable.GetInput(0).SetTime(0f);
            normalize = true;
        }

        if (GoToAngry)
        {
            GoToAngry = false;
            canPlayAngryMain = true;
            mixerEmotionPlayable.SetInputWeight(3, 1.0f);   // Set AngryTransition In to Active
            mixerEmotionPlayable.SetInputWeight(0, 0.0f);   // Deactivate Main
            mixerEmotionPlayable.SetInputWeight(2, 0.0f);
            mixerEmotionPlayable.GetInput(3).SetTime(0f);
            mixerEmotionPlayable.GetInput(3).SetDone(false);
            normalize = true;
        }
        if (canPlayAngryMain && ((mixerEmotionPlayable.GetInput(3).GetDuration() - mixerEmotionPlayable.GetInput(3).GetTime()) < 0.1)) //(mixerEmotionPlayable.GetInput(1).GetTime() >= mixerEmotionPlayable.GetInput(1).GetDuration())
        {
            canPlayAngryMain = false;
            mixerEmotionPlayable.SetInputWeight(3, 0.0f);   // Deactivate Transition
            mixerEmotionPlayable.SetInputWeight(2, 1.0f);   // Active Main
            mixerEmotionPlayable.GetInput(2).SetTime(0f);

            currentlyPlaying = "Angry";
            normalize = true;
        }



        if(normalize) normalizeWeights();

        //addInTPoseIfNecessary();

        //Debug.Log("Happy Wieght: " + mixerEmotionPlayable.GetInputWeight(0));
        //Debug.Log("Angry Wieght: " + mixerEmotionPlayable.GetInputWeight(2));
        //Debug.Log("TPose Wieght: " + mixerEmotionPlayable.GetInputWeight(4));
    }


    void normalizeWeights()
    {
        int length = mixerEmotionPlayable.GetInputCount();
        float sumOfWeights = 0;
        for (int i=0; i < length; i++)
        {
            if (mixerEmotionPlayable.GetInputWeight(i) > 0f) sumOfWeights += mixerEmotionPlayable.GetInputWeight(i);
        }
        for (int i=0; i < length; i++)
        {
            if (mixerEmotionPlayable.GetInputWeight(i) > 0f)
            {
                mixerEmotionPlayable.SetInputWeight(i, mixerEmotionPlayable.GetInputWeight(i) / sumOfWeights);
            }
        }

        normalize = false;
    }

    void addInTPoseIfNecessary()
    {
        float weightSum = 0;
        for (int i = 0; i < mixerEmotionPlayable.GetInputCount(); i++)
        {
            weightSum += mixerEmotionPlayable.GetInputWeight(i);
        }
        if(weightSum < 1f)
        {
            mixerEmotionPlayable.SetInputWeight(4, 1f - weightSum);
        }
    }

    void OnDisable()
    {

        // Destroys all Playables and PlayableOutputs created by the graph.

        playableGraph.Destroy();

    }
}

