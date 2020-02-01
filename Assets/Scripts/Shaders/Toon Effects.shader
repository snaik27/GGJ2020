Shader "Sid/Toon Effects"
{
    Properties
	{
		[Header(Albedos)]
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_DetailTex("Detail Tex", 2D) = "white" {}
		_Mix("Mix", Range(0,1)) = 0.5
		_Cutout("Detail Cutout", Range(0,1)) = 0.5
		_Width("'Pixel' Width", Float) = 20
		_Height(" 'Pixel' Height", Float) = 20

		[Space]
		[Header(Normal)]
		_NormalStrength("Normal Strength", Float) = 1
		_Normal("Normal", 2D) = "bump"{}
		[Space]
		[Header(Emission)]
		_EmissionColor("Emission Color", Color) = (0,0,0,1)
		[HDR]_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimStrength("Rim Strength", Range(0,3)) = 0.5
		_Cutoff("Cutoff", Range(0,1)) = 1
		[Space]
		[Header(Shadows)]
		[HDR]_ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
		_ShadowThresh("Shadow Threshold", Range(0,2)) = 1
		_ShadowSmooth("Shadow Smoothness", Range(0.5	,1)) = 0.6
		[Space]
		[Header(Dissolve)]
		_DissolveTex("Dissolve Tex", 2D) = "white" {}
		_DissolveAmount("Dissolve Amount", Range(0,1)) = 0.5
		_DissolveScale("Dissolve Scale", Float) = 1
		_DissolveLine("Dissolve Line", Range(0,0.2)) = 0.1
		[HDR] _DissolveLineColor("Dissolve Line Color", Color) = (1,1,1,1)
		[Space]
		[Header(Gloss)]
		_Gloss("Glossiness", Range(0,20)) = 0
		_GlossSmoothness("Gloss Smoothness", Range(0,2)) = 0
		[HDR] _GlossColor("Gloss Color", Color) = (1,1,1,1)



		_WaveA("Wave A (dir, steepness, wavelength)", Vector) = (1,0,0.5,10)
		_WaveB("Wave B", Vector) = (0,1,0.25,20)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent"  "Queue" = "Transparent"}
		
        CGPROGRAM
		#include "Common.cginc"
        #pragma surface surf Toon fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _DetailTex;
		sampler2D _Normal;
		sampler2D _DissolveTex;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_DetailTex;
			float2 uv_Normal;
			half3 viewDir;
        };

        fixed4 _Color;
		float4 _WaveA;
		float4 _WaveB;
		float4 _WaveC;
		float _Mix;
		float _NormalStrength;
		float _RimStrength;
		float _DissolveScale;
		float _Width;
		float _Height;
		half4 _EmissionColor;
		half4 _RimColor;
		half3 _ShadowColor;
		half3 _DissolveLineColor;
		half3 _GlossColor;
		half _Cutoff;
		half _ShadowThresh;
		half _ShadowSmooth;
		half _Cutout;
		half _DissolveAmount;
		half _DissolveLine;
		half _Gloss;
		half _GlossSmoothness;
		


		float3 GerstnerWave(
			float4 wave, float3 p, inout float3 tangent, inout float3 binormal
		) {
			float steepness = wave.z;
			float wavelength = wave.w;
			float k =  UNITY_PI / wavelength;
			float c = sqrt(9.8 / k);
			float2 d = normalize(wave.xy);
			float f = k * (dot(d, p.xz) - c * _Time.y);
			float a = steepness / k;

			tangent += float3(
				-d.x * d.x * (steepness * sin(f)),
				d.x * (steepness * cos(f)),
				-d.x * d.y * (steepness * sin(f))
				);
			binormal += float3(
				-d.x * d.y * (steepness * sin(f)),
				d.y * (steepness * cos(f)),
				-d.y * d.y * (steepness * sin(f))
				);
			return float3(
				//d.x * (a * cos(f)),
				(a * cos(f)),
				a * sin(f),
				//d.y * (a * cos(f))
				(a * cos(f))
				);
		}



		void vert(inout appdata_full vertexData) {
			float3 gridPoint = vertexData.vertex.xyz;
			float3 tangent = 0;
			float3 binormal = 0;
			float3 p = gridPoint;
			p += GerstnerWave(_WaveA, gridPoint, tangent, binormal);
			p += GerstnerWave(_WaveB, gridPoint, tangent, binormal);
			float3 normal = normalize(cross(binormal, tangent));
			vertexData.vertex.xyz = p;
		}
	
		
       void surf (Input IN, inout SurfaceOutput o)
        {
		   InitLightingToon(_ShadowThresh, _ShadowSmooth, _ShadowColor, _Gloss, _GlossSmoothness, _GlossColor, IN.uv_MainTex);
		   float2 position = IN.uv_MainTex;
		   position *= float2(_Width, _Height);
		   position = ceil(position);
		   position /= float2(_Width, _Height);


		   half4 mainTex = tex2D(_MainTex, position);
		   half4 detailTex = tex2D(_DetailTex, position);
		   o.Albedo = lerp(mainTex.rgb, detailTex.rgb, _Mix) * _Color.rgb;
		   o.Alpha = mainTex.a * _Color.a;


		   clip(detailTex.a - _Cutout);
		   o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
		   o.Normal.z *= _NormalStrength;
		   o.Emission = mainTex * detailTex * _EmissionColor;

		   half d = pow(1 - dot(o.Normal, IN.viewDir), 1/_RimStrength);
		   o.Emission += _RimColor * d * smoothstep(0.5, _Cutoff, d);

		   half4 noise = tex2D(_DissolveTex, IN.uv_MainTex * _DissolveScale);
		   clip(noise.r - _DissolveAmount);

		   o.Emission += step(noise.r, _DissolveAmount + _DissolveLine) * _DissolveLineColor;
        }



	  
        ENDCG
    }
    FallBack "Diffuse"
}
