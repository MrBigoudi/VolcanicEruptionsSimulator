using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSPH {

    public const int DAM_WIDTH = 4;
    public const int DAM_HEIGHT = 4;
    public const int DAM_X = (DAM_WIDTH>>1);
    public const int DAM_Y = (DAM_HEIGHT>>1);

    private GameObject particle;
    public ArrayList particlesGenerated = new ArrayList();

    public int nbMaxParticles = 5000;
    private int nbCurParticles = 0;

    public float K(float r){
        if(r<1) return 1 - ((3/2)*r*r) + ((3/4)*r*r*r);
        if(r<2) return (1/4) * ((2-r)*(2-r)*(2-r));
        else return 0;
    }

    public float K_prime(float r){
        if(r<1) return -3*r + ((9/4)*r*r);
        if(r<2) return -(3/4) * ((2-r)*(2-r));
        else return 0;    
    }

    public float W(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.mPosition, p2.mPosition) / Constants.H;
        return K(r);
    }

    public Vector2 W_grad(Particle p1, Particle p2){
        float dist = Vector2.Distance(p1.mPosition, p2.mPosition);
        float r = dist / Constants.H;
        float factor = (Constants.ALPHA_2D * K_prime(r)) / (Constants.H*dist);
        return factor*(p1.mPosition - p2.mPosition);
    }

    public ParticleSPH(GameObject particle){
        this.particle = particle;

        // creating a dam
        CreateDam();
    }

    public void Update(){
        ComputeDensity();
        ComputePressure();
        ComputeForces();
        TimeIntegration();
    }

    public void OnApplicationQuit(){
        foreach(UnityEngine.Object element in particlesGenerated){
            UnityEngine.Object.Destroy(element);
        }
    }

    public GameObject GenerateParticle(Vector3 position){
        GameObject circle = GameObject.Instantiate(particle, position, new Quaternion());
        // add the circle to the list of particles generated
        particlesGenerated.Add(circle);
        circle.GetComponent<Particle>().mPosition = new Vector2(position.x, position.y);
        nbCurParticles++;
        return circle;
    }

    public bool CanGenerateParticle(){
        return (nbCurParticles <= nbMaxParticles);
    }

    private void CreateDam(){

        float particleSize = particle.GetComponent<Renderer>().bounds.size.x;
        float particleSizeHalf = particleSize / 2;

        for(int i = -DAM_X; i<DAM_X; i++){
            for(int j = -DAM_Y; j<DAM_Y; j++){
                float x = i * particleSize + particleSizeHalf;
                float y = j * particleSize + particleSizeHalf;
                Vector3 position = new Vector3(x, y, 0.0f);
                GameObject circle = GenerateParticle(position);
                nbCurParticles = 0;
                circle.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0.0f, -2.0f));
            }
        }
    }

    private void ComputeDensity(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in particlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            float sum = 0.0f;
            // get neighbours
            ArrayList neighbours = curParticle.GetNeighbours();
            // for each neighbours add to the sum
            foreach(Particle pj in neighbours){
                float wij = W(curParticle, pj);
                sum += pj.mMass*wij;
            }
            // get the pi's density
            curParticle.mRho = sum;
        }
    }    
    
    private void ComputePressure(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in particlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            float presssure = Constants.STIFFNESS*(curParticle.mRho - Constants.RHO_0);
            // avoid negative values
            curParticle.mPressure = Mathf.Max(presssure, 0.0f);
        }
    }

    private void ComputeForces(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in particlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            // get neighbours
            ArrayList neighbours = curParticle.GetNeighbours();

            // calculate forces using neighbours
            Vector2 pressureForce = new Vector2(0.0f, 0.0f);
            Vector2 accelerationForce = new Vector2(0.0f, 0.0f);
            foreach(Particle pj in neighbours){
                // pressure force
                float factor = (pj.mMass*(curParticle.mPressure + pj.mPressure)) / (2*pj.mRho);
                pressureForce += factor*W_grad(curParticle, pj);

                // acceleration
                accelerationForce = curParticle.GetAcceleration();
            }

            curParticle.mPressureForce = pressureForce;
            curParticle.mAccelerationForce = accelerationForce;
        }
    }

    private void TimeIntegration(){
        float dt =  Time.deltaTime;
        // for every particles pi
        foreach(UnityEngine.GameObject pi in particlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();

            // get acceleration
            Vector2 acceleration = curParticle.mAccelerationForce + curParticle.mPressureForce;
            // get new velocity
            Vector2 newVelocity = curParticle.mVelocity + dt*acceleration;
            // get new position
            Vector2 newPosition = curParticle.mPosition + dt*newVelocity;

            // update particle
            curParticle.UpdatePosition(newPosition, newVelocity);
        }
    }
}