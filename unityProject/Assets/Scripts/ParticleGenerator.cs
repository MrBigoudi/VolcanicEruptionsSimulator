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
     * The maximum number of particles
    */
    public int mMaxParticles = 500;

    /**
     * The variation delta for the particle generation
    */
    public float mDelta = 5.0f;

    /**
     * Initialize the generator at launch
    */
    public void Start(){
        // init the neighbour search grid
        Grid.InitGrid();
        // init the sph solver
        mSph = new ParticleSPH(mParticle, mMaxParticles);
        // init the staggered grid
        StaggeredGrid.Init();
        // init the lava texture grid
        LavaTextureMap.Init();
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
     * Get a random position around the generator
     * @param delta The delta arround which the position can change
     * @return The random position
    */
    private Vector3 GetRandomPosition(float delta){
        float v1 = Random.value*2*delta - delta;
        float v2 = Random.value*2*delta - delta;
        Vector3 pos = transform.position;
        pos.x += v1;
        pos.z += v2;
        pos.y = mSph.GetTerrainHeight(pos);
        // Debug.Log("position: " + pos.x + ", " + pos.y + ", " + pos.z);
        return pos;
    }


    /**
     * Generate a particle
    */
    private void GenerateParticle(){
        Vector3 position = GetRandomPosition(mDelta);
        GameObject circle = mSph.GenerateParticle(position);
        // circle.GetComponent<Rigidbody2D>().AddRelativeForce(circle.GetComponent<Particle>().mVelocity);
    }

    /**
     * Input manager
    */
    private void ManageInput(){
    }

    /**
     * Check if the generator can shoot or not
    */
    private bool CanShoot(){
        // return isShooting && sph.CanGenerateParticle();
        return mSph.CanGenerateParticle();
    }
}