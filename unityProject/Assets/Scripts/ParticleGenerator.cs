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
     * The lava height map
    */
    public LavaTextureMap mLavaTextureMap;

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
        mLavaTextureMap.Init();
        // generate initial ghosts particles
        // GenerateGhostParticles();
    }

    private void GenerateGhostParticles(){
        // place a particle in every cell of the grid
        for(int i=5; i<Grid.mNbLines-5; i++){
            for(int j=5; j<Grid.mNbCols-5; j++){
                float curX = Grid.mCells[i,j].mX;
                float curZ = Grid.mCells[i,j].mZ;
                float curY = Terrain.activeTerrain.SampleHeight(new Vector3(curX, 0.0f, curZ));
                Vector3 position = new Vector3(curX, curY, curZ);
                GameObject circle = mSph.GenerateParticle(position, true);
            }
        }
    }

    /**
     * Update the generator at runtime
    */
    public void Update(){
        // UpdatePosition();
        // ManageInput();
        if(CanShoot()) GenerateParticle();
        mSph.Update();
        mLavaTextureMap.Update();
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
        pos.y = ParticleSPH.GetTerrainHeight(pos);
        // Debug.Log("position: " + pos.x + ", " + pos.y + ", " + pos.z);
        return pos;
    }


    /**
     * Generate a particle
    */
    private void GenerateParticle(){
        for(int i=0; i<1; i++){
            Vector3 position = GetRandomPosition(mDelta);
            GameObject circle = mSph.GenerateParticle(position);
            // circle.GetComponent<Rigidbody2D>().AddRelativeForce(circle.GetComponent<Particle>().mVelocity);
        }
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