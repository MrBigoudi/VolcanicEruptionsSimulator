using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/**
 * A class representing the grid storing lava heightmap
*/
public class LavaTextureMap : MonoBehaviour{
    /**
     * The lava heightmap
    */
    public float[,] sHeightmap;

    /**
     * The heightmap dimensions
    */
    public int sNbX, sNbZ;

    /**
     * The mesh
    */
    public Mesh sMesh;
    public MeshRenderer sMeshRenderer;
    public MeshFilter sMeshFilter;
    public Material sMeshMaterial;



    /**
     * Init dimensions
    */
    private void InitDimensions(){
        // get array dimensions
        sNbX = Grid.mNbCols;
        sNbZ = Grid.mNbLines;
        // Debug.Log(sNbX+" x "+sNbZ);

        sHeightmap = new float[sNbZ, sNbX];
    }

    /**
     * Init the lava heightmap
    */
    public void Init(){
        InitDimensions();

        // init heights to 0
        for(int j=0; j<sNbZ; j++){
            for(int i=0; i<sNbX; i++){
                sHeightmap[j,i] = 0.0f;

                // float curX = Grid.mCells[i,j].mX;
                // float curZ = Grid.mCells[i,j].mZ;
                // float cellWidth = Grid.mCellWidth;

                // Debug.DrawLine(new Vector3(curX, 0, curZ), new Vector3(curX+cellWidth, 0, curZ), Color.blue, 200.0f, false);
                // Debug.DrawLine(new Vector3(curX, 0, curZ), new Vector3(curX, 0, curZ+cellWidth), Color.green, 200.0f, false);
            }
        }

        // init the mesh
        sMesh = new Mesh();
        sMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        sMeshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        sMeshFilter = gameObject.AddComponent<MeshFilter>();
        sMeshMaterial = Resources.Load("LavaMaterial", typeof(Material)) as Material;
    }

    /**
     * Update the lava's heights
    */
    public void Update(){
        // find neighbours for every points of the grid
        for(int j=1; j<sNbZ-1; j++){
            for(int i=1; i<sNbX-1; i++){
                ArrayList neighbours = Grid.GetParticles(i,j);
                // interpolate all neighbours
                Vector3 curPos = new Vector3(Grid.mCells[i,j].mX, 0.0f, Grid.mCells[i,j].mZ);
                float height = 0.0f;

                foreach(Particle pj in neighbours){
                    // Vector3 pjPos = pj.GetPosition();
                    // pjPos.y = 0.0f;

                    // float r = Vector3.Distance(curPos, pjPos) / Constants.H;
                    // float weight = Constants.ALPHA_POLY6 * ParticleSPH.K_POLY6(r);

                    height += pj.mHeight;
                    // Debug.Log("pj height: "+pj.mHeight);
                }
                if(neighbours.Count > 0){
                    sHeightmap[j,i] = height/neighbours.Count;
                }else{
                    sHeightmap[j,i] = 0.0f;
                }
                // if(height>0)Debug.Log("p height: "+height);
            }
        }

        // generate the mesh
        // CreateMesh();
    }

    /**
     * Create a mesh
    */
    private void CreateMesh(){
        // create the vertices
        sMesh.vertices = GetVertices();
        // create the triangles
        sMesh.triangles = GetIndices();
        // calculate the normals
        sMesh.RecalculateNormals();
        // calculate uv coordinares
        sMesh.uv = GetUVs();

        sMeshFilter.mesh = sMesh;
        sMeshRenderer.material = sMeshMaterial;
    }

    // public void OnDrawGizmosSelected(){
    //     Vector3[] vert = GetVertices();
    //     // for(int j=0; j<sNbZ; j++){
    //     //     for(int i=0; i<sNbX; i++){
    //     //         float curX = Grid.mCells[i,j].mX;
    //     //         float curZ = Grid.mCells[i,j].mZ;
    //     //         Gizmos.color = Color.blue;
    //     //         Gizmos.DrawSphere(new Vector3(curX, 1.0f, curZ), 0.2f);
    //     //     }
    //     // }
    //     foreach(Vector3 newPos in vert){
    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawSphere(newPos, 0.2f);
    //     }
    // }

    /**
     * Get the vertices of the heightmap
    */
    private Vector3[] GetVertices(){
        List<Vector3> res = new List<Vector3>();
        for(int j=0; j<sNbZ; j++){
            for(int i=0; i<sNbX; i++){
                float curX = Grid.mCells[i,j].mX;
                float curZ = Grid.mCells[i,j].mZ;
                // Debug.Log("x: "+curX+", z: "+ curZ);

                // Vector3 newPos = new Vector3(curX, 0.0f, curZ);
                // Debug.Log("pos: "+newPos);
                if(sHeightmap[j,i]==0.0f){
                    res.Add(new Vector3(curX, 0.0f, curZ));
                }else{
                    float terrainHeight = ParticleSPH.GetTerrainHeight(new Vector3(curX, 0.0f, curZ));
                    Vector3 newPos = new Vector3(curX, sHeightmap[j,i]+terrainHeight, curZ);
                    res.Add(newPos);
                }
            }
        }
        // Debug.Log("\n\n");
        Assert.IsTrue(res.Count == sNbX*sNbZ);
        return res.ToArray();
    }

    // Get the indices to draw triangles
    private int[] GetIndices(){
        List<int> res = new List<int>();
        for(int j=0; j<sNbZ; j++){
            int curLine = j*sNbX;
            int nextLine = (j+1)*sNbX;
            if(j==sNbZ-1) nextLine = 0;

            for(int i=0; i<sNbX-1; i++){
                int nextCol = i+1;
                if(i==sNbX-1) nextCol = 0;

                // for the first side
                // first triangle
                res.Add(curLine+i);
                res.Add(curLine+nextCol);
                res.Add(nextLine+i);
                // second triangle
                res.Add(curLine+nextCol);
                res.Add(nextLine+nextCol);
                res.Add(nextLine+i);

                // for the other side
                // first triangle
                res.Add(curLine+i);
                res.Add(nextLine+i);
                res.Add(curLine+nextCol);
                // second triangle
                res.Add(curLine+nextCol);
                res.Add(nextLine+i);
                res.Add(nextLine+nextCol);
            }
        }
        int[] tmp = res.ToArray();
        // Debug.Log("Array:");
        // for(int i=0; i<tmp.Length; i++)
        //     Debug.Log(tmp[i]+"\n");
        return tmp;
    }

    // Get the uv cordinates for the texture
    private Vector2[] GetUVs(){
        List<Vector2> res = new List<Vector2>();
        for(int j=0; j<sNbZ; j++){
            for(int i=0; i<sNbX; i++){
                res.Add(new Vector2(((float)i)/((float)sNbX), ((float)j)/((float)sNbZ)));
            }
        }
        return res.ToArray();
    }

}