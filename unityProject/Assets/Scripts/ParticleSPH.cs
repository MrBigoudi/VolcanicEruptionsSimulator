using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * A class representing an sph solver
*/
public class ParticleSPH {
    /**
     * The dam's width
    */
    public const int DAM_WIDTH = 10;

    /**
     * The dam's height
    */
    public const int DAM_HEIGHT = 10;

    /**
     * The dam's x coordinate
    */
    public const int DAM_X = (DAM_WIDTH>>1);

    /**
     * The dam's y coordinate
    */
    public const int DAM_Y = (DAM_HEIGHT>>1);

    /**
     * The game object prefab representing a particle
    */
    private GameObject mParticle;

    /**
     * The list of particles to manage
    */
    public ArrayList mParticlesGenerated = new ArrayList();

    /**
     * The maximum number of particles
    */
    public int mNbMaxParticles = 0;

    /**
     * The current number of particles
    */
    private int mNbCurParticles = 0;



    /**
     * A basic constructor
     * @param particle The particle prefab
     * @param maxParticles The maximum number of particles
    */
    public ParticleSPH(GameObject particle, int maxParticles){
        mParticle = particle;
        mNbMaxParticles = maxParticles;

        // creating a dam
        // CreateDam();
    }

    /**
     * Part of the poly6 kernel calculation
     * @param r The distance between two particles
     * @return Part of the poly6 kernel calculation
    */
    public float K_POLY6(float r){
        if(r<Constants.H) {
            float tmp = ((Constants.H*Constants.H)-r*r);
            return tmp*tmp*tmp;
        }
        else return 0.0f;
    }

    /**
     * Part of the derivated poly6 kernel calculation
     * @param r The distance between two particles
     * @return Part of the derivated poly6 kernel calculation
    */
    public float K_POLY6_Prime(float r){
        if(r<Constants.H) {
            float tmp = ((Constants.H*Constants.H)-r*r);
            return 4.0f*r*tmp*tmp;
        }
        else return 0.0f;    
    }

    /**
     * Poly6 kernel calculation
     * @param p1 The first particle
     * @param p2 The second particle
     * @return The poly6 kernel calculation
    */
    public float W_POLY6(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.GetPosition(), p2.GetPosition()) / Constants.H;
        // Debug.Log(r);
        return Constants.ALPHA_POLY6 * K_POLY6(r);
    }

    /**
     * Derivated poly6 kernel calculation
     * @param p1 The first particle
     * @param p2 The second particle
     * @return The derivated poly6 kernel calculation
    */
    public Vector3 W_POLY6_Grad(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.GetPosition(), p2.GetPosition()) / Constants.H;
        // Debug.Log(r);
        return Constants.ALPHA_POLY6 * K_POLY6_Prime(r) * (p1.GetPosition() - p2.GetPosition());
    }

    /**
     * Update particles' positions
    */
    public void Update(){
        // update particles' neighbours
        ComputeNeighbours();
        // updpate particles' density
        ComputeDensity();
        // update particles' pressure
        ComputePressure();
        // update external forces
        ComputeForces();
        // integrate and update positions
        TimeIntegration();
    }

    /**
     * Free memory when quitting the app
    */
    public void OnApplicationQuit(){
        foreach(UnityEngine.Object element in mParticlesGenerated){
            UnityEngine.Object.Destroy(element);
        }
    }

    /**
     * Generate a particle
     * @param position The particle's position
     * @return The particle as a GameObject
    */
    public GameObject GenerateParticle(Vector3 position){
        GameObject circle = GameObject.Instantiate(mParticle, position, new Quaternion());
        // add the circle to the list of particles generated
        mParticlesGenerated.Add(circle);
        Particle p = circle.GetComponent<Particle>();
        // p.mPosition() = position;
        p.AssignGridCell();
        mNbCurParticles++;
        return circle;
    }

    /**
     * Check if a particle can be generated
     * @return True if it can
    */
    public bool CanGenerateParticle(){
        return (mNbCurParticles <= mNbMaxParticles);
    }

    /**
     * Create a particle dam
    */
    private void CreateDam(){
        float particleSize = mParticle.GetComponent<Renderer>().bounds.size.x;
        float particleSizeHalf = particleSize / 2;

        for(int i = -DAM_X; i<DAM_X; i++){
            for(int j = -DAM_Y; j<DAM_Y; j++){
                float x = i * particleSize + particleSizeHalf;
                float y = j * particleSize + particleSizeHalf;
                Vector3 position = new Vector3(x, y, 0.0f);
                GameObject circle = GenerateParticle(position);
                Particle p = circle.GetComponent<Particle>();
                mNbCurParticles = 0; // do not count the dam for the max number of particles
                circle.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0.0f, -1.0f, 0.0f));
            }
        }
    }

    /**
     * Update particles' neighbours
    */
    private void ComputeNeighbours(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            Assert.IsTrue(curParticle != null);
            ArrayList newNeighbours = new ArrayList();
            // get neighbours
            ArrayList neighbours = curParticle.mCell.mParticles;
            // for each neighbours check if it is below the kernel radius
            foreach(Particle pj in neighbours){
                if(Vector3.Distance(curParticle.GetPosition(), pj.GetPosition()) < Constants.H){
                    newNeighbours.Add(pj);
                }
            }

            // update new neighbours
            curParticle.mNeighbours = newNeighbours;
        }
    }

    /**
     * Update particles' density
    */
    private void ComputeDensity(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
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
    
    /**
     * Update particles' pressure
    */
    private void ComputePressure(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            float presssure = Constants.STIFFNESS*(curParticle.mRho - Constants.RHO_0);
            // avoid negative values
            curParticle.mPressure = Mathf.Max(presssure, 0.0f);
        }
    }

    /**
     * Update forces applied to particles
    */
    private void ComputeForces(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
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

    /**
     * Update particles' positions
    */
    private void TimeIntegration(){
        float dt =  Time.deltaTime;
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();

            // get acceleration
            Vector3 acceleration = curParticle.mAccelerationForce + curParticle.mPressureForce;
            // get new velocity
            Vector3 newVelocity = curParticle.mVelocity + dt*acceleration;
            // get new position
            Vector3 newPosition = curParticle.GetPosition() + dt*newVelocity;
            // newPosition.y = 0;
            Assert.IsTrue(curParticle.GetComponent<Rigidbody>().position == curParticle.GetPosition());
            newPosition.y = Terrain.activeTerrain.SampleHeight(curParticle.GetComponent<Rigidbody>().position) + Particle.mRadius;
            Debug.Log("heigthTerrain: " + Terrain.activeTerrain.SampleHeight(newPosition) + "\n"
                    + "newPos: " + newPosition.x + ", " + newPosition.y + ", " + newPosition.z + "\n"
                    + "curPos: " + curParticle.GetPosition().x + ", " + curParticle.GetPosition().y + ", " + curParticle.GetPosition().z + "\n"
                    );

            // update particle
            curParticle.UpdateRigidBody(newPosition, newVelocity);
        }
    }
}