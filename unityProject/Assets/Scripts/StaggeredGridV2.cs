using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public static class StaggeredGridV2 {

    public static float[,] _Heights;
    public static float[,] _HalfHeightsCols;
    public static float[,] _HalfHeightsLines;
    public static float[,] _HalfHeights;

    public static Vector2[,] _Gradients;
    public static float[,] _Laplacians;

    public static int _NbCols, _NbLines;
    public static float _DeltaCols, _DeltaLines;

    private static void InitDimensions(TerrainGenerator terrain){
        // get the heightmap from unity terrain
        int heightmapResolution = terrain.GetResolution();
        Vector3 terrainSize = terrain._Size;

        // get array dimensions
        _NbCols  = heightmapResolution;
        _NbLines = heightmapResolution;
        _DeltaCols  = terrainSize.x / _NbCols;
        _DeltaLines = terrainSize.z / _NbLines;
        // Debug.Log(_NbCols + ", " + _NbLines + ", " + _DeltaCols + ", " + _DeltaLines);

        // init arrays
        _Heights = new float[_NbLines, _NbCols];
        _HalfHeightsCols = new float[_NbLines, _NbCols-1];
        _HalfHeightsLines = new float[_NbLines-1, _NbCols];
        _HalfHeights = new float[_NbLines-1, _NbCols-1];
        _Gradients = new Vector2[_NbLines-1, _NbCols-1];
        _Laplacians = new float[_NbLines-2, _NbCols-2];
    }

    private static void InitHeights(TerrainGenerator terrain){
        // init usual heights
        for(int j=0; j<_NbLines; j++){
            for(int i=0; i<_NbCols; i++){
                float x = _DeltaCols*i;
                float z = _DeltaLines*j;

                _Heights[j,i] = terrain.SampleHeight(new Vector3(x, 0.0f, z));
            }
        }

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

    private static void InitGradients(){
        for(int j=0; j<_NbLines-1; j++){
            for(int i=0; i<_NbCols-1; i++){
                float dx = (_HalfHeightsLines[j,i+1]-_HalfHeightsLines[j,i]) / _DeltaCols;
                float dy = (_HalfHeightsCols[j+1,i]-_HalfHeightsCols[j,i])   / _DeltaLines;
                _Gradients[j,i] = new Vector2(dx, dy);
                // Debug.Log(_Gradients[j,i]);
            }
        }
    }

    private static void InitLaplacians(){
        for(int j=0; j<_NbLines-2; j++){
            for(int i=0; i<_NbCols-2; i++){
                float x = (1/(4*_DeltaCols*_DeltaCols))*
                                (  _HalfHeightsLines[j+1,i+2] + _HalfHeightsLines[j+1,i+1] - 2*_HalfHeightsLines[j+1,i]
                                 + _HalfHeightsLines[j,  i+2] + _HalfHeightsLines[j,  i+1] - 2*_HalfHeightsLines[j,  i]
                                );
                float y = (1/(4*_DeltaLines*_DeltaLines))*
                                (  _HalfHeightsCols[j+2,i+1] + _HalfHeightsCols[j+1,i+1] - 2*_HalfHeightsCols[j,i+1]
                                 + _HalfHeightsCols[j+2,  i] + _HalfHeightsCols[j+1,  i] - 2*_HalfHeightsCols[j,  i]
                                );
                _Laplacians[j,i] = x + y;
            }
        }
    }

    public static void Init(TerrainGenerator terrain){
        InitDimensions(terrain);
        InitHeights(terrain);
        InitGradients();
        InitLaplacians();
    }

    public static Vector2 GetIndices(Vector3 pos){
        float posX = (pos.x / _DeltaCols);
        float posZ = (pos.z / _DeltaLines);
        return new Vector2(posZ, posX);
    }

    private static float BilinearInterpolation(float x, float z, float xIdx, float zIdx, float w21, float w22, float w11, float w12){
        float xLeft  = xIdx*_DeltaCols;
        float xRight = (xIdx+1)*_DeltaCols;
        float zUp    = (zIdx+1)*_DeltaLines;
        float zDown  = zIdx*_DeltaLines;

        float x2_prime = xRight - x;
        float x1_prime = x - xLeft;
        float z2_prime = zUp - z;
        float z1_prime = z - zDown;

        float factor = 1.0f/(_DeltaCols*_DeltaLines);


        float res = ((w11*x2_prime + w21*x1_prime)*z2_prime) + ((w12*x2_prime + w22*x1_prime)*z1_prime);

        return res *= factor;
    }

    public static float GetHeight(Vector3 pos){
        Vector2 indices = GetIndices(pos);
        int zIdx = (int)indices.x;
        int xIdx = (int)indices.y;

        float x = pos.x;
        float z = pos.z;

        if(zIdx >= _NbLines-1 || xIdx >= _NbCols-1){
            return _Heights[zIdx, xIdx];
        }

        // interpolate height, bilinearly
        float upLeft    = _Heights[zIdx+1, xIdx+0];
        float upRight   = _Heights[zIdx+1, xIdx+1];
        float downLeft  = _Heights[zIdx+0, xIdx+0];
        float downRight = _Heights[zIdx+0, xIdx+1];

        float ownRes = BilinearInterpolation(x, z, xIdx, zIdx, upLeft, upRight, downLeft, downRight);

        // float unityRes = Terrain.activeTerrain.SampleHeight(pos);
        // Debug.Log("unity: " + unityRes + ", own: " + ownRes);
        // Assert.IsTrue(ownRes == unityRes);

        // return unityRes;
        return ownRes;
    }
}