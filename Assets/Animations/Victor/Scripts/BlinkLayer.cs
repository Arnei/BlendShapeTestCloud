using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkLayer : MonoBehaviour {

    public float blinkProbability = 0.14f;
    public float blinkFrequency = 0.5f;
    float elapsed = 0f;

    Animator anim;
    int BlinkTriggerHash = Animator.StringToHash("BlinkTrigger");

    // Use this for initialization
    void Start ()
    {
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= blinkFrequency)
        {
            elapsed = elapsed % blinkFrequency;
            float randomValue = Random.value;
            //Debug.Log("RandomValue: " + randomValue);
            if ( randomValue < blinkProbability)
            {
                anim.SetTrigger(BlinkTriggerHash);
            }
        }
    }



}
