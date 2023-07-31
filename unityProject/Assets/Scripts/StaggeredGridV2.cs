using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A static class representing the staggered grid
*/
public static class StaggeredGridV2 {

// ################################################################################################################################################################################################################
// ################################################################################################## ATTRIBUTES ##################################################################################################
// ################################################################################################################################################################################################################

    /**
     * The terrain's heights
    */
    public static float[,] _Heights;

    /**
     * The terrain's half heights between columns
    */
    public static float[,] _HalfHeightsCols;

    /**
     * The terrain's half heights between lines
    */
    public static float[,] _HalfHeightsLines;

    /**
     * The terrain's half heights at each corner
    */
    public static float[,] _HalfHeights;

    /**
     * The terrain's gradients at each corner
    */
    public static Vector2[,] _Gradients;

    /**
     * The number of columns in the grid
    */
    public static int _NbCols;
    
    /**
     * The number of lines in the grid
    */
    public static int _NbLines;

    /**
     * The distance between two columns
    */
    public static float _DeltaCols;
    
    /**
     * The distance between two lines
    */
    public static float _DeltaLines;


// ################################################################################################################################################################################################################
// ################################################################################################### METHODS ####################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Initiate all the dimensions in the grid
     * @param terrain The terrain heightmap
    */
    private static void InitDimensions(TerrainGenerator terrain){
        int heightmapResolution = terrain.GetResolution();
        Vector3 terrainSize = terrain._Size;

        // get array dimensions
        _NbCols  = heightmapResolution;
        _NbLines = heightmapResolution;
        _DeltaCols  = terrainSize.x / _NbCols;
        _DeltaLines = terrainSize.z / _NbLines;

        // init arrays
        _Heights = new float[_NbLines, _NbCols];
        _HalfHeightsCols = new float[_NbLines, _NbCols-1];
        _HalfHeightsLines = new float[_NbLines-1, _NbCols];
        _HalfHeights = new float[_NbLines-1, _NbCols-1];
        _Gradients = new Vector2[_NbLines-1, _NbCols-1];
    }

    /**
     * Init the half heights values
    */
    private static void InitHalfHeights(){
        // init half heights
        for(int j=0; j<_NbLines-1; j++){
            for(int i=0; i<_NbCols-1; i++){
                _HalfHeights[j,i] = (_Heights[j,i] + _Heights[j+1,i] + _Heights[j,i+1] + _Heights[j+1, i+1]) * (1.0f/4.0f);
            }
        }

        // init half heights inside lines and cols
        for(int j=0; j<_NbLines; j++){
            for(int i=0; i<_NbCols; i++){
                float up, down, left, right;
                // update half cols
                if (i<_NbCols-1){
                    left = _Heights[j,i];
                    right = _Heights[j,i+1];
                    if(j>=_NbLines-1){
                        up = 0.0f;
                    } else {
                        up = _HalfHeights[j,i];
                    }
                    if(j==0){
                        down = 0.0f;
                    } else {
                        down = _HalfHeights[j-1,i];
                    }
                    _HalfHeightsCols[j,i] = (up+down+left+right)*(1.0f/4.0f);
                }
                // update half lines
                if (j<_NbLines-1){
                    down = _Heights[j,i];
                    up = _Heights[j+1,i];
                    if(i>=_NbCols-1){
                        right = 0.0f;
                    } else {
                        right = _HalfHeights[j,i];
                    }
                    if(i==0){
                        left = 0.0f;
                    } else {
                        left = _HalfHeights[j,i-1];
                    }
                    _HalfHeightsLines[j,i] = (up+down+left+right)*(1.0f/4.0f);
                }
            }
        }
    }

    /**
     * Init the heights
     * @param The terrain heightmap
    */
    private static void InitHeights(TerrainGenerator terrain){
        // init usual heights
        for(int j=0; j<_NbLines; j++){
            for(int i=0; i<_NbCols; i++){
                _Heights[j,i] = terrain.SampleHeight(j, i);
            }
        }
        InitHalfHeights();
    }

    /**
     * Init the gradients
    */
    private static void InitGradients(){
        for(int j=0; j<_NbLines-1; j++){
            for(int i=0; i<_NbCols-1; i++){
                float dx = (_HalfHeightsLines[j,i+1]-_HalfHeightsLines[j,i]) / _DeltaCols;
                float dy = (_HalfHeightsCols[j+1,i]-_HalfHeightsCols[j,i])   / _DeltaLines;
                _Gradients[j,i] = new Vector2(dx, dy);
            }
        }
    }

    /**
     * Initiate the entire grid
     * @param The terrain heightmap
    */
    public static void Init(TerrainGenerator terrain){
        InitDimensions(terrain);
        InitHeights(terrain);
        InitGradients();
    }
}