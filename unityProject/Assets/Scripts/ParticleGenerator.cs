using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class to generate particles
*/
public class ParticleGenerator : MonoBehaviour{
    
    /**
     * The sph solver
    */
    private ParticleSPHGPU mSphGPU;

    public ComputeShader _Shader;

    /**
     * The lava height map
    */
    public LavaTextureMap mLavaTextureMap;

    [SerializeField]
    public TerrainGenerator _TerrainGenerator;

    /**
     * The maximum number of particles
    */
    [SerializeField, Range(500, 100000)]
    public int mMaxParticles = 50000;

    [SerializeField, Range(0.0f, 25.0f)]
    public float _Stiffness = Constants.STIFFNESS;

    /**
     * The variation delta for the particle generation
    */
    public float mDelta = 1.0f;

    public int GetNbCurParticles(){
        return mSphGPU.mNbCurParticles;
    }

    /**
     * Initialize the generator at launch
    */
    public void Start(){
        _TerrainGenerator.Init();
        StaggeredGridV2.Init(_TerrainGenerator);
        _TerrainGenerator.GetGradients(StaggeredGridV2._Gradients);
        _TerrainGenerator.SetNormals();
        mLavaTextureMap.Init();
        mSphGPU = gameObject.AddComponent(typeof(ParticleSPHGPU)) as ParticleSPHGPU;
        mSphGPU.Create(mMaxParticles, _Shader, _TerrainGenerator);
    }

    /**
     * Update the generator at runtime
    */
    public void Update(){
        Vector3 position = GetRandomPosition(mDelta);
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
}