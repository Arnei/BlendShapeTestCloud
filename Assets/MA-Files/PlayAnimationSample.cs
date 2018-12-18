using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;


[RequireComponent(typeof(Animator))]
public class PlayAnimationSample : MonoBehaviour
{
    public AnimationClip clip;
    public AnimationClip clip1;

    public AvatarMask headMask;

    PlayableGraph playableGraph;

    private Animator animator;
    private RuntimeAnimatorController runtimeAnimController;
    //private AnimatorController animController;

    void Start()
    {

       


 

        playableGraph = PlayableGraph.Create("ClairePlayableGraph");

        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        // Create a Mixer
        AnimationLayerMixerPlayable mixerLayerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, 2);


        playableOutput.SetSourcePlayable(mixerLayerPlayable);

        // Wrap the clip in a playable

        var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        var clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);

        
        // Wrap AnimController

        runtimeAnimController = GetComponent<Animator>().runtimeAnimatorController;
        //animController = (AnimatorController)animator.runtimeAnimatorController;
        var runtimeAnimControllerPlayable = AnimatorControllerPlayable.Create(playableGraph, runtimeAnimController);
        

        // Connect to Mixer 

        playableGraph.Connect(runtimeAnimControllerPlayable, 0, mixerLayerPlayable, 1);
        playableGraph.Connect(clipPlayable, 0, mixerLayerPlayable, 0);
  
        mixerLayerPlayable.SetInputWeight(0, 1.0f);
        mixerLayerPlayable.SetInputWeight(1, 1.0f);

        //mixerLayerPlayable.SetLayerAdditive(0, true);
        mixerLayerPlayable.SetLayerAdditive(1, true);
        Debug.Log("Is layer 0 additive: " + mixerLayerPlayable.IsLayerAdditive(0));
        Debug.Log("Is layer 1 additive: " + mixerLayerPlayable.IsLayerAdditive(1));

        //mixerLayerPlayable.SetLayerMaskFromAvatarMask(1, headMask);

        // Connect the Playable to an output

        //playableOutput.SetSourcePlayable(clipPlayable);

        // Plays the Graph.

        playableGraph.Play();
    }

    void OnDisable()
    {

        // Destroys all Playables and PlayableOutputs created by the graph.

        playableGraph.Destroy();

    }
}