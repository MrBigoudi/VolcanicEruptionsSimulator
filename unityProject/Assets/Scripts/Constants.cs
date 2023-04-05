using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants{
    public const float M_PI = 3.1416f; 

    // solver parameters
    public const float G = -10.0f;          // gravitational force
    public const float GAS_CONST = 2000.0f; // gas constant

    public const float H = 16.0f;           // kernel radius
    public const float ALPHA_2D = 10 / (7*Constants.M_PI*H*H); // alpha 2D for kernel calculation
    public const float ALPHA_3D = 1 / (Constants.M_PI*H*H*H);  // alpha 3D for kernel calculation
    
    public const float MASS = 2.5f;        // particle mass
    public const float VISC = 200.0f;      // viscosity constant
    public const float RHO_0 = 3500.0f;    // magma rest density

    public const float STIFFNESS = 1.0f;   // stiffness constant for gas law 
    
}