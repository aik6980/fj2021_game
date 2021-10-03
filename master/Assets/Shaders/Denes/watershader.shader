Shader "Natural/Water"
{
	Properties
	{
		_WaterCol ("Water Color",Color) = (0.3,0.3,1.0,1.0)
		
		_Storm ("Storminess",Range(0.01,1.0)) = 0.5
		_WaveHeight ("Wave Height",Range(0.0,1.2)) = 1.0
		_WaveSpeed ("Ripple Speed",Range(0.0001,0.02)) = 0.01
		
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
		
		_BumpMap ("Wave Bump", 2D) = "bump" {}
		_BumpScale ("Wave Scales", Vector) = (1.0,2.0,3.0,4.0)
		
		_Cube ("Cubemap", CUBE) = "" {}
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}
		CGPROGRAM
		#pragma surface surf Custom 
		#pragma target 3.0
		#include "UnityCG.cginc"
		
		struct Input 
		{
			half waveAmount;
			float3 viewDir;
			float3 worldPos;
			float3 worldRefl;
			INTERNAL_DATA
		};
		
		uniform float4 S_CameraDirection;
		sampler2D _BumpMap;
		samplerCUBE _Cube;
		half _Shininess,_Storm,_BumpStrength,_WaveHeight,_WaveSpeed;
		fixed4 _BumpScale;
		fixed4 _WaterCol;
		
		//Custom Lighting
		inline fixed4 LightingCustom (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{
			fixed diff = max (0, dot (s.Normal, lightDir)*0.5+0.5);
			
			half3 halfVector = normalize (lightDir + viewDir);
			float halfDot = saturate (dot (s.Normal, halfVector)*lerp(1.001,1.006,_Storm));
			float spec = pow (halfDot, s.Specular*128.0) * s.Gloss;
			
			fixed4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * _SpecColor.rgb * spec) * (atten * 2);
			c.a = s.Alpha + _LightColor0.a * _SpecColor.a * spec * atten;
			return c;
		}
		
		//Surface
		void surf (Input IN, inout SurfaceOutput o)
		{
			float3 rgb2lum = float3(0.30,0.59,0.11);
			float camDist = distance(_WorldSpaceCameraPos,IN.worldPos);
			float distMod = (1.0-saturate((camDist-100.0)*0.001));
			float3 wp = IN.worldPos.xyz;
			
			float waveAmount;
			float pi = 3.14159265359;
			float pi2 = pi*2.0;
			float waveFreq = 0.4*_Storm;
			float waveLength = _Storm;
			float waveFreqRads = pi2*waveFreq;
			float waveNumber = pi2/waveLength;
			waveAmount = sin (waveNumber*wp.x*0.01 - waveFreqRads*_Time.y*0.5)-sin (waveNumber*wp.x*0.005 - waveFreqRads*_Time.y*0.5);
			waveAmount = waveAmount*_Storm;
			
			float2 offset = ParallaxOffset (waveAmount, _WaveHeight*_Storm, IN.viewDir)*distMod*4.0;
			
			float2 wwp = float2(IN.worldPos.x + offset.x,IN.worldPos.z + offset.y);
			float2 bumpUvs = float2(wwp.x,wwp.y);
			
			half time = _Time.y*lerp(_WaveSpeed*0.5,_WaveSpeed,_Storm);
			
			float4 bump1 = tex2D (_BumpMap, bumpUvs/_BumpScale.x+time);
			float4 bump2 = tex2D (_BumpMap, bumpUvs/_BumpScale.y+time);
			float4 bump3 = tex2D (_BumpMap, bumpUvs/_BumpScale.z+time);
			float4 bump4 = tex2D (_BumpMap, bumpUvs/_BumpScale.w+time);
			
			float4 bump = bump1+bump2+bump3+bump4;
			bump *= 0.25;
			
			float3 normals = UnpackNormal(bump);
			normals = lerp(float3(0.0,0.0,1.0),normals,lerp(0.02,0.80,_Storm));
			
			//o.Alpha = lerp(0.4,0.9,(camDist-2.0)*0.014);
			o.Gloss = _WaterCol.a;
			o.Normal = normalize(normals);
			
			float3 worldRefl = WorldReflectionVector (IN, o.Normal);
			fixed4 reflcol = texCUBE (_Cube, worldRefl);
			
			half rim = saturate(dot (normalize(IN.viewDir), o.Normal));
			
			o.Specular = pow(2.0,5.0-_Storm*3.0);
			o.Albedo = lerp(_WaterCol*1.5,_WaterCol,_Storm);
			half refPow = lerp(0.5,2.0,_Storm);
			o.Albedo = lerp (o.Albedo,reflcol.rgb*0.5,pow(1.0-rim,refPow)+pow(rim,refPow)*0.6);
		}
		ENDCG
	} 
	Fallback "Diffuse"
}