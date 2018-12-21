#if (UNITY_EDITOR) 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[RequireComponent(typeof(Animator))]
public class PerHeadController : MonoBehaviour {

    //public GameObject faceController;
    //public FaceController faceControllerScript;

    //private bool registered = false;
    [HideInInspector]
    public List<string> emotionNames = new List<string>();
    [HideInInspector]
    public List<Emotion> emotionObjects= new List<Emotion>();

    public string activeEmotion;

    private Animator animator;
    private RuntimeAnimatorController runtimeAnimController;
    private AnimatorController animController;

    private BlendTree emotionBlendTree;


    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();

        runtimeAnimController = animator.runtimeAnimatorController;
        animController = (AnimatorController) animator.runtimeAnimatorController;
        if(!runtimeAnimController)
        {
            animController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/MA-Files/" + this.name + ".controller");
            animator.runtimeAnimatorController = animController;
        }

        setupStateMachine();


    }

    void setupStateMachine()
    {
        // Create Emotion Layer
        AnimatorControllerLayer emotionLayer = new AnimatorControllerLayer();
        emotionLayer.name = "Face Emotion Layer"; // [ToDo] Change string constant to macro

        int emotionLayerIndex = -1;
        bool emotionLayerFound = false;
        for (int i=0; i < animController.layers.Length; i++)
        {
            if (animController.layers[i].name.Equals(emotionLayer.name)) 
            {
                emotionLayer = animController.layers[i];
                emotionLayerIndex = i;
                emotionLayerFound = true;
            }
        }
        if(!emotionLayerFound)
        {
            emotionLayerIndex = animController.layers.Length;
            //animController.AddLayer(emotionLayer);

            // Create State Machine
            emotionLayer.stateMachine = new AnimatorStateMachine();
            emotionLayer.stateMachine.name = emotionLayer.name;
            // newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy; // Object will not appear in hierarchy
            emotionLayer.blendingMode = AnimatorLayerBlendingMode.Additive;
            emotionLayer.defaultWeight = 1f;

            animController.AddLayer(emotionLayer);

            // Create Blend Tree
            emotionBlendTree = new BlendTree();
            animController.CreateBlendTreeInController("Emotion State", out emotionBlendTree, emotionLayerIndex);
            emotionBlendTree.name = "Emotion Tree";
            emotionBlendTree.blendType = BlendTreeType.Direct;

            // Throw all clips in the tree
            for (int i = 0; i < emotionObjects.Count; i++)
            {
                for (int j = 0; j < emotionObjects[i].animationGroupList.Count; j++)
                {
                    if (emotionObjects[i].animationGroupList[j].transitionIn)
                    {
                        emotionBlendTree.AddChild(emotionObjects[i].animationGroupList[j].transitionIn);
                        AnimatorControllerParameter newParameter = new AnimatorControllerParameter();
                        newParameter.name = emotionObjects[i].name + "_transitionIn";
                        newParameter.type = AnimatorControllerParameterType.Float;
                        animController.AddParameter(newParameter);

                        ChildMotion[] children = emotionBlendTree.children;
                        children[children.Length - 1].directBlendParameter = emotionObjects[i].name + "_transitionIn";
                        emotionBlendTree.children = children;
                    }
                    if (emotionObjects[i].animationGroupList[j].main)
                    {
                        emotionBlendTree.AddChild(emotionObjects[i].animationGroupList[j].main);
                        animController.AddParameter(emotionObjects[i].name, AnimatorControllerParameterType.Float);

                        // Connect Parameter to Animation (Cannot edit directly https://forum.unity.com/threads/unity-5-unable-to-set-directblendparameter-values-in-directblendtree-created-by-editor-script.310021/)
                        ChildMotion[] children = emotionBlendTree.children;
                        children[children.Length - 1].directBlendParameter = emotionObjects[i].name;
                        emotionBlendTree.children = children;
                    }
                }
            }



        }




    }

    // Update is called once per frame
    void Update () {
        activeEmotion = "Happy";
        if (activeEmotion.Length != 0) playAnimation();
	}


    private void playAnimation()
    {
        animator.SetFloat(activeEmotion, 1f);

    }

    /**
public void OnValidate()
{

// Setup connection to central FaceController
if(!faceController)
{
    faceController = GameObject.Find("FaceController");
    if (faceController) Debug.Log("Found FaceController");
    faceControllerScript = (FaceController) faceController.GetComponent(typeof(FaceController));
    if (faceControllerScript) Debug.Log("Found FaceController Script");

    if (!registered)
    {
        if (faceControllerScript.registerPerHeadController(this)) registered = true;
    }
    else Debug.LogError("No FaceController found in scene. PerHeadController Scripts must be added to a FaceController");
}

}
*/

    public void updateEmotionList(List<string> newEmotions)
    {
        // Add new emotions
        foreach(string emotion in newEmotions)
        {
            if(!emotionNames.Contains(emotion))
            {
                Debug.Log("Emotion: " + emotion);
                emotionObjects.Add(new Emotion(emotion));
            }
        }
        // Delete removed emotions
        for (int i = 0; i < emotionObjects.Count; i++)
        {
            if (!newEmotions.Contains(emotionObjects[i].name))
            {
                emotionObjects.Remove(emotionObjects[i]);
                i--;
            }
        }

        emotionNames = new List<string>(newEmotions);
    }


}


# endif