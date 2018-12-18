using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Emotion
{
    public string name;
    public List<AnimationGroup> animationGroupList;

    public Emotion(string name)
    {
        this.name = name;
        animationGroupList = new List<AnimationGroup>();
        animationGroupList.Add(new AnimationGroup());
    }

}

