using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Simple script for demonstration purposes, switched between presets of animations.
 * IRRELEVANT! IT IS SEEMINGLY IMPOSSIBLE TO ACCURATELY SYNCHRONIZE STATE MACHINES
 */
public class ExpressionMixingDemo : MonoBehaviour
{
    public GameObject claire1;
    public GameObject claire2;
    public GameObject claire3;

    private Animator animator1;
    private Animator animator2;
    private Animator animator3;

    // Start is called before the first frame update
    void Start()
    {
        animator1 = claire1.GetComponent<Animator>();
        animator2 = claire2.GetComponent<Animator>();
        animator3 = claire3.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void exampleOne()
    {
        clearAll();
        animator1.SetBool(Animator.StringToHash("Start"), true);
        animator2.SetBool(Animator.StringToHash("Start"), true);
        animator3.SetBool(Animator.StringToHash("Start"), true);
        animator1.SetFloat(Animator.StringToHash("Interested"), 1.0f);
        animator2.SetFloat(Animator.StringToHash("Interested2"), 1.0f);
        animator3.SetFloat(Animator.StringToHash("Interested"), 1.0f);
        animator3.SetFloat(Animator.StringToHash("Interested2"), 1.0f);
    }

    public void exampleTwo()
    {
        clearAll();
        animator1.SetFloat(Animator.StringToHash("Entertained"), 1.0f);
        animator2.SetFloat(Animator.StringToHash("Interested"), 1.0f);
        animator3.SetFloat(Animator.StringToHash("Entertained"), 1.0f);
        animator3.SetFloat(Animator.StringToHash("Interested"), 1.0f);
    }

    public void exampleThree()
    {
        clearAll();
        animator1.SetFloat(Animator.StringToHash("Confused"), 1.0f);
        animator2.SetFloat(Animator.StringToHash("Uncomfortable"), 1.0f);
        animator3.SetFloat(Animator.StringToHash("Confused"), 1.0f);
        animator3.SetFloat(Animator.StringToHash("Uncomfortable"), 1.0f);
    }

    private void clearAll()
    {
        setToZero(animator1);
        setToZero(animator2);
        setToZero(animator3);
    }

    private void setToZero(Animator anim)
    {
        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Float)
                anim.SetFloat(parameter.name, 0.0f);
        }
    }
}
