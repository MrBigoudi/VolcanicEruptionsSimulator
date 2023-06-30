#pragma once

#include "Include.hlsl"

float K_POLY6(float r){
    if(r<H) {
        float tmp = (H*H-r*r);
        return tmp*tmp*tmp;
    }
    else return 0.0f;
}


float K_POLY6_Prime(float r){
    if(r<H) {
        float tmp = ((H*H)-r*r);
        return -6.0f*r*tmp*tmp;
    }
    else return 0.0f;    
}

float K_POLY6_Lap(float r){
    if(r<H) {
        float tmp = ((H*H)-r*r);
        return tmp*(2*r*r-H*H);
    }
    else return 0.0f;
}

float K_VISCOSITY(float r){
    float l = H;
    if(r<l) {
        return r*r*(-4.0f*r + 9.0f*l) + l*l*l*(-5.0f + 6.0f*(log(l)-log(r)));
    }
    else return 0.0f;
}

float K_VISCOSITY_Prime(float r){
    float l = H;
    if(r<l) {
        if(r != 0) return r*(-12.0f*r + 18.0f*l) - (6.0f*l*l*l) / r;
        return 0.0f;
    }
    else return 0.0f;
}

float KernelDistance(float3 p1, float3 p2){
    return sqrt((p1.x-p2.x)*(p1.x-p2.x) + (p1.z-p2.z)*(p1.z-p2.z));
}

float W_POLY6(Particle p1, Particle p2){
    float r = KernelDistance(p1._Position, p2._Position) / H;
    return ALPHA_POLY6 * K_POLY6(r);
}

float3 W_POLY6_Grad(Particle p1, Particle p2){
    float r = KernelDistance(p1._Position, p2._Position) / H;
    return ALPHA_POLY6 * K_POLY6_Prime(r) * (p1._Position - p2._Position);
}

float W_POLY6_Derivated(Particle p1, Particle p2){
    float r = KernelDistance(p1._Position, p2._Position) / H;
    return ALPHA_POLY6 * K_POLY6_Prime(r);
}

float W_POLY6_Lap(Particle p1, Particle p2){
    float r = KernelDistance(p1._Position, p2._Position) / H;
    return ALPHA_POLY6_LAPLACIAN * K_POLY6_Lap(r);
}

float W_VISCOSITY(Particle p1, Particle p2){
    float r = KernelDistance(p1._Position, p2._Position) / H;
    return ALPHA_VISCOSITY * K_VISCOSITY(r);
}

float3 W_VISCOSITY_Grad(Particle p1, Particle p2){
    float r = KernelDistance(p1._Position, p2._Position) / H;
    float prime = K_VISCOSITY_Prime(r);
    float3 pos = (p1._Position - p2._Position);
    return ALPHA_VISCOSITY * prime * pos;
}

float W_VISCOSITY_Lap(Particle p1, Particle p2){
    float r = KernelDistance(p1._Position, p2._Position) / H;
    float l = H;
    return ALPHA_VISCOSITY_LAPLACIAN*(l-r);
}