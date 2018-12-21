using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


/*
 * A helper class to properly display GoTo options in the editor
 */
[System.Serializable]
public class StringBool
{
    public string name;
    public bool goToEmotion;

    public StringBool(string name, bool goToEmotion)
    {
        this.name = name;
        this.goToEmotion = goToEmotion;
    }
}

/*
 * [TODO] Different LERPS
 * [TODO] Handle transitions with TransitionIn clips
 */
[RequireComponent(typeof(Animator))]
public class PlayablesPrototypeV2 : MonoBehaviour
{
    // A mask should be giving in order to avoid overriding body animation
    public AvatarMask headMask;

    // Public Data Structures
    [HideInInspector]
    public List<Emotion> emotionObjects = new List<Emotion>();  // Contains Clips
    [HideInInspector]
    public List<StringBool> goToEmotionList;    // Contains emotion names and whether they should be transitioned to (For Editor purporses)

    // Private Data Structures
    private Dictionary<string, int> playablesDict;  // Find the correct PlayableInputID for a given animation clip
    private Dictionary<int, string> emotionDict;    // For a given int from iterating voer goToEmotionList, find the corresponding emotion
    private Queue<int> goToEmotionNext = new Queue<int>();  // Store transition calls to play them in FIFO order

    // Animators
    private Animator animator;
    private RuntimeAnimatorController runtimeAnimController;

    // Playables
    private PlayableGraph playableGraph;
    private AnimationMixerPlayable mixerEmotionPlayable;

    // Flags for Update
    private bool fPlayTransition = false;
    private bool fInitTransition = true;
    private bool fPlayMainAfterTransition;
    private bool fWithTransitionIn;
    private bool fPlayMain;
    private bool normalize = false;

    // Persisent Variables for Update
    private string currentlyPlaying;                // The currently playing emotion
    private string transitionEmotion;               // Emotion transitioning to
    private float lerpBlendDuration = 0.5f;         // How long a transition should take
    private float currentTime = 0f;                 // How much transition time has already elapsed
    private int emotionNumber = 0;                  // Which clips to play for a given emotion
    private int previousEmotionNumber = 0;




    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();

        // Create Playable Graph
        playableGraph = PlayableGraph.Create("ClairePlayableGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

        // Create Top Level Layer Mixer
        AnimationLayerMixerPlayable mixerLayerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerLayerPlayable);

        // Create an Emotion Mixer
        mixerEmotionPlayable = AnimationMixerPlayable.Create(playableGraph, 10);    // Second argument sets number of inputs for clips to connect. Overwritten later

        // Wrap AnimController
        runtimeAnimController = animator.runtimeAnimatorController;
        var runtimeAnimControllerPlayable = AnimatorControllerPlayable.Create(playableGraph, runtimeAnimController);

        // Connect to Top Level Layer Mixer
        playableGraph.Connect(runtimeAnimControllerPlayable, 0, mixerLayerPlayable, 0);
        playableGraph.Connect(mixerEmotionPlayable, 0, mixerLayerPlayable, 1);
        mixerLayerPlayable.SetInputWeight(0, 1.0f);
        mixerLayerPlayable.SetInputWeight(1, 1.0f);
        mixerLayerPlayable.SetLayerMaskFromAvatarMask(1, headMask);

        // Wrap the clips in a playable and connects them to the emotion mixer
        // Also create two dictionaries to later be able to access the AnimationClipPlayables
        playablesDict = new Dictionary<string, int>();      
        emotionDict = new Dictionary<int, string>();
        int playablesCount = 0;
        for (int i = 0; i < emotionObjects.Count; i++)
        {
            emotionDict.Add(i, emotionObjects[i].name);
            for (int j = 0; j < emotionObjects[i].animationGroupList.Count; j++)
            {
                if (emotionObjects[i].animationGroupList[j].main)
                {
                    playablesDict.Add(emotionObjects[i].name + j, playablesCount);
                    AnimationClipPlayable temp = AnimationClipPlayable.Create(playableGraph, emotionObjects[i].animationGroupList[j].main);
                    playableGraph.Connect(temp, 0, mixerEmotionPlayable, playablesCount);
                }
                playablesCount++;
                if (emotionObjects[i].animationGroupList[j].transitionIn)
                {
                    playablesDict.Add(emotionObjects[i].name + "TransitionIn" + j, playablesCount);
                    AnimationClipPlayable temp = AnimationClipPlayable.Create(playableGraph, emotionObjects[i].animationGroupList[j].transitionIn);
                    playableGraph.Connect(temp, 0, mixerEmotionPlayable, playablesCount);

                    temp.SetDuration(emotionObjects[i].animationGroupList[j].transitionIn.length);
                }
                playablesCount++;
            }
        }

        currentlyPlaying = emotionObjects[0].name;  // If not given, assume some default value

        // Plays the Graph
        playableGraph.Play();
    }

    private void LateUpdate()
    {
        // mixerEmotionPlayable.SetInputWeight((int)PlayablesEnum.TPose, 0.0f); // Set TPose to 0

        // Check if new Transitions were requested and add them to the queue
        for(int i=0; i < goToEmotionList.Count; i++)
        {
            if(goToEmotionList[i].goToEmotion) 
            {
                goToEmotionNext.Enqueue(i);
                goToEmotionList[i].goToEmotion = false;
            }
        }

        // If no transition is playing, prepare next transition
        if(!fPlayTransition && goToEmotionNext.Count > 0)
        {
            transitionEmotion = emotionDict[goToEmotionNext.Dequeue()];
            fPlayTransition = true;
            for (int j = 0; j < emotionObjects.Count; j++)
            {
                if (emotionObjects[j].name.Equals(transitionEmotion))
                {
                    emotionNumber = Random.Range(0, emotionObjects[j].animationGroupList.Count);
                    //Debug.Log("Emotion Number: " + emotionNumber + " for Emotion: " + transitionEmotion);
                }
            }
            if (playablesDict.ContainsKey(transitionEmotion + "TransitionIn" + emotionNumber))
            {
                fWithTransitionIn = true;
            }
            else
            {
                fWithTransitionIn = false;
            }

            // Avoid transitioning to the same emotion if there is no TransitionIn animation
            if(!fWithTransitionIn && (transitionEmotion.Equals(currentlyPlaying)))
            {
                fPlayTransition = false;
            }
        }

        // Transition
        if (fPlayTransition && fWithTransitionIn)
        {
            // Initialize Transition when it begins
            if (fInitTransition)
            {
                mixerEmotionPlayable.GetInput(playablesDict[transitionEmotion + "TransitionIn" + emotionNumber]).SetTime(0f);
                mixerEmotionPlayable.GetInput(playablesDict[transitionEmotion + "TransitionIn" + emotionNumber]).SetDone(false);

                fInitTransition = false;
            }

            // LERP Transition and currently playing emotion
            // Interpolation website: http://paulbourke.net/miscellaneous/interpolation/
            currentTime += Time.deltaTime;
            float mu = currentTime / lerpBlendDuration;
            float t;
            float upcomingBlendWeight;

            // Linear Interpolation
            //t = mu;
            //upcomingBlendWeight = Mathf.Lerp(0, 1, t);

            // Cosine Interpolation
            t = (1 - Mathf.Cos(mu * Mathf.PI)) / 2;
            upcomingBlendWeight = Mathf.Lerp(0, 1, t);

            // Bezier, B-Spline? 4-point interpolation, so they need two manually defined points. No recommendations?
            // S. 148, kurze Erwänung unter Nonlinear Interpolation: https://books.google.de/books?id=yEzrBgAAQBAJ&pg=PA147&lpg=PA147&dq=facial+animation+transition+interpolation&source=bl&ots=ntfbn7k6ww&sig=FDfcPAnj_mr76FcedSKEx4zrjRU&hl=de&sa=X&ved=2ahUKEwi_u42K3q7fAhUHmYsKHfYJDncQ6AEwA3oECAcQAQ#v=onepage&q=facial%20animation%20transition%20interpolation&f=false
            // Linear, Cubic B-Spline, Cardinal Spline: https://www.researchgate.net/publication/44250675_Parametric_Facial_Expression_Synthesis_and_Animation
            // Best Bezier explanation ever: https://denisrizov.com/2016/06/02/bezier-curves-unity-package-included/



            mixerEmotionPlayable.SetInputWeight(playablesDict[currentlyPlaying + previousEmotionNumber], 1f - upcomingBlendWeight);
            mixerEmotionPlayable.SetInputWeight(playablesDict[transitionEmotion + "TransitionIn" + emotionNumber], upcomingBlendWeight);

            // End Transition CleanUp, Prepare playing Main
            if (currentTime >= lerpBlendDuration)
            {
                fPlayTransition = false;
                fInitTransition = true;
                fPlayMainAfterTransition = true;
                currentlyPlaying = transitionEmotion;
                currentTime = 0;
                previousEmotionNumber = emotionNumber;
            }
            normalize = true;
        }

        if (fPlayTransition && !fWithTransitionIn)
        {
            // LERP Transition and currently playing emotion
            // Interpolation website: http://paulbourke.net/miscellaneous/interpolation/
            currentTime += Time.deltaTime;
            float mu = currentTime / lerpBlendDuration;
            float t;
            float upcomingBlendWeight;

            // Linear Interpolation
            //t = mu;
            //upcomingBlendWeight = Mathf.Lerp(0, 1, t);

            // Cosine Interpolation
            t = (1 - Mathf.Cos(mu * Mathf.PI)) / 2;
            upcomingBlendWeight = Mathf.Lerp(0, 1, t);

            // 


            mixerEmotionPlayable.SetInputWeight(playablesDict[currentlyPlaying + previousEmotionNumber], 1f - upcomingBlendWeight);
            mixerEmotionPlayable.SetInputWeight(playablesDict[transitionEmotion + emotionNumber], upcomingBlendWeight);

            // End Transition CleanUp, Prepare playing Main
            if (currentTime >= lerpBlendDuration)
            {
                fPlayTransition = false;
                currentlyPlaying = transitionEmotion;
                currentTime = 0;
                previousEmotionNumber = emotionNumber;
            }
            normalize = true;
        }

        // Play main emotion slightly before transition ends to avoid the 1 "neutral" frame that occurs when using "isDone()"
        if (fPlayMainAfterTransition && ((mixerEmotionPlayable.GetInput(playablesDict[transitionEmotion + "TransitionIn" + emotionNumber]).GetDuration() - mixerEmotionPlayable.GetInput(playablesDict[transitionEmotion + "TransitionIn" + emotionNumber]).GetTime()) < 0.1)) //(mixerEmotionPlayable.GetInput(1).GetTime() >= mixerEmotionPlayable.GetInput(1).GetDuration())
        {
            Debug.Log("Currently PLaying: " + currentlyPlaying);
            fPlayMainAfterTransition = false;
            mixerEmotionPlayable.SetInputWeight(playablesDict[currentlyPlaying + "TransitionIn" + emotionNumber], 0.0f);   // Deactivate Transition
            mixerEmotionPlayable.SetInputWeight(playablesDict[currentlyPlaying + emotionNumber], 1.0f);   // Active Main
            mixerEmotionPlayable.GetInput(playablesDict[currentlyPlaying + emotionNumber]).SetTime(0f);
            normalize = true;
        }


        // If weights were changed, normalize them
        if (normalize) normalizeWeights();

        //addInTPoseIfNecessary();

        //Debug.Log("Happy Wieght: " + mixerEmotionPlayable.GetInputWeight(0));
        //Debug.Log("Angry Wieght: " + mixerEmotionPlayable.GetInputWeight(2));
        //Debug.Log("TPose Wieght: " + mixerEmotionPlayable.GetInputWeight(4));
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
        normalize = false;
    }

    /* // Hopefully unnecessary
    void addInTPoseIfNecessary()
    {
        float weightSum = 0;
        for (int i = 0; i < mixerEmotionPlayable.GetInputCount(); i++)
        {
            weightSum += mixerEmotionPlayable.GetInputWeight(i);
        }
        if (weightSum < 1f)
        {
            mixerEmotionPlayable.SetInputWeight((int)PlayablesEnum.TPose, 1f - weightSum);
        }
    }
    */

    /*
     * Updates the emotions used by this class,
     * by comparing existing emotions to new emotions.
     * New ones are added, missing ones are removed.
     * Used by the Controller.
     */
    public void updateEmotionList(List<string> newEmotions)
    {
        // Add new emotions
        foreach (string emotion in newEmotions)
        {
            bool contains = false;
            for (int i = 0; i < emotionObjects.Count; i++)
            {
                if (emotionObjects[i].name.Equals(emotion)) contains = true;
            }
            if (!contains)
            {
                Debug.Log("Emotion: " + emotion);
                emotionObjects.Add(new Emotion(emotion));
            }
        }
        // Delete removed emotions
        for (int i = 0; i < emotionObjects.Count; i++)
        {
            if (!newEmotions.Contains(emotionObjects[i].name))
            {
                emotionObjects.Remove(emotionObjects[i]);
                i--;
            }
        }



        goToEmotionList = new List<StringBool>();
        foreach (string emotion in newEmotions)
        {
            goToEmotionList.Add(new StringBool(emotion, false));
        }
    }

    void OnDisable()
    {

        // Destroys all Playables and PlayableOutputs created by the graph.

        playableGraph.Destroy();

    }
}

