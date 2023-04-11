using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class to generate particles
*/
public class ParticleGenerator : MonoBehaviour{
    /**
     * The game object prefab representing a particle
    */
    public GameObject mParticle;
    
    /**
     * The sph solver
    */
    public ParticleSPH mSph;

    /**
     * A boolean stating if the generator is shooting or not
    */
    private bool mIsShooting = false;

    /**
     * The maximum number of particles
    */
    public int mMaxParticles = 5000;

    /**
     * Initialize the generator at launch
    */
    public void Start(){
        // Debug.Log("Start");
        Grid.InitGrid();
        mSph = new ParticleSPH(mParticle, mMaxParticles);
    }

    /**
     * Update the generator at runtime
    */
    public void Update(){
        // UpdatePosition();
        // ManageInput();
        if(CanShoot()) GenerateParticle();
        mSph.Update();
    }

    /**
     * Update position of the generator
    */
    private void UpdatePosition(){
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(mousePosition.x, mousePosition.y);
    }

    /**
     * Free the memory at exit
    */
    public void OnApplicationQuit(){
        mSph.OnApplicationQuit();
    }

    /**
     * Generate a particle
    */
    private void GenerateParticle(){
        GameObject circle = mSph.GenerateParticle(transform.position);
        // circle.GetComponent<Rigidbody2D>().AddRelativeForce(circle.GetComponent<Particle>().mVelocity);
    }

    /**
     * Input manager
    */
    private void ManageInput(){
        if(Input.GetButtonDown("Fire1")) mIsShooting = true;
        if(Input.GetButtonUp("Fire1")) mIsShooting = false;
    }

    /**
     * Check if the generator can shoot or not
    */
    private bool CanShoot(){
        // return isShooting && sph.CanGenerateParticle();
        return mSph.CanGenerateParticle();
    }
}