using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechLayerController : MonoBehaviour {

    Animator anim;
    int CHHash = Animator.StringToHash("Vis_CH");
    int FVHash = Animator.StringToHash("Vis_FV");
    int MBPHash = Animator.StringToHash("Vis_MBP");
    int OOHash = Animator.StringToHash("Vis_OO");
    int SpeechBoolHash = Animator.StringToHash("SpeechBool");

    enum Phoneme { CH, FV, MBP, OO};
    List<Phoneme> ExamplePhonemeSequence = new List<Phoneme> { Phoneme.CH, Phoneme.FV, Phoneme.MBP, Phoneme.FV, Phoneme.OO, Phoneme.CH };
    List<float> ExamplePeakTimes = new List<float> { 0.100f, 0.225f, 0.300f, 0.460f, 0.600f, 0.730f };
    float timeZero;
    float timeCurrent;
    float timeDiff;

    bool continueSpeech = false;


	// Use this for initialization
	void Start ()
    {
        anim = GetComponent <Animator>();

	}
	
	// Update is called once per frame
	void Update ()
    {



        if (anim.GetBool(SpeechBoolHash))
        {
            timeZero = Time.timeSinceLevelLoad;
            timeCurrent = Time.timeSinceLevelLoad;

            anim.SetBool(SpeechBoolHash, false);
            continueSpeech = true;
        }

        if(continueSpeech)
        {
            continueSpeech = false;

            timeCurrent = Time.timeSinceLevelLoad;
            timeDiff = timeCurrent - timeZero;
            print(timeDiff);

            float CHValue = 0f;
            float FVValue = 0f;
            float MBPValue = 0f;
            float OOValue = 0f;

            for(int i=0; i < ExamplePeakTimes.Count; i++)
            {
                float activationTime = Math.Abs(ExamplePeakTimes[i] - timeDiff);
                if (activationTime <= 0.100f)
                {
                    continueSpeech = true;

                    if(ExamplePhonemeSequence[i] == Phoneme.CH) CHValue += 1f - (activationTime * 10);
                    else if(ExamplePhonemeSequence[i] == Phoneme.FV) FVValue += 1f - (activationTime * 10);
                    else if(ExamplePhonemeSequence[i] == Phoneme.MBP) MBPValue += 1f - (activationTime * 10);
                    else if(ExamplePhonemeSequence[i] == Phoneme.OO) OOValue += 1f - (activationTime * 10);

                }
            }

            //CHValue *= Time.deltaTime;
            //FVValue *= Time.deltaTime;
            //MBPValue *= Time.deltaTime;
            //OOValue *= Time.deltaTime;

            Mathf.Clamp(CHValue, 0f, 1f);
            Mathf.Clamp(FVValue, 0f, 1f);
            Mathf.Clamp(MBPValue, 0f, 1f);
            Mathf.Clamp(OOValue, 0f, 1f);

            anim.SetFloat(CHHash, CHValue);
            anim.SetFloat(FVHash, FVValue);
            anim.SetFloat(MBPHash, MBPValue);
            anim.SetFloat(OOHash, OOValue);


        }
    }
}
