using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class to generate particles
*/
public class ParticleGenerator : MonoBehaviour{
    
    [SerializeField]
    public ParticleSPHGPU _SphGPU;

    [SerializeField]
    public ComputeShader _Shader;

    [SerializeField]
    public TerrainGenerator _TerrainGenerator;

    [SerializeField, Range(1, 100000)]
    public int _MaxParticles = 50000;

    [SerializeField, Range(0.0f, 25.0f)]
    public float _Stiffness = Constants.STIFFNESS;

    /**
     * The variation delta for the particle generation
    */
    public float _Delta = 1.0f;

    public int GetNbCurParticles(){
        return _SphGPU.GetNbCurParticles();
    }

    /**
     * Initialize the generator at launch
    */
    public void Start(){
        _TerrainGenerator.Init();
        StaggeredGridV2.Init(_TerrainGenerator);
        _SphGPU.Create(_MaxParticles, _Shader, _TerrainGenerator);

        Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace);
    }

    /**
     * Update the generator at runtime
    */
    public void Update(){
        Vector3 position = GetRandomPosition(_Delta);
        _SphGPU.Updt(position, _Stiffness);
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