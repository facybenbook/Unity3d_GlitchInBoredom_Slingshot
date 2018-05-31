using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// code based on
// Nature Of Code - Spring Force example by Daniel Shiffman
// https://github.com/shiffman/The-Nature-of-Code-Examples/blob/master/chp03_oscillation/NOC_3_11_spring/Mover.pde
//
struct Particle
{
    Vector3 pos;
    Vector3 vel;
    Vector3 acc;
    float m;

    float damp;
    float step;

    public Particle(Vector3 pos)
    {
        this.pos = pos;
        this.vel = Vector3.zero;
        this.acc = Vector3.zero;
        
        this.m = 0.1f;

        this.damp = 0.98f;
        this.step = 0.4f;
    }

    public Vector3 position
    {
        get { return this.pos; }
        set { this.pos = value; }
    }
    public Vector3 velocity
    {
        get { return this.vel; }
        set { this.vel = value; }
    }
    public Vector3 acceleration
    {
        get { return this.acc; }
        set { this.acc = value; }
    }
    public float mass
    {
        get { return this.m; }
        set { this.m = value; }
    }
    public float damping
    {
        get { return this.damp; }
        set { this.damp = value; }
    }

    // Standard Euler integration
    public void update()
    {
        this.vel += this.acc;

        if(this.vel.magnitude > 0.1f)
        {
            Vector3 nv = this.vel;
            nv.Normalize();
            nv *= 0.1f;
            this.vel = nv;
        }

        this.vel *= this.damp;

        this.pos += this.vel * this.step;

        this.acc *= 0f;
    }

    // Newton's law: F = M * A
    public void applyForce(Vector3 f)
    {
        f *= this.m;
        this.acc += f;
    }

    public void applyForce(float f)
    {
        f *= this.m;
        this.acc.y += f;
    }
};
