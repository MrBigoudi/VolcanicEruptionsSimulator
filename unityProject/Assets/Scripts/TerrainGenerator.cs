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
                path += "/Media/new_volcano-height-map.png";
                break;
            case Volcano.Fuji:
                path += "/Media/testFuji.png";
                break;
            default:
                break;
        }
        heightmap = LoadPNG(path);
        // heightmap = GaussianBlur(heightmap);


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
    }

    

    public void Init(){
        InitTerrain();
    }

}