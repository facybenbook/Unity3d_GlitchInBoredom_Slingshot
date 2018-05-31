using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// code based on
// Nature Of Code - Spring Force example by Daniel Shiffman
// https://github.com/shiffman/The-Nature-of-Code-Examples/blob/master/chp03_oscillation/NOC_3_11_spring/Spring.pde
//
struct Spring
{
    private float K;
    private float len, minLen, maxLen;
    private Vector3 anchor;



    public Spring(Vector3 anchor, float len)
    {
        this.anchor = anchor;
        this.len = len;
        this.minLen = 0f;
        this.maxLen = 999f;
        this.K = 1.2f;
    }

    public float stiffness
    {
        get { return this.K; }
        set { this.K = value; }
    }

    public float restLength
    {
        get { return this.len; }
        set { this.len = value; }
    }

    public float minLength
    {
        get { return this.minLen; }
        set { this.minLen = value; }
    }

    public float maxLength
    {
        get { return this.maxLen; }
        set { this.maxLen = value; }
    }

    public void applyForce(ref Particle particle, ref Particle particle_anchor)
    {
        Vector3 f = particle.position - particle_anchor.position;
        float dist = f.magnitude;
        f.Normalize();
        float stretch = dist - len;

        f *= (-1f * K * stretch);
        particle.applyForce(f);
        f *= -1f;
        particle_anchor.applyForce(f);
    }
};
