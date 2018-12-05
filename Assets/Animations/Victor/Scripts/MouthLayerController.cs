using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthLayerController : MonoBehaviour {

    public float expressionWeight = 0.2f;

    Animator anim;
    // Get Control Params as Hashes for quicker access
    int CHHash = Animator.StringToHash("Vis_CH");
    int FVHash = Animator.StringToHash("Vis_FV");
    int MBPHash = Animator.StringToHash("Vis_MBP");
    int OOHash = Animator.StringToHash("Vis_OO");
    int SpeechTriggerHash = Animator.StringToHash("SpeechTrigger");
    int ContinueSpeechBoolHash = Animator.StringToHash("ContinueSpeechBool");
    int HappyHash = Animator.StringToHash("Happy");
    int SadHash = Animator.StringToHash("Sad");
    int SurpriseHash = Animator.StringToHash("Surprise");
    int AngryHash = Animator.StringToHash("Angry");
    int MouthHappyHash = Animator.StringToHash("Mouth_Happy");
    int MouthSadHash = Animator.StringToHash("Mouth_Sad");
    int MouthSurpriseHash = Animator.StringToHash("Mouth_Surprise");
    int MouthAngryHash = Animator.StringToHash("Mouth_Angry");

    List<int> expressionValues;
    List<int> mouthExpressionValues;


    // Example Sequence for Testing
    enum Phoneme { CH, FV, MBP, OO};
    List<Phoneme> ExamplePhonemeSequence = new List<Phoneme> { Phoneme.CH, Phoneme.FV, Phoneme.MBP, Phoneme.FV, Phoneme.OO, Phoneme.CH, Phoneme.OO, Phoneme.MBP, Phoneme.CH };
    List<float> ExamplePeakTimes = new List<float> { 0.100f, 0.225f, 0.300f, 0.460f, 0.600f, 0.730f, 0.850f, 0.940f, 1.090f };
    float timeZero;
    float timeCurrent;
    float timeDiff;

    float adaptiveExpressionWeight;

	// Use this for initialization
	void Start ()
    {
        anim = GetComponent <Animator>();
        expressionValues = new List<int> { HappyHash, SadHash, SurpriseHash, AngryHash };
        mouthExpressionValues = new List<int> { MouthHappyHash, MouthSadHash, MouthSurpriseHash, MouthAngryHash };
        adaptiveExpressionWeight = expressionWeight;

        // Internal Speech-Is-Active-State
        anim.SetBool(ContinueSpeechBoolHash, false);
    }
	
	// Update is called once per frame
	void Update ()
    {

        // Set expression weights depending on whether or not speech is active
        for (int i = 0; i < expressionValues.Count; i++)
        {
            if(anim.GetBool(ContinueSpeechBoolHash))  anim.SetFloat(mouthExpressionValues[i], anim.GetFloat(expressionValues[i]) * adaptiveExpressionWeight);
            else anim.SetFloat(mouthExpressionValues[i], anim.GetFloat(expressionValues[i]));
        }

        // Setup after Speech triggered
        if (anim.GetBool(SpeechTriggerHash))
        {
            timeZero = Time.timeSinceLevelLoad;
            timeCurrent = Time.timeSinceLevelLoad;

            //anim.SetBool(SpeechTriggerHash, false);
            anim.SetBool(ContinueSpeechBoolHash, true);
        }

        // Speech State
        if(anim.GetBool(ContinueSpeechBoolHash))
        {
            anim.SetBool(ContinueSpeechBoolHash, false);

            // Get Time passed since start of speaking.
            timeCurrent = Time.timeSinceLevelLoad;
            timeDiff = timeCurrent - timeZero;
            print(timeDiff);

            float CHValue = 0f;
            float FVValue = 0f;
            float MBPValue = 0f;
            float OOValue = 0f;

            // Add activation based on relative time to peak
            for(int i=0; i < ExamplePeakTimes.Count; i++)
            {
                float activationTime = Math.Abs(ExamplePeakTimes[i] - timeDiff);
                if (activationTime <= 0.100f)
                {
                    anim.SetBool(ContinueSpeechBoolHash, true);

                    if (ExamplePhonemeSequence[i] == Phoneme.CH) CHValue += 1f - (activationTime * 10);
                    else if(ExamplePhonemeSequence[i] == Phoneme.FV) FVValue += 1f - (activationTime * 10);
                    else if(ExamplePhonemeSequence[i] == Phoneme.MBP) MBPValue += 1f - (activationTime * 10);
                    else if(ExamplePhonemeSequence[i] == Phoneme.OO) OOValue += 1f - (activationTime * 10);

                }
            }

            //CHValue *= Time.deltaTime;
            //FVValue *= Time.deltaTime;
            //MBPValue *= Time.deltaTime;
            //OOValue *= Time.deltaTime;

            // Don't overactivate (1 is max blendshape activation)
            Mathf.Clamp(CHValue, 0f, 1f);
            Mathf.Clamp(FVValue, 0f, 1f);
            Mathf.Clamp(MBPValue, 0f, 1f);
            Mathf.Clamp(OOValue, 0f, 1f);

            // If phoneme activity subsides, strengthen expression
            float phonemeActivity = CHValue + FVValue + MBPValue + OOValue;
            if (phonemeActivity < 1 && (1 - phonemeActivity) > expressionWeight)
            {
                adaptiveExpressionWeight = (1 - phonemeActivity);
            }
            else adaptiveExpressionWeight = expressionWeight;


            // Weight by expression
            //float speechWeight = 1f - expressionWeight;

            //CHValue *= speechWeight;
            //FVValue *= speechWeight;
            //MBPValue *= speechWeight;
            //OOValue *= speechWeight;

            anim.SetFloat(CHHash, CHValue);
            anim.SetFloat(FVHash, FVValue);
            anim.SetFloat(MBPHash, MBPValue);
            anim.SetFloat(OOHash, OOValue);


        }
    }
}
