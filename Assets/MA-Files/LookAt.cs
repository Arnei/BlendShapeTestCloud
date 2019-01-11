using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour {

    // Public Variables
    public GameObject head;
    public GameObject rightEye;
    public GameObject leftEye;
    public GameObject lookAtObject;
    public float MaxTurnAngle = 50.0f;

    public bool pursuit;        // Pursuit or Gaze
    public float pursuitStartHeadFollow = 13.0f;
    public float pursuitStopHeadFollow = 12.0f;
    public float pursuitHeadSpeed = 3.0f;
    public float pursuitEyeSpeed = 10.0f;
    public float gazeStartHeadFollow = 14.5f;
    public float gazeStopHeadFollow = 14.0f;
    public float gazeHeadSpeed = 3.0f;
    public float gazeEyeSpeed = 15.0f;


    // have to store last rotation to undo animation, otherwise slerp doesn't work
    private Quaternion lastHeadRotation;
    private Quaternion lastLeftEyeRotation;
    private Quaternion lastRightEyeRotation;

    // Match initial rotation to the object this script is attached to
    private Quaternion headOffsetRotation;
    private Quaternion leftEyeOffsetRotation;
    private Quaternion rightEyeOffsetRotation;

    // Local rotation stores to keep animation (instead of being overwritten by lookAt)
    private Quaternion headAnimationClipOffset;
    private Quaternion headInitRotation;
    private Quaternion leftEyeAnimationClipOffset;
    private Quaternion leftEyeInitRotation;
    private Quaternion rightEyeAnimationClipOffset;
    private Quaternion rightEyeInitRotation;

    //float xVelocity = 0.0f;
    //float yVelocity = 0.0f;
    //float zVelocity = 0.0f;

    // Various triggers for head Follow motion
    private bool headFollow = true;
    private float leftEyeLookAtDiffAngle;
    private float rightEyeLookAtDiffAngle;
    private GameObject gazePosition;

    // Pursuit/Gaze mode variables
    private float eyeSpeed;
    private float headSpeed;
    private float startHeadFollow;
    private float stopHeadFollow;

    // Use this for initialization
    void Start () {
        // Save initial rotation to later calculate deviations from it
        headInitRotation = head.transform.localRotation;
        leftEyeInitRotation = leftEye.transform.localRotation;
        rightEyeInitRotation = rightEye.transform.localRotation;

        headInitRotation = new Quaternion(-0.15f, 0, 0, 1.0f); // SOMEHOW INITIAL ROTATION IS WRONG; SOMEHOW THIS IS RIGHT. THIS IS A HACK

        // find rotation needed to get the object's z facing forward and y facing upwards
        headOffsetRotation = Quaternion.Inverse(this.transform.rotation) * head.transform.rotation;
        leftEyeOffsetRotation = Quaternion.Inverse(this.transform.rotation) * leftEye.transform.rotation;
        rightEyeOffsetRotation = Quaternion.Inverse(this.transform.rotation) * rightEye.transform.rotation;

        //headOffsetLocalRotation = Quaternion.Inverse(this.transform.rotation) * head.transform.localRotation;

        gazePosition = new GameObject();
    }

    void Update()
    {
        // Reset the animation offset, so a new one can be applied and to not interfere with the ProcessLookFor calculations
        head.transform.localRotation = Quaternion.Inverse(headAnimationClipOffset) * head.transform.localRotation;
        leftEye.transform.localRotation = Quaternion.Inverse(leftEyeAnimationClipOffset) * leftEye.transform.localRotation;
        rightEye.transform.localRotation = Quaternion.Inverse(rightEyeAnimationClipOffset) * rightEye.transform.localRotation;

        lastHeadRotation = head.transform.rotation;
        lastLeftEyeRotation = leftEye.transform.rotation;
        lastRightEyeRotation = rightEye.transform.rotation;
    }

    void LateUpdate()
    {
        // Find difference from neutral rotation (looking straight forward) to the animation rotation (looking whereever)
        headAnimationClipOffset = head.transform.localRotation * Quaternion.Inverse(headInitRotation);
        leftEyeAnimationClipOffset = leftEye.transform.localRotation * Quaternion.Inverse(leftEyeInitRotation);
        rightEyeAnimationClipOffset = rightEye.transform.localRotation * Quaternion.Inverse(rightEyeInitRotation);

        // Set constants based on mode
        if(pursuit)
        {
            eyeSpeed = pursuitEyeSpeed;
            headSpeed = pursuitHeadSpeed;
            startHeadFollow = pursuitStartHeadFollow;
            stopHeadFollow = pursuitStopHeadFollow;
        }
        else
        {
            eyeSpeed = gazeEyeSpeed;
            headSpeed = gazeHeadSpeed;
            startHeadFollow = gazeStartHeadFollow;
            stopHeadFollow = gazeStopHeadFollow;
        }

        // Process lookAts in order

        // Based on last frames calculations, let the head should follow lookAtObject or not
        if (headFollow)
        {
            ProcessLookFor(head, headOffsetRotation, lastHeadRotation, headSpeed, lookAtObject.transform);
            gazePosition.transform.position = new Vector3(lookAtObject.transform.position.x, lookAtObject.transform.position.y, lookAtObject.transform.position.z);
        }
        else
        {
            ProcessLookFor(head, headOffsetRotation, lastHeadRotation, headSpeed, gazePosition.transform);
        }
        //ProcessLookFor(head, headOffsetRotation, lastHeadRotation, 2.0f, lookAtObject.transform);
        ProcessLookFor(leftEye, leftEyeOffsetRotation, lastLeftEyeRotation, eyeSpeed, lookAtObject.transform);
        ProcessLookFor(rightEye, rightEyeOffsetRotation, lastRightEyeRotation, eyeSpeed, lookAtObject.transform);

        // Calculate by how much the current eye rotationd deviates from neutral position
        leftEyeLookAtDiffAngle = Quaternion.Angle(leftEye.transform.localRotation, Quaternion.Inverse(leftEyeInitRotation));
        rightEyeLookAtDiffAngle = Quaternion.Angle(rightEye.transform.localRotation, Quaternion.Inverse(rightEyeInitRotation));
        Debug.Log("Right Eye Angle: " + rightEyeLookAtDiffAngle);
        Debug.Log("Left Eye Angle: " + leftEyeLookAtDiffAngle);

        // Decide whether the head should follow lookAtObject on the next frame
        if ((rightEyeLookAtDiffAngle + leftEyeLookAtDiffAngle) / 2 > startHeadFollow)
        {
            headFollow = true;
        }
        else if ((rightEyeLookAtDiffAngle + leftEyeLookAtDiffAngle) / 2 <= stopHeadFollow)
        {
            headFollow = false;
        }
        

        // Add found difference after the lookAt overwrite
        head.transform.localRotation = headAnimationClipOffset * head.transform.localRotation;
        leftEye.transform.localRotation = leftEyeAnimationClipOffset * leftEye.transform.localRotation;
        rightEye.transform.localRotation = rightEyeAnimationClipOffset * rightEye.transform.localRotation;

    }

    // process look for object
    void ProcessLookFor(GameObject inObject, Quaternion inOffsetRotation, Quaternion lastRotation, float inSpeed, Transform target)
    {
        // now look at player by rotating the true forward rotation by the look at rotation
        Vector3 toCamera = target.transform.position - inObject.transform.position;

        // look to camera.  this rotates forward vector towards camera
        // make sure to rotate by the object's offset first, since they aren't always forward
        Quaternion lookToCamera = Quaternion.LookRotation(toCamera);

        // find difference between forward vector and look to camera
        Quaternion diffQuat = Quaternion.Inverse(this.transform.rotation) * lookToCamera;

        // if outside range, lerp back to middle
        if (diffQuat.eulerAngles.y > MaxTurnAngle && diffQuat.eulerAngles.y < 360.0f - MaxTurnAngle)
            inObject.transform.rotation = Quaternion.Slerp(lastRotation, this.transform.rotation * inOffsetRotation, inSpeed * Time.deltaTime);
        else
        {
            // lerp rotation to camera, making sure to rotate by the object's offset since they aren't always forward
            inObject.transform.rotation = Quaternion.Slerp(lastRotation, lookToCamera * inOffsetRotation, inSpeed * Time.deltaTime);


            /** TRIED TO USE FORMULAS RELYING ON EULER ANGLES. DIDN'T WORK
                 
            Quaternion diffAngle = lastRotation * Quaternion.Inverse(lookToCamera * inOffsetRotation);
            //Debug.Log("TMepAngle: "+tempAngle);
            float XLeftMaxSpeedHoriz = 473 * (1 - Mathf.Exp(-diffAngle.eulerAngles.x / 7.8f));       // From "Realistic Avatar and Head Animation Using a Neurobiological Model of Visual Attention", Itti, Dhavale, Pighin
            float XLeftHorizDuration = 0.025f + 0.00235f * diffAngle.eulerAngles.x;      // From "Eyes Alive", Lee, Badler
            float YLeftMaxSpeedHoriz = 473 * (1 - Mathf.Exp(-diffAngle.eulerAngles.y / 7.8f));      
            float YLeftHorizDuration = 0.025f + 0.00235f * diffAngle.eulerAngles.y;      
            float ZLeftMaxSpeedHoriz = 473 * (1 - Mathf.Exp(-diffAngle.eulerAngles.z / 7.8f));     
            float ZLeftHorizDuration = 0.025f + 0.00235f * diffAngle.eulerAngles.z;
            Debug.Log("Durations. X: " + XLeftMaxSpeedHoriz + " Y: " + YLeftMaxSpeedHoriz + " Z: " + ZLeftMaxSpeedHoriz);
            float xAngle = Mathf.SmoothDampAngle(lastRotation.eulerAngles.x, Quaternion.Inverse(lookToCamera * inOffsetRotation).eulerAngles.x, ref xVelocity, XLeftHorizDuration, XLeftMaxSpeedHoriz);
            float yAngle = Mathf.SmoothDampAngle(lastRotation.eulerAngles.y, Quaternion.Inverse(lookToCamera * inOffsetRotation).eulerAngles.y, ref yVelocity, YLeftHorizDuration, YLeftMaxSpeedHoriz);
            float zAngle = Mathf.SmoothDampAngle(lastRotation.eulerAngles.z, Quaternion.Inverse(lookToCamera * inOffsetRotation).eulerAngles.z, ref zVelocity, ZLeftHorizDuration, ZLeftMaxSpeedHoriz);
            inObject.transform.rotation = Quaternion.Euler(inObject.transform.rotation.eulerAngles.x, yAngle, inObject.transform.rotation.eulerAngles.z);

            //float resultOfSmoothDamp = Mathf.SmoothDampAngle(0, diffAngle.eulerAngles.y, ref zVelocity, leftHorizDuration, leftMaxSpeedHoriz);
            //Debug.Log("resultOfSmoothDamp :" + resultOfSmoothDamp);

            */
        }
    }
}
