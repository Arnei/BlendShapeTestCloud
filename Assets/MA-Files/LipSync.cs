using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Provides lip sync animation to a character
 *      TODO: Make it microphone compatible
 */
[RequireComponent(typeof(energyBasedLipSync), typeof(AudioSource))]
public class LipSync : MonoBehaviour
{
    [Header("Controls")]
    [Tooltip("Manual control to start or stop displaying lip-sync animation")]
    public bool speaking = false;
    public bool useMicrophone = false;
    public bool usePhonemeDemonstrationValues = false;

    [Header("Adjustable Parameters")]
    [Tooltip("By how many percent non-lip sync shapes should be reduced (Range 0-1)")]
    public float expressionWeight = 0.8f;                   
    [Tooltip("Value at which lip-sync becomes active. Emperically determined.")]
    public float minimumSpeechActivationLevel = 0.0001f;    // Possible Improvement 1: Provide option to determine this automatically
                                                            // Possible Improvement 2: Provide range instead of a single value. The rangeValue may influence blend weights to a degree.
    [Tooltip("Time to complete blend-in/-out in seconds")]
    public float blendTime = 0.5f;

    [Header("Adjustable Parameters for Energy Based Lip-Sync")]
    [Tooltip("Determines degree by which spectrum data is smoothed with previous data(Range 0-1)")]
    public float smoothingVariable = 0.3f;
    [Tooltip("Should be set depending on the noise level")]
    public float sensitivityThreshold = 0.5f;
    [Tooltip("Depends on the individual. Recommended is 0.8 for females and 1.0 for males")]
    public float vocalTractLength = 1.0f;

    [Header("Fill In Components")]
    [Tooltip("The object that has the characters blendshapes")]
    public GameObject objectWithSkinnedMeshRenderer;        
    [Tooltip("Index of the kiss shape (Mouth puckered up and slightly open / Affricate)")]
    public int kissShapeIndex;                              
    [Tooltip("Index of the lips pressed shape(Plosive)")]
    public int lipsPressedShapeIndex;
    [Tooltip("Index of the mouth open shape (When not using a jaw bone)")]
    public int mouthOpenShapeIndex;
    [Tooltip("An array containing all indices for shapes that influence the mouth.")]
    public int[] lowerFaceShapesIndices;                   // TODO: Find a better way to set it up than hard-coding

    [Header("Only needs to be filled in if the character does not use a mouthOpen Blendshape")]
    [Tooltip("Check if the jaw joint should be rotated (instead of using a mouth Open shape)")]
    public bool useJawRotationInstead;                     
    [Tooltip("The jaw joint")]
    public GameObject jaw;                                  
    [Tooltip("The neutral (mouth closed) rotation of the jaw joint")]
    public float zeroRotation;                            
    [Tooltip("The rotation that describes how far the mouth should maximally be opened")]
    public float maxRotation;                              


    // For accessing blendshapes
    private SkinnedMeshRenderer skinnedMeshRenderer;        
    private Mesh skinnedMesh;

    // For getting speech-shape values
    private energyBasedLipSync energyShapeValuesCalculator; // Attached script that calculates lip sync shape values based on energy
    private float[] shapeValues = new float[3];             // Storage for shape values from energyShapeValuesCalculator
    // For getting speech-shape values from the dummy script
    private phonemeBasedLipSync phonemeShapeValuesCalculator;
    private float[] phonemeShapeValues = new float[8];

    // For blending in and out of speech mode
    private float currentTime = 0f;
    private float actualExpressionWeight;
    private float actualSpeechWeight;
    private bool executeMain = false;
    private bool speakingInit = true;
    private bool speakingLastFrame;
    private bool isSpeaking = false;
    private bool isSpeakingLastFrame;
    private AudioSource audio;
    float[] clipSampleData = new float[1024];

    private bool usingMicrophone = false;
    private AudioClip originalClip;

    // For performance testing
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();


    // Start is called before the first frame update
    void Start()
    {
        lowerFaceShapesIndices = Enumerable.Range(0, 8).Concat(Enumerable.Range(33, 35)).ToArray();     // Manually init

        if (objectWithSkinnedMeshRenderer)
        {
            skinnedMeshRenderer = objectWithSkinnedMeshRenderer.GetComponent<SkinnedMeshRenderer>();
        }
        else if(!skinnedMeshRenderer)
        {
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        }

        skinnedMesh = skinnedMeshRenderer.sharedMesh;
        energyShapeValuesCalculator = GetComponent<energyBasedLipSync>();
        phonemeShapeValuesCalculator = GetComponent<phonemeBasedLipSync>();

        speakingLastFrame = speaking;
        isSpeakingLastFrame = isSpeaking;

        audio = this.GetComponent<AudioSource>();
        originalClip = audio.clip;
        //audio.Play();
    }


    // Update is called once per frame
    void LateUpdate()
    {
        stopwatch.Start();      // For performance test purposes

        if(useMicrophone && !usingMicrophone)
        {
            usingMicrophone = true;
            startMicrophone();
        }
        if(!useMicrophone && usingMicrophone)
        {
            usingMicrophone = false;
            endMicrophone();
        }

        /*
         * Pre loop that sets weights for smooth blend-ins and blend-outs
         */
        // Get frequencies to determine wether someone is speaking or not
        audio.GetSpectrumData(clipSampleData, 0, FFTWindow.Rectangular);
        float currentAverageVolume = clipSampleData.Average();
        if (currentAverageVolume > minimumSpeechActivationLevel)
        {
            isSpeaking = true;
        }
        else if (isSpeaking)
        {
            isSpeaking = false;
        }

        // Check if one of the variables controlling speech activation changed last frame, 
        if (speaking != speakingLastFrame)
        {
            speakingInit = true;
        }
        speakingLastFrame = speaking;
        if (isSpeaking != isSpeakingLastFrame)
        {
            speakingInit = true;
        }
        isSpeakingLastFrame = isSpeaking;

        // Enable/Disable main loop and set blend-in/-out weights
        if (speaking)
        {
            if(isSpeaking)                      // If both lip-sync is activated and speech is detected, blend lip-sync in
            {
                if (speakingInit)               // Initialize Blend
                {
                    if(currentTime < 0f) currentTime = 0.0f;    
                    executeMain = true;
                    speakingInit = false;
                    actualSpeechWeight = 1.0f;
                }
                if (currentTime < blendTime)  // Blend
                {
                    actualExpressionWeight = Mathf.Lerp(1.0f, expressionWeight, (currentTime / blendTime));
                    currentTime += Time.deltaTime;
                }
                else
                {

                }
            }
        }
        if(!speaking || !isSpeaking)        // If either lip-sync got deactivated or no more speech is detected, blend lip-sync out.
        {                                   // To ensure smooth transitions between multiple sudden changes between speaking and not-speaking, 
                                            // decrement currentTime instead of incrementing it.
                if (speakingInit)           // Initialize Blend
                {
                    if(currentTime > blendTime) currentTime = blendTime;
                    speakingInit = false;
                }
                if (currentTime > 0.0f)    // Blend 
                {
                    actualExpressionWeight = Mathf.Lerp(1.0f, expressionWeight, (currentTime / blendTime));
                    actualSpeechWeight = Mathf.Lerp(0f, 1.0f, (currentTime / blendTime));
                    currentTime -= Time.deltaTime;
                }
                else
                {
                    executeMain = false;
                }
        }


        /*
         * Main Loop that handles the actual display of the lip-sync animation
         */
        if (executeMain)
        {
            // Set Variables for the energy calculations
            energyShapeValuesCalculator.smoothingVariable = smoothingVariable;
            energyShapeValuesCalculator.sensitivityThreshold = sensitivityThreshold;
            energyShapeValuesCalculator.vocalTractLength = vocalTractLength;

            // Lower the intensity of other shapes on the lower half of the face
            foreach (int shapeIndex in lowerFaceShapesIndices)
            {
                float weight = skinnedMeshRenderer.GetBlendShapeWeight(shapeIndex);
                weight = weight * actualExpressionWeight;
                skinnedMeshRenderer.SetBlendShapeWeight(shapeIndex, weight);
            }


            if(!usePhonemeDemonstrationValues)
            {
                // Get Blendshape Values
                shapeValues = energyShapeValuesCalculator.getShapeValues();

                // Set Blendshape Values
                skinnedMeshRenderer.SetBlendShapeWeight(kissShapeIndex, shapeValues[0] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(lipsPressedShapeIndex, shapeValues[1] * 100f * actualSpeechWeight);
                if (!useJawRotationInstead)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(mouthOpenShapeIndex, shapeValues[2] * 100f * actualSpeechWeight);
                }
                else                    // If there is no "Mouth Open" shape, set jaw joint rotation values instead
                {
                    float goalAngle = Mathf.Lerp(zeroRotation, maxRotation, shapeValues[2] * actualSpeechWeight);
                    jaw.transform.localEulerAngles = new Vector3(jaw.transform.localEulerAngles.x, jaw.transform.localEulerAngles.y, goalAngle);
                }
            }
            else
            {
                // Get Blendshape Values
                phonemeShapeValuesCalculator.updateVisemeValues();
                phonemeShapeValuesCalculator.getVisemeValues(out phonemeShapeValues);

                // Set Blendshape Values
                skinnedMeshRenderer.SetBlendShapeWeight(0, phonemeShapeValues[0] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(1, phonemeShapeValues[1] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(2, phonemeShapeValues[2] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(3, phonemeShapeValues[3] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(4, phonemeShapeValues[4] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(5, phonemeShapeValues[5] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(6, phonemeShapeValues[6] * 100f * actualSpeechWeight);
                skinnedMeshRenderer.SetBlendShapeWeight(7, phonemeShapeValues[7] * 100f * actualSpeechWeight);

                
                float goalAngle = Mathf.Lerp(zeroRotation, maxRotation, phonemeShapeValues[0] * actualSpeechWeight);
                jaw.transform.localEulerAngles = new Vector3(jaw.transform.localEulerAngles.x, jaw.transform.localEulerAngles.y, goalAngle);
            }

        }


        //Debug.Log("Time elapsed for lip sync: " + (stopwatch.Elapsed));
        stopwatch.Reset();
    }


    private void startMicrophone()
    {
        audio.clip = Microphone.Start(null, true, 1, 22050);
        audio.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        Debug.Log("start playing... position is " + Microphone.GetPosition(null));
        audio.Play();
    }

    private void endMicrophone()
    {
        Microphone.End(null);
        audio.clip = originalClip;
        audio.Play();
    }


}
