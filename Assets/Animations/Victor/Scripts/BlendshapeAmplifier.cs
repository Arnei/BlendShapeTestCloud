using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script zum nachträglichen Manipulieren von Blendshapes
// Linear: Addiert Wert zu bestehenden Werten
// Multiplicative: Multipliziert Wert mit bestehenden Werten
// Beide Methoden führen sehr schnell zu unschönen Gesichtsausdrücken und Artefakten

public class BlendshapeAmplifier : MonoBehaviour {

    public Animator anim;
    public bool linear = false;
    public bool multiplicative = false;
    public int linearScalar = 0;
    public float multiplicativeScalar = 1;

    private int blendShapeCount;
    private SkinnedMeshRenderer smr;

    

    // Use this for initialization
    void Start ()
    {
        smr = GetSMRWithBlendshapes(anim);
        blendShapeCount = smr.sharedMesh.blendShapeCount;
        Debug.Log("Blendshapecount: " + smr.sharedMesh.blendShapeCount);
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        // Linear Mode
        if(linear)
        {
            for(int i=0; i < blendShapeCount; i++)
            {
                float scaledWeight = smr.GetBlendShapeWeight(i) + linearScalar;
                smr.SetBlendShapeWeight(i, Mathf.Clamp(scaledWeight, 0f, 100f));
            }
        }
        // Multiplicative Mode
        if(multiplicative)
        {
            for (int i = 0; i < blendShapeCount; i++)
            {
                float scaledWeight = smr.GetBlendShapeWeight(i) * multiplicativeScalar;
                smr.SetBlendShapeWeight(i, Mathf.Clamp(scaledWeight, 0f, 100f));
            }
        }
	}

    private SkinnedMeshRenderer GetSMRWithBlendshapes(Animator anim)
    {
        SkinnedMeshRenderer[] SMRs = anim.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in SMRs)
        {
            if (smr.sharedMesh.blendShapeCount > 0) return smr;
        }
        return null;
    }
}
