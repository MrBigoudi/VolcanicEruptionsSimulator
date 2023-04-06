using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ParticleSPH {

    public const int DAM_WIDTH = 10;
    public const int DAM_HEIGHT = 10;
    public const int DAM_X = (DAM_WIDTH>>1);
    public const int DAM_Y = (DAM_HEIGHT>>1);

    private GameObject particle;
    public ArrayList particlesGenerated = new ArrayList();

    public int nbMaxParticles = 0;
    private int nbCurParticles = 0;

    public ParticleSPH(GameObject particle, int maxParticles){
        this.particle = particle;
        nbMaxParticles = maxParticles;

        // creating a dam
        // CreateDam();
    }

    public float K_POLY6(float r){
        if(r<Constants.H) {
            float tmp = ((Constants.H*Constants.H)-r*r);
            return tmp*tmp*tmp;
        }
        else return 0.0f;
    }

    public float K_POLY6_Prime(float r){
        if(r<Constants.H) {
            float tmp = ((Constants.H*Constants.H)-r*r);
            return 4.0f*r*tmp*tmp;
        }
        else return 0.0f;    
    }

    public float W_POLY6(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.mPosition, p2.mPosition) / Constants.H;
        // Debug.Log(r);
        return Constants.ALPHA_POLY6 * K_POLY6(r);
    }

    public Vector3 W_POLY6_Grad(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.mPosition, p2.mPosition) / Constants.H;
        // Debug.Log(r);
        return Constants.ALPHA_POLY6 * K_POLY6_Prime(r) * (p1.mPosition - p2.mPosition);
    }

    public void Update(){
        ComputeNeighbours();
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
        Particle p = circle.GetComponent<Particle>();
        p.mPosition = position;
        p.AssignGridCell();
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
                Particle p = circle.GetComponent<Particle>();
                nbCurParticles = 0;
                circle.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0.0f, -1.0f, 0.0f));
            }
        }
    }

    private void ComputeNeighbours(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in particlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            Assert.IsTrue(curParticle != null);
            ArrayList newNeighbours = new ArrayList();
            // get neighbours
            ArrayList neighbours = curParticle.mCell.mParticles;
            // for each neighbours check if it is below the kernel radius
            foreach(Particle pj in neighbours){
                if(Vector3.Distance(curParticle.mPosition, pj.mPosition) < Constants.H){
                    newNeighbours.Add(pj);
                }
            }

            // update new neighbours
            curParticle.mNeighbours = newNeighbours;
        }
    }

    private void ComputeDensity(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in particlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            Assert.IsTrue(curParticle != null);
            float sum = 0.0f;
            int n = 0;
            // get neighbours
            ArrayList neighbours = curParticle.mNeighbours;
            // for each neighbours add to the sum
            foreach(Particle pj in neighbours){
                // Debug.Log("TEST NEIGHBOUR");
                float wij = W_POLY6(curParticle, pj);
                sum += pj.mMass*wij;
                n++;
            }
            // Debug.Log("n: " + n + ", sum: " + sum);
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
            ArrayList neighbours = curParticle.mNeighbours;

            // calculate forces using neighbours
            Vector3 pressureForce = new Vector3();
            foreach(Particle pj in neighbours){
                // pressure force
                Assert.IsTrue(pj.mRho > 0.0f);
                float factor = (pj.mMass*(curParticle.mPressure + pj.mPressure)) / (2.0f*pj.mRho);
                // Debug.Log("factor: " + factor + ", rho: " + pj.mRho);
                pressureForce += factor*W_POLY6_Grad(curParticle, pj);

            }
            curParticle.mPressureForce = pressureForce;
            curParticle.mAccelerationForce = curParticle.GetAcceleration();
        }
    }

    private void TimeIntegration(){
        float dt =  Time.deltaTime;
        // for every particles pi
        foreach(UnityEngine.GameObject pi in particlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();

            // get acceleration
            Vector3 acceleration = curParticle.mAccelerationForce + curParticle.mPressureForce;
            // get new velocity
            Vector3 newVelocity = curParticle.mVelocity + dt*acceleration;
            // get new position
            Vector3 newPosition = curParticle.mPosition + dt*newVelocity;

            // update particle
            curParticle.UpdateRigidBody(newPosition, newVelocity);
        }
    }
}