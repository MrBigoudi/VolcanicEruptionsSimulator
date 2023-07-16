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
    public float   _Density;
    public float   _Mass;
    public float   _Volume;
};

/**
 * A class representing an sph solver
*/
public class ParticleSPHGPU : MonoBehaviour{

    [TweakableMember]
    public bool _DisplayParticles = false;
    [SerializeField]
    public ParticleDisplay _ParticleDisplay;
    [SerializeField]
    public bool _GaussianBlur = false;

    private int _NbMaxParticles;
    private int _NbCurParticles;

    private ComputeShader _Shader;

    private int _NbCurParticlesId;
    private int _NewParticlesId;
    private int _NbNewParticlesId;
    private ComputeBuffer _NewParticlesBuffer;

    // compute shader functions
    private int _KernelGenerateParticleId;
    private int _KernelUpdateDensitiesId;
    private int _KernelUpdateMassesAndVolumesId;
    private int _KernelPropagateParticleUpdateId;
    private int _KernelUpdateHeightsId;
    private int _KernelTimeIntegrationId;
    private int _KernelUpdateTerrainHeightsId;
    private int _KernelGaussianBlurTerrainHeightsId;

    // compute shader buffers
    private int _ParticlesId;
    private int _HeightsId;
    private int _HeightsGradientsId;
    private int _PositionsId;
    private int _VolumesId;
    private int _DensitiesId;
    private int _MassesId;
    private ComputeBuffer _ParticlesBuffer;
    private ComputeBuffer _HeightsBuffer;
    private ComputeBuffer _HeightsGradientsBuffer;
    private ComputeBuffer _PositionsBuffer;
    private ComputeBuffer _VolumesBuffer;
    private ComputeBuffer _DensitiesBuffer;
    private ComputeBuffer _MassesBuffer;

    private int _StaggeredGridId;
    private int _StaggeredGridHeightsId;
    private int _StaggeredGridHalfHeightsId;
    private int _StaggeredGridGradientsId;
    private int _StaggeredGridLaplaciansId;
    private ComputeBuffer _StaggeredGridBuffer;
    private ComputeBuffer _StaggeredGridHeightsBuffer;
    private ComputeBuffer _StaggeredGridHalfHeightsBuffer;
    private ComputeBuffer _StaggeredGridGradientsBuffer;
    private ComputeBuffer _StaggeredGridLaplaciansBuffer;

    private int _InitialTerrainHeightsId;
    private int _TerrainHeightsId;
    private int _TerrainHeightsTmpId;
    private ComputeBuffer _InitialTerrainHeightsBuffer;
    private ComputeBuffer _TerrainHeightsBuffer;
    private ComputeBuffer _TerrainHeightsTmpBuffer;
    private float[] _InitialTerrainHeights;
    private float[] _TerrainHeights;
    private float[] _TerrainHeightsTmp;

    // future buffer data
    private float[]   _Heights;
    private Vector3[] _HeightsGradients;
    private Vector3[] _Positions;
    private float[]   _Volumes;
    private float[]   _Densities;
    private float[]   _Masses;

    private ParticleGPU[] _Particles;

    [TweakableMember, Range(0.0f, 0.2f)]
    public float _DT = 0.1f;
    private float _ElapsedTime;

    [TweakableMember, Range(0.0f, 10.0f)]
    public float _Spike = 2.0f;
    [TweakableMember, Range(0.0f, 10.0f)]
    public float _KernelRadius = 2.0f;
    [TweakableMember, Range(0.0f, 2.0f)]
    public float _Stiffness = 1.0f;
    [TweakableMember, Range(0.0f, 10.0f)]
    public float _ParticleInitialHeight = 1.0f;

    private float _AlphaPoly6;
    private float _AlphaPoly6Laplacian;
    private float _AlphaViscosity;
    private float _AlphaViscosityLaplacian;


    // some usefull values
    private int _TerrainNbCols;
    private int _TerrainNbLines;
    private float _TerrainDeltaCols;
    private float _TerrainDeltaLines;
    private Vector3 _TerrainSize;

    [SerializeField]
    public Material _TerrainMaterial;
    private Mesh _TerrainMesh;
    private MeshFilter _TerrainMeshFilter;
    private MeshRenderer _TerrainRenderer;

    public int GetNbCurParticles(){
        return _NbCurParticles;
    }

    private void UpdateKernelFactors(){
        _AlphaPoly6              = 4.0f  / (Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
        _AlphaPoly6Laplacian     = 32.0f / (Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
        _AlphaViscosity          = 10.0f / (9.0f*Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
        _AlphaViscosityLaplacian = 40.0f / (Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
    }

    public void Create(int maxParticles, ComputeShader shader, TerrainGenerator terrain){
        _NbMaxParticles = maxParticles;
        _NbCurParticles = 0;
        _Shader = shader;
        _ElapsedTime = 0.0f;
        Init(terrain);
    }

    private void InitKernelsIds(){
        // init functions id
        _KernelGenerateParticleId           = _Shader.FindKernel("GenerateParticle");
        _KernelUpdateDensitiesId            = _Shader.FindKernel("UpdateDensities");
        _KernelUpdateMassesAndVolumesId     = _Shader.FindKernel("UpdateMassesAndVolumes");
        _KernelPropagateParticleUpdateId    = _Shader.FindKernel("PropagateParticleUpdate");
        _KernelUpdateHeightsId              = _Shader.FindKernel("UpdateHeights");
        _KernelTimeIntegrationId            = _Shader.FindKernel("TimeIntegration");
        _KernelUpdateTerrainHeightsId       = _Shader.FindKernel("UpdateTerrainHeights");
        _KernelGaussianBlurTerrainHeightsId = _Shader.FindKernel("GaussianBlurTerrainHeights");
    }

    private void InitStaggeredGridIds(){
        _StaggeredGridId            = Shader.PropertyToID("_StaggeredGrid");
        _StaggeredGridHeightsId     = Shader.PropertyToID("_StaggeredGridHeights");
        _StaggeredGridHalfHeightsId = Shader.PropertyToID("_StaggeredGridHalfHeights");
        _StaggeredGridGradientsId   = Shader.PropertyToID("_StaggeredGridGradients");
        _StaggeredGridLaplaciansId  = Shader.PropertyToID("_StaggeredGridLaplacians");
    }

    private void InitParticlesIds(){
        _NbCurParticlesId   = Shader.PropertyToID("_NbCurParticles");
        _NewParticlesId     = Shader.PropertyToID("_NewParticles");
        _NbNewParticlesId   = Shader.PropertyToID("_NbNewParticles");

        _ParticlesId        = Shader.PropertyToID("_Particles");
        _HeightsId          = Shader.PropertyToID("_Heights");
        _HeightsGradientsId = Shader.PropertyToID("_HeightsGradients");
        _PositionsId        = Shader.PropertyToID("_Positions");
        _VolumesId          = Shader.PropertyToID("_Volumes");
        _DensitiesId        = Shader.PropertyToID("_Densities");
        _MassesId           = Shader.PropertyToID("_Masses");

        _InitialTerrainHeightsId = Shader.PropertyToID("_InitialTerrainHeights");
        _TerrainHeightsId        = Shader.PropertyToID("_TerrainHeights");
        _TerrainHeightsTmpId     = Shader.PropertyToID("_TerrainHeightsTmp");
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
        _Densities        = new float[_NbMaxParticles];
        _Masses           = new float[_NbMaxParticles];

        _Particles = new ParticleGPU[_NbMaxParticles];
        _InitialTerrainHeights = Convert2dArray(StaggeredGridV2._Heights);
        _TerrainHeights = Convert2dArray(StaggeredGridV2._Heights);
        _TerrainHeightsTmp = Convert2dArray(StaggeredGridV2._Heights);
    }

    private void SendKernelFactorsToGPU(){
        UpdateKernelFactors();
        _Shader.SetFloat("ALPHA_POLY6", _AlphaPoly6);
        _Shader.SetFloat("ALPHA_POLY6_LAPLACIAN", _AlphaPoly6Laplacian);
        _Shader.SetFloat("ALPHA_VISCOSITY", _AlphaViscosity);
        _Shader.SetFloat("ALPHA_VISCOSITY_LAPLACIAN", _AlphaViscosityLaplacian);
    }

    private void SendConstantsToGPU(){
        _Shader.SetFloat("DT", _DT);
        _Shader.SetInt("MAX_PARTICLES", _NbMaxParticles);
        _Shader.SetFloat("PI", Constants.PI);
        _Shader.SetFloat("G", Constants.G);
        _Shader.SetFloat("RHO_0", Constants.RHO_0);
    }

    private void InitGpuValues(){
        SendConstantsToGPU();
        _Shader.SetBool("FirstTimeBlur", true);
        _Shader.SetBool("GaussianBlur", _GaussianBlur);
        UpdateGPUValues();        
    }

    private float[] Convert2dArray(float[,] arr){
        int nbLines = arr.GetLength(0);
        int nbCols  = arr.GetLength(1);
        // Debug.Log(nbLines + ", " + nbCols + ", " + arr.GetLength(0) + ", " + arr.GetLength(1));
        // Assert.IsTrue(_TerrainNbCols == arr.GetLength(0) && _TerrainNbLines == arr.GetLength(1));

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
        // Debug.Log(nbLines + ", " + nbCols + ", " + arr.GetLength(0) + ", " + arr.GetLength(1));
        // Assert.IsTrue(_TerrainNbCols == arr.GetLength(0) && _TerrainNbLines == arr.GetLength(1));

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
        // Debug.Log(nbLines + ", " + nbCols + ", " + arr.GetLength(0) + ", " + arr.GetLength(1));
        // Assert.IsTrue(_TerrainNbCols == arr.GetLength(0) && _TerrainNbLines == arr.GetLength(1));

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
        _Shader.SetBuffer(_KernelUpdateDensitiesId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateMassesAndVolumesId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateParticleUpdateId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelTimeIntegrationId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateTerrainHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelGaussianBlurTerrainHeightsId, id, buffer);
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
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*10);
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
        float[] heights     = Convert2dArray(StaggeredGridV2._Heights);
        float[] halfHeights = Convert2dArray(StaggeredGridV2._HalfHeights);
        Vector2[] gradients = Convert2dArray(StaggeredGridV2._Gradients);
        float[] laplacians  = Convert2dArray(StaggeredGridV2._Laplacians);
        // init grid buffers
        _StaggeredGridHeightsBuffer     = SetData(heights, _StaggeredGridHeightsId);
        _StaggeredGridHalfHeightsBuffer = SetData(halfHeights, _StaggeredGridHalfHeightsId);
        _StaggeredGridGradientsBuffer   = SetData(gradients, _StaggeredGridGradientsId);
        _StaggeredGridLaplaciansBuffer  = SetData(laplacians, _StaggeredGridLaplaciansId);
    }

    private void InitGpuBuffers(){
        _ParticlesBuffer        = SetData(_Particles, _ParticlesId);
        _PositionsBuffer        = SetData(_Positions, _PositionsId);
        _HeightsBuffer          = SetData(_Heights, _HeightsId);
        _HeightsGradientsBuffer = SetData(_HeightsGradients, _HeightsGradientsId);
        _VolumesBuffer          = SetData(_Volumes, _VolumesId);
        _DensitiesBuffer        = SetData(_Densities, _DensitiesId);
        _MassesBuffer           = SetData(_Masses, _MassesId);

        _InitialTerrainHeightsBuffer = SetData(_InitialTerrainHeights, _InitialTerrainHeightsId);
        _TerrainHeightsBuffer        = SetData(_TerrainHeights, _TerrainHeightsId);
        _TerrainHeightsTmpBuffer     = SetData(_TerrainHeightsTmp, _TerrainHeightsTmpId);
    }

    private void GetTerrainValues(TerrainGenerator terrain){
        _TerrainSize    = terrain._Size;
        _TerrainNbCols  = terrain.GetResolution();
        _TerrainNbLines = terrain.GetResolution();

        Vector3 terrainSize = terrain._Size;
        _TerrainDeltaCols  = terrainSize.x / _TerrainNbCols;
        _TerrainDeltaLines = terrainSize.z / _TerrainNbLines;
    }

    private void Init(TerrainGenerator terrain){
        GetTerrainValues(terrain);
        InitIds();
        InitGpuValues();
        InitBuffersData();
        InitGpuBuffers();
        InitStaggeredGrid();
        InitMesh();
    }

    private void GenerateParticle(Vector3 position){
        _ElapsedTime += Time.deltaTime;
        if(_NbCurParticles < _NbMaxParticles && _ElapsedTime > _DT){
            int delta = (int)(_ElapsedTime / _DT);
            int cpt = 0;
            // Debug.Log(_ElapsedTime + ", " + _DT + ", " + delta);
            ParticleGPU[] data = new ParticleGPU[delta];

            // put new particles in buffer
            for(int i=0; i<delta; i++){
                if(_NbCurParticles >= _NbMaxParticles) break;
                _NbCurParticles++;
                cpt ++;

                float radius = (_ParticleInitialHeight / 2.0f);
                float volume = (4.0f/3.0f)*Constants.PI*radius*radius*radius; // 4/3 * pi * r^3
                float density = Constants.RHO_0;
                float mass = volume * density;

                ParticleGPU p = new ParticleGPU{
                                        _Position = position,
                                        _Height = _ParticleInitialHeight,
                                        _HeightGradient = Vector3.zero,
                                        _Density = density,
                                        _Mass = mass,
                                        _Volume = volume
                                    };
                data[i] = p;
            }

            // update gpu side
            int res = cpt / 8 + 1;
            _Shader.SetInt(_NbNewParticlesId, cpt);
            _NewParticlesBuffer = SetData(data, _NewParticlesId);
            // restore correct values
            _Shader.SetInt(_NbCurParticlesId, _NbCurParticles);
            _ElapsedTime -= (delta*_DT);
            _Shader.Dispatch(_KernelGenerateParticleId, res, 1, 1);
        }
    }

    private void UpdateHeights(int res){
        _Shader.Dispatch(_KernelUpdateHeightsId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateParticleUpdateId, res, 1, 1);
    }

    private void TimeIntegration(int res){
        // _Shader.SetFloat("DT", Time.deltaTime);
        _Shader.Dispatch(_KernelTimeIntegrationId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateParticleUpdateId, res, 1, 1);
    }

    private void UpdateTerrainHeights(){
        int res = (_TerrainNbCols*_TerrainNbLines / 1024) + 1;
        _Shader.Dispatch(_KernelUpdateTerrainHeightsId, res, 1, 1);

        if(_GaussianBlur){
            _Shader.Dispatch(_KernelGaussianBlurTerrainHeightsId, res, 1, 1);
            _Shader.SetBool("FirstTimeBlur", false);
        }
    }

    private void UpdateGPUValues(){
        SendKernelFactorsToGPU();
        _Shader.SetFloat("STIFF", _Stiffness);
        _Shader.SetFloat("H", _KernelRadius);
        _Shader.SetFloat("SPIKE", _Spike);
    }

    private void UpdateVolumes(int res){
        _Shader.Dispatch(_KernelUpdateDensitiesId, res, 1, 1);
        _Shader.Dispatch(_KernelUpdateMassesAndVolumesId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateParticleUpdateId, res, 1, 1);
    }

    public void Updt(Vector3 position){
        UpdateGPUValues();        
        int res = (_NbMaxParticles / 128)+1;
        // generate particle in GPU
        GenerateParticle(position);
        // calculate heights in gpu
        UpdateHeights(res);
        // time integration gpu
        TimeIntegration(res);
        // update the terrain heights
        UpdateTerrainHeights();
        // display particles
        if(_DisplayParticles){
            UpdateParticleMesh();
        }
    }

    private void ReleaseBuffers(){
        _ParticlesBuffer.Dispose();
        _HeightsBuffer.Dispose();
        _HeightsGradientsBuffer.Dispose();
        _PositionsBuffer.Dispose();
        _VolumesBuffer.Dispose();
        _DensitiesBuffer.Dispose();
        _MassesBuffer.Dispose();

        _NewParticlesBuffer.Dispose();
        _InitialTerrainHeightsBuffer.Dispose();
        _TerrainHeightsBuffer.Dispose();
        _TerrainHeightsTmpBuffer.Dispose();

        _StaggeredGridBuffer.Dispose();
        _StaggeredGridHeightsBuffer.Dispose();
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
        _TerrainMaterial.SetBuffer("_TerrainHeights", _TerrainHeightsBuffer);
        _TerrainMaterial.SetBuffer("_InitialTerrainHeights", _InitialTerrainHeightsBuffer);
    }

    private void InitTerrainMesh(){
        _TerrainMesh = new Mesh();
        _TerrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _TerrainMeshFilter = gameObject.AddComponent<MeshFilter>();
        _TerrainMeshFilter.mesh = _TerrainMesh;
        _TerrainRenderer = gameObject.AddComponent<MeshRenderer>();
        _TerrainRenderer.material = _TerrainMaterial;
        UpdateTerrainMesh();
    }

    private void InitParticlesMesh(){
        if(_DisplayParticles){
            // Debug.Log("Particles mesh initialized");
            _ParticleDisplay.InitMesh(_PositionsBuffer);
        }
    }

    private void InitMesh(){
        InitTerrainMesh();
        InitParticlesMesh();        
    }

    public void UpdateParticleMesh(){
        _ParticleDisplay.UpdateParticleMesh(_NbCurParticles);
    }

    private void UpdateTerrainMesh(){
        SetMaterialBuffers();
        TerrainSetVertices();
        TerrainSetIndices();
        TerrainSetNormals();
        _TerrainMesh.UploadMeshData(false);
    }

    private void TerrainSetNormals(){
        Vector3[] normals = new Vector3[_TerrainNbLines*_TerrainNbCols];
        for(int j=1; j<_TerrainNbLines; j++){
            for(int i=1; i<_TerrainNbCols; i++){
                int idx = i + j*_TerrainNbCols;
                normals[idx].x = StaggeredGridV2._Gradients[j-1,i-1].x;
                normals[idx].z = StaggeredGridV2._Gradients[j-1,i-1].y;
            }
        }
        _TerrainMesh.SetNormals(normals);
    }

    private void TerrainSetVertices(){
        Vector3[] vertices = new Vector3[_TerrainNbLines*_TerrainNbCols];
        for(int j=0; j<_TerrainNbLines; j++){
            for(int i=0; i<_TerrainNbCols; i++){
                float x = i * _TerrainDeltaCols;
                float z = j * _TerrainDeltaLines;
                int idx = i + j*_TerrainNbCols;
                vertices[idx].x = x;
                vertices[idx].z = z;
            }
        }
        _TerrainMesh.SetVertices(vertices);
    }

    private void TerrainSetIndices(){
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
        _TerrainMesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }

}