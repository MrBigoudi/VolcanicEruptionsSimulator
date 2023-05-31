using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class representing the grid storing lava heightmap
*/
public class LavaTextureMap : MonoBehaviour{
    /**
     * The lava heightmap
    */
    public static float[,] sHeightmap;

    /**
     * The heightmap dimensions
    */
    public static int sNbX, sNbZ;

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

        sHeightmap = new float[sNbZ, sNbX];
    }

    /**
     * Init the lava heightmap
    */
    public static void Init(){
        InitDimensions();

        // init heights to 0
        for(int j=0; j<sNbZ; j++){
            for(int i=0; i<sNbX; i++){
                sHeightmap[j,i] = 0.0f;
            }
        }
    }

    /**
     * Get the indices of the current position inside the arrays
     * @param pos The current position of the object in world space coordinates
     * @return An array [j,i] with the indices in the arrays
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
     * Update the lava's height given a particle
     * @param p The particle representing
    */
}