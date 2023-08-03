using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;

/**
 * An enumeration representing the type of volcano to use for the simulation
*/
public enum Volcano {
    StHelen,
    Fuji,
    Flat,
    Slope,
}

/**
 * A class to create a heightmap of the terrain
*/
public class TerrainGenerator : MonoBehaviour{

// ################################################################################################################################################################################################################
// ################################################################################################## ATTRIBUTES ##################################################################################################
// ################################################################################################################################################################################################################

    /**
     * The class used to regroup all the serializied fields
    */
    [SerializeField]
    public Tweakable _Fields;

    /**
     * The type of terrain
    */
    private Volcano _VolcanoImage;

    /**
     * The terrain size
    */
    public Vector3 _Size;

    /**
     * The scale applied to the grayscale values of the terrain to make it look higher or lower
    */
    private float _Scale;

    /**
     * The terrain resolution
    */
    private int _Resolution;

    /**
     * The terrain heightmap
    */
    public float[,] _Heights;

    /**
     * The position of the particle generator
    */
    public Vector3 _ParticleGeneratorPos;

    /**
     * The camera position
    */
    private Vector3 _CameraPosition;

    /**
     * The camera rotation
    */
    private Vector3 _CameraRotation;

    /**
     * The camera
    */
    private Camera _Camera;

    /**
     * Boolean to tell if the terrain texture should be applied
    */
    public bool _UseTerrainTexture;


// ################################################################################################################################################################################################################
// ################################################################################################### METHODS ####################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Initiate the serialized fields
    */
    public void Awake(){
        _VolcanoImage = _Fields._VolcanoImage;
        _Camera = _Fields._Camera;
        _UseTerrainTexture = false;
    }

    /**
     * Create a texture from a png file
     * @param filePath The relative path to the grayscale image
     * @return A 2D texture containing the heights stored in the image
    */
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

    /**
     * Get the height at a given position in the heightmap
     * @param j The ordinate in the heightmap
     * @param i The abscissa in the heightmap
     * @return The height as a float
    */
    public float SampleHeight(int j, int i){
        return _Heights[j,i];
    }

    /**
     * Getter for the terrain resolution
     * @return The terrain resolution
    */
    public int GetResolution(){
        return _Resolution;
    }

    /**
     * Creates the texture of a terrain made of one big slope
     * @return The newly created texture
    */
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

    /**
     * Initiate the terrain for the Fuji volcano
     * @return The path to the Fuji grayscale image
    */
    private string InitFuji(){
        _ParticleGeneratorPos = new Vector3(259, 0, 270);
        _CameraPosition = new Vector3(288.45f, 81.8649f, 311.8634f);
        _CameraRotation = new Vector3(22.62f, -144.729f, 0);
        _Size = new Vector3(512, 0, 512);
        _Scale = 64.0f;        
        return "/Media/Fuji.png";
    }

    /**
     * Initiate the terrain for the St Helen volcano
     * @return The path to the St Helen grayscale image
    */
    private string InitStHelen(){
        _ParticleGeneratorPos = new Vector3(183.5f, 0, 215.6f);
        _CameraPosition = new Vector3(224.308f, 84.87954f, 226.2918f);
        _CameraRotation = new Vector3(44.59f, -143.212f, 0);
        _Size = new Vector3(306, 0, 306);
        _Scale = 64.0f; 
        _UseTerrainTexture = true;
        return "/Media/StHelen.png";
    }

    /**
     * Initiate the terrain for the flat terrain
     * @return The path to the black image
    */
    private string InitFlat(){
        _ParticleGeneratorPos = new Vector3(64.0f, 0, 64.0f);
        _CameraPosition = new Vector3(75.31268f, 16.81098f, 76.12329f);
        _CameraRotation = new Vector3(38.606f, -129.947f, 0);
        _Size = new Vector3(128, 0, 128);
        _Scale = 1.0f; 
        return "/Media/Flat.png";
    }

    /**
     * Initiate the terrain for the slope terrain
     * @return An empty path
    */
    private string InitSlope(){
        _ParticleGeneratorPos = new Vector3(64.0f, 0, 64.0f);
        _CameraPosition = new Vector3(64.48229f, 28.78457f, 81.80547f);
        _CameraRotation = new Vector3(31.902f, -175.497f, 0);
        _Size = new Vector3(128, 0, 128);
        _Scale = 32.0f; 
        return "";
    }

    /**
     * Update the camera
    */
    private void UpdateCamera(){
        _Camera.transform.position = _CameraPosition;
        _Camera.transform.rotation = Quaternion.Euler(_CameraRotation);
    }

    /**
     * Initiate the terrain
    */
    private void InitTerrain(){
        Texture2D heightmap = null;
        string path = Application.dataPath;

        // get the type of the terrain
        switch(_VolcanoImage){
            case Volcano.Fuji:
                path += InitFuji();
                break;
            case Volcano.Flat:
                path += InitFlat();
                break;
            case Volcano.StHelen:
                path += InitStHelen();
                break;
            case Volcano.Slope:
                path = InitSlope();
                heightmap = CreateSlope();
                break;
            default:
                break;
        }
        if(!string.Equals(path, "")){
            heightmap = LoadPNG(path);
        }

        // initiate terrain attributes
        _Resolution = heightmap.width;
        _Heights = new float[_Resolution, _Resolution];

        // initiate the heightmap
        for(int j=0; j<_Resolution; j++){
            for(int i=0; i<_Resolution; i++){
                float val =  heightmap.GetPixel(i, j).grayscale;
                _Heights[j,i] = val*_Scale;
            }
        }

        // move the camera
        UpdateCamera();
    }

    /**
     * Initiate the terrain generator
    */
    public void Init(){
        InitTerrain();
    }

}