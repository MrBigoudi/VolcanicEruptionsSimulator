using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/**
 * A class representing the grid storing lava heightmap
*/
public class LavaTextureMap : MonoBehaviour{

    List<Vector3> _Points;

    /**
     * The mesh
    */
    Mesh sMesh;
    MeshFilter sMeshFilter;

    public Material _Material;

    /**
     * Init the lava heightmap
    */
    public void Awake(){
        // init the mesh
        sMesh = new Mesh();
        sMeshFilter = gameObject.AddComponent<MeshFilter>();
        _Points = new List<Vector3>();

        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _Material;
    }

    private void UpdtMesh(){
        sMesh.vertices = _Points.ToArray();
        int[] indices = new int[sMesh.vertices.Length];
        for (int i = 0; i < indices.Length; i++) {
            indices[i] = i;
        }
        sMesh.SetIndices(indices, MeshTopology.Points, 0);
    }

    /**
     * Update the lava's heights
    */
    public void Updt(List<Vector3> points){
        FetchPoints(points);
        UpdtMesh();
    }

    private void FetchPoints(List<Vector3> points){
        _Points = points;
    }

}