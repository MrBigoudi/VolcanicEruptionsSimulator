using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class to generate particles
*/
public class ParticleGenerator : MonoBehaviour{
    
    [SerializeField]
    public Tweakable _Fields;

    private ParticleSPHGPU _SphGPU;
    private ComputeShader _Shader;
    private TerrainGenerator _TerrainGenerator;

    public int GetNbCurParticles(){
        return _SphGPU.GetNbCurParticles();
    }

    /**
     * Initialize the generator at launch
    */
    public void Start(){
        _SphGPU = _Fields._SphGPU;
        _Shader = _Fields._Shader;
        _TerrainGenerator = _Fields._TerrainGenerator;

        _TerrainGenerator.Init();
        StaggeredGridV2.Init(_TerrainGenerator);
        _SphGPU.Create(_Shader, _TerrainGenerator);

        Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SetLeakDetectionMode(Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace);
    }

    /**
     * Update the generator at runtime
    */
    public void Update(){
        Vector3 pos = transform.position;
        pos.y = StaggeredGridV2.GetHeight(pos);
        _SphGPU.Updt(transform.position);
    }

}