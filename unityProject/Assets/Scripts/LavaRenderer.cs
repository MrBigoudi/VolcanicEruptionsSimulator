using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LavaRenderer : MonoBehaviour{
    private MeshFilter mMeshFilter;

    public void Render(){
        mMeshFilter = GetComponent<MeshFilter>();
        List<Vector3> vert = LavaTextureMap.GetVertices();
        
        // init indices
        List<int> indices = new List<int>();
        for(int i=0; i<vert.Count; i++){
            indices.Add(i);
            indices.Add((i+1)%vert.Count);
            indices.Add((i+2)%vert.Count);
        }
        

        Mesh mesh = new Mesh();
        mesh.vertices = vert.ToArray();
        mesh.triangles = indices.ToArray();

        mMeshFilter.mesh = mesh;
    }
}