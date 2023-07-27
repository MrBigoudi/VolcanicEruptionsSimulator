using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;


public class ParticleDisplay : MonoBehaviour{

    [SerializeField]
    public Tweakable _Fields;

    private Material _ParticleMaterial;
    private Mesh _ParticleMesh;
    private MeshFilter _ParticleMeshFilter;
    private MeshRenderer _ParticleRenderer;

    private ComputeBuffer _PositionsBuffer;
    private ComputeBuffer _TemperaturesBuffer;

    private float _ParticlesMeshHeights;

    public void Awake(){
        _ParticleMaterial = _Fields._ParticleMaterial;
        _ParticlesMeshHeights = _Fields._ParticlesMeshHeights;
    }

    public void UpdateParticleHeight(){
        _ParticlesMeshHeights = _Fields._ParticlesMeshHeights;
        _ParticleMaterial.SetFloat("_ParticlesMeshHeights", _ParticlesMeshHeights);
    }

    private void ParticleSetIndices(int nbMaxParticles){
        int[] indices = new int[nbMaxParticles];
        for(int i=0; i<nbMaxParticles; i++){
            indices[i] = i;
        }
        _ParticleMesh.SetIndices(indices, MeshTopology.Points, 0);
    }

    
    private void ParticleSetVertices(int nbMaxParticles){
        Vector3[] vertices = new Vector3[nbMaxParticles];
        for(int i=0; i<nbMaxParticles; i++){
            vertices[i] = Vector3.zero;
        }
        _ParticleMesh.SetVertices(vertices);
    }

    public void UpdateParticleMesh(int nbCurParticles, bool displayParticles){
        // update property
        // Debug.Log(nbCurParticles);
        _ParticleMaterial.SetInteger("_NbCurParticles", nbCurParticles);
        _ParticleMaterial.SetInteger("_DisplayParticles", displayParticles ? 1 : 0);
        _ParticleMesh.RecalculateBounds();
    }

    private void InitMeshProperties(){
        _ParticleMesh = new Mesh();
        _ParticleMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _ParticleMeshFilter = gameObject.AddComponent<MeshFilter>();
        _ParticleMeshFilter.mesh = _ParticleMesh;
        _ParticleRenderer = gameObject.AddComponent<MeshRenderer>();
        _ParticleRenderer.material = _ParticleMaterial;
    }

    private void SetMaterialBuffers(ComputeBuffer positionsBuffer, ComputeBuffer temperaturesBuffer){
        _PositionsBuffer = positionsBuffer;
        _TemperaturesBuffer = temperaturesBuffer;
        _ParticleMaterial.SetBuffer("_ParticlesPositions", _PositionsBuffer);
        _ParticleMaterial.SetBuffer("_ParticlesTemperatures", _TemperaturesBuffer);
    }

    public void InitMesh(ComputeBuffer positionsBuffer, ComputeBuffer temperaturesBuffer){
        InitMeshProperties();
        int nbMaxParticles = positionsBuffer.count;
        ParticleSetVertices(nbMaxParticles);
        ParticleSetIndices(nbMaxParticles);
        SetMaterialBuffers(positionsBuffer, temperaturesBuffer);
        _ParticleMesh.UploadMeshData(false);
    }

}