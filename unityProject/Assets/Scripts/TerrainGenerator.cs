using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;

public enum Volcano {
    Basic
}

public class TerrainGenerator : MonoBehaviour{

    [SerializeField]
    public Volcano _VolcanoImage = Volcano.Basic;

    [SerializeField]
    public Vector3 _Size = Vector3.zero;

    [SerializeField, Range(32.0f, 1024.0f)]
    public float _Scale = 32.0f;

    private int _Resolution;
    public float[,] _Heights;
    public float[,] _Normals;

    [SerializeField]
    public Material _Material;
    private Mesh _Mesh;
    private MeshFilter _MeshFilter;

    private Texture2D LoadPNG(string filePath){
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath)){
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }

    public float SampleHeight(Vector3 pos){
        float deltaCols  = _Size.x / (_Resolution - 1.0f);
        float deltaLines = _Size.z / (_Resolution - 1.0f);
        int i = (int)(pos.x / deltaCols);
        int j = (int)(pos.z / deltaLines);
        if(i >= _Resolution || j >= _Resolution || i<0 || j<0)
            Debug.Log(i + ", " + j + ": " + deltaCols + ", " + deltaLines + ", " + _Resolution + _Size);
        return _Heights[j,i];
    }

    public int GetResolution(){
        return _Resolution;
    }

    private void InitTerrain(){
        Texture2D heightmap = null;
        string path = Application.dataPath;

        switch(_VolcanoImage){
            case Volcano.Basic:
                path += "/Media/volcano-height-map.png";
                heightmap = LoadPNG(path);
                break;
            default:
                break;
        }

        _Size = new Vector3(512.0f, 0.0f, 512.0f);
        _Resolution = heightmap.width / 2;
        _Heights = new float[_Resolution, _Resolution];

        // Debug.Log(_Resolution + ", " + _Size);

        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                _Heights[j,i] = heightmap.GetPixel(i*2, j*2).grayscale;
            }
        }
    }

    private void InitMesh(){
        _Mesh = new Mesh();
        _Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _MeshFilter = gameObject.AddComponent<MeshFilter>();
        _MeshFilter.mesh = _Mesh;
        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _Material;
    }

    public void Init(){
        InitTerrain();
        InitMesh();
    }

    private void SetVertices(){
        // init vertices
        Vector3[] vertices = new Vector3[_Resolution*_Resolution];
        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                float x = i * _Size.x / _Resolution;
                float z = j * _Size.z / _Resolution;
                float y =  _Heights[j,i] * _Scale;
                vertices[i + j*_Resolution] = new Vector3(x, y, z);
            }
        }
        // Debug.Log(vertices.Length);
        _Mesh.SetVertices(vertices);
    }

    private void SetIndices(){
        // init indices
        int nbIndices = (_Resolution-1)*(_Resolution-1)*12;
        int[] indices = new int[nbIndices];
        int idx = 0;
        for(int j=0; j<_Resolution-1; j++){
            for(int i=0; i<_Resolution-1; i++){
                // first side
                // first triangle
                indices[idx++]   = (j * _Resolution + i);
                indices[idx++] = (j * _Resolution + (i+1));
                indices[idx++] = ((j+1) * _Resolution + i);
                // second triangle
                indices[idx++] = ((j+1) * _Resolution + i);
                indices[idx++] = (j * _Resolution + (i+1));
                indices[idx++] = ((j+1) * _Resolution + (i+1));

                // second side
                // first triangle
                indices[idx++]   = (j * _Resolution + i);
                indices[idx++] = ((j+1) * _Resolution + i);
                indices[idx++] = (j * _Resolution + (i+1));
                // second triangle
                indices[idx++] = ((j+1) * _Resolution + i);
                indices[idx++] = ((j+1) * _Resolution + (i+1));
                indices[idx++] = (j * _Resolution + (i+1));
            }
        }
        // Debug.Log(vertices.Length + ", " + nbIndices);
        _Mesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }

    private void UpdtMesh(){
        
        SetVertices();
        SetIndices();
        _Mesh.UploadMeshData(false);
    }


    public void Updt(float[,] newHeights){
        // _Heights = newHeights;
        UpdtMesh();
    }

}