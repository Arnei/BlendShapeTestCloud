using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class activationTimesWithIndices
{
    public float activationTime;
    public int index;

    public activationTimesWithIndices(float activationTime, int index)
    {
        this.activationTime = activationTime;
        this.index = index;
    }
}

public class phonemeBasedLipSync : MonoBehaviour
{

    /*
     * Process of acquiring Phonemes and their timing:
     * DictationRecognizer did not give good results. Everything else doesn't return single words/is not in real-time/too much of a hassle to set up
     * Converted sentence to ARPABET (phonemes) via CMU: http://www.speech.cs.cmu.edu/cgi-bin/cmudict?in=something+funny (Last checked 17.01.2019)
     * Converted sentence to Reallusion set of phonemes per hand. https://manual.reallusion.com/3DXchange_6/ENU/Pipeline/04_Modify_Page/Face_Setup_Section/Basic_Lip_Shapes.htm (Last checked 17.01.2019)
     * Transcribed phoneme timings per hand.
     */
    /* 
     * DictationRecognizer results:
     *  27 	what's the time
        27	something
        28	something funny
        28	something divided
        28	something to eat
        29	look up
        30	look up to
        30	look up the post
        31	I
        31	it's
        32	I switch
        33	treating
        33	are you really think
     */
    List<string> phonemeSequence = new List<string> { "D", "AE", "T", "K", "W", "Ih", "K", "B", "Er", "Z", "F", "Ah", "K", "S", "J", "Ah", "M", "P", "T", "Ih", "N", "D", "Ah", "EE", "R", "Oh", "V", "R", "Ih", "Ch", "Th", "Ih", "N", "D", "Oh", "G", "L", "OO", "K", "Ah", "T", "Ih", "S", "Ah", "T", "F", "Oh", "R", "H", "Ih", "Z", "F", "Ih", "L", "D", "Ih", "OO", "Ah", "G", "Er", "N", "K", "R", "Ih", "Er", "T", "Ih", "NG", "K", "Er", "Ah", "S" };
    List<float> peakTimes = new List<float> { 4.37f, 4.45f, 4.58f, 4.71f, 4.75f, 4.85f, 4.95f, 4.50f, 5.2f, 5.42f, 5.48f, 5.6f, 5.75f, 5.81f, 6.01f, 6.15f, 6.28f, 6.32f, 6.36f, 6.4f, 6.48f, 6.52f, 6.6f, 6.63f, 6.7f, 6.8f, 6.9f, 7.0f, 7.2f, 7.25f, 7.35f, 7.45f, 7.5f, 7.6f, 8.0f, 8.68f, 8.75f, 8.82f, 8.90f, 9.05f, 9.21f, 9.37f, 9.50f, 9.65f, 10.36f, 10.40f, 10.42f, 10.46f, 10.52f, 10.59f, 10.61f, 10.67f, 10.78f, 10.80f, 10.86f, 10.95f, 11.00f, 11.11f, 11.20f, 11.30f, 11.58f, 11.64f, 11.69f, 11.79f, 11.85f, 11.93f, 12.0f, 12.10f, 12.17f, 12.35f, 12.60f };

    float currentLevelTime;

    float vOpenValue;
    float vExplosiveValue;
    float vDentalValue;
    float vTightOValue;
    float vTightValue;
    float vWideValue;
    float vAffricateValue;
    float vLipOpenValue;

    activationTimesWithIndices[] activations;

    public int maxNumberOfActiveVisemes = 5;
    public float minActivationThreshold = 0.1f;

    AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        activations = new activationTimesWithIndices[peakTimes.Count];
        for (int i = 0; i < peakTimes.Count; i++)
        {
            activations[i] = new activationTimesWithIndices(0f, i);
        }


        audio = GetComponent<AudioSource>();
        currentLevelTime = audio.time;

    }

    // Update is called once per frame
    void Update()
    {
        //updateVisemeValues();
    }

    public void updateVisemeValues()
    {
        vOpenValue = 0f;
        vExplosiveValue = 0f;
        vDentalValue = 0f;
        vTightOValue = 0f;
        vTightValue = 0f;
        vWideValue = 0f;
        vAffricateValue = 0f;
        vLipOpenValue = 0f;

        float countActivePhonemes = 0;
        currentLevelTime = audio.time;

        for (int i = 0; i < peakTimes.Count; i++)
        {
            activations[i].activationTime = Mathf.Abs(peakTimes[i] - currentLevelTime);
        }
        activationTimesWithIndices[] sortedActivations = activations.OrderBy(o => o.activationTime).ToArray();



        for (int i = 0; i < maxNumberOfActiveVisemes; i++)
        {
            //float activationTime = Mathf.Abs(peakTimes[i] - currentLevelTime);
            // If activationTime over threshold
            // Get corresponding phoneme and compute viseme
            if (sortedActivations[i].activationTime <= minActivationThreshold)
            {
                float activationTime = sortedActivations[i].activationTime;
                countActivePhonemes++;

                switch (phonemeSequence[i])
                {
                    case "AE":
                        vOpenValue += (1f - activationTime) * 0.4f;
                        vWideValue += (1f - activationTime) * 1.0f;
                        vLipOpenValue += (1f - activationTime) * 0.4f;
                        break;
                    case "Ah":
                        vOpenValue += (1f - activationTime) * 1.0f;
                        break;
                    case "B":
                    case "M":
                    case "P":
                        vExplosiveValue += (1f - activationTime) * 1.0f;
                        break;
                    case "Ch":
                    case "J":
                        vAffricateValue += (1f - activationTime) * 1.0f;
                        break;
                    case "EE":
                        vWideValue += (1f - activationTime) * 1.0f;
                        vLipOpenValue += (1f - activationTime) * 0.9f;
                        break;
                    case "Er":
                        vLipOpenValue += (1f - activationTime) * 0.25f;
                        vAffricateValue += (1f - activationTime) * 0.9f;
                        break;
                    case "F":
                    case "V":
                        vDentalValue += (1f - activationTime) * 1.0f;
                        break;
                    case "Ih":
                        vOpenValue += (1f - activationTime) * 0.15f;
                        vLipOpenValue += (1f - activationTime) * 0.70f;
                        break;
                    case "K":
                    case "G":
                    case "H":
                    case "NG":
                        vOpenValue += (1f - activationTime) * 0.30f;
                        vLipOpenValue += (1f - activationTime) * 0.40f;
                        break;
                    case "Oh":
                        vOpenValue += (1f - activationTime) * 0.80f;
                        vTightValue += (1f - activationTime) * 0.80f;
                        break;
                    case "R":
                        vOpenValue += (1f - activationTime) * 0.10f;
                        vTightOValue += (1f - activationTime) * 0.90f;
                        break;
                    case "S":
                    case "Z":
                        vWideValue += (1f - activationTime) * 0.20f;
                        vLipOpenValue += (1f - activationTime) * 1.00f;
                        break;
                    case "T":
                    case "L":
                    case "D":
                    case "N":
                        vOpenValue += (1f - activationTime) * 0.15f;
                        vLipOpenValue += (1f - activationTime) * 0.80f;
                        break;
                    case "Th":
                        vOpenValue += (1f - activationTime) * 0.20f;
                        vLipOpenValue += (1f - activationTime) * 0.70f;
                        break;
                    case "W":
                    case "OO":
                        vTightOValue += (1f - activationTime) * 1.00f;
                        vLipOpenValue += (1f - activationTime) * 0.70f;
                        break;
                    default:
                        Debug.Log("Invalid phoneme");
                        break;
                }
            }
        }

        if(countActivePhonemes == 0)
        {
            vOpenValue = 0f;
            vExplosiveValue = 0f;
            vDentalValue = 0f;
            vTightOValue = 0f;
            vTightValue = 0f;
            vWideValue = 0f;
            vAffricateValue = 0f;
            vLipOpenValue = 0f;
        }
        else
        {
            vOpenValue /= countActivePhonemes;
            vExplosiveValue /= countActivePhonemes;
            vDentalValue /= countActivePhonemes;
            vTightOValue /= countActivePhonemes;
            vTightValue /= countActivePhonemes;
            vWideValue /= countActivePhonemes;
            vAffricateValue /= countActivePhonemes;
            vLipOpenValue /= countActivePhonemes;
        }
    }

    public void getVisemeValues(out float[] shapeValues)
    {
        shapeValues = new float[] { vOpenValue, vExplosiveValue, vDentalValue, vTightOValue, vTightValue, vWideValue, vAffricateValue, vLipOpenValue };
    }
}
