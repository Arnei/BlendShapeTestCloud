using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FaceControllerPrototypeQuickTest : MonoBehaviour {

    public bool goToHappy = false;
    public bool goToNeutral = false;

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
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Emotion Tree.Emotion Tree")) Debug.Log("This is an Emotion Tree");
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Emotion Tree.Happy")) Debug.Log("This is a Happy");


    }


}
