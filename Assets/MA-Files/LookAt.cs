using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour {

    public GameObject rightEye;
    public GameObject leftEye;
    public GameObject lookAtObject;
    public GameObject head;

    private Quaternion forwardRotation;

    public float MaxTurnAngle = 50.0f;

    // have to store last rotation to undo animation, otherwise slerp doesn't work
    private Quaternion lastHeadRotation;
    private Quaternion lastLeftEyeRotation;
    private Quaternion lastRightEyeRotation;

    // private variables
    private Quaternion headOffsetRotation;
    private Quaternion leftEyeOffsetRotation;
    private Quaternion rightEyeOffsetRotation;

    private Quaternion leftEyeAnimationClipOffset;
    private Quaternion leftEyeInitRotation;
    private Quaternion rightEyeAnimationClipOffset;
    private Quaternion rightEyeInitRotation;

    // Use this for initialization
    void Start () {
        // Save initial rotation to later calculate deviations from it
        leftEyeInitRotation = leftEye.transform.rotation;
        rightEyeInitRotation = rightEye.transform.rotation;

        // find rotation needed to get the object's z facing forward and y facing upwards
        leftEyeOffsetRotation = Quaternion.Inverse(this.transform.rotation) * leftEye.transform.rotation;
        rightEyeOffsetRotation = Quaternion.Inverse(this.transform.rotation) * rightEye.transform.rotation;
    }

    void Update()
    {
        // Reset the animation offset, so a new one can be applied and to not interfere with the LookFor calculations
        leftEye.transform.rotation = Quaternion.Inverse(leftEyeAnimationClipOffset) * leftEye.transform.rotation;
        rightEye.transform.rotation = Quaternion.Inverse(rightEyeAnimationClipOffset) * rightEye.transform.rotation;

        lastLeftEyeRotation = leftEye.transform.rotation;
        lastRightEyeRotation = rightEye.transform.rotation;
    }

    void LateUpdate()
    {
        Debug.Log("Difference between Update: " + lastLeftEyeRotation + " and LateUpdate: " + leftEye.transform.rotation);
        Debug.Log("Difference between lefteye: " + leftEye.transform.rotation + "and head: " + leftEyeInitRotation);

        // NEW. Idea: Find rotation from neutral rotation (looking straight forward) to the animation rotation (looking whereever)
        // Quaternion animationRotation = Quaternion.FromToRotation(leftEye.transform.forward, head.transform.forward);
        leftEyeAnimationClipOffset = leftEye.transform.rotation * Quaternion.Inverse(leftEyeInitRotation);
        rightEyeAnimationClipOffset = rightEye.transform.rotation * Quaternion.Inverse(rightEyeInitRotation);
        Debug.Log("Animation Offset: " + leftEyeAnimationClipOffset);

        // process in order
        ProcessLookFor(leftEye, leftEyeOffsetRotation, lastLeftEyeRotation, 10.0f);
        ProcessLookFor(rightEye, rightEyeOffsetRotation, lastRightEyeRotation, 10.0f);

        // NEW
        //Debug.Log("New: " + leftEye.transform.rotation + " Old: " + animationRotation);
        //Debug.Log("RotateTowards: " + leftEye.transform.rotation * animationRotation);
        // leftEye.transform.rotation = Quaternion.RotateTowards(leftEye.transform.rotation, animationRotation, 1);

        leftEye.transform.rotation = leftEyeAnimationClipOffset * leftEye.transform.rotation;
        rightEye.transform.rotation = rightEyeAnimationClipOffset * rightEye.transform.rotation;
    }

    // process look for object
    void ProcessLookFor(GameObject inObject, Quaternion inOffsetRotation, Quaternion lastRotation, float inSpeed)
    {
        // now look at player by rotating the true forward rotation by the look at rotation
        Vector3 toCamera = lookAtObject.transform.position - inObject.transform.position;

        // look to camera.  this rotates forward vector towards camera
        // make sure to rotate by the object's offset first, since they aren't always forward
        Quaternion lookToCamera = Quaternion.LookRotation(toCamera);

        // find difference between forward vector and look to camera
        Quaternion diffQuat = Quaternion.Inverse(this.transform.rotation) * lookToCamera;

        // if outside range, lerp back to middle
        if (diffQuat.eulerAngles.y > MaxTurnAngle && diffQuat.eulerAngles.y < 360.0f - MaxTurnAngle)
            inObject.transform.rotation = Quaternion.Slerp(lastRotation, this.transform.rotation * inOffsetRotation, inSpeed * Time.deltaTime);
        else
            // lerp rotation to camera, making sure to rotate by the object's offset since they aren't always forward
            inObject.transform.rotation = Quaternion.Slerp(lastRotation, lookToCamera * inOffsetRotation, inSpeed * Time.deltaTime);
    }

}
