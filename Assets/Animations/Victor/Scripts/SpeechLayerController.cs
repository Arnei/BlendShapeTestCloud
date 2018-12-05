using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechLayerController : MonoBehaviour {

    public GameObject childwithSkinnedMeshRenderer;

    Animator anim;
    // Get Control Params as Hashes for quicker access
    int CHHash = Animator.StringToHash("Vis_CH");
    int FVHash = Animator.StringToHash("Vis_FV");
    int MBPHash = Animator.StringToHash("Vis_MBP");
    int OOHash = Animator.StringToHash("Vis_OO");
    int SpeechBoolHash = Animator.StringToHash("SpeechBool");

    // Example Sequence for Testing
    enum Phoneme { CH, FV, MBP, OO};
    List<Phoneme> ExamplePhonemeSequence = new List<Phoneme> { Phoneme.CH, Phoneme.FV, Phoneme.MBP, Phoneme.FV, Phoneme.OO, Phoneme.CH, Phoneme.OO, Phoneme.MBP, Phoneme.CH };
    List<float> ExamplePeakTimes = new List<float> { 0.100f, 0.225f, 0.300f, 0.460f, 0.600f, 0.730f, 0.850f, 0.940f, 1.090f };
    float timeZero;
    float timeCurrent;
    float timeDiff;

    // Internal Speech-Is-Active-State
    bool continueSpeech = false;

    SkinnedMeshRenderer smr;
    int mouthBlendshapeStartIndex = 16;
    int mouthBlendshapeEndIndex = 38;
    float[] mouthBlendshapes = new float[44];      // 44 Total number of blendshapes

    // Use this for initialization
    void Start ()
    {
        anim = GetComponent <Animator>();
        smr = childwithSkinnedMeshRenderer.GetComponent<SkinnedMeshRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Setup after Speech triggered
        if (anim.GetBool(SpeechBoolHash))
        {
            timeZero = Time.timeSinceLevelLoad;
            timeCurrent = Time.timeSinceLevelLoad;

            anim.SetBool(SpeechBoolHash, false);
            continueSpeech = true;

            for(int i=mouthBlendshapeStartIndex; i < mouthBlendshapeEndIndex; i++)
            {
                mouthBlendshapes[i] = smr.GetBlendShapeWeight(i);
                Debug.Log("BS Values: " + mouthBlendshapes[i]);
                smr.SetBlendShapeWeight(i, 0);
            }
        }

        // Speech State
        if (continueSpeech)
        {
            continueSpeech = false;

            // Get Time passed since start of speaking.
            timeCurrent = Time.timeSinceLevelLoad;
            timeDiff = timeCurrent - timeZero;
            print(timeDiff);

            float CHValue = 0f;
            float FVValue = 0f;
            float MBPValue = 0f;
            float OOValue = 0f;

            // Add activation based on relative time to peak
            for (int i = 0; i < ExamplePeakTimes.Count; i++)
            {
                float activationTime = Math.Abs(ExamplePeakTimes[i] - timeDiff);
                if (activationTime <= 0.100f)
                {
                    continueSpeech = true;

                    if (ExamplePhonemeSequence[i] == Phoneme.CH) CHValue += 1f - (activationTime * 10);
                    else if (ExamplePhonemeSequence[i] == Phoneme.FV) FVValue += 1f - (activationTime * 10);
                    else if (ExamplePhonemeSequence[i] == Phoneme.MBP) MBPValue += 1f - (activationTime * 10);
                    else if (ExamplePhonemeSequence[i] == Phoneme.OO) OOValue += 1f - (activationTime * 10);

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

            anim.SetFloat(CHHash, CHValue);
            anim.SetFloat(FVHash, FVValue);
            anim.SetFloat(MBPHash, MBPValue);
            anim.SetFloat(OOHash, OOValue);


        }
    }


    private void LateUpdate()
    {
        // Setup after Speech triggered
        if (anim.GetBool(SpeechBoolHash))
        {
            for (int i = mouthBlendshapeStartIndex; i < mouthBlendshapeEndIndex; i++)
            {
                mouthBlendshapes[i] = smr.GetBlendShapeWeight(i);
                smr.SetBlendShapeWeight(i, 0);
            }
        }

        if (continueSpeech)
        {
            for (int i = mouthBlendshapeStartIndex; i < mouthBlendshapeEndIndex; i++)
            {
                smr.SetBlendShapeWeight(i, 0);
            }
        }
    }
}
