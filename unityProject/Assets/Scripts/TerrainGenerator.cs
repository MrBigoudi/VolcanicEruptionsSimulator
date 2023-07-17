using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;

public enum Volcano {
    Basic,
    Fuji,
    Flat,
    Slope,
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

    public float SampleHeight(int j, int i){
        return _Heights[j,i];
    }

    public int GetResolution(){
        return _Resolution;
    }

    public Texture2D CreateSlope(){
        Texture2D texture = new Texture2D(512, 512);
        for (int x = 0; x < texture.width; x++){
            float v = x/512.0f;
            Color color = new Color(v,v,v,1);
            for (int y = 0; y < texture.height; y++){
                texture.SetPixel(x, y, color);
            }
        }
        return texture;
    }

    private void InitTerrain(){
        Texture2D heightmap = null;
        string path = Application.dataPath;
        bool pathValid = true;

        switch(_VolcanoImage){
            case Volcano.Basic:
                path += "/Media/new_volcano-height-map.png";
                break;
            case Volcano.Fuji:
                path += "/Media/testFuji.png";
                break;
            case Volcano.Flat:
                path += "/Media/Flat.png";
                break;
            case Volcano.Slope:
                pathValid = false;
                heightmap = CreateSlope();
                break;
            default:
                break;
        }
        if(pathValid){
            heightmap = LoadPNG(path);
            // heightmap = GaussianBlur(heightmap);
        }


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