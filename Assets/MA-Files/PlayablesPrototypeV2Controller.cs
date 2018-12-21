using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Hub class that is used to control facial animation on all characters.
 * To be attached to an emtpy GameObject.
 * 
 * You only ever need one of these in your scene. Multiple are possible, but unnecessary and possibly harder to manage.
 */
public class PlayablesPrototypeV2Controller : MonoBehaviour
{
    // Public Data Structures
    public List<PlayablesPrototypeV2> characterScripts;     // References to all PlayablePrototypesV2 in the scene
    [HideInInspector]
    public List<string> emotionNames = new List<string> { "Happy", "Sad", "Angry" };


    // Search and add all GameObjects with PerHeadControllers in the scene
    public void getAllHeadControllers()
    {
        PlayablesPrototypeV2[] characterScriptsArray = FindObjectsOfType<PlayablesPrototypeV2>();
        Debug.Log("Found " + characterScriptsArray.Length + " instances with this script attached");
        characterScripts = new List<PlayablesPrototypeV2>(characterScriptsArray);
    }

    // Update Emotions in all PerHeadControllers
    public void updateAllHeadControllers()
    {
        foreach (PlayablesPrototypeV2 characterScript in characterScripts)
        {
            if (characterScript == null) characterScripts.Remove(characterScript);
            characterScript.updateEmotionList(emotionNames);
        }
    }


}
