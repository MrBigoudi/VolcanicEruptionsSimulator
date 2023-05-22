using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * A static class representing the staggered grid for computing height and gradients
*/
public static class StaggeredGrid {
    /**
     * The terrain heightmap x positions
    */
    private static float[] sHeightmapX;

    /**
     * The terrain heightmap z positions
    */
    private static float[] sHeightmapZ;

    /**
     * The terrain heightmap
    */
    private static float[,] sHeightmap;

    /**
     * The x gradient
    */
    private static float[,] sGradX;

    /**
     * The y gradient
    */
    private static float[,] sGradZ;

    /**
     * The number of rows
    */
    private static int sNbZ;

    /**
     * The number of columns
    */
    private static int sNbX;

    /**
     * The difference between rows
    */
    private static float sDz;

    /**
     * The difference between cols
    */
    private static float sDx;

    /**
     * Init dimensions
    */
    private static void InitDimensions(){
        // get the heightmap from unity terrain
        TerrainData terrainData = Terrain.activeTerrain.terrainData;
        int heightmapResolution = terrainData.heightmapResolution;

        // get array dimensions
        sNbX = heightmapResolution;
        sNbZ = heightmapResolution;
        sDx = (terrainData.size.x) / (heightmapResolution-1);
        sDz = (terrainData.size.z) / (heightmapResolution-1);

        sHeightmap = new float[sNbZ, sNbX];
        sHeightmapX = new float[sNbX];
        sHeightmapZ = new float[sNbZ];

        sGradZ = new float[sNbZ-1, sNbX];
        sGradX = new float[sNbZ, sNbX-1];
    }

    /**
     * Init the terrain heightmap
    */
    private static void InitHeightMap(){
        for(int j=0; j<sNbZ; j++){
            for(int i=0; i<sNbX; i++){
                float curX = sDx*i;
                float curZ = sDz*j;
                // unity sample of the height
                sHeightmap[j,i] = Terrain.activeTerrain.SampleHeight(new Vector3(curX, .0f, curZ));
                // init the positions for interpolations later
                sHeightmapX[i] = curX;
                sHeightmapZ[j] = curZ;
            }
        }
    }

    /**
     * Init the grid
    */
    public static void Init(){
        // init the dimensions
        InitDimensions();

        // init terrain
        InitHeightMap();

        // init x gradients
        for(int j=0; j<sNbZ; j++){
            for(int i=0; i<sNbZ-1; i++){
                sGradX[j,i] = (sHeightmap[j,i+1] - sHeightmap[j,i]) / sDx;
            }
        }

        // init y gradients
        for(int j=0; j<sNbZ-1; j++){
            for(int i=0; i<sNbX; i++){
                sGradZ[j,i] = (sHeightmap[j+1,i] - sHeightmap[j,i]) / sDz;
            }
        }
    }

    /**
     * Get the indices of the current position inside the arrays
     * @param pos The current position of the object in world space coordinates
     * @return An array [i,j] with the indices in the arrays
    */
    private static int[] getIndices(Vector3 pos){
        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;

        // convert world-space position to terrain-local coordinates
        Vector3 terrainPos = pos - terrain.transform.position;

        // convert terrain-local coordinates to heightmap index coordinates
        int resolution = terrainData.heightmapResolution;
        int mapX = (int)(terrainPos.x / terrainData.size.x * (resolution - 1));
        int mapZ = (int)(terrainPos.z / terrainData.size.z * (resolution - 1));

        int[] array = {mapZ, mapX};

        return array;
    }

    /**
     * Get the height of a point given it's position
     * @param pos The position of the point
     * @return The height of the point in the heightmap
    */
    public static float GetHeight(Vector3 pos){
        int[] indices = getIndices(pos);
        int zIdx = indices[0];
        int xIdx = indices[1];
        // Debug.Log("xidx: "+xIdx+", zIdx: "+zIdx+ ", pos: " + pos);

        float x = pos.x;
        float z = pos.z;

        float xLeft  = sHeightmapX[xIdx];
        float xRight = sHeightmapX[xIdx+1];
        float zUp    = sHeightmapZ[zIdx+1];
        float zDown  = sHeightmapZ[zIdx];

        // interpolate height, bilinear
        float upLeftY = sHeightmap[zIdx+1, xIdx];
        float upRightY = sHeightmap[zIdx+1, xIdx+1];
        float downLeftY = sHeightmap[zIdx, xIdx];
        float downRightY = sHeightmap[zIdx, xIdx+1];

        float ownRes = (downLeftY*(xRight-x)*(zUp-z))+(downRightY*(x-xLeft)*(zUp-z)
                        +(upLeftY*(xRight-x)*(z-zDown))+(upRightY*(x-xLeft)*(z-zDown)));
        ownRes /= sDx*sDz;


        // float unityRes = Terrain.activeTerrain.SampleHeight(pos);
        // Debug.Log("unity: " + unityRes + ", own: " + ownRes);
        // Assert.IsTrue(ownRes == unityRes);

        // return unityRes;
        return ownRes;
    }

    /**
     * Get the gradient of a point given it's position
     * @param p The particle for which we're searching the gradient
     * @return The gradient of the point as a vector3
    */
    public static Vector3 GetGradient(Particle p){
        Vector3 pos = p.transform.position;
        int[] indices = getIndices(pos);

        float newX = sGradX[indices[0], indices[1]] - p.mHeight;
        float newZ = sGradZ[indices[0], indices[1]] - p.mHeight;

        return new Vector3(newX, pos.y, newZ);
    }
}