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
     * The list of particles to remove
    */
    public ArrayList mParticlesToRemove = new ArrayList();

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

        // init the staggered grid
        StaggeredGrid.Init();
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
     * Part of the viscosity kernel calculation
     * @param r The distance between two particles
     * @return Part of the viscosity kernel calculation
    */
    public float K_VISCOSITY(float r){
        float l = Constants.H;
        if(r<l) {
            return r*r*(-4.0f*r + 9.0f*l) + l*l*l*(-5.0f + 6.0f*(Mathf.Log(l)-Mathf.Log(r)));
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
            return -6.0f*r*tmp*tmp;
        }
        else return 0.0f;    
    }

    /**
     * Part of the derivated viscosity kernel calculation
     * @param r The distance between two particles
     * @return Part of the derivated viscosity kernel calculation
    */
    public float K_VISCOSITY_Prime(float r){
        float l = Constants.H;
        if(r<l) {
            if(r != 0) return r*(-12.0f*r + 18.0f*l) - (6.0f*l*l*l) / r;
            return 0.0f;
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
     * Viscosity kernel calculation
     * @param p1 The first particle
     * @param p2 The second particle
     * @return The viscosity kernel calculation
    */
    public float W_VISCOSITY(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.GetPosition(), p2.GetPosition()) / Constants.H;
        // Debug.Log(r);
        return Constants.ALPHA_VISCOSITY * K_VISCOSITY(r);
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
     * Derivated viscosity kernel calculation
     * @param p1 The first particle
     * @param p2 The second particle
     * @return The derivated viscosity kernel calculation
    */
    public Vector3 W_VISCOSITY_Grad(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.GetPosition(), p2.GetPosition()) / Constants.H;
        float prime = K_VISCOSITY_Prime(r);
        Vector2 pos = (p1.GetPosition() - p2.GetPosition());
        // Debug.Log("radius: " + r);
        // Debug.Log("alpha: " + Constants.ALPHA_VISCOSITY);
        // Debug.Log("prime: " + prime);
        // Debug.Log("pos: " + pos);
        return Constants.ALPHA_VISCOSITY * prime * pos;
    }

    /**
     * Laplacien viscosity kernel calculation
     * @param p1 The first particle
     * @param p2 The second particle
     * @return The derivated viscosity kernel calculation
    */
    public float W_VISCOSITY_laplacien(Particle p1, Particle p2){
        float r = Vector2.Distance(p1.GetPosition(), p2.GetPosition()) / Constants.H;
        float l = Constants.H;
        return Constants.ALPHA_VISCOSITY_LAPLACIEN*(l-r);
    }

    /**
     * Get the height at a given position
     * @param pos The current position\
     * @return The height
    */
    public float GetTerrainHeight(Vector3 pos){
        return StaggeredGrid.GetHeight(pos);
    }

    /**
     * Compute the surface gradient at a given point
     * @param p The current particle
     * @return The gradient
    */
    public Vector3 GetGradient(Particle p){
        Vector3 pos = p.transform.position;

        // decompose the position
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;

        float curSurface = GetTerrainHeight(pos) + p.mHeight;

        float partialX = GetTerrainHeight(new Vector3(x+Constants.GRAD_DELTA, y, z)) - curSurface;
        float partialZ = GetTerrainHeight(new Vector3(x, y, z+Constants.GRAD_DELTA)) - curSurface;

        return new Vector3(partialX, y, partialZ);
    }


    /**
     * Update particles' positions
    */
    public void Update(){
        // update particles' neighbours
        ComputeNeighbours();
        // updpate particles' density
        ComputeDensity();
        // update viscosity forces applied
        // ComputeViscosity();
        // integrate and update positions
        TimeIntegration();
        // update the color for debugging purposes
        UpdateColors();
        // remove the particles otside the boundaries
        RemoveParticles();
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
                // circle.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0.0f, -1.0f, 0.0f));
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
            ArrayList neighbours = curParticle.mCell.GetAllParticles();
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
     * Update particles' colours
    */
    private void UpdateColors(){
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
            pi.GetComponent<Particle>().UpdateColor();
        }
    }

    /**
     * Update particles' density
    */
    private void ComputeDensity(){
        // Debug.Log("\n\n################################ Density ##############################\n\n");
        Particle.sMaxRho = 0.0f;
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            Assert.IsTrue(curParticle != null);
            float sum = 0.0f;
            int n = 0;
            // get neighbours
            ArrayList neighbours = curParticle.mNeighbours;
            // Debug.Log("nbNeighbour: " + neighbours.Count);
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
            if(sum > Particle.sMaxRho) Particle.sMaxRho = sum;
            // update the particle's height
            curParticle.UpdateHeight();
            // update the particle's mass
            // curParticle.UpdateMass();
        }
    }

    /**
     * Update viscosity force applied on particles
    */
    private void ComputeViscosity(){
        Particle.sMaxVisc = 0.0f;
        // for every particles pi
        foreach(UnityEngine.GameObject pi in mParticlesGenerated){
            Particle curParticle = pi.GetComponent<Particle>();
            Assert.IsTrue(curParticle != null);

            Vector3 sumVelocity = new Vector3();
            float sumVisc = 0.0f;
            // get neighbours
            ArrayList neighbours = curParticle.mNeighbours;
            Assert.IsTrue(neighbours.Count > 0);

            // for each neighbours add to the sums
            foreach(Particle pj in neighbours){

                // get the velocity
                Vector3 u_ji = pj.mVelocity - curParticle.mVelocity;
                float laplacienW_ij = W_VISCOSITY_laplacien(curParticle, pj);
                float factor = 0.0f;
                if(pj.mRho != 0.0f){
                    factor = (pj.mMass/pj.mRho)*laplacienW_ij;
                } else {
                    factor = (pj.mMass/Constants.RHO_0)*laplacienW_ij;
                }
                sumVelocity += factor*u_ji;
                sumVisc += factor*pj.mVisc;
            }
            // get the viscosity force applied on pi
            curParticle.mVisc = sumVisc;
            if(sumVisc > Particle.sMaxVisc) Particle.sMaxVisc = sumVisc;
            curParticle.mViscosityForce = sumVisc*curParticle.mMass*sumVelocity;
            // Debug.Log("visocsity: "+curParticle.mViscosityForce);
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
            // get position
            Vector3 curPosition = curParticle.GetPosition();
            // get new velocity
            Vector3 newVelocity = (-Constants.G / Constants.STIFFNESS)*GetGradient(curParticle);// + curParticle.mViscosityForce;
            // get new position
            Vector3 newPosition = dt*newVelocity + curPosition;
            // newPosition.y = 0;
            // Assert.IsTrue(curParticle.GetComponent<Rigidbody>().position == curParticle.GetPosition());
            newPosition.y = GetTerrainHeight(newPosition) + curParticle.GetComponent<SphereCollider>().radius;

            // update particle
            if(!curParticle.UpdateRigidBody(newPosition, newVelocity)){
                mParticlesToRemove.Add(pi);
            }
        }
    }

    /**
     * Remove the particles outside the boundary
    */
    private void RemoveParticles(){
        // for every particles to remove pi
        while(mParticlesToRemove.Count>0){
            UnityEngine.GameObject pi = (UnityEngine.GameObject) mParticlesToRemove[0];
            Particle curParticle = pi.GetComponent<Particle>();
            // delete the particle
            mParticlesGenerated.Remove(pi);
            mParticlesToRemove.RemoveAt(0);
            // clean the cell
            if(curParticle.mCell != null) curParticle.mCell.mParticles.Remove(curParticle);
            UnityEngine.Object.Destroy(pi);
            mNbCurParticles--;
        }
    }

}