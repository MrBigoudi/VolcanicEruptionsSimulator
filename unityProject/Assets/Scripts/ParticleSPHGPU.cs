using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public struct StaggeredGridGPU{
    public int _NbCols, _NbLines;
    public float _DeltaCols, _DeltaLines;    
};

public struct NeighbourGridGPU{
    public int _NbCols, _NbLines;
    public float _DeltaCols, _DeltaLines;    
    public int _NbMaxNeighbours;
};

public struct ParticleGPU{
    public Vector3 _Velocity;
    public Vector3 _Position;
    public float   _Height;
    public Vector3 _HeightGradient;
    public float   _Density;
    public float   _Mass;
    public float   _Volume;
    public int     _Cell;
    public int     _Id;
    public float   _Temperature;
};

/**
 * A class representing an sph solver
*/
public class ParticleSPHGPU : MonoBehaviour{

    [SerializeField]
    public Tweakable _Fields;

    private bool _DisplayParticles;
    private bool _DisplayLava;
    private ParticleDisplay _ParticleDisplay;
    private bool _GaussianBlur;

    private int _NbMaxParticles;
    private int _NbCurParticles;
    private float _InitialPositionDelta;
    private float _Mu;
    private float _Ke;
    private float _ThetaE;

    private ComputeShader _Shader;

    private int _NbCurParticlesId;
    private int _NbNewParticlesId;
    private int _NewPositionId;
    private int _ParticleInitialHeightId;
    private int _RandId;

    // compute shader functions
    private int _KernelGenerateParticleId;
    private int _KernelResetNeighboursCounterId;
    private int _KernelUpdateNeighboursId;
    private int _KernelUpdateDensitiesId;
    private int _KernelPropagateDensityUpdateId;
    private int _KernelPropagateHeightUpdateId;
    private int _KernelPropagatePositionUpdateId;
    private int _KernelUpdateHeightsId;
    private int _KernelUpdateTemperaturesId;
    private int _KernelPropagateTemperatureUpdateId;
    private int _KernelUpdateVelocitiesId;
    private int _KernelPropagateVelocityUpdateId;
    private int _KernelTimeIntegrationId;
    private int _KernelUpdateTerrainHeightsId;
    private int _KernelUpdateTerrainDensitiesId;
    private int _KernelUpdateTerrainDensitiesStarId;
    private int _KernelGaussianBlurTerrainHeightsId;

    // compute shader buffers
    private int _ParticlesId;
    private int _HeightsId;
    private int _HeightsGradientsId;
    private int _PositionsId;
    private int _VolumesId;
    private int _DensitiesId;
    private int _MassesId;
    private int _TemperaturesId;
    private int _VelocitiesId;
    private ComputeBuffer _ParticlesBuffer;
    private ComputeBuffer _HeightsBuffer;
    private ComputeBuffer _HeightsGradientsBuffer;
    private ComputeBuffer _PositionsBuffer;
    private ComputeBuffer _VolumesBuffer;
    private ComputeBuffer _DensitiesBuffer;
    private ComputeBuffer _MassesBuffer;
    private ComputeBuffer _TemperaturesBuffer;
    private ComputeBuffer _VelocitiesBuffer;

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

    private int _NeighbourGridId;
    private int _NeighbourGridCellsId;
    private int _NeighbourGridCellsCounterId;
    private ComputeBuffer _NeighbourGridBuffer;
    private ComputeBuffer _NeighbourGridCellsBuffer;
    private ComputeBuffer _NeighbourGridCellsCounterBuffer;
    private float[] _NeighbourGridCells;
    private int[] _NeighbourGridCellsCounter;
    private int _NeighbourGridNbCols;
    private int _NeighbourGridNbLines;
    private int _NeighbourGridNbMaxNeighbours;

    private int _InitialTerrainHeightsId;
    private int _TerrainHeightsId;
    private int _TerrainHeightsTmpId;
    private ComputeBuffer _InitialTerrainHeightsBuffer;
    private ComputeBuffer _TerrainHeightsBuffer;
    private ComputeBuffer _TerrainHeightsTmpBuffer;
    private float[] _InitialTerrainHeights;
    private float[] _TerrainHeights;
    private float[] _TerrainHeightsTmp;

    private float _TerrainDensityMax;
    private float _TerrainDensityMin;
    private int _TerrainDensitiesId;
    private int _TerrainDensitiesRadiiId;
    private int _TerrainDensitiesHatId;
    private int _TerrainDensitiesStarId;
    private int _TerrainDensitiesMinMaxId;
    private ComputeBuffer _TerrainDensitiesBuffer;
    private ComputeBuffer _TerrainDensitiesRadiiBuffer;
    private ComputeBuffer _TerrainDensitiesHatBuffer;
    private ComputeBuffer _TerrainDensitiesStarBuffer;
    private ComputeBuffer _TerrainDensitiesMinMaxBuffer;
    private float[] _TerrainDensities;
    private float[] _TerrainDensitiesRadii;
    private float[] _TerrainDensitiesHat;
    private float[] _TerrainDensitiesStar;
    private Vector2[] _TerrainDensitiesMinMax;

    // future buffer data
    private float[]   _Heights;
    private Vector3[] _HeightsGradients;
    private Vector3[] _Positions;
    private Vector3[] _Velocities;
    private float[]   _Volumes;
    private float[]   _Densities;
    private float[]   _Masses;
    private float[]   _Temperatures;

    private ParticleGPU[] _Particles;

    private float _DT;
    private float _ElapsedTime;

    private float _Spike;
    private float _KernelRadius;
    private float _Stiffness;
    private float _ParticleInitialHeight;

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

    private Material _TerrainMaterial;
    private Mesh _TerrainMesh;
    private MeshFilter _TerrainMeshFilter;
    private MeshRenderer _TerrainRenderer;

    public void UpdateTweakableFields(){
        _DisplayParticles = _Fields._DisplayParticles;
        _DisplayLava = _Fields._DisplayLava;
        _GaussianBlur = _Fields._GaussianBlur;
        _NbMaxParticles = _Fields._NbMaxParticles;
        _InitialPositionDelta = _Fields._InitialPositionDelta;
        _DT = _Fields._DT;
        _Spike = _Fields._Spike;
        _KernelRadius = _Fields._KernelRadius;
        _Stiffness = _Fields._Stiffness;
        _ParticleInitialHeight = _Fields._ParticleInitialHeight;
        _TerrainDensityMax = _Fields._TerrainDensityMax;
        _TerrainDensityMin = _Fields._TerrainDensityMin;
        _Mu = _Fields._Mu;
        _Ke = _Fields._Ke;
        _ThetaE = _Fields._ThetaE;
    }

    public void Awake(){
        _ParticleDisplay = _Fields._ParticleDisplay;
        _TerrainMaterial = _Fields._TerrainMaterial;
        UpdateTweakableFields();
    }

    public void Update(){
        UpdateTweakableFields();
    }

    public int GetNbCurParticles(){
        return _NbCurParticles;
    }

    private void UpdateKernelFactors(){
        _AlphaPoly6              = 4.0f  / (Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
        _AlphaPoly6Laplacian     = 32.0f / (Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
        _AlphaViscosity          = 10.0f / (9.0f*Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
        _AlphaViscosityLaplacian = 40.0f / (Constants.PI*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius*_KernelRadius);
    }

    public void Create(ComputeShader shader, TerrainGenerator terrain){
        _NbCurParticles = 0;
        _Shader = shader;
        _ElapsedTime = 0.0f;
        Init(terrain);
    }

    private void InitKernelsIds(){
        // init functions id
        _KernelGenerateParticleId           = _Shader.FindKernel("GenerateParticle");
        _KernelResetNeighboursCounterId     = _Shader.FindKernel("ResetNeighboursCounter");
        _KernelUpdateNeighboursId           = _Shader.FindKernel("UpdateNeighbours");
        _KernelUpdateDensitiesId            = _Shader.FindKernel("UpdateDensities");
        _KernelUpdateHeightsId              = _Shader.FindKernel("UpdateHeights");

        _KernelUpdateTemperaturesId         = _Shader.FindKernel("UpdateTemperatures");
        _KernelPropagateTemperatureUpdateId = _Shader.FindKernel("PropagateTemperatureUpdate");
        _KernelUpdateVelocitiesId           = _Shader.FindKernel("UpdateVelocities");
        _KernelPropagateVelocityUpdateId    = _Shader.FindKernel("PropagateVelocityUpdate");

        _KernelTimeIntegrationId            = _Shader.FindKernel("TimeIntegration");
        _KernelUpdateTerrainHeightsId       = _Shader.FindKernel("UpdateTerrainHeights");
        _KernelUpdateTerrainDensitiesId     = _Shader.FindKernel("UpdateTerrainDensities");
        _KernelUpdateTerrainDensitiesStarId = _Shader.FindKernel("UpdateTerrainDensitiesStar");
        _KernelGaussianBlurTerrainHeightsId = _Shader.FindKernel("GaussianBlurTerrainHeights");
        _KernelPropagateDensityUpdateId     = _Shader.FindKernel("PropagateDensityUpdate");
        _KernelPropagateHeightUpdateId      = _Shader.FindKernel("PropagateHeightUpdate");
        _KernelPropagatePositionUpdateId    = _Shader.FindKernel("PropagatePositionUpdate");

    }

    private void InitStaggeredGridIds(){
        _StaggeredGridId            = Shader.PropertyToID("_StaggeredGrid");
        _StaggeredGridHeightsId     = Shader.PropertyToID("_StaggeredGridHeights");
        _StaggeredGridHalfHeightsId = Shader.PropertyToID("_StaggeredGridHalfHeights");
        _StaggeredGridGradientsId   = Shader.PropertyToID("_StaggeredGridGradients");
        _StaggeredGridLaplaciansId  = Shader.PropertyToID("_StaggeredGridLaplacians");
    }

    private void InitNeighbourGridIds(){
        _NeighbourGridId = Shader.PropertyToID("_NeighbourGrid");
        _NeighbourGridCellsId = Shader.PropertyToID("_NeighbourGridCells");
        _NeighbourGridCellsCounterId = Shader.PropertyToID("_NeighbourGridCellsCounter");
    }

    private void InitParticlesIds(){
        _NbCurParticlesId   = Shader.PropertyToID("_NbCurParticles");
        _NbNewParticlesId   = Shader.PropertyToID("_NbNewParticles");
        _NewPositionId      = Shader.PropertyToID("_NewPosition");
        _ParticleInitialHeightId = Shader.PropertyToID("_ParticleInitialHeightId");

        _ParticlesId        = Shader.PropertyToID("_Particles");
        _HeightsId          = Shader.PropertyToID("_Heights");
        _HeightsGradientsId = Shader.PropertyToID("_HeightsGradients");
        _PositionsId        = Shader.PropertyToID("_Positions");
        _VelocitiesId       = Shader.PropertyToID("_Velocities");
        _VolumesId          = Shader.PropertyToID("_Volumes");
        _DensitiesId        = Shader.PropertyToID("_Densities");
        _MassesId           = Shader.PropertyToID("_Masses");
        _TemperaturesId     = Shader.PropertyToID("_Temperatures");

        _RandId           = Shader.PropertyToID("RAND");

        _InitialTerrainHeightsId = Shader.PropertyToID("_InitialTerrainHeights");
        _TerrainHeightsId        = Shader.PropertyToID("_TerrainHeights");
        _TerrainHeightsTmpId     = Shader.PropertyToID("_TerrainHeightsTmp");

        _TerrainDensitiesId      = Shader.PropertyToID("_TerrainDensities");
        _TerrainDensitiesRadiiId = Shader.PropertyToID("_TerrainDensitiesRadii");
        _TerrainDensitiesHatId   = Shader.PropertyToID("_TerrainDensitiesHat");
        _TerrainDensitiesStarId  = Shader.PropertyToID("_TerrainDensitiesStar");
        _TerrainDensitiesMinMaxId  = Shader.PropertyToID("_TerrainDensitiesMinMax");
    }

    private void InitIds(){
        InitKernelsIds();
        InitParticlesIds();        
    }

    private void InitBuffersData(){
        _Heights          = new float[_NbMaxParticles];
        _HeightsGradients = new Vector3[_NbMaxParticles];
        _Positions        = new Vector3[_NbMaxParticles];
        _Velocities       = new Vector3[_NbMaxParticles];
        _Volumes          = new float[_NbMaxParticles];
        _Densities        = new float[_NbMaxParticles];
        _Masses           = new float[_NbMaxParticles];
        _Temperatures     = new float[_NbMaxParticles];

        _Particles = new ParticleGPU[_NbMaxParticles];
        _InitialTerrainHeights = Convert2dArray(StaggeredGridV2._Heights);
        _TerrainHeights = Convert2dArray(StaggeredGridV2._Heights);
        _TerrainHeightsTmp = Convert2dArray(StaggeredGridV2._Heights);
        
        _TerrainDensities = new float[StaggeredGridV2._NbCols * StaggeredGridV2._NbLines];
        _TerrainDensitiesRadii = new float[StaggeredGridV2._NbCols * StaggeredGridV2._NbLines];
        _TerrainDensitiesHat = new float[StaggeredGridV2._NbCols * StaggeredGridV2._NbLines];
        _TerrainDensitiesStar = new float[StaggeredGridV2._NbCols * StaggeredGridV2._NbLines];
        _TerrainDensitiesMinMax = new Vector2[StaggeredGridV2._NbCols * StaggeredGridV2._NbLines];
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
        _Shader.SetFloat("ALPHA", Constants.ALPHA);

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
        _Shader.SetBuffer(_KernelResetNeighboursCounterId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateNeighboursId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateDensitiesId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateHeightsId, id, buffer);

        _Shader.SetBuffer(_KernelUpdateTemperaturesId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateTemperatureUpdateId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateVelocitiesId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateVelocityUpdateId, id, buffer);

        _Shader.SetBuffer(_KernelTimeIntegrationId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateTerrainHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateTerrainDensitiesId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateTerrainDensitiesStarId, id, buffer);
        _Shader.SetBuffer(_KernelGaussianBlurTerrainHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateDensityUpdateId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateHeightUpdateId, id, buffer);
        _Shader.SetBuffer(_KernelPropagatePositionUpdateId, id, buffer);

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
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*14 + sizeof(int)*2);
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

    private ComputeBuffer SetData(NeighbourGridGPU[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*2 + sizeof(int)*3);
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

    private void InitNeighbourGridAttributes(){
        // init grid data
        NeighbourGridGPU[] gridArray = new NeighbourGridGPU[1];
        NeighbourGridGPU grid = new NeighbourGridGPU();
        grid._DeltaCols  = _KernelRadius*2;
        grid._DeltaLines = _KernelRadius*2;
        grid._NbCols     = (int)(_TerrainSize.x / grid._DeltaCols);
        grid._NbLines    = (int)(_TerrainSize.z / grid._DeltaLines);
        grid._NbMaxNeighbours = _NbMaxParticles > 8192 ? 8192 : _NbMaxParticles;

        _NeighbourGridNbCols = grid._NbCols;
        _NeighbourGridNbLines = grid._NbLines;
        _NeighbourGridNbMaxNeighbours = grid._NbMaxNeighbours;
        // Debug.Log("nbCol: " + _NeighbourGridNbCols 
        //         + ", nbLines: " + _NeighbourGridNbLines
        //         + ", deltaCols: " + grid._DeltaCols
        //         + ", deltaLines: " + grid._DeltaLines
        //         + ", nbMax: " + _NeighbourGridNbMaxNeighbours
        // );
        
        gridArray[0] = grid;
        
        // init grid buffer
        _NeighbourGridBuffer = SetData(gridArray, _NeighbourGridId);
    }

    private void InitNeighbourGrid(){
        InitNeighbourGridIds();
        InitNeighbourGridAttributes();
        _NeighbourGridCells = new float[_NeighbourGridNbCols*_NeighbourGridNbLines*_NeighbourGridNbMaxNeighbours];
        _NeighbourGridCellsCounter = new int[_NeighbourGridNbCols*_NeighbourGridNbLines];
        _NeighbourGridCellsBuffer = SetData(_NeighbourGridCells, _NeighbourGridCellsId);
        _NeighbourGridCellsCounterBuffer = SetData(_NeighbourGridCellsCounter, _NeighbourGridCellsCounterId);
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
        _VelocitiesBuffer       = SetData(_Velocities, _VelocitiesId);
        _HeightsBuffer          = SetData(_Heights, _HeightsId);
        _HeightsGradientsBuffer = SetData(_HeightsGradients, _HeightsGradientsId);
        _VolumesBuffer          = SetData(_Volumes, _VolumesId);
        _DensitiesBuffer        = SetData(_Densities, _DensitiesId);
        _MassesBuffer           = SetData(_Masses, _MassesId);
        _TemperaturesBuffer     = SetData(_Temperatures, _TemperaturesId);

        _InitialTerrainHeightsBuffer = SetData(_InitialTerrainHeights, _InitialTerrainHeightsId);
        _TerrainHeightsBuffer        = SetData(_TerrainHeights, _TerrainHeightsId);
        _TerrainHeightsTmpBuffer     = SetData(_TerrainHeightsTmp, _TerrainHeightsTmpId);

        _TerrainDensitiesBuffer       = SetData(_TerrainDensities, _TerrainDensitiesId);
        _TerrainDensitiesRadiiBuffer  = SetData(_TerrainDensitiesRadii, _TerrainDensitiesRadiiId);
        _TerrainDensitiesHatBuffer    = SetData(_TerrainDensitiesHat, _TerrainDensitiesHatId);
        _TerrainDensitiesStarBuffer   = SetData(_TerrainDensitiesStar, _TerrainDensitiesStarId);
        _TerrainDensitiesMinMaxBuffer = SetData(_TerrainDensitiesMinMax, _TerrainDensitiesMinMaxId);
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
        InitNeighbourGrid();
        InitMesh();
    }

    private void GenerateParticle(Vector3 position){
        _ElapsedTime += Time.deltaTime;

        if(_NbCurParticles < _NbMaxParticles && _ElapsedTime > _DT){
            int delta = (int)(_ElapsedTime / _DT);
            delta = Double.IsNaN(delta) ? 10 : delta;
            int cpt = 0;

            // put new particles in buffer
            for(int i=0; i<delta; i++){
                if(_NbCurParticles >= _NbMaxParticles) break;
                _NbCurParticles++;
                cpt++;
            }

            // update gpu side
            int res = _NbMaxParticles / 128 + 1;
            // _Shader.SetFloat("DT", Time.deltaTime);
            _Shader.SetInt(_NbNewParticlesId, cpt);
            _Shader.SetInt(_NbCurParticlesId, _NbCurParticles);
            _Shader.SetVector(_NewPositionId, position);

            _ElapsedTime -= (delta*_DT);
            _Shader.Dispatch(_KernelGenerateParticleId, res, 1, 1);
        }
    }

    private void UpdateHeights(int res){
        _Shader.Dispatch(_KernelUpdateHeightsId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateHeightUpdateId, res, 1, 1);
    }

    private void TimeIntegration(int res){
        // _Shader.SetFloat("DT", Time.deltaTime);
        _Shader.Dispatch(_KernelTimeIntegrationId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagatePositionUpdateId, res, 1, 1);
    }

    private void UpdateTerrainHeights(){
        int res = (_TerrainNbCols*_TerrainNbLines / 1024) + 1;
        _Shader.Dispatch(_KernelUpdateTerrainDensitiesId, res, 1, 1);
        _Shader.Dispatch(_KernelUpdateTerrainDensitiesStarId, res, 1, 1);
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
        _Shader.SetFloat(_RandId, GetRandomValue(_InitialPositionDelta));
        _Shader.SetFloat(_ParticleInitialHeightId, _ParticleInitialHeight);

        _Shader.SetFloat("_TerrainDensityMax", _TerrainDensityMax);
        _Shader.SetFloat("_TerrainDensityMin", _TerrainDensityMin);

        _Shader.SetBool("_DisplayLava", _DisplayLava);

        _Shader.SetFloat("_Mu", _Mu);
        _Shader.SetFloat("_Ke", _Ke);
        _Shader.SetFloat("_ThetaE", _ThetaE);
    }

    private void UpdateVolumes(int res){
        _Shader.Dispatch(_KernelUpdateDensitiesId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateDensityUpdateId, res, 1, 1);
    }

    private void DebugParticles(){
        _ParticlesBuffer.GetData(_Particles);
        for(int i=0; i<_NbCurParticles; i++){
            ParticleGPU p = _Particles[i];
            Debug.Log(
                "pos: " + p._Position +
                ", h: " + p._Height +
                ", hGrad: " + p._HeightGradient +
                ", dens: " + p._Density +
                ", mass: " + p._Mass +
                ", vol: " + p._Volume +
                ", cell: " + p._Cell +
                ", id: " + p._Id
            );
        }
    }

    private void DebugNeighbours(){
    _NeighbourGridCellsCounterBuffer.GetData(_NeighbourGridCellsCounter);
        for(int j=0; j<_NeighbourGridNbLines; j++){
            for(int i=0; i<_NeighbourGridNbCols; i++){
                int idx = j*_NeighbourGridNbCols + i;
                if(_NeighbourGridCellsCounter[idx]!=0){
                    Debug.Log("Count["+idx+"] = "+_NeighbourGridCellsCounter[idx]);
                }
            }
        }
    }

    private void UpdateNeighbours(){
        int res = (_NeighbourGridNbCols*_NeighbourGridNbLines / 1024) + 1;
        _Shader.Dispatch(_KernelResetNeighboursCounterId, res, 1, 1);
        _Shader.Dispatch(_KernelUpdateNeighboursId, res, 1, 1);
        // DebugParticles();  
        // DebugNeighbours();      
    }

    private void UpdateTemperatures(int res){
        _Shader.Dispatch(_KernelUpdateTemperaturesId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateTemperatureUpdateId, res, 1, 1);
    }

    private void UpdateVelocities(int res){
        _Shader.Dispatch(_KernelUpdateVelocitiesId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateVelocityUpdateId, res, 1, 1);
    }

    public void Updt(Vector3 position){
        UpdateGPUValues();        
        int res = (_NbMaxParticles / 128)+1;
        // generate particle in GPU
        GenerateParticle(position);
        // update the neighbours
        UpdateNeighbours();
        // calculate heights in gpu
        UpdateHeights(res);
        // update densities
        // UpdateVolumes(res);
        UpdateTemperatures(res);
        UpdateVelocities(res);
        // time integration gpu
        TimeIntegration(res);
        // update the terrain heights
        UpdateTerrainHeights();
        // display particles
        UpdateParticleMesh();
    }

    private void ReleaseBuffers(){
        _ParticlesBuffer.Dispose();
        _HeightsBuffer.Dispose();
        _HeightsGradientsBuffer.Dispose();
        _PositionsBuffer.Dispose();
        _VelocitiesBuffer.Dispose();
        _VolumesBuffer.Dispose();
        _DensitiesBuffer.Dispose();
        _MassesBuffer.Dispose();
        _TemperaturesBuffer.Dispose();

        _InitialTerrainHeightsBuffer.Dispose();
        _TerrainHeightsBuffer.Dispose();
        _TerrainHeightsTmpBuffer.Dispose();

        _TerrainDensitiesBuffer.Dispose();
        _TerrainDensitiesRadiiBuffer.Dispose();
        _TerrainDensitiesHatBuffer.Dispose();
        _TerrainDensitiesStarBuffer.Dispose();
        _TerrainDensitiesMinMaxBuffer.Dispose();

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
        // if(_DisplayParticles){
            // Debug.Log("Particles mesh initialized");
            _ParticleDisplay.InitMesh(_PositionsBuffer);
        // }
    }

    private void InitMesh(){
        InitTerrainMesh();
        InitParticlesMesh();        
    }

    public void UpdateParticleMesh(){
        _ParticleDisplay.UpdateParticleMesh(_NbCurParticles, _DisplayParticles);
        _ParticleDisplay.UpdateParticleHeight();
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

    private float GetRandomValue(float delta){
        float v = UnityEngine.Random.value*2*delta - delta;
        return v;
    }

}