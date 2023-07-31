using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;

/**
 * An enumeration representing the type of volcano to use for the simulation
 TODO: change camera's position and particle generator's positions as well depending on the volcano
*/
public enum Volcano {
    Fuji,
    Flat,
    Slope,
    StHelen,
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


// ################################################################################################################################################################################################################
// ################################################################################################### METHODS ####################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Initiate the serialized fields
    */
    public void Awake(){
        _VolcanoImage = _Fields._VolcanoImage;
        _Size = _Fields._Size;
        _Scale = _Fields._Scale;
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
     * Initiate the terrain
    */
    private void InitTerrain(){
        Texture2D heightmap = null;
        string path = Application.dataPath;
        bool pathValid = true;

        // get the type of the terrain
        switch(_VolcanoImage){
            case Volcano.Fuji:
                path += "/Media/Fuji.png";
                break;
            case Volcano.Flat:
                path += "/Media/Flat.png";
                break;
            case Volcano.StHelen:
                path += "/Media/StHelen.png";
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
    }

    /**
     * Initiate the terrain generator
    */
    public void Init(){
        InitTerrain();
    }

}