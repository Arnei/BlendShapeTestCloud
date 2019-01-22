using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Based on LLorach et al. 2016: "Web-based live speech-driven lip-sync: An audio-driven rule-based approach"
 */

[RequireComponent (typeof(AudioSource))]
public class energyBasedLipSync : MonoBehaviour
{
    public float smoothingVariable = 0.1f;          // Should range from 0 to 1
    public float sensitivityThreshold = 0.5f;       // Should range from 0 to 1
    public float vocalTractLength = 1.0f;           // 1 for females, 0.8 for males

    private float[] boundingFrequencies;
    private int[] frequencyDataIndices;
    private int samplesPerBlock;
    private float samplingFrequency;

    private AudioSource audio;

    private float[] spectrum;
    private float[] previousSpectrum;

    private float[] shapeValues;



    // Start is called before the first frame update
    void Start()
    {
        samplesPerBlock = 1024;
        spectrum = new float[samplesPerBlock];
        shapeValues = new float[3];
        //previousSpectrum = new float[samplesPerBlock];

        /*
        AudioConfiguration config = AudioSettings.GetConfiguration();
        config.sampleRate = 44100;
        AudioSettings.Reset(config);
        */
        samplingFrequency = AudioSettings.outputSampleRate;
        Debug.Log("Sampling frequency: " + samplingFrequency + ". Should be 44.1 kHz ");
        
        boundingFrequencies = new[] {0f,
            500f * vocalTractLength,
            700f * vocalTractLength,
            3000f * vocalTractLength,
            6000f * vocalTractLength };

        frequencyDataIndices = new int[boundingFrequencies.Length];
        for (int i = 0; i < boundingFrequencies.Length; i++)
        {
            frequencyDataIndices[i] = Mathf.RoundToInt((2 * samplesPerBlock / samplingFrequency) * boundingFrequencies[i]);
            Debug.Log("Frequ Data Ind " + i + ": " + frequencyDataIndices[i]);
        }

        audio = GetComponent<AudioSource>();
        //audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        updateShapeValues();
        //if (shapeValues[0] > 0 || shapeValues[1] > 0 || shapeValues[2] > 0)
        //Debug.Log("Shape Values: " + shapeValues[0]+ " " +shapeValues[1]+ " "+ shapeValues[2]);
    }

    /*
     * Should be called each frame as long as there is audio input, even if the shapeValues are not used
     */
    public void updateShapeValues()
    {
        // Step 1: Perform Blackman windowing and FFT
        audio.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);

        // Draw Spectrum Data for fun
        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
        }

        // Step 2 to 6
        if (previousSpectrum != null) smoothingStep(spectrum, previousSpectrum);
        previousSpectrum = spectrum;
        dbConversion(spectrum);
        scaleBySensitivity(spectrum);
        computeEnergy(spectrum);
        shapeValues[0] = kissBS(spectrum);
        shapeValues[1] = lipsBS(spectrum);
        shapeValues[2] = mouthBS(spectrum);
    }

    /*
     * Returns the values for the three shapes kiss, lips and mouth open (in that order)
     */
    public float[] getShapeValues()
    {
        return shapeValues;
    }


    /*
     * Step 2: Smooth over time by using previously smoothed Data
     * Input newSpectrum: Frequency domain data from the last audio block
     * Input previousSpectrum: Frequency domain data from this audio block
     * Return: (Should) Modifies "newSpectrum"
     */
    void smoothingStep(float[] newSpectrum, float[] previousSpectrum)
    {
        for (int i = 0; i < newSpectrum.Length; i++)
        {
            newSpectrum[i] = smoothingVariable * previousSpectrum[i] + (1 - smoothingVariable) * Mathf.Abs(newSpectrum[i]);
        }
    }

    /*
     * Step 3: Convert to dB
     * Input smoothedSpectrum: Previously smoothed frequency domain data
     * Return: (Should) Modifies "smoothedSpectrum"
     */
    void dbConversion(float[] smoothedSpectrum)
    {
        for (int i = 0; i < smoothedSpectrum.Length; i++)
        {
            smoothedSpectrum[i] = 20 * Mathf.Log10(smoothedSpectrum[i]);
        }
    }

    /*
     * Step 4: Scale dB to range [-0.5, 0.5] (depending on sensitivity threshold)
     * Input dBSpectrum: Previously converted to dB and smoothed frequency domain data
     * Return: (Should) Modifies "dbSpectrum"
     */
    void scaleBySensitivity(float[] dbSpectrum)
    {
        for (int i = 0; i < dbSpectrum.Length; i++)
        {
            dbSpectrum[i] = sensitivityThreshold + (dbSpectrum[i] + 20) / 140;      // Constants based on the assumption that the input ranges from -25dB to -160dB
        }
    }

    /*
     * Step 5: Compute Energy bins based on bounding frequencies
     * Input scaledDBSpecturm: Previously scaled and converted to dB and smoothed frequency domain data
     * Return: (Should) Modifies "scaledDBSpectrum"
     */
    void computeEnergy(float[] scaledDBSpectrum)
    {
        for (int i = 0; i < boundingFrequencies.Length - 1; i++)
        {
            float sumOfstPSD = 0;
            for (int j = frequencyDataIndices[i]; j < frequencyDataIndices[i + 1]; j++)
            {
                if(scaledDBSpectrum[j] > 0) sumOfstPSD += scaledDBSpectrum[j];
            }
            scaledDBSpectrum[i] = (1.0f / (frequencyDataIndices[i + 1] - frequencyDataIndices[i])) * sumOfstPSD;
        }
    }

    /*
     * Step 6.1: Blendshape value for the kiss shape
     */
    float kissBS(float[] energy)
    {
        if (energy[1] >= 0.2f)
        {
            return 1f - 2f * energy[2];
        }
        else
        {
            return (1f - 2f * energy[2]) * 5f * energy[1];
        }
    }

    /*
     * Step 6.2: Blendshape value for the lips shape
     */
    float lipsBS(float[] energy)
    {
        return 3f * energy[3];
    }

    /*
     * Step 6.3: Blendshape value for the mouth open shape
     */
    float mouthBS(float[] energy)
    {
        return 0.8f * (energy[1] - energy[3]);
    }
}
