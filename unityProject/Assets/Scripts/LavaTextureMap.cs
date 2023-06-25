using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/**
 * A class representing the grid storing lava heightmap
*/
public class LavaTextureMap : MonoBehaviour{

    List<Particle> _Particles;
    Vector4[] _Positions;
    float[] _Heights;

    public Material _Material;
    private const int _ArraySize = 1023;

    /**
     * Init the lava heightmap
    */
    public void Awake(){
        // init the mesh
        _Positions = new Vector4[_ArraySize];
        _Heights = new float[_ArraySize];

        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _Material;
    }

    private void UpdtMesh(){
        _Material.SetVectorArray("_ParticlePositions", _Positions);
        _Material.SetFloatArray("_ParticleHeights", _Heights);
    }

    /**
     * Update the lava's heights
    */
    public void Updt(List<Particle> particles){
        FetchPositions(particles);
        FetchHeights(particles);
        UpdtMesh();
    }

    private void FetchPositions(List<Particle> particles){
        _Positions = new Vector4[_ArraySize];
        for(int i=0; i<particles.Count; i++){
            Particle p = particles[i];
            Vector3 pos = p.GetPosition();
            _Positions[i] = new Vector4(pos.x, pos.y, pos.z, 1.0f);
        }
    }

    private void FetchHeights(List<Particle> particles){
        _Heights = new float[_ArraySize];
        for(int i=0; i<particles.Count; i++){
            Particle p = particles[i];
            _Heights[i] = p.mHeight;
        }
    }

}