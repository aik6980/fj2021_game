Shader "Custom/WaterSurfaceShader"
{
    Properties
    {
        [Header(Colours)]
        [HDR] _SurfaceColour("Surface Colour", Color) = (.1,.3,.8,1)
        [HDR]_DeepWaterColour("Deep Water Colour", Color) = (.1,.3,.3,1)
        [HDR]_IntersectionColour("Intersection Colour", Color) = (1,1,1,1)

        [Header(Thresholds)]
        _IntersectionThreshold("Intersction threshold", Range(0,10)) = 0
        _DeepWaterThreshold("Deep Water Threshold", Range(0,10)) = 0
        _FoamThreshold("Foam threshold", Range(0,10)) = 0

        [Header(Normal maps)]
        [Normal]_NormalA("Normal A", 2D) = "bump" {}
        [Normal]_NormalB("Normal B", 2D) = "bump" {}
        _NormalStrength("Normal strength", float) = 1
		_NormalBStrength("NormalB strength", float) = 1
		_NormalPanningVelocity("Panning velocity", Vector) = (0,0,0,0)

        [Header(Foam)]
        _FoamTexture("Foam texture", 2D) = "white" {}
        _FoamTextureSpeedX("Foam texture speed X", float) = 0
        _FoamTextureSpeedY("Foam texture speed Y", float) = 0
        _FoamLinesSpeed("Foam lines speed", float) = 0
        _FoamIntensity("Foam intensity", float) = 1

        [Header(Rendering)]
        _RenderTexture("Render texture", 2D) = "black" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _FresnelPower("Fresnel power", float) = 1
        _Cube("Reflection Cubemap", CUBE) = "" {}

        [Header(Displacement)]
        _Direction("Direction", Vector) = (1.0, 0.0, 0.0, 1.0)
        _Steepness("Steepness", Range(0.000001, 0.0001)) = 0.5
        _Freq("Frequency", Range(100.0, 10000.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:blend vertex:vert

        #include "UnityCG.cginc"
        #include "Assets/Shaders/Noise/ClassicNoise3D.hlsl"

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

       struct Input
        {
            float4 screenPos;
            float3 worldPos;
            float3 localPos;
            float3 viewDir;
            //float3 worldRefl;
            //INTERNAL_DATA
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
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        static float TAU = 2 * UNITY_PI;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.localPos = v.vertex.xyz;
        
            // displacement
            float3 pos = v.vertex.xyz * _Freq + sin(_Time.y);
            float4 dir = normalize(_Direction);
            
            float default_wavelength = 2 * UNITY_PI;
            float wL = default_wavelength / _Freq;
            float phase = sqrt(9.8 / wL);
            float disp = wL * (dot(dir, pos) - (phase * _Time.y));
        
            float peak = _Steepness / wL; 
            //pos.x += dir.x * (peak * cos(disp));
            //pos.y = peak * sin(disp);
            //pos.z += dir.y * (peak * cos(disp));

            float3 displace = 0.0;
            float s = 1;
            float w = 0.5;
            const float epsilon = 0.0001;
            for (int i = 0; i < 6; i++)
            {
                float v0 = ClassicNoise(pos + float3(0.0, 0.0, 0.0));
                float vx = ClassicNoise(pos + float3(epsilon, 0.0, 0.0));
                float vy = ClassicNoise(pos + float3(0.0, epsilon, 0.0));
                float vz = ClassicNoise(pos + float3(0.0, 0.0, epsilon));
                displace += w * float3(vx - v0, vy - v0, vz - v0) / epsilon;

                s *= 2.0;
                w *= 0.5;
            }

            //v.vertex.xyz = pos;
            v.vertex.xyz += v.normal * displace.y * _Steepness;
        }



        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 pos = IN.localPos;

            float2 rt = pos.xz - _CamPosition.xz;
            rt = rt / (_OrthographicCamSize * 2);
            rt += 0.5;
            fixed4 renderTex = tex2D(_RenderTexture, rt);

            float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            depth = LinearEyeDepth(depth);

            float fogDiff = saturate((depth - IN.screenPos.w) / _DeepWaterThreshold);
            float intersectionDiff = saturate((depth - IN.screenPos.w) / _IntersectionThreshold);

			fixed4 final_colour = lerp(lerp(_IntersectionColour, _SurfaceColour, intersectionDiff), _DeepWaterColour, fogDiff);

			float foamDiff = saturate((depth - IN.screenPos.w) / _FoamThreshold);
			float foam = 0;
			if (foamDiff > 0)
			{
				foamDiff *= (1.0 - renderTex.b);

				float foamTex = tex2D(_FoamTexture, pos.xz * _FoamTexture_ST.xy + _Time.y * float2(_FoamTextureSpeedX, _FoamTextureSpeedY));
				foam = step(foamDiff - (saturate(sin((foamDiff - _Time.y * _FoamLinesSpeed) * 8 * UNITY_PI)) * (1.0 - foamDiff)), foamTex);
			}

            float fresnel = pow(1.0 - saturate(dot(o.Normal, normalize(IN.viewDir))), _FresnelPower);

            float2 spherical_proj = float2(saturate(((atan2(pos.z, pos.x) / UNITY_PI) + 1.0) / 2.0), (0.5 - (asin(pos.y) / UNITY_PI)));

            float a = frac(_Time.y * .1) * TAU;
            float2 uv = spherical_proj + (1 - spherical_proj * float2(sin(a) * 0.0008, cos(a) * 0.0008));

            float2 uv_offset = renderTex.rg + _Time.y * _NormalPanningVelocity.xy;

            float3 normalA = UnpackNormalWithScale(tex2D(_NormalA, uv * _NormalA_ST.xy + uv_offset), _NormalStrength);
            float3 normalB = UnpackNormalWithScale(tex2D(_NormalB, uv * _NormalB_ST.xy + uv_offset), _NormalBStrength);
            
			float3 normal = normalize(normalA + normalB);
			foam += normal.y;

			o.Albedo = final_colour.rgb /*+ texCUBE(_Cube, WorldReflectionVector(IN, o.Normal)).rgb * 0.0000000001*/
						+ foam * _FoamIntensity;
			o.Normal = normal;
			o.Smoothness = _Glossiness;
			o.Alpha = lerp(lerp(final_colour.a * fresnel, 1.0, foam), _DeepWaterColour.a, fogDiff);
            //o.Emission = foam * _FoamIntensity;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
