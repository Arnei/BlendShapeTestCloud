using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class dicationRecognizerTest : MonoBehaviour
{
    [SerializeField]
    private Text m_Hypotheses;

    [SerializeField]
    private Text m_Recognitions;

    private DictationRecognizer m_DictationRecognizer;

    AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start(null, true, 1, 22050);
        audio.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        Debug.Log("start playing... position is " + Microphone.GetPosition(null));
        audio.Play();

        m_DictationRecognizer = new DictationRecognizer();

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            Debug.LogFormat("Dictation result: {0}", text);
            m_Recognitions.text += text + "\n";
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            Debug.LogFormat("Dictation hypothesis: {0}", text);
            m_Hypotheses.text += text;
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        m_DictationRecognizer.Start();
    }
}
