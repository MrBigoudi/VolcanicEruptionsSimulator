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
        // get array dimensions
        sNbX = Grid.mNbCols;
        sNbZ = Grid.mNbLines;

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
     * Update the lava's heights
    */
    public static void Update(){
        // find neighbours for every points of the grid
        for(int j=1; j<sNbZ-1; j++){
            for(int i=1; i<sNbX-1; i++){
                ArrayList neighbours = Grid.GetParticles(i,j);
                // interpolate all neighbours
                Vector3 curPos = new Vector3(Grid.mCells[i,j].mX, 0.0f, Grid.mCells[i,j].mZ);
                float height = 0.0f;

                foreach(Particle pj in neighbours){
                    Vector3 pjPos = pj.GetPosition();
                    pjPos.y = 0.0f;

                    float r = Vector3.Distance(curPos, pjPos) / Constants.H;
                    float weight = Constants.ALPHA_POLY6 * ParticleSPH.K_POLY6(r);

                    height += pj.mHeight;
                    // Debug.Log("pj height: "+pj.mHeight);
                }

                sHeightmap[j,i] = height;
                // Debug.Log("p height: "+height);
            }
        }
    }

    /**
     * Get the vertices of the heightmap
    */
    public static List<Vector3> GetVertices(){
        List<Vector3> res = new List<Vector3>();
        for(int j=0; j<sNbZ; j++){
            for(int i=0; i<sNbX; i++){
                Vector3 newPos = new Vector3(Grid.mCells[i,j].mX, sHeightmap[j,i], Grid.mCells[i,j].mZ);
                Debug.Log("pos: "+newPos);
                res.Add(newPos);
            }
        }
        return res;
    }

}