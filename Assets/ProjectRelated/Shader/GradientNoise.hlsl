#ifndef GRADIENTNOISE_INCLUDED
#define GRADIENTNOISE_INCLUDED

void Hash_Tchou_2_1_uint(uint2 v, out uint o)
{
    v.y ^= 1103515245U;
    v.x += v.y;
    v.x *= v.y;
    v.x ^= v.y >> 5u;
    v.x *= 0x27d4eb2du;
    o = v.x;
}

void Hash_Tchou_2_1_float(float2 i, out float o)
{
    uint r;
    uint2 v = (uint2) (int2) round(i);
    Hash_Tchou_2_1_uint(v, r);
    o = (r >> 8) * (1.0 / float(0x00ffffff));
}

float2 Unity_GradientNoise_Deterministic_Dir_float(float2 p)
{
    float x;
    Hash_Tchou_2_1_float(p, x);
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

void Unity_GradientNoise_Deterministic_float(float2 UV, float3 Scale, out float Out)
{
    float2 p = UV * Scale.xy;
    float2 ip = floor(p);
    float2 fp = frac(p);
    float d00 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip), fp);
    float d01 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
    float d10 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
    float d11 = dot(Unity_GradientNoise_Deterministic_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
    fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
    Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
}

#endif