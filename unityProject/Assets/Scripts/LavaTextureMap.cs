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
    Vector2[] _Indices;
    float[] _Heights;
    List<Vector3> _UpdatedIndices;
    Vector3[,] _Tmp;
    int _NbCols;

    // public Material _Material;
    // private Mesh _Mesh;
    // private MeshFilter _MeshFilter;

    // [SerializeField]
    // private Camera _Camera;

    [SerializeField, Range(0, 2)]
    public float _Spike = 1.0f;

    // private RenderTexture _RenderTexture;

    private float[,] _TerrainHeights;
    private float[,] _InitialTerrainHeights;

    public TerrainGenerator _TerrainGenerator;

    private float[,] CopyHeights(float[,] h){
        int iMax = h.GetLength(0);
        int jMax = h.GetLength(1);
        float[,] newH = new float[iMax, jMax];

        for(int i=0; i<iMax; i++){
            for(int j=0; j<jMax; j++){
                newH[i,j] = h[i,j];
            }
        }

        return newH;
    }

    // private void GetTexture(){
    //     int res = (_TerrainGenerator.GetResolution() / 4) * 4;
    //     _RenderTexture = RenderTexture.GetTemporary(res, res);

    //     //transfer image from rendertexture to texture
    //     Texture2D texture = new Texture2D(res, res, TextureFormat.ARGB32, false);

    //     _Camera.targetTexture = _RenderTexture;
    //     _Camera.Render();
    //     RenderTexture.active = _RenderTexture;
    //     texture.ReadPixels(new Rect(Vector2.zero, new Vector2(res, res)), 0, 0);
    //     texture.Apply();  // Apply changes to the texture

    //     //save texture to file
    //     // byte[] png = texture.EncodeToPNG();
    //     // File.WriteAllBytes("Assets/test.png", png);
    //     // AssetDatabase.Refresh();
        

    //     //clean up variables
    //     RenderTexture.active = null;
    //     RenderTexture.ReleaseTemporary(_RenderTexture);
    //     DestroyImmediate(texture);
    // }

    private void UpdateTerrainHeights(){
        int len = _Heights.Length;
        // _TerrainHeights = CopyHeights(_InitialTerrainHeights);
        // List<Vector3> updatedIndices = new List<Vector3>();

        // get new position
        
        // float maxHeight = 0.0f;
        for(int i=0; i<len; i++){
            Vector2 indices = _Indices[i];
            int zIdx = (int)indices.x;
            int xIdx = (int)indices.y;

            _Tmp[zIdx, xIdx] += new Vector3(1, _Heights[i], 1); // _Tmp[] = (nbParticles, totalHeight, addedtolist)
            // if(_Tmp[zIdx, zIdx].x > maxHeight){
            //     maxHeight = _Tmp[zIdx, xIdx].x;
            // }
        }

        // normalize heights and update terrain
        for(int i=0; i<len; i++){
            Vector2 indices = _Indices[i];
            int zIdx = (int)indices.x;
            int xIdx = (int)indices.y;

            if(_Tmp[zIdx, xIdx].z == 0) continue;

            float v1 = (_Tmp[zIdx, xIdx].y / _Tmp[zIdx, xIdx].x);
            // Debug.Log(v1);
            float v2 = _InitialTerrainHeights[zIdx, xIdx];
            // _TerrainHeights[zIdx, xIdx] = (v1*_Spike + v2);

            _UpdatedIndices.Add(new Vector3(zIdx, xIdx, (v1*_Spike + v2)));
            _Tmp[zIdx, xIdx] = Vector3.zero;

        }
        
        // NormalizeTerrain(_InitialMaxHeight);
        // _TerrainGenerator.Updt(_TerrainHeights, updatedIndices);
        _TerrainGenerator.Updt(_UpdatedIndices);
        _UpdatedIndices.Clear();
    }


    /**
     * Init the lava heightmap
    */
    public void Init(){
        // init the mesh
        // _Positions = new Vector3[_ArraySize];
        // _Heights = new float[_ArraySize];

        // _Mesh = new Mesh();
        // _Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // _MeshFilter = gameObject.AddComponent<MeshFilter>();
        // _MeshFilter.mesh = _Mesh;
        // Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        // renderer.material = _Material;

        _TerrainHeights = CopyHeights(_TerrainGenerator._Heights);
        _InitialTerrainHeights = CopyHeights(_TerrainGenerator._Heights);
        _NbCols = _TerrainHeights.GetLength(1);
        _UpdatedIndices = new List<Vector3>();
        _Tmp = new Vector3[_TerrainHeights.GetLength(0),_TerrainHeights.GetLength(1)];
    }

    // private void UpdtMesh(){
    //     _Mesh.SetVertices(_Positions);

    //     int _ArraySize = _Positions.Length;

    //     // Generate indices for the points
    //     int[] indices = new int[_ArraySize];
    //     Vector2[] uvs = new Vector2[_ArraySize];

    //     for (int i = 0; i < _ArraySize; i++){
    //         indices[i] = i;
    //         uvs[i] = new Vector2(_Heights[i], 0.0f);
    //     }
        
    //     _Mesh.SetIndices(indices, MeshTopology.Points, 0);
    //     _Mesh.SetUVs(0, uvs);

    //     _Mesh.UploadMeshData(false);
    // }

    /**
     * Update the lava's heights
    */
    public void Updt(int nbParticles, float[] heights, Vector3[] positions, Vector2[] indices){
        _Positions = new Vector3[nbParticles];
        _Heights = new float[nbParticles];
        _Indices = new Vector2[nbParticles];

        for(int i=0; i<nbParticles; i++){
            _Positions[i] = positions[i];
            _Heights[i] = heights[i];
            _Indices[i] = indices[i];
            // if(i==0) 
                // Debug.Log(i + ": " + heights[i] + ", " + positions[i]);
        }
        // UpdtMesh();
        // GetTexture();
        UpdateTerrainHeights();
    }

}