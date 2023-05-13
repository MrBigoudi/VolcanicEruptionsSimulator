using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * A static class representing the staggered grid for computing height and gradients
*/
public static class StaggeredGrid {
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
     * Init the terrain heightmap
     * @param nbX The number of cols
     * @param nbY The number of rows
     * @param dx The x delta
     * @param dz The z delta
    */
    private static void InitHeightMap(int nbX, int nbZ, float dx, float dz){
        for(int i=0; i<nbX; i++){
            for(int j=0; j<nbZ; j++){
                float curX = dx*i;
                float curZ = dz*j;
                // unity sample of the height
                sHeightmap[i,j] = Terrain.activeTerrain.SampleHeight(new Vector3(curX, .0f, curZ));
            }
        }
    }

    /**
     * Init the grid
    */
    public static void Init(){
        // get the heightmap from unity terrain
        TerrainData terrainData = Terrain.activeTerrain.terrainData;
        int heightmapResolution = terrainData.heightmapResolution;

        // get array dimensions
        int nbX = heightmapResolution;
        int nbZ = heightmapResolution;
        float dx = (terrainData.size.x) / (heightmapResolution-1);
        float dz = (terrainData.size.z) / (heightmapResolution-1);

        sHeightmap = new float[nbX, nbZ];
        sGradX = new float[nbX-1, nbZ-1];
        sGradZ = new float[nbX-1, nbZ-1];

        // init terrain
        InitHeightMap(nbX, nbZ, dx, dz);

        // init x gradients
        for(int i=0; i<nbX-1; i++){
            for(int j=0; j<nbZ-1; j++){
                sGradX[i,j] = (sHeightmap[i+1,j] - sHeightmap[i,j]) / dx;
            }
        }

        // init y gradients
        for(int i=0; i<nbX-1; i++){
            for(int j=0; j<nbZ-1; j++){
                sGradX[i,j] = (sHeightmap[i,j+1] - sHeightmap[i,j]) / dz;
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

        int[] array = {mapX, mapZ};

        return array;
    }

    /**
     * Get the height of a point given it's position
     * @param pos The position of the point
     * @return The height of the point in the heightmap
    */
    public static float GetHeight(Vector3 pos){
        int[] indices = getIndices(pos);
        // float ownRes = sHeightmap[indices[0], indices[1]];
        // float unityRes = Terrain.activeTerrain.SampleHeight(pos);
        // Debug.Log("unity: " + unityRes + ", own: " + ownRes);
        // Assert.IsTrue(ownRes == unityRes);

        return Terrain.activeTerrain.SampleHeight(pos);
    }

    /**
     * Get the gradient of a point given it's position
     * @param p The particle for which we're searching the gradient
     * @return The gradient of the point as a vector3
    */
    public static Vector3 GetGradient(Particle p){
        Vector3 pos = p.transform.position;
        int[] indices = getIndices(pos);

        float newX = sGradX[indices[0], indices[1]];
        float newZ = sGradZ[indices[0], indices[1]];

        return new Vector3(newX, pos.y, newZ);
    }
}