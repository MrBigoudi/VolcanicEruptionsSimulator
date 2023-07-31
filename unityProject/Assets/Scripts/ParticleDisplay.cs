using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEditor;

/**
 * The class responsible of displaying particles
*/
public class ParticleDisplay : MonoBehaviour{

// ################################################################################################################################################################################################################
// ################################################################################################## ATTRIBUTES ##################################################################################################
// ################################################################################################################################################################################################################

    /**
     * The class used to regroup all the serializied fields
    */
    [SerializeField]
    public Tweakable _Fields;

    /**
     * The particle material
    */
    private Material _ParticleMaterial;

    /**
     * The particle mesh
    */
    private Mesh _ParticleMesh;

    /**
     * The particle mesh filter
    */
    private MeshFilter _ParticleMeshFilter;

    /**
     * The particle mesh renderer
    */
    private MeshRenderer _ParticleRenderer;

    /**
     * The buffer updating particles' positions inside the compute shader
    */
    private ComputeBuffer _PositionsBuffer;

    /**
     * The buffer updating particles' temperatures inside the compute shader
    */
    private ComputeBuffer _TemperaturesBuffer;

    /**
     * The height added to the particles if it's needed to make them go up or down during the simulation
    */
    private float _ParticlesMeshHeights;


// ################################################################################################################################################################################################################
// ################################################################################################### METHODS ####################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Initiate the serialized fields
    */
    public void Awake(){
        _ParticleMaterial = _Fields._ParticleMaterial;
        _ParticlesMeshHeights = _Fields._ParticlesMeshHeights;
    }

    /**
     * Update the height and send it to the material's shader
    */
    public void UpdateParticleHeight(){
        _ParticlesMeshHeights = _Fields._ParticlesMeshHeights;
        _ParticleMaterial.SetFloat("_ParticlesMeshHeights", _ParticlesMeshHeights);
    }

    /**
     * Initiate the mesh indices and topology
     * @param nbMaxParticles The maximum number of particles which is the number of indices set for the mesh
    */
    private void ParticleSetIndices(int nbMaxParticles){
        int[] indices = new int[nbMaxParticles];
        for(int i=0; i<nbMaxParticles; i++){
            indices[i] = i;
        }
        _ParticleMesh.SetIndices(indices, MeshTopology.Points, 0);
    }

    /**
     * Initiate the mesh vertices
     * @param nbMaxParticles The maximum number of particles which is the number of indices set for the mesh
    */
    private void ParticleSetVertices(int nbMaxParticles){
        Vector3[] vertices = new Vector3[nbMaxParticles];
        for(int i=0; i<nbMaxParticles; i++){
            vertices[i] = Vector3.zero;
        }
        _ParticleMesh.SetVertices(vertices);
    }

    /**
     * Update the mesh by sending new values in the material shader
     * @param nbCurParticles The current number of particles
     * @param displayParticles A boolean to tell if particles should be hidden of visible
    */
    public void UpdateParticleMesh(int nbCurParticles, bool displayParticles){
        _ParticleMaterial.SetInteger("_NbCurParticles", nbCurParticles);
        _ParticleMaterial.SetInteger("_DisplayParticles", displayParticles ? 1 : 0);
    }

    /**
     * Initiate the mesh properties
    */
    private void InitMeshProperties(){
        _ParticleMesh = new Mesh();
        _ParticleMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _ParticleMeshFilter = gameObject.AddComponent<MeshFilter>();
        _ParticleMeshFilter.mesh = _ParticleMesh;
        _ParticleRenderer = gameObject.AddComponent<MeshRenderer>();
        _ParticleRenderer.material = _ParticleMaterial;
    }

    /**
     * Set the buffers for the material shader
     * @param positionBuffer The buffer containing the particles' positons
     * @param temperaturesBuffer The buffer containing the particles' temperatures
    */
    private void SetMaterialBuffers(ComputeBuffer positionsBuffer, ComputeBuffer temperaturesBuffer){
        _PositionsBuffer = positionsBuffer;
        _TemperaturesBuffer = temperaturesBuffer;
        _ParticleMaterial.SetBuffer("_ParticlesPositions", _PositionsBuffer);
        _ParticleMaterial.SetBuffer("_ParticlesTemperatures", _TemperaturesBuffer);
    }

    /**
     * Initiate the mesh and the shader
     * @param positionBuffer The buffer containing the particles' positons
     * @param temperaturesBuffer The buffer containing the particles' temperatures 
    */
    public void InitMesh(ComputeBuffer positionsBuffer, ComputeBuffer temperaturesBuffer){
        InitMeshProperties();
        int nbMaxParticles = positionsBuffer.count;
        ParticleSetVertices(nbMaxParticles);
        ParticleSetIndices(nbMaxParticles);
        SetMaterialBuffers(positionsBuffer, temperaturesBuffer);
        _ParticleMesh.UploadMeshData(false);
    }

}