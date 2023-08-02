using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * A class merging all the serialized fields to ease the tweaking of values from the editor during the simulation
*/
public class Tweakable : MonoBehaviour{
    // Other classes
    [SerializeField]
    public ParticleSPHGPU _SphGPU;
    [SerializeField]
    public ComputeShader _Shader;
    [SerializeField]
    public TerrainGenerator _TerrainGenerator;
    [SerializeField]
    public ParticleDisplay _ParticleDisplay;
    [SerializeField]
    public Camera _Camera;
    [SerializeField]
    public ParticleGenerator _ParticleMaker;

    // Materials
    [SerializeField]
    public Material _ParticleMaterial;
    [SerializeField]
    public Material _TerrainMaterial;

    // Constants values
    [SerializeField]
    public Volcano _VolcanoImage = Volcano.StHelen;
    [SerializeField]
    public bool _GaussianBlur = false;

    // Tweakable values
    [SerializeField]
    public bool _DisplayParticles = false;
    [SerializeField]
    public bool _DisplayLava = true;
    

    [SerializeField, Range(1, 100000)]
    public int _NbMaxParticles = 100000;
    [SerializeField, Range(0.0f, 10.0f)]
    public float _InitialPositionDelta = 2.0f;
    [SerializeField, Range(0.0f, 0.25f)]
    public float _DT = 0.01f;
    [SerializeField, Range(0.0f, 10.0f)]
    public float _Spike = 2.0f;
    [SerializeField, Range(0.0f, 5.0f)]
    public float _KernelRadius = 1.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    public float _RenderKernelRadius = 10.0f;

    [SerializeField, Range(-5.0f, 5.0f)]
    public float _ParticlesMeshHeights = 0.0f;

    [SerializeField, Range(1, 10000)]
    public float _Mu = 100.0f;
    [SerializeField, Range(1, 10000)]
    public float _Ke = 100.0f;
    [SerializeField, Range(1, 3000)]
    public float _ThetaE = 1423.0f;

    [SerializeField, Range(1, 512)]
    public float _ColorShade = 100.0f;
}