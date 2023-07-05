using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;

public enum Volcano {
    Basic,
    Fuji
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
    public Vector2[,] _Gradients;

    private Vector3[] _Vertices;
    private Vector3[] _InitVertices;
    private Vector3[] _Normals;
    private int[] _Indices;
    private Vector2[] _UVs;

    private float _MaxGradX;
    private float _MaxGradY;
    private float _MinGradX;
    private float _MinGradY;

    [SerializeField]
    public Material _Material;
    private Mesh _Mesh;
    private MeshFilter _MeshFilter;

    private Texture2D LoadPNG(string filePath){
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath)){
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2, TextureFormat.R16, false);
            tex.LoadImage(fileData);
        }
        return tex;
    }

    public float SampleHeight(Vector3 pos){
        float deltaCols  = _Size.x / _Resolution;
        float deltaLines = _Size.z / _Resolution;
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
                break;
            case Volcano.Fuji:
                path += "/Media/testFuji.png";
                break;
            default:
                break;
        }
        heightmap = LoadPNG(path);


        _Size = new Vector3(512.0f, 0.0f, 512.0f);
        _Resolution = heightmap.width;
        _Heights = new float[_Resolution, _Resolution];

        // Debug.Log(_Resolution + ", " + _Size);

        Vector3 max = Vector3.zero;
        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                float val =  heightmap.GetPixel(i, j).grayscale;
                if(val > max.y) max = new Vector3(j, val, i);
                _Heights[j,i] = val*_Scale;
            }
        }
        // Debug.Log(max);

        _Vertices = new Vector3[_Resolution*_Resolution];
        _InitVertices = new Vector3[_Resolution*_Resolution];
        _Gradients = new Vector2[_Resolution, _Resolution];
        _Normals = new Vector3[_Resolution*_Resolution];
        _UVs = new Vector2[_Resolution*_Resolution];
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

        SetAllVertices();
        SetIndices();
        SetUVs();
        _Mesh.UploadMeshData(false);
    }

    public void Init(){
        InitTerrain();
        InitMesh();
    }

    private void SetAllVertices(){
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
                _InitVertices[idx].x = x;
                _InitVertices[idx].y = y;
                _InitVertices[idx].z = z;
            }
        }
        // Debug.Log(vertices.Length);
        _Mesh.SetVertices(_Vertices);
    }

    public void GetGradients(Vector2[,] grad){
        int iMax = grad.GetLength(1);
        int jMax = grad.GetLength(0);

        for(int j=0; j<jMax; j++){
            for(int i=0; i<iMax; i++){
                _Gradients[j,i] = grad[j,i];
            }
        }
    }

    public void SetNormals(){
        // init normals
        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                int idx = i + j*_Resolution;
                if(i==_Resolution-1 || j==_Resolution-1){
                    _Normals[idx] = Vector3.zero;
                    continue;
                }
                Vector2 grad = _Gradients[j,i];
                // _Normals[idx] = new Vector3(grad.x, _Heights[j,i]-_Heights[j+1,i], grad.y);
                _Normals[idx] = new Vector3(grad.x, 0, grad.y);
                // _Normals[idx] = new Vector3(0, 0, 1-(grad.y-_MinGradY) / (_MaxGradY-_MinGradY));
                // _Normals[idx] = new Vector3(1-(grad.x-_MinGradX) / (_MaxGradX-_MinGradX), 0, 0);
            }
        }
        // Debug.Log(vertices.Length);
        _Mesh.SetNormals(_Normals);
    }

    private void SetVertices(List<Vector3> updatedIndices){
        int len = updatedIndices.Count;

        // update needed vertices
        for(int k=0; k<len; k++){
            int i = (int)updatedIndices[k].y;
            int j = (int)updatedIndices[k].x;
            float x = i * _Size.x / _Resolution;
            float z = j * _Size.z / _Resolution;
            float y =  updatedIndices[k].z;
            int idx = i + j*_Resolution;
            _Vertices[idx].x = x;
            _Vertices[idx].y = y;
            _Vertices[idx].z = z;
        }
        _Mesh.SetVertices(_Vertices);

        // reset vertices
        for(int k=0; k<len; k++){
            int i = (int)updatedIndices[k].x;
            int j = (int)updatedIndices[k].y;
            int idx = i + j*_Resolution;
            _Vertices[idx] = _InitVertices[idx];
        }
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

    private void SetUVs(){
        System.Random rand = new System.Random();
        // init vertices
        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                float y =  _Heights[j,i];
                int idx = i + j*_Resolution;
                _UVs[idx].x = (float)rand.NextDouble();
                _UVs[idx].y = y;
            }
        }
        // Debug.Log(vertices.Length);
        _Mesh.SetUVs(0, _UVs);
    }

    private void UpdtMesh(List<Vector3> updatedIndices){
        // _Heights = newHeights;
        // SetAllVertices();
        SetVertices(updatedIndices);
        // SetIndices();
        // SetNormals();
        _Mesh.UploadMeshData(false);
    }


    public void Updt(List<Vector3> updatedIndices){
        UpdtMesh(updatedIndices);
    }

}