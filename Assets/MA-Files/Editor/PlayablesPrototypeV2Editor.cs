using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * COPIED FROM PERHEADCONTROLLEREDITOR
 * 
 * Provide a graphic interface to
 * - Add AnimationClips to a character
 * - Add a mask to the facial animation
 * - Manually trigger transitions
 */

[CustomEditor(typeof(PlayablesPrototypeV2))]
public class PlayablesPrototypeV2Editor : Editor
{

    PlayablesPrototypeV2 m_Target;
    bool showClips = false;

    // Override OnInsepctorGUI() to draw your own editor
    public override void OnInspectorGUI()
    {
        m_Target = (PlayablesPrototypeV2)target;

        DrawDefaultInspector(); // Draws all the stuff Unity would normally draw
        DrawEmotionList();
    }

    void DrawEmotionList()
    {
        // Draw Transition buttons
        GUILayout.Label("Go to next Emotion", EditorStyles.boldLabel);
        for(int i = 0; i < m_Target.goToEmotionList.Count; i++)
        {
            EditorGUI.BeginChangeCheck();

            bool newGoToEmotion = EditorGUILayout.ToggleLeft(m_Target.goToEmotionList[i].name, m_Target.goToEmotionList[i].goToEmotion);

            if (EditorGUI.EndChangeCheck())
            {
                m_Target.goToEmotionList[i].goToEmotion = newGoToEmotion;
            }
        }
            
        // Draw emotion list
        showClips = EditorGUILayout.Foldout(showClips, "EmotionClips");
        if (showClips)
        {
            for (int i = 0; i < m_Target.emotionObjects.Count; i++)
            {
                DrawElement(i);
            }
        }


    }

    // Draw an entry for a single emotion
    void DrawElement(int index)
    {
        if (index < 0 || index >= m_Target.emotionObjects.Count) return;

        // Draw the entries within each emotion
        GUILayout.Label(m_Target.emotionObjects[index].name, EditorStyles.boldLabel);
        for (int animIndex = 0; animIndex < m_Target.emotionObjects[index].animationGroupList.Count; animIndex++)
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


        // Draw a Button to add more clips
        if (GUILayout.Button("Add new Clips", GUILayout.Height(20)))
        {
            Undo.RecordObject(m_Target, "Add new Clips");
            m_Target.emotionObjects[index].animationGroupList.Add(new AnimationGroup());
            EditorUtility.SetDirty(m_Target);
        }



    }



}
