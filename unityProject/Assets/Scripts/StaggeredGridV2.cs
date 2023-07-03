using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public static class StaggeredGridV2 {

    public static float[,] _Heights;
    public static float[,] _HalfHeightsCols;
    public static float[,] _HalfHeightsLines;

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
        _DeltaCols  = (terrainSize.x) / (_NbCols-1);
        _DeltaLines = (terrainSize.z) / (_NbLines-1);
        // Debug.Log(_NbCols + ", " + _NbLines + ", " + _DeltaCols + ", " + _DeltaLines);

        // init arrays
        _Heights = new float[_NbLines, _NbCols];
        _HalfHeightsCols = new float[_NbLines, _NbCols-1];
        _HalfHeightsLines = new float[_NbLines-1, _NbCols];
        _Gradients = new Vector2[_NbLines-1, _NbCols-1];
        _Laplacians = new float[_NbLines-2, _NbCols-2];
    }

    private static void InitHeights(TerrainGenerator terrain){
        for(int j=0; j<_NbLines; j++){
            for(int i=0; i<_NbCols; i++){
                float x = _DeltaCols*i;
                float z = _DeltaLines*j;
                float xHalf = _DeltaCols*(i+0.5f);
                float zHalf = _DeltaLines*(j+0.5f);

                _Heights[j,i] = terrain.SampleHeight(new Vector3(x, 0.0f, z));
                // Debug.Log("heights[" + j + "," + i + "] = " + _Heights[j,i]);
                if (i<_NbCols-1){
                    _HalfHeightsCols[j,i] = (_Heights[j,i]+_Heights[j,i+1])/2;
                }
                if (j<_NbLines-1){
                    _HalfHeightsLines[j,i] = (_Heights[j,i]+_Heights[j+1,i])/2;
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

    public static int[] GetIndices(Vector3 pos){
        int posX = (int)(pos.x / _DeltaCols);
        int posZ = (int)(pos.z / _DeltaLines);
        int[] array = {posZ, posX};
        return array;
    }

    private static float BilinearInterpolation(float x, float z, float xIdx, float zIdx, float upLeft, float upRight, float downLeft, float downRight){
        float xLeft  = xIdx*_DeltaCols;
        float xRight = (xIdx+1)*_DeltaCols;
        float zUp    = (zIdx+1)*_DeltaLines;
        float zDown  = zIdx*_DeltaLines;

        float res = (downLeft*(xRight-x)*(zUp-z))+(downRight*(x-xLeft)*(zUp-z)
                        +(upLeft*(xRight-x)*(z-zDown))+(upRight*(x-xLeft)*(z-zDown)));
        return res /= _DeltaCols*_DeltaLines;
    }

    public static float GetHeight(Vector3 pos){
        int[] indices = GetIndices(pos);
        int zIdx = indices[0];
        int xIdx = indices[1];

        float x = pos.x;
        float z = pos.z;

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