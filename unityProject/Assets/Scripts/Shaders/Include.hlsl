#pragma once

#include "Constants.hlsl"

struct StaggeredGrid{
    StructuredBuffer<float>  _Heights;
    StructuredBuffer<float>  _HalfHeightsCols;
    StructuredBuffer<float>  _HalfHeightsLines;

    StructuredBuffer<float2> _Gradients;
    StructuredBuffer<float>  _Laplacians;

    uint _NbCols, _NbLines;
    float _DeltaCols, _DeltaLines;
};

struct Particle{
    float3 _Position;
    float  _Height;
    float2 _HeightGradient;
};

uniform StaggeredGrid _StaggeredGrid;
RWStructuredBuffer<Particle> _Particles;
RWStructuredBuffer<uint> _Neighbours;