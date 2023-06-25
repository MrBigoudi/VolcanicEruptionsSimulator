using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/**
 * A class representing the grid storing lava heightmap
*/
public class LavaTextureMap : MonoBehaviour{

    List<Particle> _Particles;
    Vector3[] _Positions;
    float[] _Heights;

    public Material _Material;
    private const int _ArraySize = 1023;
    private Mesh _Mesh;
    private MeshFilter _MeshFilter;

    /**
     * Init the lava heightmap
    */
    public void Awake(){
        // init the mesh
        _Positions = new Vector3[_ArraySize];
        _Heights = new float[_ArraySize];

        _Mesh = new Mesh();
        _Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _MeshFilter = gameObject.AddComponent<MeshFilter>();
        _MeshFilter.mesh = _Mesh;
        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _Material;
    }

    private void UpdtMesh(){
        _Mesh.SetVertices(_Positions);

        // Generate indices for the points
        int[] indices = new int[_ArraySize];
        Vector2[] uvs = new Vector2[_ArraySize];

        for (int i = 0; i < _ArraySize; i++){
            indices[i] = i;
            uvs[i] = new Vector2(_Heights[i], 0.0f);
        }
        
        _Mesh.SetIndices(indices, MeshTopology.Points, 0);
        _Mesh.SetUVs(0, uvs);

        _Mesh.UploadMeshData(false);
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
        _Positions = new Vector3[_ArraySize];
        for(int i=0; i<particles.Count; i++){
            Particle p = particles[i];
            _Positions[i] = p.GetPosition();
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