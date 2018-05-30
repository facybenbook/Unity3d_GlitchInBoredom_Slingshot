using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// code based on
// Nature Of Code - Spring Force example by Daniel Shiffman
// https://github.com/shiffman/The-Nature-of-Code-Examples/blob/master/chp03_oscillation/NOC_3_11_spring/Spring.pde
//
struct Spring
{
    private const float K = 15f;
    private float len;
    private Vector3 anchor;

    public Spring(Vector3 anchor, float len)
    {
        this.anchor = anchor;
        this.len = len;
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

        // update anchor
        this.anchor = particle_anchor.position;
    }

    public void constrainLength(ref Particle particle, float minlen, float maxlen)
    {
        Vector3 dir = particle.position - this.anchor;
        float d = dir.magnitude;
        dir.Normalize();

        if (d < minlen)
        {
            dir *= minlen;
            
            // Reset position and stop from moving (not realistic physics)
            particle.position = this.anchor + dir;
            particle.velocity = Vector3.zero;
        }
        else if (d > maxlen)
        {
            dir *= maxlen;

            // Reset position and stop from moving (not realistic physics)
            particle.position = this.anchor + dir;
            particle.velocity = Vector3.zero;
        }
    }
};
