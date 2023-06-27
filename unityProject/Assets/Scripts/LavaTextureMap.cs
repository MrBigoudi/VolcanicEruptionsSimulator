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

    List<Particle> _Particles;
    Vector3[] _Positions;

    float  [] _Heights;
    Vector3[] _HeightsGradients;
    float  [] _HeightsLaplacians;

    float  [] _TerrainHeights;
    Vector3[] _TerrainGradients;
    float  [] _TerrainLaplacians;

    public Material _Material;
    private const int _ArraySize = 1023;
    private Mesh _Mesh;
    private MeshFilter _MeshFilter;
    private RenderTexture _RenderTexture;

    private void GetTexture(){
        int res = (Terrain.activeTerrain.terrainData.heightmapResolution / 4) * 4;
        _RenderTexture = RenderTexture.GetTemporary(res, res);
        Graphics.Blit(null, _RenderTexture, _Material);

        //transfer image from rendertexture to texture
        Texture2D texture = new Texture2D(res, res, TextureFormat.ARGB32, false);
        RenderTexture.active = _RenderTexture;
        texture.ReadPixels(new Rect(Vector2.zero, new Vector2(res, res)), 0, 0);
        texture.Apply();  // Apply changes to the texture

        //save texture to file
        byte[] png = texture.EncodeToPNG();
        File.WriteAllBytes("Assets/test.png", png);
        AssetDatabase.Refresh();

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
        Vector2[] heights = new Vector2[_ArraySize];
        Vector3[] heightsGradients = new Vector3[_ArraySize];
        Vector2[] terrain = new Vector2[_ArraySize];
        Vector3[] terrainGradients = new Vector3[_ArraySize];

        for (int i = 0; i < _ArraySize; i++){
            indices[i] = i;
            heights[i] = new Vector3(_Heights[i], _HeightsLaplacians[i]);
            heightsGradients[i] = _HeightsGradients[i];
            terrain[i] = new Vector3(_TerrainHeights[i], _TerrainLaplacians[i]);
            terrainGradients[i] = _TerrainGradients[i];
        }
        
        _Mesh.SetIndices(indices, MeshTopology.Points, 0);
        _Mesh.SetUVs(0, heights);
        _Mesh.SetUVs(1, terrain);
        _Mesh.SetUVs(2, heightsGradients);
        _Mesh.SetUVs(3, terrainGradients);

        _Material.SetFloat("dt", Time.deltaTime);

        _Mesh.UploadMeshData(false);
    }

    /**
     * Update the lava's heights
    */
    public void Updt(List<Particle> particles){
        FetchParticlesInfos(particles);
        UpdtMesh();
        GetTexture();
    }

    private void FetchParticlesInfos(List<Particle> particles){
        _Positions = new Vector3[_ArraySize];

        _Heights = new float[_ArraySize];
        _HeightsGradients = new Vector3[_ArraySize];
        _HeightsLaplacians = new float[_ArraySize];

        _TerrainHeights = new float[_ArraySize];
        _TerrainGradients = new Vector3[_ArraySize];
        _TerrainLaplacians = new float[_ArraySize];

        for(int i=0; i<particles.Count; i++){
            Particle p = particles[i];

            _Positions[i] = p.GetPosition();

            _Heights[i] = p.mHeight;
            _HeightsGradients[i] = p.mHeightGradient;
            _HeightsLaplacians[i] = p.mHeightLaplacian;

            _TerrainHeights[i] = StaggeredGridV2.GetHeight(p.GetPosition());
            _TerrainGradients[i] = StaggeredGridV2.GetGradient(p);
            _TerrainLaplacians[i] = StaggeredGridV2.GetLaplacian(p.GetPosition());
        }
    }
}