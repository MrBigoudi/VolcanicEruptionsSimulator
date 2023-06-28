using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;

/**
 * A class representing the grid storing lava heightmap
*/
public class LavaTextureMap : MonoBehaviour{

    List<ParticleGPU> _Particles;
    Vector3[] _Positions;
    float[] _Heights;

    public Material _Material;
    // private const int _ArraySize = 1023;
    private Mesh _Mesh;
    private MeshFilter _MeshFilter;

    [SerializeField]
    Camera _Camera;

    private RenderTexture _RenderTexture;

    private void GetTexture(){
        int res = (Terrain.activeTerrain.terrainData.heightmapResolution / 4) * 4;
        _RenderTexture = RenderTexture.GetTemporary(res, res);

        //transfer image from rendertexture to texture
        Texture2D texture = new Texture2D(res, res, TextureFormat.ARGB32, false);

        _Camera.targetTexture = _RenderTexture;
        _Camera.Render();
        RenderTexture.active = _RenderTexture;
        texture.ReadPixels(new Rect(Vector2.zero, new Vector2(res, res)), 0, 0);
        texture.Apply();  // Apply changes to the texture

        //save texture to file
        // byte[] png = texture.EncodeToPNG();
        // File.WriteAllBytes("Assets/test.png", png);
        // AssetDatabase.Refresh();

        //clean up variables
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(_RenderTexture);
        DestroyImmediate(texture);
    }

    /**
     * Init the lava heightmap
    */
    public void Awake(){
        // init the mesh
        // _Positions = new Vector3[_ArraySize];
        // _Heights = new float[_ArraySize];

        _Mesh = new Mesh();
        _Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _MeshFilter = gameObject.AddComponent<MeshFilter>();
        _MeshFilter.mesh = _Mesh;
        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _Material;
    }

    private void UpdtMesh(){
        _Mesh.SetVertices(_Positions);

        int _ArraySize = _Positions.Length;

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
    public void Updt(int nbParticles, float[] heights, Vector3[] positions){
        _Positions = new Vector3[nbParticles];
        _Heights = new float[nbParticles];

        for(int i=0; i<nbParticles; i++){
            _Positions[i] = positions[i];
            _Heights[i] = heights[i];
            // if(i==0) 
                // Debug.Log(i + ": " + heights[i] + ", " + positions[i]);
        }
        UpdtMesh();
        GetTexture();
    }

    // private void FetchPositions(List<ParticleGPU> particles){
    //     _Positions = new Vector3[_ArraySize];
    //     for(int i=0; i<particles.Count; i++){
    //         ParticleGPU p = particles[i];
    //         _Positions[i] = p._Position;
    //     }
    // }

    // private void FetchHeights(List<ParticleGPU> particles){
    //     _Heights = new float[_ArraySize];
    //     for(int i=0; i<particles.Count; i++){
    //         Particle p = particles[i];
    //         _Heights[i] = p._Height;
    //     }
    // }

}