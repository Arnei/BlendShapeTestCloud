using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindSkinnedMeshRenderer : MonoBehaviour {

    public SkinnedMeshRenderer[] SMRs;


    // Use this for initialization
    void Start () {
        SMRs = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(SkinnedMeshRenderer smr in SMRs)
        {
            Debug.Log(smr.gameObject.name);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
