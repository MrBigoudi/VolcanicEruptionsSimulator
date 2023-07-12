using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;


public class ParticleDisplay : MonoBehaviour{

    [SerializeField]
    public Material _ParticleMaterial;
    private Mesh _ParticleMesh;
    private MeshFilter _ParticleMeshFilter;
    private MeshRenderer _ParticleRenderer;

    private void ParticleSetIndices(int nbCurParticles){
        int[] indices = new int[nbCurParticles];
        for(int i=0; i<nbCurParticles; i++){
            indices[i] = i;
        }
        _ParticleMesh.SetIndices(indices, MeshTopology.Points, 0);
    }

    
    private void ParticleSetVertices(Vector3[] positions, int nbCurParticles){
        Vector3[] vertices = new Vector3[nbCurParticles];
        for(int i=0; i<nbCurParticles; i++){
            vertices[i] = positions[i];
        }
        _ParticleMesh.SetVertices(vertices);
    }

    public void UpdateParticleMesh(Vector3[] positions, int nbCurParticles){
        ParticleSetVertices(positions, nbCurParticles);
        ParticleSetIndices(nbCurParticles);
        _ParticleMesh.UploadMeshData(false);
    }

    public void Awake(){
        _ParticleMesh = new Mesh();
        _ParticleMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _ParticleMeshFilter = gameObject.AddComponent<MeshFilter>();
        _ParticleMeshFilter.mesh = _ParticleMesh;
        _ParticleRenderer = gameObject.AddComponent<MeshRenderer>();
        _ParticleRenderer.material = _ParticleMaterial;
    }

}