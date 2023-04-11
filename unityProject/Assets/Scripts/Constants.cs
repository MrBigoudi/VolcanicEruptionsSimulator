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
     * The gas constant
    */
    public const float GAS_CONST = 2000.0f;

    /**
     * The kernel radius
    */
    public const float H = 2.0f;

    /**
     * Factor for poly6 kernel calculations
    */
    public const float ALPHA_POLY6 = 4.0f / (PI*H*H*H*H*H*H*H*H);
    
    /**
     * The viscosity constant
    */
    public const float VISC = 200.0f;

    /**
     * The lava rest density
    */
    public const float RHO_0 = 3500.0f;

    /**
     * The stiffness constant for gas law
    */
    public const float STIFFNESS = G/RHO_0;
    
}