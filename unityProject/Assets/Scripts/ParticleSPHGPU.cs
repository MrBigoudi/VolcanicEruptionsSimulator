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
    public int     _Id;
    public Vector3 _Position;
    public float   _Height;
    public Vector3 _HeightGradient;
    public float   _Volume;
    public int     _Cell;
};

/**
 * A class representing an sph solver
*/
public class ParticleSPHGPU : MonoBehaviour{

    public int mNbMaxParticles = 0;
    public int mNbCurParticles = 0;

    private static int _MAX_NEIGHBOURS = 1000;

    public ComputeShader _Shader;

    private int _NbCurParticlesId;
    private int _NewParticleId;
    private ComputeBuffer _NewParticleBuffer;

    // compute shader functions
    private int _KernelGenerateParticleId;
    private int _KernelAssignCellId;
    private int _KernelFillNeighboursListId;
    private int _KernelUpdateHeightsId;
    private int _KernelPropagateHeightsId;
    private int _KernelTimeIntegrationId;
    private int _KernelPropagatePositionsId;

    // compute shader buffers
    private int _ParticlesId;
    private int _NeighboursId;
    private int _HeightsId;
    private int _HeightsGradientsId;
    private int _PositionsId;
    private int _VolumesId;
    private int _CellsId;
    private ComputeBuffer _ParticlesBuffer;
    private ComputeBuffer _NeighboursBuffer;
    private ComputeBuffer _HeightsBuffer;
    private ComputeBuffer _HeightsGradientsBuffer;
    private ComputeBuffer _PositionsBuffer;
    private ComputeBuffer _VolumesBuffer;
    private ComputeBuffer _CellsBuffer;

    private int _StaggeredGridId;
    private int _StaggeredGridHeightsId;
    private int _StaggeredGridHalfHeightsColsId;
    private int _StaggeredGridHalfHeightsLinesId;
    private int _StaggeredGridGradientsId;
    private int _StaggeredGridLaplaciansId;
    private ComputeBuffer _StaggeredGridBuffer;
    private ComputeBuffer _StaggeredGridHeightsBuffer;
    private ComputeBuffer _StaggeredGridHalfHeightsColsBuffer;
    private ComputeBuffer _StaggeredGridHalfHeightsLinesBuffer;
    private ComputeBuffer _StaggeredGridGradientsBuffer;
    private ComputeBuffer _StaggeredGridLaplaciansBuffer;

    // future buffer data
    private int[]     _Neighbours;
    public  float[]   _Heights;
    private Vector3[] _HeightsGradients;
    public  Vector3[] _Positions;
    private float[]   _Volumes;
    private int[]     _Cells;

    private ParticleGPU[] _Particles;


    public void Create(int maxParticles, ComputeShader shader, TerrainGenerator terrain){
        mNbMaxParticles = maxParticles;
        _Shader = shader;
        // Debug.Log(_Shader);
        Init(terrain);
    }

    private void InitKernelsIds(){
        // init functions id
        _KernelGenerateParticleId   = _Shader.FindKernel("GenerateParticle");
        _KernelAssignCellId         = _Shader.FindKernel("AssignCell");
        _KernelFillNeighboursListId = _Shader.FindKernel("FillNeighboursList");
        _KernelUpdateHeightsId      = _Shader.FindKernel("UpdateHeights");
        _KernelPropagateHeightsId   = _Shader.FindKernel("PropagateHeights");
        _KernelTimeIntegrationId    = _Shader.FindKernel("TimeIntegration");
        _KernelPropagatePositionsId = _Shader.FindKernel("PropagatePositions");
        // Debug.Log(_KernelGenerateParticleId + ", " + _KernelAssignCellId + ", " + _KernelFillNeighboursListId);
    }

    private void InitStaggeredGridIds(){
        _StaggeredGridId                 = Shader.PropertyToID("_StaggeredGrid");
        _StaggeredGridHeightsId          = Shader.PropertyToID("_StaggeredGridHeights");
        _StaggeredGridHalfHeightsColsId  = Shader.PropertyToID("_StaggeredGridHalfHeightsCols");
        _StaggeredGridHalfHeightsLinesId = Shader.PropertyToID("_StaggeredGridHalfHeightsLines");
        _StaggeredGridGradientsId        = Shader.PropertyToID("_StaggeredGridGradients");
        _StaggeredGridLaplaciansId       = Shader.PropertyToID("_StaggeredGridLaplacians");
    }

    private void InitParticlesIds(){
        _NbCurParticlesId   = Shader.PropertyToID("_NbCurParticles");
        _NewParticleId      = Shader.PropertyToID("_NewParticle");

        _ParticlesId        = Shader.PropertyToID("_Particles");
        _NeighboursId       = Shader.PropertyToID("_Neighbours");
        _HeightsId          = Shader.PropertyToID("_Heights");
        _HeightsGradientsId = Shader.PropertyToID("_HeightsGradients");
        _PositionsId        = Shader.PropertyToID("_Positions");
        _VolumesId          = Shader.PropertyToID("_Volumes");
        _CellsId            = Shader.PropertyToID("_Cells");
    }

    private void InitIds(){
        InitKernelsIds();
        InitParticlesIds();        
    }

    private void InitBuffersData(){
        _Neighbours       = new int[_MAX_NEIGHBOURS*mNbMaxParticles];
        _Heights          = new float[mNbMaxParticles];
        _HeightsGradients = new Vector3[mNbMaxParticles];
        _Positions        = new Vector3[mNbMaxParticles];
        _Volumes          = new float[mNbMaxParticles];
        _Cells            = new int[mNbMaxParticles];
        _Particles = new ParticleGPU[mNbMaxParticles];
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

        _Shader.SetInt("MAX_NEIGHBOURS", _MAX_NEIGHBOURS);
        _Shader.SetInt("MAX_PARTICLES", mNbMaxParticles);

        int nbCols = (int)(((int)(terrain._Size.x+1)) / (2*Constants.H));
        _Shader.SetInt("GRID_NB_COLS", nbCols);
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
        _Shader.SetBuffer(_KernelAssignCellId, id, buffer);
        _Shader.SetBuffer(_KernelFillNeighboursListId, id, buffer);
        _Shader.SetBuffer(_KernelUpdateHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelPropagateHeightsId, id, buffer);
        _Shader.SetBuffer(_KernelTimeIntegrationId, id, buffer);
        _Shader.SetBuffer(_KernelPropagatePositionsId, id, buffer);
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
        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(int)*2 + sizeof(float)*8);
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
        Vector2[] gradients      = Convert2dArray(StaggeredGridV2._Gradients);
        float[] laplacians       = Convert2dArray(StaggeredGridV2._Laplacians);
        // init grid buffers
        _StaggeredGridHeightsBuffer          = SetData(heights, _StaggeredGridHeightsId);
        _StaggeredGridHalfHeightsColsBuffer  = SetData(halfHeightsCols, _StaggeredGridHalfHeightsColsId);
        _StaggeredGridHalfHeightsLinesBuffer = SetData(halfHeightsLines, _StaggeredGridHalfHeightsLinesId);
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
        _CellsBuffer            = SetData(_Cells, _CellsId);
        _NeighboursBuffer       = SetData(_Neighbours, _NeighboursId);
    }

    private void Init(TerrainGenerator terrain){
        InitIds();
        InitGpuValues(terrain);
        InitBuffersData();
        InitGpuBuffers();
        InitStaggeredGrid();
    }

    private void GenerateParticle(Vector3 position, int res){

        if(mNbCurParticles <= mNbMaxParticles){
            // init CPU part
            mNbCurParticles++;
            _Shader.SetInt("_NbCurParticles", mNbCurParticles);

            // init GPU part
            ParticleGPU p = new ParticleGPU{
                                    _Id = mNbCurParticles-1,
                                    _Position = position,
                                    _Height = 0.4f,
                                    _HeightGradient = Vector3.zero,
                                    _Volume = 200.0f/Constants.RHO_0,
                                    _Cell = 0
                                };
            ParticleGPU[] data = new ParticleGPU[1];
            data[0] = p;
            _NewParticleBuffer = SetData(data, _NewParticleId);

            _Shader.Dispatch(_KernelGenerateParticleId, res, 1, 1);
        }
    }

    private void AssignCell(int res){
        _Shader.Dispatch(_KernelAssignCellId, res, 1, 1);
    }

    private void FillNeighboursList(int res){
        _Shader.Dispatch(_KernelFillNeighboursListId, res, 1, 1);
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

    private void FetchHeigthsAndPositions(){
        _HeightsBuffer.GetData(_Heights);
        _PositionsBuffer.GetData(_Positions);
    }

    public void Updt(Vector3 position, float stiffness){
        _Shader.SetFloat("STIFF", stiffness);
        int res = mNbMaxParticles / 100;
        // generate particle in GPU
        GenerateParticle(position, res);
        // find particles cells in gpu
        AssignCell(res);
        // fill neighbour list in gpu
        FillNeighboursList(res);
        // calculate heights in gpu
        UpdateHeights(res);
        // time integration gpu
        TimeIntegration(res);
        // get back the heights and the positions
        FetchHeigthsAndPositions();
    }

    private void ReleaseBuffers(){
        _ParticlesBuffer.Dispose();
        _NeighboursBuffer.Dispose();
        _HeightsBuffer.Dispose();
        _HeightsGradientsBuffer.Dispose();
        _PositionsBuffer.Dispose();
        _VolumesBuffer.Dispose();
        _CellsBuffer.Dispose();
        _NewParticleBuffer.Dispose();

        _StaggeredGridBuffer.Dispose();
        _StaggeredGridHeightsBuffer.Dispose();
        _StaggeredGridHalfHeightsColsBuffer.Dispose();
        _StaggeredGridHalfHeightsLinesBuffer.Dispose();
        _StaggeredGridGradientsBuffer.Dispose();
        _StaggeredGridLaplaciansBuffer.Dispose();
    }

    public void OnApplicationQuit(){
        // release buffers
        ReleaseBuffers();
    }

    public void OnDisable(){
        ReleaseBuffers();
    }
}