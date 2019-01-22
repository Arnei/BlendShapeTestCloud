using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionDemo : MonoBehaviour
{
    public InputField mainInputField;
    public InputField mainInputField2;
    public InputField mainInputField3;

    public GameObject claire1;
    public GameObject claire2;
    public GameObject claire3;

    private PlayablesPrototypeV2 playables1;
    private PlayablesPrototypeV2 playables2;
    private PlayablesPrototypeV2 playables3;

    // Start is called before the first frame update
    void Start()
    {
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onEndEdit.AddListener(delegate { LockInput(mainInputField); });
        mainInputField2.onEndEdit.AddListener(delegate { LockInput(mainInputField2); });
        mainInputField3.onEndEdit.AddListener(delegate { LockInput(mainInputField3); });

        mainInputField.text = "StaticHappy";
        mainInputField2.text = "StaticAngry";
        mainInputField3.text = "StaticSadness";

        playables1 = claire1.GetComponent<PlayablesPrototypeV2>();
        playables2 = claire2.GetComponent<PlayablesPrototypeV2>();
        playables3 = claire3.GetComponent<PlayablesPrototypeV2>();

        playables1.playEmotion("StaticNeutral");
        playables2.playEmotion("StaticNeutral");
        playables3.playEmotion("StaticNeutral");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playStaticHappy()
    {
        playables1.playEmotion("StaticHappy");
        playables2.playEmotion("StaticHappy");
        playables3.playEmotion("StaticHappy");
    }

    public void playInputEmotion(string nextEmotion)
    {

    }



    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        playables1.playEmotion(input.text);
        playables2.playEmotion(input.text);
        playables3.playEmotion(input.text);

        if (input.text.Length > 0)
        {
            Debug.Log("Text has been entered");
        }
        else if (input.text.Length == 0)
        {
            Debug.Log("Main Input Empty");
        }
    }

}
