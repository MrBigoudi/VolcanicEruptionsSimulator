using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants{
    public const float PI = 3.1416f; 

    // solver parameters
    public const float G = 9.81f;          // gravitational force
    public const float GAS_CONST = 2000.0f; // gas constant

    public const float H = 2.0f;           // kernel radius
    public const float ALPHA_POLY6 = 4.0f / (PI*H*H*H*H*H*H*H*H);
    
    public const float VISC = 200.0f;      // viscosity constant
    public const float RHO_0 = 3500.0f;    // magma rest density

    public const float STIFFNESS = G/RHO_0;   // stiffness constant for gas law 
    
}