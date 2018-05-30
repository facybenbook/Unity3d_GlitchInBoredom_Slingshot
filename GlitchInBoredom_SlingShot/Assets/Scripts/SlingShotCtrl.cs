using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlingShotCtrl : MonoBehaviour {
    public GameObject mHead;
    public Transform mHead_L, mHead_R;
    public Transform mHead_lookAt;
    public Transform[] mLJoints, mRJoints;
    private int numJoints;

    public float mSpringRestLength = 0.15f;
    public float mSpringMinLength = 0.1f;
    public float mSpringMaxLength = 0.3f;
    public float mGravity = -0.98f;

    private Particle[] mLPoints, mRPoints;
    private Spring[] mFLSprings, mFRSprings, mBLSprings, mBRSprings;

    void Start () {
        initResource();
    }
	
	void Update () {
        updateSpring();
        fixPoint();
        updateJoint();
    }

    void fixPoint()
    {
        mLPoints[0].position = mLJoints[0].position;
        mRPoints[0].position = mRJoints[0].position;

        mLPoints[numJoints-1].position = mHead_L.position;
        mRPoints[numJoints-1].position = mHead_R.position;

        // update head rotation
        // -z
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

    void updateSpring()
    {
        for (int i = 0; i < numJoints - 1; i++)
        {
            // update spring and apply f to the point
            {
                mFLSprings[i].applyForce(ref mLPoints[i + 1], ref mLPoints[i]);
                mFRSprings[i].applyForce(ref mRPoints[i + 1], ref mRPoints[i]);

                mBLSprings[i].applyForce(ref mLPoints[numJoints - 2 - i], ref mLPoints[numJoints - 1 - i]);
                mBRSprings[i].applyForce(ref mRPoints[numJoints - 2 - i], ref mRPoints[numJoints - 1 - i]);
            }

            // constrain length
            {
                mFLSprings[i].constrainLength(ref mLPoints[i + 1], mSpringMinLength, mSpringMaxLength);
                mFRSprings[i].constrainLength(ref mRPoints[i + 1], mSpringMinLength, mSpringMaxLength);

                mBLSprings[i].constrainLength(ref mLPoints[numJoints - 2 - i], mSpringMinLength, mSpringMaxLength);
                mBRSprings[i].constrainLength(ref mRPoints[numJoints - 2 - i], mSpringMinLength, mSpringMaxLength);
            }

            // update points
            // without first and last points
            if (i > 0)
            {
                mLPoints[i].applyForce(mGravity);
                mRPoints[i].applyForce(mGravity);

                // update point's position
                mLPoints[i].update();
                mRPoints[i].update();
            }
        }
    }

    void updateJoint()
    {
        for (int i = 0; i < numJoints; i++)
        {
            // update loc
            {
                mLJoints[i].position = mLPoints[i].position;
                mRJoints[i].position = mRPoints[i].position;
            }

            // update rot
            {
                Vector3 dir = Vector3.zero;
                Vector3 rand = Vector3.zero;
                Vector3 left = Vector3.zero;
                Vector3 up = Vector3.zero;

                // *** axis of joints face -z
                dir = (i != numJoints - 1) ? mLJoints[i].position - mLJoints[i + 1].position : mLJoints[i-1].position - mLJoints[i].position;
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

        mLPoints = new Particle[numJoints];
        mRPoints = new Particle[numJoints];

        mFLSprings = new Spring[numJoints-1];
        mFRSprings = new Spring[numJoints-1];

        mBLSprings = new Spring[numJoints-1];
        mBRSprings = new Spring[numJoints-1];

        // initialize objects
        for (int i = 0; i < numJoints; i++)
        {
            // points
            mLPoints[i] = (i != numJoints - 1) ? new Particle(mLJoints[i].position) : mLPoints[i] = new Particle(mHead_L.position);
            mRPoints[i] = (i != numJoints - 1) ? new Particle(mRJoints[i].position) : mLPoints[i] = new Particle(mHead_R.position);

            // springs
            if (i < numJoints - 1)
            {
                mFLSprings[i] = new Spring(mLPoints[i].position, mSpringRestLength);
                mFRSprings[i] = new Spring(mRPoints[i].position, mSpringRestLength);

                mBLSprings[i] = new Spring(mLPoints[numJoints - 1 - i].position, mSpringRestLength);
                mBRSprings[i] = new Spring(mRPoints[numJoints - 1 - i].position, mSpringRestLength);
            }
        }
    }
}
