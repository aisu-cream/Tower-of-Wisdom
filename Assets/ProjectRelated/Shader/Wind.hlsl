#ifndef WIND_INCLUDED
#define WIND_INCLUDED

#include "GradientNoise.hlsl"

void GetWindOffset(float3 PositionWS, float2 UV0, float Time, float2 WindMovement, float WindStrength, float WindDensity, out float2 WindOffset)
{
    float gradient;
    Unity_GradientNoise_Deterministic_float(PositionWS.xz + Time * WindMovement, WindDensity, gradient);
    
    float2 windDir = 0;
    
    if (dot(WindMovement, WindMovement) > 1e-10)
        windDir = normalize(WindMovement);
    
    float bendWeight = saturate(UV0.y);
    bendWeight *= bendWeight;
    
    WindOffset = windDir * (gradient - 0.5) * WindStrength * bendWeight;
}

#endif