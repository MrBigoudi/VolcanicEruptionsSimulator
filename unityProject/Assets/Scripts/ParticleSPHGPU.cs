using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct StaggeredGridGPU{
    public int _NbCols, _NbLines;
    public float _DeltaCols, _DeltaLines;    
};

public struct ParticleGPU{
    public Vector3 _Position;
    public float   _Height;
    public Vector3 _HeightGradient;
    public float   _Volume;
};

/**
 * A class representing an sph solver
*/
public class ParticleSPHGPU : MonoBehaviour{

    private int _NbMaxParticles;
    private int _NbCurParticles;

    private ComputeShader _Shader;

    private int _NbCurParticlesId;
    private int _NewParticleId;
    private ComputeBuffer _NewParticleBuffer;

    // compute shader functions
    private int _KernelGenerateParticleId;
    private int _KernelUpdateHeightsId;
    private int _KernelPropagateHeightsId;
    private int _KernelTimeIntegrationId;
    private int _KernelPropagatePositionsId;
    private int _KernelUpdateTerrainHeightsId;

    // compute shader buffers
    private int _ParticlesId;
    private int _HeightsId;
    private int _HeightsGradientsId;
    private int _PositionsId;
    private int _VolumesId;
    private ComputeBuffer _ParticlesBuffer;
    private ComputeBuffer _HeightsBuffer;
    private ComputeBuffer _HeightsGradientsBuffer;
    private ComputeBuffer _PositionsBuffer;
    private ComputeBuffer _VolumesBuffer;

    private int _StaggeredGridId;
    private int _StaggeredGridHeightsId;
    private int _StaggeredGridHalfHeightsColsId;
    private int _StaggeredGridHalfHeightsLinesId;
    private int _StaggeredGridHalfHeightsId;
    private int _StaggeredGridGradientsId;
    private int _StaggeredGridLaplaciansId;
    private ComputeBuffer _StaggeredGridBuffer;
    private ComputeBuffer _StaggeredGridHeightsBuffer;
    private ComputeBuffer _StaggeredGridHalfHeightsColsBuffer;
    private ComputeBuffer _StaggeredGridHalfHeightsLinesBuffer;
    private ComputeBuffer _StaggeredGridHalfHeightsBuffer;
    private ComputeBuffer _StaggeredGridGradientsBuffer;
    private ComputeBuffer _StaggeredGridLaplaciansBuffer;

    private int _TerrainHeightsId;
    private ComputeBuffer _TerrainHeightsBuffer;
    private float[] _TerrainHeights;

    // future buffer data
    private float[]   _Heights;
    private Vector3[] _HeightsGradients;
    private Vector3[] _Positions;
    private float[]   _Volumes;

    private ParticleGPU[] _Particles;

    // some usefull values
    private int _TerrainNbCols;
    private int _TerrainNbLines;
    private Vector3 _TerrainSize;

    [SerializeField]
    public Material _Material;
    private Mesh _Mesh;
    private MeshFilter _MeshFilter;

    public int GetNbCurParticles(){
        return _NbCurParticles;
    }

    public void Create(int maxParticles, ComputeShader shader, TerrainGenerator terrain){
        _NbMaxParticles = maxParticles;
        _NbCurParticles = 0;
        _Shader = shader;
        Init(terrain);
    }

    private void InitKernelsIds(){
        // init functions id
        _KernelGenerateParticleId     = _Shader.FindKernel("GenerateParticle");
        _KernelUpdateHeightsId        = _Shader.FindKernel("UpdateHeights");
        _KernelPropagateHeightsId     = _Shader.FindKernel("PropagateHeights");
        _KernelTimeIntegrationId      = _Shader.FindKernel("TimeIntegration");
        _KernelPropagatePositionsId   = _Shader.FindKernel("PropagatePositions");
        _KernelUpdateTerrainHeightsId = _Shader.FindKernel("UpdateTerrainHeights");
    }

    private void InitStaggeredGridIds(){
        _StaggeredGridId                 = Shader.PropertyToID("_StaggeredGrid");
        _StaggeredGridHeightsId          = Shader.PropertyToID("_StaggeredGridHeights");
        _StaggeredGridHalfHeightsColsId  = Shader.PropertyToID("_StaggeredGridHalfHeightsCols");
        _StaggeredGridHalfHeightsLinesId = Shader.PropertyToID("_StaggeredGridHalfHeightsLines");
        _StaggeredGridHalfHeightsId      = Shader.PropertyToID("_StaggeredGridHalfHeights");
        _StaggeredGridGradientsId        = Shader.PropertyToID("_StaggeredGridGradients");
        _StaggeredGridLaplaciansId       = Shader.PropertyToID("_StaggeredGridLaplacians");
    }

    private void InitParticlesIds(){
        _NbCurParticlesId   = Shader.PropertyToID("_NbCurParticles");
        _NewParticleId      = Shader.PropertyToID("_NewParticle");

        _ParticlesId        = Shader.PropertyToID("_Particles");
        _HeightsId          = Shader.PropertyToID("_Heights");
        _HeightsGradientsId = Shader.PropertyToID("_HeightsGradients");
        _PositionsId        = Shader.PropertyToID("_Positions");
        _VolumesId          = Shader.PropertyToID("_Volumes");

        _TerrainHeightsId   = Shader.PropertyToID("_TerrainHeights");
    }

    private void InitIds(){
        InitKernelsIds();
        InitParticlesIds();        
    }

    private void InitBuffersData(){
        _Heights          = new float[_NbMaxParticles];
        _HeightsGradients = new Vector3[_NbMaxParticles];
        _Positions        = new Vector3[_NbMaxParticles];
        _Volumes          = new float[_NbMaxParticles];

        _Particles = new ParticleGPU[_NbMaxParticles];
        _TerrainHeights = Convert2dArray(StaggeredGridV2._Heights);
    }

    private void InitGpuValues(TerrainGenerator terrain){
        _Shader.SetFloat("H", Constants.H);
        _Shader.SetFloat("PI", Constants.PI);
        _Shader.SetFloat("G", Constants.G);

        _Shader.SetFloat("STIFF", Constants.STIFFNESS);
        _Shader.SetFloat("RHO_0", Constants.RHO_0);

        _Shader.SetFloat("ALPHA_POLY6", Constants.ALPHA_POLY6);
        _Shader.SetFloat("ALPHA_POLY6_LAPLACIAN", Constants.ALPHA_POLY6_LAPLACIAN);
        _Shader.SetFloat("ALPHA_VISCOSITY", Constants.ALPHA_VISCOSITY);
        _Shader.SetFloat("ALPHA_VISCOSITY_LAPLACIAN", Constants.ALPHA_VISCOSITY_LAPLACIAN);

        _Shader.SetFloat("SPIKE", 1.0f);

        _Shader.SetInt("MAX_PARTICLES", _NbMaxParticles);

        _TerrainSize    = terrain._Size;
        _TerrainNbCols  = terrain.GetResolution();
        _TerrainNbLines = terrain.GetResolution();
    }

    private float[] Convert2dArray(float[,] arr){
        int nbLines = arr.GetLength(0);
        int nbCols  = arr.GetLength(1);

        float[] res = new float[nbLines*nbCols];
        float val;

        for(int j = 0; j<nbLines; j++){
            for(int i = 0; i<nbCols; i++){
                val = arr[j,i];
                res[i+j*nbCols] = val;
            }
        }
        return res;
    }

    private Vector2[] Convert2dArray(Vector2[,] arr){
        int nbLines = arr.GetLength(0);
        int nbCols  = arr.GetLength(1);

        Vector2[] res = new Vector2[nbLines*nbCols];
        Vector2 val;

        for(int j = 0; j<nbLines; j++){
            for(int i = 0; i<nbCols; i++){
                val = arr[j,i];
                res[i+j*nbCols] = val;
            }
        }
        return res;
    }

    private Vector3[] Convert2dArray(Vector3[,] arr){
        int nbLines = arr.GetLength(0);
        int nbCols  = arr.GetLength(1);

        Vector3[] res = new Vector3[nbLines*nbCols];
        Vector3 val;

        for(int j = 0; j<nbLines; j++){
            for(int i = 0; i<nbCols; i++){
                val = arr[j,i];
                res[i+j*nbCols] = val;
            }
        }
        return res;
    }

    private void SendDataToAllKernels(ComputeBuffer buffer, int id){
        // set to every kernel
        _Shader.SetBuffer(_KernelGenerateParticleId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelTimeIntegrationId, id, buffer);
        _Shader.SetBuffer(_KernelPropagatePositionsId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateTerrainHeightsId, id, buffer);
    }

    private ComputeBuffer SetData(float[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float));
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    private ComputeBuffer SetData(Vector2[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*2);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    private ComputeBuffer SetData(Vector3[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*3);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    private ComputeBuffer SetData(int[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(int));
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    private ComputeBuffer SetData(ParticleGPU[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*8);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    private ComputeBuffer SetData(StaggeredGridGPU[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*2 + sizeof(int)*2);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    private void InitStaggeredGridAttributes(){
        // init grid data
        StaggeredGridGPU[] gridArray = new StaggeredGridGPU[1];
        StaggeredGridGPU grid = new StaggeredGridGPU();
        grid._NbCols     = StaggeredGridV2._NbCols;
        grid._NbLines    = StaggeredGridV2._NbLines;
        grid._DeltaCols  = StaggeredGridV2._DeltaCols;
        grid._DeltaLines = StaggeredGridV2._DeltaLines;
        gridArray[0] = grid;
        
        // init grid buffer
        _StaggeredGridBuffer = SetData(gridArray, _StaggeredGridId);
    }

    private void InitStaggeredGrid(){
        // init grid itself
        InitStaggeredGridIds();
        InitStaggeredGridAttributes();
        // init grid data
        float[] heights          = Convert2dArray(StaggeredGridV2._Heights);
        float[] halfHeightsCols  = Convert2dArray(StaggeredGridV2._HalfHeightsCols);
        float[] halfHeightsLines = Convert2dArray(StaggeredGridV2._HalfHeightsLines);
        float[] halfHeights      = Convert2dArray(StaggeredGridV2._HalfHeights);
        Vector2[] gradients      = Convert2dArray(StaggeredGridV2._Gradients);
        float[] laplacians       = Convert2dArray(StaggeredGridV2._Laplacians);
        // init grid buffers
        _StaggeredGridHeightsBuffer          = SetData(heights, _StaggeredGridHeightsId);
        _StaggeredGridHalfHeightsColsBuffer  = SetData(halfHeightsCols, _StaggeredGridHalfHeightsColsId);
        _StaggeredGridHalfHeightsLinesBuffer = SetData(halfHeightsLines, _StaggeredGridHalfHeightsLinesId);
        _StaggeredGridHalfHeightsBuffer      = SetData(halfHeights, _StaggeredGridHalfHeightsId);
        _StaggeredGridGradientsBuffer        = SetData(gradients, _StaggeredGridGradientsId);
        _StaggeredGridLaplaciansBuffer       = SetData(laplacians, _StaggeredGridLaplaciansId);
        // release buffers
        // _StaggeredGridHeightsBuffer.Dispose();
        // _StaggeredGridHalfHeightsColsBuffer.Dispose();
        // _StaggeredGridHalfHeightsLinesBuffer.Dispose();
        // _StaggeredGridGradientsBuffer.Dispose();
        // _StaggeredGridLaplaciansBuffer.Dispose();
    }

    private void InitGpuBuffers(){
        _ParticlesBuffer        = SetData(_Particles, _ParticlesId);
        _PositionsBuffer        = SetData(_Positions, _PositionsId);
        _HeightsBuffer          = SetData(_Heights, _HeightsId);
        _HeightsGradientsBuffer = SetData(_HeightsGradients, _HeightsGradientsId);
        _VolumesBuffer          = SetData(_Volumes, _VolumesId);

        _TerrainHeightsBuffer   = SetData(_TerrainHeights, _TerrainHeightsId);
    }

    private void Init(TerrainGenerator terrain){
        InitIds();
        InitGpuValues(terrain);
        InitBuffersData();
        InitGpuBuffers();
        InitStaggeredGrid();
        InitMesh();
    }

    private void GenerateParticle(Vector3 position, int res){
        if(_NbCurParticles < _NbMaxParticles){
            // init CPU part
            _Shader.SetInt(_NbCurParticlesId, _NbCurParticles);
            _NbCurParticles++;

            // init GPU part
            ParticleGPU p = new ParticleGPU{
                                    _Position = position,
                                    _Height = 0.4f,
                                    _HeightGradient = Vector3.zero,
                                    _Volume = 200.0f/Constants.RHO_0
                                };
            ParticleGPU[] data = new ParticleGPU[1];
            data[0] = p;
            _NewParticleBuffer = SetData(data, _NewParticleId);

            _Shader.Dispatch(_KernelGenerateParticleId, res, 1, 1);
        }
    }

    private void UpdateHeights(int res){
        _Shader.Dispatch(_KernelUpdateHeightsId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateHeightsId, res, 1, 1);
    }

    private void TimeIntegration(int res){
        _Shader.SetFloat("DT", Time.deltaTime);
        _Shader.Dispatch(_KernelTimeIntegrationId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagatePositionsId, res, 1, 1);
    }

    private void UpdateTerrainHeights(){
        int res = (_TerrainNbCols*_TerrainNbLines / 1024) + 1;
        _Shader.Dispatch(_KernelUpdateTerrainHeightsId, res, 1, 1);
    }

    public void Updt(Vector3 position, float stiffness){
        _Shader.SetFloat("STIFF", stiffness);
        int res = (_NbMaxParticles / 128)+1;
        // generate particle in GPU
        GenerateParticle(position, res);
        // calculate heights in gpu
        UpdateHeights(res);
        // time integration gpu
        TimeIntegration(res);
        // update the terrain heights
        UpdateTerrainHeights();
    }

    private void ReleaseBuffers(){
        _ParticlesBuffer.Dispose();
        _HeightsBuffer.Dispose();
        _HeightsGradientsBuffer.Dispose();
        _PositionsBuffer.Dispose();
        _VolumesBuffer.Dispose();
        _NewParticleBuffer.Dispose();
        _TerrainHeightsBuffer.Dispose();

        _StaggeredGridBuffer.Dispose();
        _StaggeredGridHeightsBuffer.Dispose();
        _StaggeredGridHalfHeightsColsBuffer.Dispose();
        _StaggeredGridHalfHeightsLinesBuffer.Dispose();
        _StaggeredGridHalfHeightsBuffer.Dispose();
        _StaggeredGridGradientsBuffer.Dispose();
        _StaggeredGridLaplaciansBuffer.Dispose();
    }

    public void OnApplicationQuit(){
        // release buffers
        ReleaseBuffers();
    }

    public void OnDisable(){
        // ReleaseBuffers();
    }

    private void SetMaterialBuffers(){
        _Material.SetBuffer("_TerrainHeights", _TerrainHeightsBuffer);
        _Material.SetBuffer("_InitialTerrainHeights", _StaggeredGridHeightsBuffer);
    }

    private void InitMesh(){
        _Mesh = new Mesh();
        _Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _MeshFilter = gameObject.AddComponent<MeshFilter>();
        _MeshFilter.mesh = _Mesh;
        Renderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = _Material;

        SetMaterialBuffers();
        SetVertices();
        SetIndices();
        _Mesh.UploadMeshData(false);
    }

    private void SetVertices(){
        Vector3[] vertices = new Vector3[_TerrainNbLines*_TerrainNbCols];
        for(int j=0; j<_TerrainNbLines; j++){
            for(int i=0; i<_TerrainNbCols; i++){
                float x = i * _TerrainSize.x / _TerrainNbCols;
                float z = j * _TerrainSize.z / _TerrainNbLines;
                int idx = i + j*_TerrainNbCols;
                vertices[idx].x = x;
                vertices[idx].z = z;
            }
        }
        _Mesh.SetVertices(vertices);
    }

    private void SetIndices(){
        // init indices
        int[] indices = new int[_TerrainNbLines*_TerrainNbCols*12];
        int idx = 0;
        for(int j=0; j<_TerrainNbLines-1; j++){
            for(int i=0; i<_TerrainNbCols-1; i++){
                int id1 = i + j*_TerrainNbCols;
                int id2 = id1 + 1;
                int id3 = id1 + _TerrainNbCols;
                int id4 = id3 + 1;
                // first side
                // first triangle
                indices[idx++] = id1;
                indices[idx++] = id2;
                indices[idx++] = id3;
                // second triangle
                indices[idx++] = id3;
                indices[idx++] = id2;
                indices[idx++] = id4;

                // second side
                // first triangle
                indices[idx++] = id1;
                indices[idx++] = id3;
                indices[idx++] = id2;
                // second triangle
                indices[idx++] = id3;
                indices[idx++] = id4;
                indices[idx++] = id2;
            }
        }
        // Debug.Log(vertices.Length + ", " + nbIndices);
        _Mesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }

}