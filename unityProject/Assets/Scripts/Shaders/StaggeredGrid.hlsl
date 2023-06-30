#pragma once
#include "Include.hlsl"

// the staggered grid inside GPU

uint StaggeredGridConvertIndices(uint posZ, uint posX){
    return posZ + posX * _StaggeredGrid._NbLines;
}

uint2 StaggeredGridGetIndices(float3 pos){
    uint posX = (pos.x / _StaggeredGrid._DeltaCols);
    uint posZ = (pos.z / _StaggeredGrid._DeltaLines);
    return uint2(posZ, posX);
};

float StaggeredGridBilinearInterpolation(float x, float z, float xIdx, float zIdx, float upLeft, float upRight, float downLeft, float downRight){
    float xLeft  = xIdx*_StaggeredGrid._DeltaCols;
    float xRight = (xIdx+1)*_StaggeredGrid._DeltaCols;
    float zUp    = (zIdx+1)*_StaggeredGrid._DeltaLines;
    float zDown  = zIdx*_StaggeredGrid._DeltaLines;

    float res = (downLeft*(xRight-x)*(zUp-z))+(downRight*(x-xLeft)*(zUp-z)
                    +(upLeft*(xRight-x)*(z-zDown))+(upRight*(x-xLeft)*(z-zDown)));
    return res /= _StaggeredGrid._DeltaCols*_StaggeredGrid._DeltaLines;
}

float StaggeredGridGetHeight(float3 pos){
    uint2 indices = StaggeredGridGetIndices(pos);
    uint zIdx = indices[0];
    uint xIdx = indices[1];

    float x = pos.x;
    float z = pos.z;

    // interpolate height, bilinearly
    float upLeft    = _StaggeredGrid._Heights[StaggeredGridConvertIndices(zIdx+1, xIdx+0)];
    float upRight   = _StaggeredGrid._Heights[StaggeredGridConvertIndices(zIdx+1, xIdx+1)];
    float downLeft  = _StaggeredGrid._Heights[StaggeredGridConvertIndices(zIdx+0, xIdx+0)];
    float downRight = _StaggeredGrid._Heights[StaggeredGridConvertIndices(zIdx+0, xIdx+1)];

    float ownRes = StaggeredGridBilinearInterpolation(x, z, xIdx, zIdx, upLeft, upRight, downLeft, downRight);
    return ownRes;
}

float3 StaggeredGridGetGradient(Particle p){
    float3 pos = p._Position;
    uint2 indices = StaggeredGridGetIndices(pos);
    uint zIdx = indices[0];
    uint xIdx = indices[1];

    float x = pos.x;
    float z = pos.z;

    // interpolate gradients bilinearly
    float2 upLeft    = (zIdx >= _StaggeredGrid._NbLines-1 || xIdx == 0)                        ? float2(0.0f, 0.0f) : _StaggeredGrid._Gradients[StaggeredGridConvertIndices(zIdx-1+1, xIdx-1+0)];
    float2 upRight   = (zIdx >= _StaggeredGrid._NbLines-1 || xIdx >= _StaggeredGrid._NbCols-1) ? float2(0.0f, 0.0f) : _StaggeredGrid._Gradients[StaggeredGridConvertIndices(zIdx-1+1, xIdx-1+1)];
    float2 downLeft  = (zIdx == 0 || xIdx == 0)                                                ? float2(0.0f, 0.0f) : _StaggeredGrid._Gradients[StaggeredGridConvertIndices(zIdx-1+0, xIdx-1+0)];
    float2 downRight = (zIdx == 0 || xIdx >= _StaggeredGrid._NbCols-1)                         ? float2(0.0f, 0.0f) : _StaggeredGrid._Gradients[StaggeredGridConvertIndices(zIdx-1+0, xIdx-1+1)];

    float dx = StaggeredGridBilinearInterpolation(x, z, xIdx, zIdx, upLeft.x, upRight.x, downLeft.x, downRight.x) - p._Height;
    float dz = StaggeredGridBilinearInterpolation(x, z, xIdx, zIdx, upLeft.y, upRight.y, downLeft.y, downRight.y) - p._Height;

    return float3(dx, pos.y, dz);
}

float StaggeredGridGetLaplacian(float3 pos){
    uint2 indices = StaggeredGridGetIndices(pos);
    uint zIdx = indices[0];
    uint xIdx = indices[1];

    float x = pos.x;
    float z = pos.z;

    // interpolate gradients bilinearly
    float upLeft    = (zIdx >= _StaggeredGrid._NbLines-2 || xIdx <= 1)                        ? 0.0f : _StaggeredGrid._Laplacians[StaggeredGridConvertIndices(zIdx-2+1, xIdx-2+0)];
    float upRight   = (zIdx >= _StaggeredGrid._NbLines-2 || xIdx >= _StaggeredGrid._NbCols-2) ? 0.0f : _StaggeredGrid._Laplacians[StaggeredGridConvertIndices(zIdx-2+1, xIdx-2+1)];
    float downLeft  = (zIdx <= 1 || xIdx <= 1)                                                ? 0.0f : _StaggeredGrid._Laplacians[StaggeredGridConvertIndices(zIdx-2+0, xIdx-2+0)];
    float downRight = (zIdx <= 1 || xIdx >= _StaggeredGrid._NbCols-1)                         ? 0.0f : _StaggeredGrid._Laplacians[StaggeredGridConvertIndices(zIdx-2+0, xIdx-2+1)];

    float ownRes = StaggeredGridBilinearInterpolation(x, z, xIdx, zIdx, upLeft, upRight, downLeft, downRight);
    return ownRes;
}