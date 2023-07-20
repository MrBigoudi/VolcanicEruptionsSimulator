using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


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

    // Materials
    [SerializeField]
    public Material _ParticleMaterial;
    [SerializeField]
    public Material _TerrainMaterial;

    // Constants values
    [SerializeField]
    public Volcano _VolcanoImage = Volcano.Basic;
    [SerializeField]
    public Vector3 _Size = new Vector3(512.0f, 0.0f, 512.0f);
    [SerializeField, Range(32.0f, 1024.0f)]
    public float _Scale = 32.0f;


    // Tweakable values
    [SerializeField]
    public bool _DisplayParticles = false;
    [SerializeField]
    public bool _DisplayLava = true;
    [SerializeField]
    public bool _GaussianBlur = false;
    [SerializeField, Range(1, 100000)]
    public int _NbMaxParticles = 50000;
    [SerializeField, Range(0.0f, 10.0f)]
    public float _InitialPositionDelta = 1.0f;
    [SerializeField, Range(0.0f, 0.25f)]
    public float _DT = 0.01f;
    [SerializeField, Range(0.0f, 2.0f)]
    public float _Spike = 0.1f;
    [SerializeField, Range(0.0f, 10.0f)]
    public float _KernelRadius = 5.0f;
    [SerializeField, Range(0.0f, 10.0f)]
    public float _Stiffness = 3.0f;
    [SerializeField, Range(0.0f, 100.0f)]
    public float _ParticleInitialHeight = 2.0f;
    [SerializeField, Range(1, 200)]
    public float _TerrainDensityMax = 150;
    [SerializeField, Range(1, 200)]
    public float _TerrainDensityMin = 30;

    [SerializeField, Range(0.0f, 5.0f)]
    public float _ParticlesMeshHeights = 0.1f;

}