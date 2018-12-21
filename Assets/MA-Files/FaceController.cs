#if (UNITY_EDITOR) 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceController : MonoBehaviour {

    public List<PerHeadController> characterScripts;
    [HideInInspector]
    public List<string> emotionNames = new List<string>{ "Happy", "Sad", "Angry" };


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

        /**
         * // Old Code for automatization of updating emotion list in all controllers
    public void OnValidate()
    {

    foreach(PerHeadController characterScript in characterScripts)
    {
        if (characterScript == null) characterScripts.Remove(characterScript);
        characterScript.updateEmotionList(emotionNames);
    }

    }
    */

    // Search and add all GameObjects with PerHeadControllers in the scene
    public void getAllHeadControllers()
    {
        PerHeadController[] characterScriptsArray = FindObjectsOfType<PerHeadController>();
        Debug.Log("Found " + characterScriptsArray.Length + " instances with this script attached");
        characterScripts = new List<PerHeadController>(characterScriptsArray);
    }

    // Update Emotions in all PerHeadControllers
    public void updateAllHeadControllers()
    {
        foreach (PerHeadController characterScript in characterScripts)
        {
            if (characterScript == null) characterScripts.Remove(characterScript);
            characterScript.updateEmotionList(emotionNames);
        }
    }



    /**
     * Old register function for automatically registering to a FaceController on spawn
    public bool registerPerHeadController(PerHeadController newScript)
    {
        if (characterScripts.Contains(newScript))
        {
            return false;
        }
        else
        {
            characterScripts.Add(newScript);
            Debug.Log("New Script added");
            return true;
        }

    }
    */
}

# endif
