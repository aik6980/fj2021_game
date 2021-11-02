#ifndef SPHERICAL_TEXTURE_PROJECTION__HLSL_
#define SPHERICAL_TEXTURE_PROJECTION__HLSL_

//https ://www.shadertoy.com/view/3dKGzG
static const float PI_2 = UNITY_PI * 0.5;
static const float PI_4 = UNITY_PI / 4.0;
static const float PI_8 = UNITY_PI / 8.0;
static const float D_PI = 2.0 * UNITY_PI;

// modify these to change some aspect
static const bool withTilingBorderGradient = true;
static const float tilingBorderGradiantAngle = UNITY_PI / 8.0 * 0.2;

struct UV
{
    float3 a[3];
};

UV smoothSphereToCubicTiling(float2 ang, float smoothAng) 
{
    if (smoothAng < .0) smoothAng = -smoothAng;
    if (smoothAng > PI_2)
    {
        UV result = (UV)0.0;
        return result;
    }

    float t1 = PI_4 - .5 * smoothAng;
    float t2 = PI_4 + .5 * smoothAng;

    // the pole
    float3 uv1;
    {
        float r = (PI_2 - abs(ang.y));	//radius from center to border
        float body = 1. - smoothstep(t1, t2, r);
        float2 uv = sign(ang.y) * float2(r * cos(ang.x), r * sin(ang.x)) / PI_4;
        uv1 = body > 0. ? float3(uv, body) : 0.0;
    }

    // the 'current' tile
    float3 uv2;
    float n = floor((ang.x + PI_4) / PI_2);
    float tiled_lttd = ang.y;
    float tiled_lgtd = ang.x - n * PI_2;
    {
        float body = (1. - smoothstep(t1, t2, abs(tiled_lgtd)))
            * (1. - smoothstep(t1, t2, abs(tiled_lttd)));
        float2 uv = float2(tiled_lgtd, tiled_lttd) / PI_4;
        uv2 = body > 0. ? float3(uv, body) : 0.0;
    }

    // the 'side' tile (left or right)
    float3 uv3;
    {
        tiled_lgtd -= sign(tiled_lgtd) * PI_2;
        float body = (1. - smoothstep(t1, t2, abs(tiled_lgtd)))
            * (1. - smoothstep(t1, t2, abs(tiled_lttd)));
        float2 uv = float2(tiled_lgtd, tiled_lttd) / PI_4;
        uv3 = body > 0. ? float3(uv, body) : 0.0;
    }

    float err = (uv1 + uv2 + uv3).z;
    float3 err_cor = 1.0;
    if (err > 1.)
        err_cor.z = 1. / err;

    //float3[] result = float3[](
    //    uv1* err_cor,
    //    uv2* err_cor,
    //    uv3* err_cor);

    UV result;
    float3 tmp[] = {
        uv1 * err_cor,
        uv2 * err_cor,
        uv3 * err_cor
    };
    result.a = tmp;

    return result;
}

#endif
