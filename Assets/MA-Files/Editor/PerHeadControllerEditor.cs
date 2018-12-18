using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PerHeadController))]
public class PerHeadControllerEditor : Editor {

    PerHeadController m_Target;
    bool showClips = false;

    // Override OnInsepctorGUI() to draw your own editor
    public override void OnInspectorGUI()
    {
        m_Target = (PerHeadController)target;

        DrawDefaultInspector(); // Draws all the stuff Unity would normally draw
        DrawEmotionList();
    }

    void DrawEmotionList()
    {
        //GUILayout.Label("Emotions", EditorStyles.boldLabel);

        showClips = EditorGUILayout.Foldout(showClips, "EmotionClips");
        if (showClips)
        {
            for (int i = 0; i < m_Target.emotionObjects.Count; i++)
            {
                DrawElement(i);
            }
        }


    }

    void DrawElement(int index)
    {
        if (index < 0 || index >= m_Target.emotionObjects.Count) return;

        GUILayout.Label(m_Target.emotionObjects[index].name, EditorStyles.boldLabel);
        for (int animIndex = 0; animIndex < m_Target.emotionObjects[index].animationGroupList.Count; animIndex++)
        {
            //GUILayout.BeginHorizontal();
            {

                EditorGUI.BeginChangeCheck();

                AnimationClip newTransitionIn = (AnimationClip)EditorGUILayout.ObjectField("Transition In:", m_Target.emotionObjects[index].animationGroupList[animIndex].transitionIn, typeof(AnimationClip), false, GUILayout.ExpandWidth(true));
                AnimationClip newMain = (AnimationClip)EditorGUILayout.ObjectField("Main Animation:", m_Target.emotionObjects[index].animationGroupList[animIndex].main, typeof(AnimationClip), false);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_Target, "Modify Emotion"); //Create Undo/Redo Step

                    m_Target.emotionObjects[index].animationGroupList[animIndex].transitionIn = newTransitionIn;
                    m_Target.emotionObjects[index].animationGroupList[animIndex].main = newMain;

                    EditorUtility.SetDirty(m_Target); // If not serizalized, Unity needs to be told that a component has changed so it is saved correctly
                }

                if (GUILayout.Button("Remove", GUILayout.MinWidth(30), GUILayout.MaxWidth(60)))
                {
                    Undo.RecordObject(m_Target, "Delete Clip");
                    m_Target.emotionObjects[index].animationGroupList.RemoveAt(animIndex);
                    EditorUtility.SetDirty(m_Target);
                }
            }
            //GUILayout.EndHorizontal();
        }


        // Draw a Button to add more clips
        if (GUILayout.Button("Add new Clips", GUILayout.Height(20)))
        {
            Undo.RecordObject(m_Target, "Add new Clips");
            m_Target.emotionObjects[index].animationGroupList.Add(new AnimationGroup());
            EditorUtility.SetDirty(m_Target);
        }

      

    }



}
