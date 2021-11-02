Shader "Custom/Ocean"
{
    Properties
    {
        [Header(Colours)]
        [HDR]_SurfaceColour("Surface Colour", Color) = (0.04, 0.38, 0.88, 1.0)
        [HDR]_DeepWaterColour("Deep Water Colour", Color) = (0.04, 0.35, 0.78, 1.0)
        [HDR]_IntersectionColour("Intersection Colour", Color) = (1,1,1,1)
        [HDR]_FoamColour("Foam Colour", Color) = (0.8125, 0.9609, 0.9648, 1.0)

        [Header(Thresholds)]
        _IntersectionThreshold("Intersction threshold", Range(0,10)) = 0
        _DeepWaterThreshold("Deep Water Threshold", Range(0,10)) = 0
        _FoamThreshold("Foam threshold", Range(0,10)) = 0

        [Header(Normal maps)]
        _NormalStrength("Normal strength", float) = 1
        _NormalBStrength("NormalB strength", float) = 1
        _NormalPanningVelocity("Panning velocity", Vector) = (0,0,0,0)

        [Header(Foam)]
        _FoamTextureSpeedX("Foam texture speed X", float) = 0
        _FoamTextureSpeedY("Foam texture speed Y", float) = 0
        _FoamLinesSpeed("Foam lines speed", float) = 0
        _FoamIntensity("Foam intensity", float) = 1

        [Header(Rendering)]
        _RenderTexture("Render texture", 2D) = "black" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _FresnelPower("Fresnel power", float) = 1
        _Cube("Reflection Cubemap", CUBE) = "" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows alpha:blend vertex:vert nolightmap
        //#pragma surface surf Standard fullforwardshadows alpha:blend vertex:vert tessellate:tessDistance nolightmap

        #include "UnityCG.cginc"
        #include "Assets/Shaders/Noise/ClassicNoise2D.hlsl"
        #include "Assets/Shaders/Noise/ClassicNoise3D.hlsl"
        #include "Assets/Shaders/SphericalTextureProjection.hlsl"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.0

        struct Input
        {
            float4 screenPos;
            float3 worldPos;
            float3 localPos;
            float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA
        };

        fixed4 _SurfaceColour;
        fixed4 _DeepWaterColour;
        fixed4 _IntersectionColour;

        float _IntersectionThreshold;
        float _DeepWaterThreshold;
        float _FoamThreshold;

        sampler2D _NormalA;
        sampler2D _NormalB;
        float4 _NormalA_ST;
        float4 _NormalB_ST;
        float _NormalStrength;
        float _NormalBStrength;
        float4 _NormalPanningVelocity;

        sampler2D _FoamTexture;
        float4 _FoamTexture_ST;
        float _FoamTextureSpeedX;
        float _FoamTextureSpeedY;
        float _FoamLinesSpeed;
        float _FoamIntensity;

        sampler2D _RenderTexture;
        half _Glossiness;
        float _FresnelPower;

        sampler2D _CameraDepthTexture;
        float3 _CamPosition;
        float _OrthographicCamSize;

        samplerCUBE _Cube;

        float _Steepness, _Freq;
        float4 _Direction;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)


        static float TAU = 2 * UNITY_PI;

        // Converts spherical coordinates to Cartesian coordinates
        float3 sph2cart(float phi, float theta) {
            float3 unit = float3(0.0, 0.0, 0.0);
            unit.x = cos(phi) * cos(theta);
            unit.y = sin(phi) * cos(theta);
            unit.z = sin(theta);
            return normalize(unit);
        }

        // Calculates tangent vector for unit sphere
        float3 sph2tan(float phi) {
            float3 tangent = float3(0.0, 0.0, 0.0);
            tangent.x = -sin(phi);
            tangent.y = cos(phi);
            return normalize(tangent);
        }

        void vert(inout appdata_full v, out Input o)
        //void vert(inout appdata_full v)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.localPos = v.vertex.xyz;

            //float3 n_sample = pos * 20.0 + _Time.y * 0.01;
            //getHeight(n_sample);
        }

        const float3x3 m3 = float3x3(0.00, 0.80, 0.60,
            -0.80, 0.36, -0.48,
            -0.60, -0.48, 0.64);
        const float3x3 m3i = float3x3(0.00, -0.80, -0.60,
            0.80, 0.36, -0.48,
            0.60, -0.48, 0.64);

        float fbm2(float2 x, int octaves)
        {
            float amplitude = 0.5;
            float frequency = 3.0;
            float value = 0.0;

            for (int i = 0; i < octaves; i++) {
                value += amplitude * ClassicNoise(frequency * x);
                amplitude *= 0.5;
                frequency *= 2.0;
            }
            return value;
        }

        float4 fbm(float3 x, int octaves)
        {
            float f = 1.9;  // could be 2.0
            float s = 0.49;  // could be 0.5
            float a = 0.0;
            float b = 1.0;
            float3  d = float3(0.0, 0.0, 0.0);
            float3x3  m = float3x3(1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);

            for (int i = 0; i < octaves; i++)
            {
                float4 n = ClassicNoise(x);
                a += b * n.x;                 // accumulate values
                d += b * mul(n.yzw, m);      // accumulate derivatives
                b *= s;

                x = f * x;// mul(x, transpose(m3));
                m = f * m;// mul(m, m3i);
            }

            return float4(a, d);
        }

        static const float HEIGHT = 0.35;
        static const int OCTAVES = 9;
        float getHeight(float3 pos) 
        {
            return HEIGHT * fbm(pos, OCTAVES).x;
        }


        //http://iquilezles.org/www/articles/normalsSDF/normalsSDF.htm
        //https://stackoverflow.com/questions/33736199/calculating-normals-for-a-height-map
        float3 getNormal(float3 p) 
        {

            //Making the normal sample distance depend on the ray length and resolution
            //leads to less noise.
            //float eps = (0.05 / iResolution.y) * pow(t, 1.55);
            float eps = 0.005;

            //Central difference method for estimating the derivatives and normal of a surface.
            /*
            float left = getHeight(vec3(p.x-eps, p.y, p.z), limit);
            float right = getHeight(vec3(p.x+eps, p.y, p.z), limit);
            float top = getHeight(vec3(p.x, p.y, p.z-eps), limit);
            float bottom = getHeight(vec3(p.x, p.y, p.z+eps), limit);

            float uy = right-left;
            vec3 u = normalize(vec3(2.0*eps, uy, 0.0));

            float vy = bottom-top;
            vec3 v = normalize(vec3(0.0, vy, 2.0*eps));

            return normalize(cross(v,u));
            */

            //The above is equivalent to the following:
            //return normalize(fbm(p, OCTAVES).yzw);

            float3 n = float3(
                getHeight(float3(p.x - eps, p.y, p.z))
                - getHeight(float3(p.x + eps, p.y, p.z)),

                2.0 * eps,

                getHeight(float3(p.x, p.y, p.z - eps))
                - getHeight(float3(p.x, p.y, p.z + eps))
                );

            return normalize(n);
        }

        float3 WorldToTangentNormalVector(Input IN, float3 normal) {
            float3 t2w0 = WorldNormalVector(IN, float3(1, 0, 0));
            float3 t2w1 = WorldNormalVector(IN, float3(0, 1, 0));
            float3 t2w2 = WorldNormalVector(IN, float3(0, 0, 1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        float circ(float2 pos, float2 c, float s)
        {
            c = abs(pos - c);
            c = min(c, 1.0 - c);

            return smoothstep(0.0, 0.002, sqrt(s) - sqrt(dot(c, c))) * -1.0;
        }


        // Foam pattern for the water constructed out of a series of circles
        float waterlayer(float2 uv)
        {
            uv = fmod(uv, 1.0); // Clamp to [0..1]

            float ret = 1.0;
            ret += circ(uv, float2(0.37378, 0.277169), 0.0268181);
            ret += circ(uv, float2(0.0317477, 0.540372), 0.0193742);
            ret += circ(uv, float2(0.430044, 0.882218), 0.0232337);
            ret += circ(uv, float2(0.641033, 0.695106), 0.0117864);
            ret += circ(uv, float2(0.0146398, 0.0791346), 0.0299458);
            ret += circ(uv, float2(0.43871, 0.394445), 0.0289087);
            ret += circ(uv, float2(0.909446, 0.878141), 0.028466);
            ret += circ(uv, float2(0.310149, 0.686637), 0.0128496);
            ret += circ(uv, float2(0.928617, 0.195986), 0.0152041);
            ret += circ(uv, float2(0.0438506, 0.868153), 0.0268601);
            ret += circ(uv, float2(0.308619, 0.194937), 0.00806102);
            ret += circ(uv, float2(0.349922, 0.449714), 0.00928667);
            ret += circ(uv, float2(0.0449556, 0.953415), 0.023126);
            ret += circ(uv, float2(0.117761, 0.503309), 0.0151272);
            ret += circ(uv, float2(0.563517, 0.244991), 0.0292322);
            ret += circ(uv, float2(0.566936, 0.954457), 0.00981141);
            ret += circ(uv, float2(0.0489944, 0.200931), 0.0178746);
            ret += circ(uv, float2(0.569297, 0.624893), 0.0132408);
            ret += circ(uv, float2(0.298347, 0.710972), 0.0114426);
            ret += circ(uv, float2(0.878141, 0.771279), 0.00322719);
            ret += circ(uv, float2(0.150995, 0.376221), 0.00216157);
            ret += circ(uv, float2(0.119673, 0.541984), 0.0124621);
            ret += circ(uv, float2(0.629598, 0.295629), 0.0198736);
            ret += circ(uv, float2(0.334357, 0.266278), 0.0187145);
            ret += circ(uv, float2(0.918044, 0.968163), 0.0182928);
            ret += circ(uv, float2(0.965445, 0.505026), 0.006348);
            ret += circ(uv, float2(0.514847, 0.865444), 0.00623523);
            ret += circ(uv, float2(0.710575, 0.0415131), 0.00322689);
            ret += circ(uv, float2(0.71403, 0.576945), 0.0215641);
            ret += circ(uv, float2(0.748873, 0.413325), 0.0110795);
            ret += circ(uv, float2(0.0623365, 0.896713), 0.0236203);
            ret += circ(uv, float2(0.980482, 0.473849), 0.00573439);
            ret += circ(uv, float2(0.647463, 0.654349), 0.0188713);
            ret += circ(uv, float2(0.651406, 0.981297), 0.00710875);
            ret += circ(uv, float2(0.428928, 0.382426), 0.0298806);
            ret += circ(uv, float2(0.811545, 0.62568), 0.00265539);
            ret += circ(uv, float2(0.400787, 0.74162), 0.00486609);
            ret += circ(uv, float2(0.331283, 0.418536), 0.00598028);
            ret += circ(uv, float2(0.894762, 0.0657997), 0.00760375);
            ret += circ(uv, float2(0.525104, 0.572233), 0.0141796);
            ret += circ(uv, float2(0.431526, 0.911372), 0.0213234);
            ret += circ(uv, float2(0.658212, 0.910553), 0.000741023);
            ret += circ(uv, float2(0.514523, 0.243263), 0.0270685);
            ret += circ(uv, float2(0.0249494, 0.252872), 0.00876653);
            ret += circ(uv, float2(0.502214, 0.47269), 0.0234534);
            ret += circ(uv, float2(0.693271, 0.431469), 0.0246533);
            ret += circ(uv, float2(0.415, 0.884418), 0.0271696);
            ret += circ(uv, float2(0.149073, 0.41204), 0.00497198);
            ret += circ(uv, float2(0.533816, 0.897634), 0.00650833);
            ret += circ(uv, float2(0.0409132, 0.83406), 0.0191398);
            ret += circ(uv, float2(0.638585, 0.646019), 0.0206129);
            ret += circ(uv, float2(0.660342, 0.966541), 0.0053511);
            ret += circ(uv, float2(0.513783, 0.142233), 0.00471653);
            ret += circ(uv, float2(0.124305, 0.644263), 0.00116724);
            ret += circ(uv, float2(0.99871, 0.583864), 0.0107329);
            ret += circ(uv, float2(0.894879, 0.233289), 0.00667092);
            ret += circ(uv, float2(0.246286, 0.682766), 0.00411623);
            ret += circ(uv, float2(0.0761895, 0.16327), 0.0145935);
            ret += circ(uv, float2(0.949386, 0.802936), 0.0100873);
            ret += circ(uv, float2(0.480122, 0.196554), 0.0110185);
            ret += circ(uv, float2(0.896854, 0.803707), 0.013969);
            ret += circ(uv, float2(0.292865, 0.762973), 0.00566413);
            ret += circ(uv, float2(0.0995585, 0.117457), 0.00869407);
            ret += circ(uv, float2(0.377713, 0.00335442), 0.0063147);
            ret += circ(uv, float2(0.506365, 0.531118), 0.0144016);
            ret += circ(uv, float2(0.408806, 0.894771), 0.0243923);
            ret += circ(uv, float2(0.143579, 0.85138), 0.00418529);
            ret += circ(uv, float2(0.0902811, 0.181775), 0.0108896);
            ret += circ(uv, float2(0.780695, 0.394644), 0.00475475);
            ret += circ(uv, float2(0.298036, 0.625531), 0.00325285);
            ret += circ(uv, float2(0.218423, 0.714537), 0.00157212);
            ret += circ(uv, float2(0.658836, 0.159556), 0.00225897);
            ret += circ(uv, float2(0.987324, 0.146545), 0.0288391);
            ret += circ(uv, float2(0.222646, 0.251694), 0.00092276);
            ret += circ(uv, float2(0.159826, 0.528063), 0.00605293);
            return max(ret, 0.0);
        }

        // Procedural texture generation for the water
        float3 water_colour(float2 uv, float3 cdir, float iTime)
        {
            uv *= 0.25;
            uv += fbm2(uv, 2) * 0.1;

            // Parallax height distortion with two directional waves at
            // slightly different angles.
            float2 a = 0.025 * cdir.xz / cdir.y; // Parallax offset
            float h = sin(uv.x + iTime); // Height at UV
            uv += a * h;
            h = sin(0.841471 * uv.x - 0.540302 * uv.y + iTime);
            uv += a * h;

            // Texture distortion
            float d1 = fmod(uv.x + uv.y, UNITY_PI * 2);
            float d2 = fmod((uv.x + uv.y + 0.25) * 1.3, UNITY_PI * 6);
            d1 = iTime * 0.07 + d1;
            d2 = iTime * 0.5 + d2;
            float2 dist = float2(
                sin(d1) * 0.15 + sin(d2) * 0.05,
                cos(d1) * 0.15 + cos(d2) * 0.05
            );

            float3 ret = lerp(_SurfaceColour, 0.75 * _SurfaceColour, waterlayer(uv + dist.xy));
            ret = lerp(ret, 1.0, waterlayer(1.0 - uv - dist.yx));
            return ret;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0, 0, 1));

            float3 pos = IN.localPos;
            // Convert Cartesian position to spherical coordinates
            float phi = atan2(IN.localPos.y, IN.localPos.x);
            float theta = atan2(IN.localPos.z, sqrt(IN.localPos.x * IN.localPos.x + IN.localPos.y * IN.localPos.y));

            float2 rt = pos.xz - _CamPosition.xz;
            rt = rt / (_OrthographicCamSize * 2);
            rt += 0.5;
            fixed4 renderTex = tex2D(_RenderTexture, rt);

            float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            depth = LinearEyeDepth(depth);

            float fogDiff = saturate((depth - IN.screenPos.w) / _DeepWaterThreshold);
            float intersectionDiff = saturate((depth - IN.screenPos.w) / _IntersectionThreshold);

            float3 n_sample = pos * 20.0 + _Time.y * 0.01;

            // get colour
            float a = _Time.y * 0.2;
            UV sh_uvs = smoothSphereToCubicTiling(float2(phi, theta), tilingBorderGradiantAngle);
            float3 col = 0.0;
            for (int i = 0; i < 3; i++)
            {
                float3 sh_uv = sh_uvs.a[i];
                if (sh_uv.z > .0)
                {
                    //float3 t_col = 1.0 - sh_uv.z;
                    float3 t_col = water_colour(sh_uv.xy * 512.0, float3(0, 1, 0), a).xyz;

                    col += t_col * (withTilingBorderGradient ? sh_uv.z : 1.0);
                }
            }


            float3 surface_colour = col;
            fixed4 final_colour = lerp(lerp(_IntersectionColour, float4(surface_colour, 1.0), intersectionDiff), float4(surface_colour, 1.0), fogDiff);

            //float foamDiff = saturate((depth - IN.screenPos.w) / _FoamThreshold);
            float foam = 0;
            //if (foamDiff > 0)
            //{
            //    foamDiff *= (1.0 - renderTex.b);
            //
            //    float foamTex = tex2D(_FoamTexture, pos.xz * _FoamTexture_ST.xy + _Time.y * float2(_FoamTextureSpeedX, _FoamTextureSpeedY));
            //    foam = step(foamDiff - (saturate(sin((foamDiff - _Time.y * _FoamLinesSpeed) * 8 * UNITY_PI)) * (1.0 - foamDiff)), foamTex);
            //}

            float fresnel = pow(1.0 - saturate(dot(o.Normal, normalize(IN.viewDir))), _FresnelPower);

            //float2 uv_offset = renderTex.rg + _Time.y * _NormalPanningVelocity.xy;

            //float3 normalA = UnpackNormalWithScale(tex2D(_NormalA, uv * _NormalA_ST.xy + uv_offset), _NormalStrength);
            //float3 normalB = UnpackNormalWithScale(tex2D(_NormalB, uv * _NormalB_ST.xy + uv_offset), _NormalBStrength);

            //float3 normalA = r1.derivs; //ClassicNoise(pos * 20.0 + _Time.y * 0.1).xyz;
            //float3 normalB = r1.derivs; //ClassicNoise(pos * 200.0 + _Time.y * 0.5).xyz;

            float3 normal = getNormal(n_sample);
            foam += normal.y;

            //o.Albedo = n.val;
            o.Albedo = final_colour.rgb; /*+ texCUBE(_Cube, WorldReflectionVector(IN, o.Normal)).rgb * 0.0000000001*/
                       + foam * _FoamIntensity;
            //o.Normal = WorldToTangentNormalVector(IN, normal);
            //o.Smoothness = _Glossiness;
            o.Alpha = 1.0;// lerp(lerp(final_colour.a * fresnel, 1.0, foam), _DeepWaterColour.a, fogDiff);
            //o.Emission = foam * _FoamIntensity;
        }
        
        ENDCG
    }

    FallBack "Diffuse"
}
