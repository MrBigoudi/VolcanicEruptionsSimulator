using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class to generate particles
*/
public class ParticleGenerator : MonoBehaviour{

// ################################################################################################################################################################################################################
// ################################################################################################## ATTRIBUTES ##################################################################################################
// ################################################################################################################################################################################################################
    /**
     * The class used to regroup all the serializied fields
    */
    [SerializeField]
    public Tweakable _Fields;

    /**
     * The main class containing the core of the sph simulation
    */
    private ParticleSPHGPU _SphGPU;

    /**
     * The compute shader containing the core of the sph simulation
    */
    private ComputeShader _Shader;

    /**
     * The class that generates the terrain
    */
    private TerrainGenerator _TerrainGenerator;


// ################################################################################################################################################################################################################
// ################################################################################################### METHODS ####################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Get the number of particles
     * @return The number of particles
    */
    public int GetNbCurParticles(){
        return _SphGPU.GetNbCurParticles();
    }

    /**
     * Initialize everything
    */
    public void Start(){
        // get serialized fields
        _SphGPU = _Fields._SphGPU;
        _Shader = _Fields._Shader;
        _TerrainGenerator = _Fields._TerrainGenerator;

        // initialize other classes
        _TerrainGenerator.Init();
        StaggeredGridV2.Init(_TerrainGenerator);
        _SphGPU.Create(_Shader, _TerrainGenerator);

        // set to avoid warnings
        Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace);
    }

    /**
     * Update the simulation
    */
    public void Update(){
        Vector3 pos = transform.position;
        _SphGPU.Updt(pos);
    }

}