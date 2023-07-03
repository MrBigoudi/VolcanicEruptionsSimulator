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

    private Vector3[] _Vertices;
    private int[] _Indices;

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
        // if(i >= _Resolution || j >= _Resolution || i<0 || j<0)
        //     Debug.Log(i + ", " + j + ": " + deltaCols + ", " + deltaLines + ", " + _Resolution + _Size);
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

        Vector3 max = Vector3.zero;
        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                float val =  heightmap.GetPixel(i*2, j*2).grayscale;
                if(val > max.y) max = new Vector3(j, val, i);
                _Heights[j,i] = val*_Scale;
            }
        }
        // Debug.Log(max);

        _Vertices = new Vector3[_Resolution*_Resolution];
        int nbIndices = (_Resolution-1)*(_Resolution-1)*12;
        _Indices = new int[nbIndices];
    }

    private void InitMesh(){
        _Mesh = new Mesh();
        _Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _MeshFilter = gameObject.AddComponent<MeshFilter>();
        _MeshFilter.mesh = _Mesh;
        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _Material;

        SetVertices();
        SetIndices();
        _Mesh.UploadMeshData(false);
    }

    public void Init(){
        InitTerrain();
        InitMesh();
    }

    private void SetVertices(){
        // init vertices
        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                float x = i * _Size.x / _Resolution;
                float z = j * _Size.z / _Resolution;
                float y =  _Heights[j,i];
                int idx = i + j*_Resolution;
                _Vertices[idx].x = x;
                _Vertices[idx].y = y;
                _Vertices[idx].z = z;
            }
        }
        // Debug.Log(vertices.Length);
        _Mesh.SetVertices(_Vertices);
    }

    private void SetIndices(){
        // init indices
        int idx = 0;
        for(int j=0; j<_Resolution-1; j++){
            for(int i=0; i<_Resolution-1; i++){
                int id1 = j * _Resolution + i;
                int id2 = id1 + 1;
                int id3 = id1 + _Resolution;
                int id4 = id3 + 1;
                // first side
                // first triangle
                _Indices[idx++] = id1;
                _Indices[idx++] = id2;
                _Indices[idx++] = id3;
                // second triangle
                _Indices[idx++] = id3;
                _Indices[idx++] = id2;
                _Indices[idx++] = id4;

                // second side
                // first triangle
                _Indices[idx++] = id1;
                _Indices[idx++] = id3;
                _Indices[idx++] = id2;
                // second triangle
                _Indices[idx++] = id3;
                _Indices[idx++] = id4;
                _Indices[idx++] = id2;
            }
        }
        // Debug.Log(vertices.Length + ", " + nbIndices);
        _Mesh.SetIndices(_Indices, MeshTopology.Triangles, 0);
    }

    private void UpdtMesh(){
        SetVertices();
        // SetIndices();
        // SetNormals();
        _Mesh.UploadMeshData(false);
    }


    public void Updt(float[,] newHeights){
        _Heights = newHeights;
        UpdtMesh();
    }

}