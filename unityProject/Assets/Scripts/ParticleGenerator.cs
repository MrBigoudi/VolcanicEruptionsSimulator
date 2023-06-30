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
    // public Particle mParticle;
    
    /**
     * The sph solver
    */
    // public ParticleSPH mSph;
    public ParticleSPHGPU mSphGPU;

    public ComputeShader _Shader;

    /**
     * The lava height map
    */
    public LavaTextureMap mLavaTextureMap;

    /**
     * The maximum number of particles
    */
    [SerializeField, Range(500, 100000)]
    public int mMaxParticles = 50000;

    [SerializeField, Range(1.0f, 100.0f)]
    public float _Stiffness = Constants.STIFFNESS;

    /**
     * The variation delta for the particle generation
    */
    public float mDelta = 1.0f;

    /**
     * Initialize the generator at launch
    */
    public void Start(){
        // init the neighbour search grid
        // Grid.InitGrid();
        // init the sph solver
        // mSph = new ParticleSPH(mParticle, mMaxParticles);
        // init the staggered grid
        StaggeredGridV2.Init();
        // init the lava texture grid
        // mLavaTextureMap.Init();
        mSphGPU = gameObject.AddComponent(typeof(ParticleSPHGPU)) as ParticleSPHGPU;
        mSphGPU.Create(mMaxParticles, _Shader);
    }

    /**
     * Update the generator at runtime
    */
    public void Update(){
        // UpdatePosition();
        // ManageInput();
        Vector3 position = GetRandomPosition(mDelta);
        // if(CanShoot()) {
        //     position = GenerateParticle();
        // }
        // mSph.Update();
        mSphGPU.Updt(position, _Stiffness);
        mLavaTextureMap.Updt(mSphGPU.mNbCurParticles, mSphGPU._Heights, mSphGPU._Positions);
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
        pos.y = StaggeredGridV2.GetHeight(pos);
        // Debug.Log("position: " + pos.x + ", " + pos.y + ", " + pos.z);
        return pos;
    }


    // /**
    //  * Generate a particle
    // */
    // private Vector3 GenerateParticle(){
    //     // for(int i=0; i<1; i++){
    //         Vector3 position = GetRandomPosition(mDelta);
    //         Particle circle = mSph.GenerateParticle(position);
    //         // circle.GetComponent<Rigidbody2D>().AddRelativeForce(circle.GetComponent<Particle>().mVelocity);
    //     // }
    //     return position;
    // }

    // /**
    //  * Input manager
    // */
    // private void ManageInput(){
    // }

    // /**
    //  * Check if the generator can shoot or not
    // */
    // private bool CanShoot(){
    //     // return isShooting && sph.CanGenerateParticle();
    //     return mSph.CanGenerateParticle();
    // }
}