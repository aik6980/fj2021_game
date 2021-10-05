Shader "Particles/CustomLitParticlesSurface" 
{

	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_AddColor("AddColor", Color) = (0,0,0,0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpTex("Normal Map (RGB)", 2D) = "bump" {}
		_BumpScale ("Scale", Float) = 1.0
		_Cutoff("Cutoff" , Range(0,1)) = .4
	}

	SubShader 
	{

	Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
	//Tags{ "Queue" = "Geometry" }

	LOD 200
	ZWrite Off

	CGPROGRAM
	// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Lambert vertex:vert alpha:fade //AlphaTest:_Cutoff

	// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0
	sampler2D _MainTex;
	sampler2D _BumpTex;
	float4 _BumpTex_ST;
	half _BumpScale;

	struct Input 
	{
		float2 uv_MainTex;
		//float3 normal;
		//float4 lpos;
		fixed4 vcolor;
	};

	fixed4 _Color;
	fixed4 _AddColor;

	void vert(inout appdata_full v, out Input o) 
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);
		//o.normal = v.normal;
		//o.lpos = v.vertex;
		o.vcolor = v.color;
	}

	inline fixed3 CombineNormalMaps(fixed3 base, fixed3 detail) 
	{
		base += fixed3(0, 0, 1);
		detail = fixed3(-1, -1, 1);
		return base * dot(base, detail) / base.z - detail;
	}

	void surf (Input IN, inout SurfaceOutput o) 
	{
		float4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.vcolor * _Color;
		//--------------------------------------------------------------
		// o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_MainTex_BumpTex_ST.xy)); —> Remplacé par les 2 lignes suivantes
		half4 normalMap = tex2D(_BumpTex, IN.uv_MainTex*_BumpTex_ST.xy);
		o.Normal = UnpackScaleNormal(normalMap, _BumpScale);
		//--------------------------------------------------------------
		o.Albedo = c.rgb;
		o.Alpha = c.a;
		o.Emission = _AddColor;
	}

	ENDCG
	}
	//Fallback "Diffuse"
	Fallback "Transparent/Cutout/VertexLit"
}