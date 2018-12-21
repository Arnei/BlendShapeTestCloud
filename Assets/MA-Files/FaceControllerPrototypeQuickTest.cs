using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FaceControllerPrototypeQuickTest : MonoBehaviour {

    public bool goToHappy = false;
    public bool goToAngry = false;
    public bool goToNeutral = false;

    private bool canPlayHappyMain = false;
    private bool canPlayAngryMain = false;
    private Animator animator;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if(goToHappy)
        {
            animator.SetFloat("HappyTransitionIn", 1);
            animator.Play("HappyTransitionIn", -1, 0);

            
        }
        //if (animator.GetCurrentAnimatorStateInfo(1).IsName("Emotion Tree.Emotion Tree")) Debug.Log("This is an Emotion Tree");
        //if (animator.GetCurrentAnimatorStateInfo(1).IsName("Emotion Tree.Happy")) Debug.Log("This is a Happy");

        AnimatorClipInfo[] animClipInf = animator.GetCurrentAnimatorClipInfo(1);
        //Debug.Log("AnimClipInfo Length: " + animClipInf.Length);
        //Debug.Log("AnimClipInfo 0 Name: " + animClipInf[0].clip.name);


        /*
        if (goToHappy)
        {
            goToHappy = false;
            canPlayHappyMain = true;
            mixerEmotionPlayable.SetInputWeight(1, 1.0f);   // Set HappyTransition In to Active
            mixerEmotionPlayable.SetInputWeight(0, 0.0f);   // Deactivate Main
            mixerEmotionPlayable.SetInputWeight(2, 0.0f);
            mixerEmotionPlayable.GetInput(1).SetTime(0f);
            mixerEmotionPlayable.GetInput(1).SetDone(false);
        }
        if (canPlayHappyMain && ((mixerEmotionPlayable.GetInput(1).GetDuration() - mixerEmotionPlayable.GetInput(1).GetTime()) < 0.1)) //(mixerEmotionPlayable.GetInput(1).GetTime() >= mixerEmotionPlayable.GetInput(1).GetDuration())
        {
            canPlayHappyMain = false;
            mixerEmotionPlayable.SetInputWeight(1, 0.0f);   // Deactivate Transition
            mixerEmotionPlayable.SetInputWeight(0, 1.0f);   // Active Main
            mixerEmotionPlayable.GetInput(0).SetTime(0f);
        }

        if (goToAngry)
        {
            goToAngry = false;
            canPlayAngryMain = true;
            mixerEmotionPlayable.SetInputWeight(3, 1.0f);   // Set HappyTransition In to Active
            mixerEmotionPlayable.SetInputWeight(0, 0.0f);   // Deactivate Main
            mixerEmotionPlayable.SetInputWeight(2, 0.0f);
            mixerEmotionPlayable.GetInput(3).SetTime(0f);
            mixerEmotionPlayable.GetInput(3).SetDone(false);
        }
        if (canPlayAngryMain && ((mixerEmotionPlayable.GetInput(3).GetDuration() - mixerEmotionPlayable.GetInput(3).GetTime()) < 0.1)) //(mixerEmotionPlayable.GetInput(1).GetTime() >= mixerEmotionPlayable.GetInput(1).GetDuration())
        {
            canPlayAngryMain = false;
            mixerEmotionPlayable.SetInputWeight(3, 0.0f);   // Deactivate Transition
            mixerEmotionPlayable.SetInputWeight(2, 1.0f);   // Active Main
            mixerEmotionPlayable.GetInput(2).SetTime(0f);
        }
        */

    }


}
