using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * The structure matching the compute shader representation of the staggered grid
*/
public struct StaggeredGridGPU{
    public int _NbCols, _NbLines;
    public float _DeltaCols, _DeltaLines;    
};

/**
 * The structure matching the compute shader representation of the neighbour grid
*/
public struct NeighbourGridGPU{
    public int _NbCols, _NbLines;
    public float _DeltaCols, _DeltaLines;    
    public int _NbMaxNeighbours;
};

/**
 * The structure matching the compute shader representation of a particle
*/
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
 * The main class exchanging with the sph solver in the compute shader
*/
public class ParticleSPHGPU : MonoBehaviour{

// ################################################################################################################################################################################################################
// ################################################################################################## ATTRIBUTES ##################################################################################################
// ################################################################################################################################################################################################################

    /**
     * The class used to regroup all the serializied fields
    */
    [SerializeField]
    public Tweakable _Fields;

    /**
     * Boolean to tell if particles should be visible or not
    */
    private bool _DisplayParticles;

    /**
     * Boolean to tell if lava should be visible or not
    */
    private bool _DisplayLava;

    /**
     * The class containing the particle mesh
    */
    private ParticleDisplay _ParticleDisplay;

    /**
     * Boolean to tell if the terrain should be smoothed
    */
    private bool _GaussianBlur;

    /**
     * The maximum number of particles
    */
    private int _NbMaxParticles;
    
    /**
     * The current number of particles
    */
    private int _NbCurParticles;
    
    /**
     * The radius of the circle within which particles can spawn
    */
    private float _InitialPositionDelta;
    
    /**
     * The lava viscosity constant
    */
    private float _Mu;
    
    /**
     * The lava initial viscosity constant
    */
    private float _Ke;
    
    /**
     * The lava initial temperature constant
    */
    private float _ThetaE;
    
    
    /**
     * The shade of the lava color
    */
    private float _ColorShade;
    
    /**
     * The compute shader containing the core of our simulation
    */
    private ComputeShader _Shader;
    
    /**
     * Id for the number of current particle variable inside the GPU
    */
    private int _NbCurParticlesId;

    /**
     * Id for the number of new particles variable inside the GPU
    */
    private int _NbNewParticlesId;

    /**
     * Id for the new position variable inside the GPU
    */
    private int _NewPositionId;

    /**
     * Id for the random float variable inside the GPU
    */
    private int _RandId;

    /**
     * The delta time for the simulation
    */
    private float _DT;

    /**
     * The current time elapsed since last update
    */
    private float _ElapsedTime;

    /**
     * A variable to elevate of decrease the lava height when rendering
    */
    private float _Spike;

    /**
     * The kernel radius
    */
    private float _KernelRadius;

    // ##########################################
    // ######## Compute Shader Functions ########
    // ##########################################
    private int _KernelGenerateParticleId;
    private int _KernelResetNeighboursCounterId;
    private int _KernelUpdateNeighboursId;
    private int _KernelAssignNeighboursId;
    private int _KernelPropagateHeightUpdateId;
    private int _KernelPropagatePositionUpdateId;
    private int _KernelUpdateHeightsId;
    private int _KernelUpdateTemperaturesId;
    private int _KernelPropagateTemperatureUpdateId;
    private int _KernelUpdateVelocitiesId;
    private int _KernelPropagateVelocityUpdateId;
    private int _KernelTimeIntegrationId;
    private int _KernelUpdateTerrainHeightsId;
    private int _KernelUpdateTerrainTemperaturesId;
    private int _KernelGaussianBlurTerrainHeightsId;

    // ##########################################
    // ############ Particle Buffers ############
    // ##########################################
    private int _ParticlesId;
    private int _HeightsId;
    private int _HeightsGradientsId;
    private int _PositionsId;
    private int _TemperaturesId;
    private int _VelocitiesId;
    private int _ParticleNeighboursId;
    private ComputeBuffer _ParticlesBuffer;
    private ComputeBuffer _HeightsBuffer;
    private ComputeBuffer _HeightsGradientsBuffer;
    private ComputeBuffer _PositionsBuffer;
    private ComputeBuffer _TemperaturesBuffer;
    private ComputeBuffer _VelocitiesBuffer;
    private ComputeBuffer _ParticleNeighboursBuffer;
    private ParticleGPU[] _Particles;
    private float[]   _Heights;
    private Vector3[] _HeightsGradients;
    private Vector3[] _Positions;
    private float[]   _Temperatures;
    private Vector3[] _Velocities;
    private int[]     _ParticleNeighbours;

    // ##########################################
    // ######### Staggered Grid Buffers #########
    // ##########################################
    private int _StaggeredGridId;
    private int _StaggeredGridHeightsId;
    private int _StaggeredGridHalfHeightsId;
    private int _StaggeredGridGradientsId;
    private ComputeBuffer _StaggeredGridBuffer;
    private ComputeBuffer _StaggeredGridHeightsBuffer;
    private ComputeBuffer _StaggeredGridHalfHeightsBuffer;
    private ComputeBuffer _StaggeredGridGradientsBuffer;

    // ##########################################
    // ######### Neighbors Grid Buffers #########
    // ##########################################
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

    // ##########################################
    // ############ Terrain Buffers #############
    // ##########################################
    private int _InitialTerrainHeightsId;
    private int _TerrainHeightsId;
    private int _TerrainTemperaturesId;
    private ComputeBuffer _InitialTerrainHeightsBuffer;
    private ComputeBuffer _TerrainHeightsBuffer;
    private ComputeBuffer _TerrainTemperaturesBuffer;
    private float[] _InitialTerrainHeights;
    private float[] _TerrainHeights;
    private float[] _TerrainTemperatures;

    // ##########################################
    // ############# Terrain Values #############
    // ##########################################
    private int _TerrainNbCols;
    private int _TerrainNbLines;
    private float _TerrainDeltaCols;
    private float _TerrainDeltaLines;
    private Vector3 _TerrainSize;

    // ##########################################
    // ############ Terrain Material ############
    // ##########################################
    private Material _TerrainMaterial;
    private Mesh _TerrainMesh;
    private MeshFilter _TerrainMeshFilter;
    private MeshRenderer _TerrainRenderer;


// ################################################################################################################################################################################################################
// ########################################################################################### Initialization Methods #############################################################################################
// ################################################################################################################################################################################################################

    /**
     * Initiate the serialized fields
    */
    public void Awake(){
        _ParticleDisplay = _Fields._ParticleDisplay;
        _TerrainMaterial = _Fields._TerrainMaterial;
        _GaussianBlur = _Fields._GaussianBlur;
        _NbMaxParticles = _Fields._NbMaxParticles;
        UpdateTweakableFields();
    }

    /**
     * Initiate the class
     * @param shader The core of the sph simulation
     * @param terrain The heightmap of the terrain
    */
    public void Create(ComputeShader shader, TerrainGenerator terrain){
        _NbCurParticles = 0;
        _Shader = shader;
        _ElapsedTime = 0.0f;
        Init(terrain);
    }

    /**
     * Initiate the kernels ids
    */
    private void InitKernelsIds(){
        // kernels for particles updates
        _KernelGenerateParticleId           = _Shader.FindKernel("GenerateParticle");
        _KernelTimeIntegrationId            = _Shader.FindKernel("TimeIntegration");
        _KernelUpdateHeightsId              = _Shader.FindKernel("UpdateHeights");
        _KernelUpdateTemperaturesId         = _Shader.FindKernel("UpdateTemperatures");
        _KernelUpdateVelocitiesId           = _Shader.FindKernel("UpdateVelocities");

        // kernels for neighbours functions
        _KernelResetNeighboursCounterId     = _Shader.FindKernel("ResetNeighboursCounter");
        _KernelUpdateNeighboursId           = _Shader.FindKernel("UpdateNeighbours");
        _KernelAssignNeighboursId           = _Shader.FindKernel("AssignNeighbours");
        
        // kernels for terrain updates
        _KernelUpdateTerrainHeightsId       = _Shader.FindKernel("UpdateTerrainHeights");
        _KernelUpdateTerrainTemperaturesId  = _Shader.FindKernel("UpdateTerrainTemperatures");
        _KernelGaussianBlurTerrainHeightsId = _Shader.FindKernel("GaussianBlurTerrainHeights");

        // kernels for updates propagations
        _KernelPropagateHeightUpdateId      = _Shader.FindKernel("PropagateHeightUpdate");
        _KernelPropagatePositionUpdateId    = _Shader.FindKernel("PropagatePositionUpdate");
        _KernelPropagateVelocityUpdateId    = _Shader.FindKernel("PropagateVelocityUpdate");
        _KernelPropagateTemperatureUpdateId = _Shader.FindKernel("PropagateTemperatureUpdate");
    }

    /**
     * Init the staggered grid buffers' ids
    */
    private void InitStaggeredGridIds(){
        _StaggeredGridId            = Shader.PropertyToID("_StaggeredGrid");
        _StaggeredGridHeightsId     = Shader.PropertyToID("_StaggeredGridHeights");
        _StaggeredGridHalfHeightsId = Shader.PropertyToID("_StaggeredGridHalfHeights");
        _StaggeredGridGradientsId   = Shader.PropertyToID("_StaggeredGridGradients");
    }

    /**
     * Init the neighbours grid buffers' ids
    */
    private void InitNeighbourGridIds(){
        _NeighbourGridId = Shader.PropertyToID("_NeighbourGrid");
        _NeighbourGridCellsId = Shader.PropertyToID("_NeighbourGridCells");
        _NeighbourGridCellsCounterId = Shader.PropertyToID("_NeighbourGridCellsCounter");
    }

    /**
     * Init the variables' ids
    */
    private void InitVariablesIds(){
        _NbCurParticlesId   = Shader.PropertyToID("_NbCurParticles");
        _NbNewParticlesId   = Shader.PropertyToID("_NbNewParticles");
        _NewPositionId      = Shader.PropertyToID("_NewPosition");
        _RandId           = Shader.PropertyToID("RAND");
    }

    /**
     * Init the terrain buffers' ids
    */
    private void InitTerrainIds(){
        _InitialTerrainHeightsId = Shader.PropertyToID("_InitialTerrainHeights");
        _TerrainHeightsId        = Shader.PropertyToID("_TerrainHeights");
        _TerrainTemperaturesId   = Shader.PropertyToID("_TerrainTemperatures");
    }

    /**
     * Init the particle buffers' ids
    */
    private void InitParticlesIds(){
        _ParticlesId        = Shader.PropertyToID("_Particles");
        _HeightsId          = Shader.PropertyToID("_Heights");
        _HeightsGradientsId = Shader.PropertyToID("_HeightsGradients");
        _PositionsId        = Shader.PropertyToID("_Positions");
        _VelocitiesId       = Shader.PropertyToID("_Velocities");
        _TemperaturesId     = Shader.PropertyToID("_Temperatures");
        _ParticleNeighboursId = Shader.PropertyToID("_ParticleNeighbours");
    }

    /**
     * Init all the GPU ids
    */
    private void InitIds(){
        InitKernelsIds();
        InitParticlesIds();      
        InitStaggeredGridIds();
        InitNeighbourGridIds();
        InitTerrainIds();
        InitVariablesIds();  
    }

    /**
     * Init terrain buffers data
    */
    private void InitTerrainData(){
        _InitialTerrainHeights = Convert2dArray(StaggeredGridV2._Heights);
        _TerrainHeights = Convert2dArray(StaggeredGridV2._Heights);
        _TerrainTemperatures = new float[StaggeredGridV2._NbCols * StaggeredGridV2._NbLines];
    }

    /**
     * Init particles' buffers data
    */
    private void InitParticlesData(){
        _Particles        = new ParticleGPU[_NbMaxParticles];
        _Heights          = new float[_NbMaxParticles];
        _HeightsGradients = new Vector3[_NbMaxParticles];
        _Temperatures     = new float[_NbMaxParticles];
        _Velocities       = new Vector3[_NbMaxParticles];
        _Positions        = new Vector3[_NbMaxParticles];
    }

    /**
     * Send constants to gpu
    */
    private void SendConstantsToGPU(){
        _Shader.SetInt("MAX_PARTICLES", _NbMaxParticles);
        _Shader.SetFloat("PI", Constants.PI);
        _Shader.SetFloat("G", Constants.G);
        _Shader.SetFloat("RHO_0", Constants.RHO_0);
        _Shader.SetFloat("ALPHA", Constants.ALPHA);
    }

    /**
     * Init the values inside the GPU
    */
    private void InitGpuValues(){
        SendConstantsToGPU();
        _Shader.SetBool("FirstTimeBlur", true);
        _Shader.SetBool("GaussianBlur", _GaussianBlur);
        UpdateGPUValues();        
    }

    /**
     * Send the buffers to all kernel functions
    */
    private void SendDataToAllKernels(ComputeBuffer buffer, int id){
        // kernels for particles updates
        _Shader.SetBuffer(_KernelGenerateParticleId, id, buffer);
        _Shader.SetBuffer(_KernelTimeIntegrationId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateTemperaturesId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateVelocitiesId, id, buffer);

        // kernels for neighbours functions
        _Shader.SetBuffer(_KernelResetNeighboursCounterId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateNeighboursId, id, buffer);
        _Shader.SetBuffer(_KernelAssignNeighboursId, id, buffer);
        
        // kernels for terrain updates
        _Shader.SetBuffer(_KernelUpdateTerrainHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateTerrainTemperaturesId, id, buffer);
        _Shader.SetBuffer(_KernelGaussianBlurTerrainHeightsId, id, buffer);

        // kernels for updates propagations
        _Shader.SetBuffer(_KernelPropagateHeightUpdateId, id, buffer);
        _Shader.SetBuffer(_KernelPropagatePositionUpdateId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateVelocityUpdateId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateTemperatureUpdateId, id, buffer);
    }

    /**
     * Init the staggered grid attributes
    */
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

    /**
     * Init the neighbours grid attributes
    */
    private void InitNeighbourGridAttributes(){
        // init grid data
        NeighbourGridGPU[] gridArray = new NeighbourGridGPU[1];
        NeighbourGridGPU grid = new NeighbourGridGPU();
        grid._DeltaCols  = _KernelRadius*2;
        grid._DeltaLines = _KernelRadius*2;
        grid._NbCols     = (int)(_TerrainSize.x / grid._DeltaCols);
        grid._NbLines    = (int)(_TerrainSize.z / grid._DeltaLines);
        grid._NbMaxNeighbours = _NbMaxParticles > 1024 ? 1024 : _NbMaxParticles;

        _NeighbourGridNbCols = grid._NbCols;
        _NeighbourGridNbLines = grid._NbLines;
        _NeighbourGridNbMaxNeighbours = grid._NbMaxNeighbours;
        
        gridArray[0] = grid;
        
        // init grid buffer
        _NeighbourGridBuffer = SetData(gridArray, _NeighbourGridId);
    }

    /**
     * Init the neighbours grid for the gpu
    */
    private void InitNeighbourGrid(){
        InitNeighbourGridAttributes();
        _NeighbourGridCells = new float[_NeighbourGridNbCols*_NeighbourGridNbLines*_NeighbourGridNbMaxNeighbours];
        _NeighbourGridCellsCounter = new int[_NeighbourGridNbCols*_NeighbourGridNbLines];
        _NeighbourGridCellsBuffer = SetData(_NeighbourGridCells, _NeighbourGridCellsId);
        _NeighbourGridCellsCounterBuffer = SetData(_NeighbourGridCellsCounter, _NeighbourGridCellsCounterId);

        _ParticleNeighbours = new int[_NbMaxParticles*_NeighbourGridNbMaxNeighbours];
        _ParticleNeighboursBuffer = SetData(_ParticleNeighbours, _ParticleNeighboursId);
    }

    /**
     * Init the staggered grid for the gpu
    */
    private void InitStaggeredGrid(){
        // init grid itself
        InitStaggeredGridAttributes();
        // init grid data
        float[] heights     = Convert2dArray(StaggeredGridV2._Heights);
        float[] halfHeights = Convert2dArray(StaggeredGridV2._HalfHeights);
        Vector2[] gradients = Convert2dArray(StaggeredGridV2._Gradients);
        // init grid buffers
        _StaggeredGridHeightsBuffer     = SetData(heights, _StaggeredGridHeightsId);
        _StaggeredGridHalfHeightsBuffer = SetData(halfHeights, _StaggeredGridHalfHeightsId);
        _StaggeredGridGradientsBuffer   = SetData(gradients, _StaggeredGridGradientsId);
    }

    /**
     * Init the terrain for the gpu
    */
    private void InitTerrain(){
        InitTerrainData();
        _InitialTerrainHeightsBuffer = SetData(_InitialTerrainHeights, _InitialTerrainHeightsId);
        _TerrainHeightsBuffer        = SetData(_TerrainHeights, _TerrainHeightsId);
        _TerrainTemperaturesBuffer   = SetData(_TerrainTemperatures, _TerrainTemperaturesId);
    }

    /**
     * Init the particles for the gpu
    */
    private void InitParticles(){
        InitParticlesData();
        _ParticlesBuffer        = SetData(_Particles, _ParticlesId);
        _HeightsBuffer          = SetData(_Heights, _HeightsId);
        _HeightsGradientsBuffer = SetData(_HeightsGradients, _HeightsGradientsId);
        _TemperaturesBuffer     = SetData(_Temperatures, _TemperaturesId);
        _PositionsBuffer        = SetData(_Positions, _PositionsId);
        _VelocitiesBuffer       = SetData(_Velocities, _VelocitiesId);
    }

    private void InitGPU(){
        InitGpuValues();
        InitTerrain();
        InitParticles();
        InitNeighbourGrid();
        InitStaggeredGrid();
    }

    /**
     * Get the terrain informations from the heightmap
     * @param terrain The heightmap of the terrain
    */
    private void GetTerrainValues(TerrainGenerator terrain){
        _TerrainSize    = terrain._Size;
        _TerrainNbCols  = terrain.GetResolution();
        _TerrainNbLines = terrain.GetResolution();

        Vector3 terrainSize = terrain._Size;
        _TerrainDeltaCols  = terrainSize.x / _TerrainNbCols;
        _TerrainDeltaLines = terrainSize.z / _TerrainNbLines;
    }

    /**
     * Initiate everything
     * @param terrain The heightmap of the terrain
    */
    private void Init(TerrainGenerator terrain){
        GetTerrainValues(terrain);
        InitIds();
        InitGPU();
        InitMesh();
    }


// ################################################################################################################################################################################################################
// ############################################################################################### Update Methods #################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Update the serialized fields
    */
    public void UpdateTweakableFields(){
        _DisplayParticles = _Fields._DisplayParticles;
        _DisplayLava = _Fields._DisplayLava;
        _InitialPositionDelta = _Fields._InitialPositionDelta;
        _DT = _Fields._DT;
        _Spike = _Fields._Spike;
        _KernelRadius = _Fields._KernelRadius;
        _Mu = _Fields._Mu;
        _Ke = _Fields._Ke;
        _ThetaE = _Fields._ThetaE;
        _ColorShade = _Fields._ColorShade;
    }

    /**
     * Update GPU variables
    */
    private void UpdateGPUValues(){
        _Shader.SetFloat("DT", _DT);
        _Shader.SetFloat("H", _KernelRadius);
        _Shader.SetFloat("SPIKE", _Spike);
        _Shader.SetFloat(_RandId, GetRandomValue(_InitialPositionDelta));

        _Shader.SetBool("_DisplayLava", _DisplayLava);

        _Shader.SetFloat("_Mu", _Mu);
        _Shader.SetFloat("_Ke", _Ke);
        _Shader.SetFloat("_ThetaE", _ThetaE);

        _TerrainMaterial.SetFloat("_ColorShade", _ColorShade);
    }

    /**
     * Update the particles' heights
     * @param res The number of threads for the GPU kernel
    */
    private void UpdateHeights(int res){
        _Shader.Dispatch(_KernelUpdateHeightsId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateHeightUpdateId, res, 1, 1);
    }

    /**
     * Update the particles' positions
     * @param res The number of threads for the GPU kernel
    */
    private void TimeIntegration(int res){
        _Shader.Dispatch(_KernelTimeIntegrationId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagatePositionUpdateId, res, 1, 1);
    }

    /**
     * Update the terrain heights
    */
    private void UpdateTerrainHeights(){
        int res = (_TerrainNbCols*_TerrainNbLines / 1024) + 1;
        _Shader.Dispatch(_KernelUpdateTerrainTemperaturesId, res, 1, 1);
        _Shader.Dispatch(_KernelUpdateTerrainHeightsId, res, 1, 1);
        // smooth the terrain once if user wants to
        if(_GaussianBlur){
            _Shader.Dispatch(_KernelGaussianBlurTerrainHeightsId, res, 1, 1);
            _Shader.SetBool("FirstTimeBlur", false);
        }
    }

    /**
     * Update the particles' neighbours
     * @param res The number of threads for the GPU kernel
    */
    private void UpdateNeighbours(int res){
        int res2 = (_NeighbourGridNbCols*_NeighbourGridNbLines / 1024) + 1;
        _Shader.Dispatch(_KernelResetNeighboursCounterId, res2, 1, 1);
        _Shader.Dispatch(_KernelUpdateNeighboursId, res2, 1, 1);
        _Shader.Dispatch(_KernelAssignNeighboursId, res, 1, 1);
        // DebugParticles();  
        // DebugNeighbours();      
    }

    /**
     * Update the particles' temperatures
     * @param res The number of threads for the GPU kernel
    */
    private void UpdateTemperatures(int res){
        _Shader.Dispatch(_KernelUpdateTemperaturesId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateTemperatureUpdateId, res, 1, 1);
    }

    /**
     * Update the particles' velocities
     * @param res The number of threads for the GPU kernel
    */
    private void UpdateVelocities(int res){
        _Shader.Dispatch(_KernelUpdateVelocitiesId, res, 1, 1);
        _Shader.Dispatch(_KernelPropagateVelocityUpdateId, res, 1, 1);
    }

    /**
     * Generate new particles
     * @param position The new position of the particles (random delta is then added inside the GPU to not have them spawn at the same place everytime)
     * @param nbNewParticles The number of new particles
    */
    private void GenerateParticle(Vector3 position, int nbNewParticles){
        if(_NbCurParticles < _NbMaxParticles){
            int cpt = 0;

            // put new particles in buffer
            for(int i=0; i<nbNewParticles; i++){
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
            _Shader.Dispatch(_KernelGenerateParticleId, res, 1, 1);
        }
    }

    /**
     * The main infinite loop of the simulation
     * @param position The new position for the particles
    */
    public void Updt(Vector3 position){
        UpdateTweakableFields();
        UpdateGPUValues();
        int res = (_NbMaxParticles / 128)+1;
        _ElapsedTime += Time.deltaTime;

        if(_ElapsedTime > _DT){
            int delta = (int)(_ElapsedTime / _DT);
            delta = Double.IsNaN(delta) ? 10 : delta;
            // generate particles in GPU
            GenerateParticle(position, delta);
            // update the neighbours
            UpdateNeighbours(res);
            // update particles' attributes
            UpdateHeights(res);
            UpdateTemperatures(res);
            UpdateVelocities(res);
            TimeIntegration(res);
            // update values for the rendering part
            UpdateTerrainHeights();
            UpdateParticleMesh();

            _ElapsedTime -= (delta*_DT);
        }
    }

    /**
     * Release all the buffers to save memory space
    */
    private void ReleaseBuffers(){
        _ParticlesBuffer.Dispose();
        _HeightsBuffer.Dispose();
        _HeightsGradientsBuffer.Dispose();
        _PositionsBuffer.Dispose();
        _VelocitiesBuffer.Dispose();
        _TemperaturesBuffer.Dispose();
        _ParticleNeighboursBuffer.Dispose();

        _InitialTerrainHeightsBuffer.Dispose();
        _TerrainHeightsBuffer.Dispose();
        _TerrainTemperaturesBuffer.Dispose();

        _StaggeredGridBuffer.Dispose();
        _StaggeredGridHeightsBuffer.Dispose();
        _StaggeredGridHalfHeightsBuffer.Dispose();
        _StaggeredGridGradientsBuffer.Dispose();
    }

    /**
     * Call when leaving the application
    */
    public void OnApplicationQuit(){
        // release buffers
        ReleaseBuffers();
    }

    /**
     * Set buffer for the terrain material
    */
    private void SetMaterialBuffers(){
        _TerrainMaterial.SetBuffer("_TerrainHeights", _TerrainHeightsBuffer);
        _TerrainMaterial.SetBuffer("_InitialTerrainHeights", _InitialTerrainHeightsBuffer);
        _TerrainMaterial.SetBuffer("_TerrainTemperatures", _TerrainTemperaturesBuffer);
    }

    /**
     * Initiate the terrain mesh
    */
    private void InitTerrainMesh(){
        _TerrainMesh = new Mesh();
        _TerrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _TerrainMeshFilter = gameObject.AddComponent<MeshFilter>();
        _TerrainMeshFilter.mesh = _TerrainMesh;
        _TerrainRenderer = gameObject.AddComponent<MeshRenderer>();
        _TerrainRenderer.material = _TerrainMaterial;
        UpdateTerrainMesh();
    }

    /**
     * Init the particle mesh
    */
    private void InitParticlesMesh(){
        _ParticleDisplay.InitMesh(_PositionsBuffer, _TemperaturesBuffer);
    }

    /**
     * Init all the meshes
    */
    private void InitMesh(){
        InitTerrainMesh();
        InitParticlesMesh();        
    }

    /**
     * Update the particle mesh
    */
    public void UpdateParticleMesh(){
        _ParticleDisplay.UpdateParticleMesh(_NbCurParticles, _DisplayParticles);
        _ParticleDisplay.UpdateParticleHeight();
    }

    /**
     * Update the terrain mesh
    */
    private void UpdateTerrainMesh(){
        SetMaterialBuffers();
        TerrainSetVertices();
        TerrainSetIndices();
        TerrainSetUVs();
        _TerrainMesh.UploadMeshData(false);
    }

    /**
     * Init the terrain uvs
    */
    private void TerrainSetUVs(){
        Vector2[] uvs = new Vector2[_TerrainNbLines*_TerrainNbCols];
        for(int j=1; j<_TerrainNbLines; j++){
            for(int i=1; i<_TerrainNbCols; i++){
                int idx = i + j*_TerrainNbCols;
                uvs[idx].x = i / (1.0f*_TerrainNbLines);
                uvs[idx].y = j / (1.0f*_TerrainNbCols);
            }
        }
        _TerrainMesh.SetUVs(0, uvs);
    }

    /**
     * Init the terrain vertices
    */
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

    /**
     * Init the terrain indices
    */
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

    /**
     * Get a random value
     * @param delta The value is between 0 and 2*delta
     * @return The random floating point value
    */
    private float GetRandomValue(float delta){
        float v = UnityEngine.Random.value*2*delta - delta;
        return v;
    }


// ################################################################################################################################################################################################################
// ############################################################################################## Usefull Functions ###############################################################################################
// ################################################################################################################################################################################################################

    /**
     * Convert a float 2D array into a 1D array
     * @param arr The 2D array
     * @return The 1D array
    */
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

    /**
     * Convert a vector2 2D array into a 1D array
     * @param arr The 2D array
     * @return The 1D array
    */
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

    /**
     * Convert a vector3 2D array into a 1D array
     * @param arr The 2D array
     * @return The 1D array
    */
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

    /**
     * Set data to a float buffer and send the newly created buffer to all kernels
     * @param data The buffer's data
     * @param id The buffer's id
     * @return THe newly created buffer
    */
    private ComputeBuffer SetData(float[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float));
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    /**
     * Set data to a vector2 buffer and send the newly created buffer to all kernels
     * @param data The buffer's data
     * @param id The buffer's id
     * @return THe newly created buffer
    */
    private ComputeBuffer SetData(Vector2[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*2);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    /**
     * Set data to a vector3 buffer and send the newly created buffer to all kernels
     * @param data The buffer's data
     * @param id The buffer's id
     * @return THe newly created buffer
    */
    private ComputeBuffer SetData(Vector3[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*3);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    /**
     * Set data to an integer buffer and send the newly created buffer to all kernels
     * @param data The buffer's data
     * @param id The buffer's id
     * @return THe newly created buffer
    */
    private ComputeBuffer SetData(int[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(int));
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    /**
     * Set data to a particle buffer and send the newly created buffer to all kernels
     * @param data The buffer's data
     * @param id The buffer's id
     * @return THe newly created buffer
    */
    private ComputeBuffer SetData(ParticleGPU[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*14 + sizeof(int)*2);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    /**
     * Set data to a staggered grid buffer and send the newly created buffer to all kernels
     * @param data The buffer's data
     * @param id The buffer's id
     * @return THe newly created buffer
    */
    private ComputeBuffer SetData(StaggeredGridGPU[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*2 + sizeof(int)*2);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    /**
     * Set data to a neighbour grid buffer and send the newly created buffer to all kernels
     * @param data The buffer's data
     * @param id The buffer's id
     * @return THe newly created buffer
    */
    private ComputeBuffer SetData(NeighbourGridGPU[] data, int id){
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float)*2 + sizeof(int)*3);
        buffer.SetData(data);
        SendDataToAllKernels(buffer, id);
        return buffer;
    }

    /**
     * Get the number of particles
     * @return The number of particles
    */
    public int GetNbCurParticles(){
        return _NbCurParticles;
    }

    /**
     * Print the particles from the GPU (only for debug purposes)
    */
    private void DebugParticles(){
        _ParticlesBuffer.GetData(_Particles);
        for(int i=0; i<_NbCurParticles; i++){
            ParticleGPU p = _Particles[i];
            Debug.Log(
                "pos: "     + p._Position +
                ", h: "     + p._Height +
                ", hGrad: " + p._HeightGradient +
                ", dens: "  + p._Density +
                ", mass: "  + p._Mass +
                ", vol: "   + p._Volume +
                ", temp: "  + p._Temperature +
                ", cell: "  + p._Cell +
                ", id: "    + p._Id
            );
        }
    }

    /**
     * Print the number of particles per cells from the GPU (only for debug purposes)
    */
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

}