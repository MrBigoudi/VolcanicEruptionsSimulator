using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A static class to use constants
*/
public static class Constants{
    
    /**
     * The pi constant
    */
    public const float PI = 3.1416f; 

    /**
     * The gravitational force
    */
    public const float G = 9.81f;

    /**
     * The kernel radius
    */
    public const float H = 2.0f;

    /**
     * Factor for poly6 kernel calculations
    */
    public const float ALPHA_POLY6 = 4.0f / (PI*H*H*H*H*H*H*H*H);

    /**
     * Factor for poly6 kernel laplacian calculations
    */
    public const float ALPHA_POLY6_LAPLACIAN = 32.0f / (PI*H*H*H*H*H*H*H*H);

    /**
     * Factor for viscosity kernel calculations
    */
    public const float ALPHA_VISCOSITY = 10.0f / (9.0f*PI*H*H*H*H*H);

    /**
     * Factor for viscosity kernel laplacian calculations
    */
    public const float ALPHA_VISCOSITY_LAPLACIAN = 40.0f / (PI*H*H*H*H*H);

    /**
     * The lava rest density
    */
    public const float RHO_0 = 2500.0f;

    /**
     * The stiffness constant
    */
    public const float STIFFNESS = 50;
    
}