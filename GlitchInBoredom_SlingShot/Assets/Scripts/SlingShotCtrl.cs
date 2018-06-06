using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingShotCtrl : MonoBehaviour
{
    public GameObject mHead;
    public Transform mHead_L, mHead_R;
    public Transform mHead_lookAt;
    private bool isHeadGrabbed = false;
    private bool wasHeadGrabbed = false;
    public Transform[] mLJoints, mRJoints;
    private int numJoints;
    private Vector3 pHeadPosition;

    public float mSpringRestLength = 0.15f;
    public float mSpringMinLength = 0.1f;
    public float mSpringMaxLength = 0.3f;


    public float mSpringStiffness = 1.2f;
    public float mGravity = -0.98f;

    private Particle mHeadPoint;
    private Particle[] mLPoints, mRPoints;
    private Spring[] mFLSprings, mFRSprings;

    void Start()
    {
        initResource();
    }

    void Update()
    {
        updateSpring();
        updateHead();
        fixAnchor();

        updateJoint();

        pHeadPosition = mHeadPoint.position;
    }

    void fixAnchor()
    {
        // fix anchor points to slingshot's body 
        mLPoints[0].position = mLJoints[0].position;
        mRPoints[0].position = mRJoints[0].position;
    }

    void updateHead()
    {
        {
            //calc head's positional delta force
            {
                Vector3 dir = pHeadPosition - mHeadPoint.position;
                float mag = dir.magnitude;
                dir.Normalize();

                Vector3 f = dir * mag * 20f;
                mHeadPoint.applyForce(f);

                // update shader event
                float illum = mag * 200f;
                mHead.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Emission", illum);
                mHead.GetComponent<MeshRenderer>().sharedMaterial.SetVector("_EmissionColor", f);
            }

            // apply head point to head object
            isHeadGrabbed = mHead.GetComponent<SelfCollisionCheck>().checkCollision;

            if (!isHeadGrabbed)
            {
                // update slingshot event
                if (wasHeadGrabbed)
                {
                    Vector3 dir = mHead_lookAt.position - mHeadPoint.position;
                    float dist = dir.magnitude;
                    dir.Normalize();

                    if (dist > 0.3f)
                    {
                        GetComponent<Confetti_Ribbon>().launchRibbon(
                            dir, mHeadPoint.position + dir * 0.5f);
                    }
                }
                wasHeadGrabbed = false;

                mHead.transform.position = mHeadPoint.position;

            }
            else
            {
                mHeadPoint.position = mHead.transform.position;
                wasHeadGrabbed = true;
            }
        }

        // update head rotation
        // - z
        {
            Vector3 dir = mHead.transform.position - mHead_lookAt.position;
            dir.Normalize();

            Vector3 rand = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(dir, rand)) == 1f)
                rand = -Vector3.forward;

            Vector3 left = Vector3.Cross(dir, rand);
            left.Normalize();
            Vector3 up = Vector3.Cross(left, dir);
            up.Normalize();

            mHead.transform.rotation = Quaternion.LookRotation(dir, up);
        }
    }

    void updateSpring()
    {
        for (int i = 0; i < numJoints - 1; i++)
        {
            // update spring variables
            {
                mFLSprings[i].stiffness = mSpringStiffness;
                mFRSprings[i].stiffness = mSpringStiffness;

                mFLSprings[i].restLength = mSpringRestLength;
                mFRSprings[i].restLength = mSpringRestLength;

                mFLSprings[i].minLength = mSpringMinLength;
                mFRSprings[i].minLength = mSpringMinLength;

                mFLSprings[i].maxLength = mSpringMaxLength;
                mFRSprings[i].maxLength = mSpringMaxLength;
            }

            // update spring and apply f to the point
            {
                if (i < numJoints - 2)
                {
                    mFLSprings[i].applyForce(ref mLPoints[i + 1], ref mLPoints[i]);
                    mFRSprings[i].applyForce(ref mRPoints[i + 1], ref mRPoints[i]);
                }
                else
                {
                    mFLSprings[i].applyForce(ref mHeadPoint, ref mLPoints[i]);
                    mFRSprings[i].applyForce(ref mHeadPoint, ref mRPoints[i]);
                }
            }

            // update points
            // without first fixed points on slingshot's body
            if (i > 0)
            {
                mLPoints[i].applyForce(mGravity);
                mRPoints[i].applyForce(mGravity);

                mLPoints[i].update();
                mRPoints[i].update();
            }
        }
        // update shared tip point
        mHeadPoint.applyForce(mGravity);
        mHeadPoint.update();
    }

    void updateJoint()
    {
        for (int i = 0; i < numJoints; i++)
        {
            // update loc
            {
                if (i != numJoints - 1)
                {
                    mLJoints[i].position = mLPoints[i].position;
                    mRJoints[i].position = mRPoints[i].position;
                }
                else
                {
                    mLJoints[i].position = mHeadPoint.position;
                    mRJoints[i].position = mHeadPoint.position;
                }
            }

            // update rot
            {
                Vector3 dir = Vector3.zero;
                Vector3 rand = Vector3.zero;
                Vector3 left = Vector3.zero;
                Vector3 up = Vector3.zero;

                // *** axis of joints face -z
                dir = (i != numJoints - 1) ? mLJoints[i].position - mLJoints[i + 1].position : mLJoints[i - 1].position - mLJoints[i].position;
                dir.Normalize();

                rand = Vector3.up;
                if (Mathf.Abs(Vector3.Dot(dir, rand)) == 1f)
                    rand = -Vector3.forward;

                left = Vector3.Cross(dir, rand);
                left.Normalize();
                up = Vector3.Cross(left, dir);
                up.Normalize();

                mLJoints[i].rotation = Quaternion.LookRotation(dir, up);

                // *** axis of joints face -z
                dir = (i != numJoints - 1) ? mRJoints[i].position - mRJoints[i + 1].position : mRJoints[i - 1].position - mRJoints[i].position;
                dir.Normalize();

                rand = Vector3.up;
                if (Mathf.Abs(Vector3.Dot(dir, rand)) == 1f)
                    rand = -Vector3.forward;

                left = Vector3.Cross(dir, rand);
                left.Normalize();
                up = Vector3.Cross(left, dir);
                up.Normalize();

                mRJoints[i].rotation = Quaternion.LookRotation(dir, up);
            }
        }
    }

    void initResource()
    {
        numJoints = mLJoints.Length;

        mLPoints = new Particle[numJoints - 1];
        mRPoints = new Particle[numJoints - 1];

        mFLSprings = new Spring[numJoints - 1];
        mFRSprings = new Spring[numJoints - 1];

        // initialize objects
        {
            // head point for slingshot head
            // this will be the shared tip point for both joints
            mHeadPoint = new Particle(mHead.transform.position);

            for (int i = 0; i < numJoints - 1; i++)
            {
                // points
                mLPoints[i] = new Particle(mLJoints[i].position);
                mRPoints[i] = new Particle(mRJoints[i].position);

                // springs
                {
                    mFLSprings[i] = new Spring(mLPoints[i].position, mSpringRestLength);
                    mFRSprings[i] = new Spring(mRPoints[i].position, mSpringRestLength);
                }
            }
        }
    }
}
