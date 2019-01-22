using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMicrophone : MonoBehaviour
{
    private AudioSource audio;

    private float[] sample = new float[1024];
    private float[] spectrum = new float[1024];


    // Start is called before the first frame update
    void Start()
    {


        audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start(null, true, 1, 22050);
        audio.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        Debug.Log("start playing... position is " + Microphone.GetPosition(null));
        audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Frequency: " + audio.clip.frequency);
        /*
        audio.clip.GetData(samples, 0);
        for(int i=0; i < samples.Length; i++)
        {
            Debug.Log("Sample " + i + ": " +samples[i]);
        }
        */

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
    }
}
