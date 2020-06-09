// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Toon Poster" {
	Properties{
		_UnshadeFactor("Tooniness", Range(0, 1)) = 1
		_Ramp("Light Ramp", 2D) = "white" {}
		_MainTex("Base (RGB)", 2D) = "white" { }
		_Color("Color", Color) = (1, 1, 1, 1)
		_Specularity("Specularity", Range(0, 1)) = 0
		_HighlightStrength("Highlight Strength", Range(0, 10)) = 1
		_EdgeBias("Edge Bias", Range(0, 1)) = 0.6

		_BumpMap("Normal Map", 2D) = "bump" {}
		_MetallicGlossMap("Metallic", 2D) = "black" {}
	}

		SubShader{
			Tags {
				"RenderType" = "Opaque"
				"Queue" = "Geometry+0"
			}

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"
			#include "UnityStandardUtils.cginc"
			#include "Lighting.cginc"

			#pragma target 3.0
			#pragma surface surf ToonPoster fullforwardshadows

			struct Input {
				float2 uv_BumpMap;
				float2 uv_MainTex;
				float2 uv_MetallicGlossMap;
				float3 viewDir;
				float4 screenPos;
			};

			half _UnshadeFactor;
			sampler2D _Ramp;
			half _HighlightStrength;
			half _EdgeBias;
			half _Specularity;
			half4 _Color;

			sampler2D _BumpMap;
			sampler2D _MainTex;
			sampler2D _MetallicGlossMap;

			sampler2D _CameraDepthNormalsTexture;
			float4 _CameraDepthNormalsTexture_TexelSize;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			half4 LightingToonPoster(SurfaceOutputStandard surf, float3 viewDir, UnityGI gi) {
				// Material properties
				half3 specular;
				half oneMinusReflectivity;
				DiffuseAndSpecularFromMetallic(surf.Albedo, surf.Metallic, /*out*/ specular, /*out*/ oneMinusReflectivity);
				float3 reflected = normalize(-reflect(gi.light.dir, surf.Normal));
				float specularity = dot(viewDir, reflected) * _Specularity;

				// Light setup
				float ndotl = dot(surf.Normal, gi.light.dir) * 0.5 + 0.5;
				float lightIncidence = gi.light.color * ndotl;
				float3 ramp = tex2D(_Ramp, lightIncidence.xx).rgb;
				float3 specularRamp = tex2D(_Ramp, specularity.xx).rgb;
				gi.light.color = ramp * _LightColor0.rgb;

				float rim = 1 - saturate(dot(surf.Normal, viewDir));
				float facing = -dot(viewDir, gi.light.dir) * 0.5 + 0.25;

				// sigmoid of rim for smooth falloff centered at _EdgeBias
				float outline = 1 / (1 + exp(-100 * (rim - _EdgeBias)));
				float highlight = _HighlightStrength * outline * facing * outline;

				float4 std = LightingStandard(surf, viewDir, gi);

				float3 toonDiffuse = surf.Albedo * (gi.light.color / 2 + ShadeSH9(float4(viewDir, 1)));
				float3 toonSpecular = specular * gi.indirect.specular * specularRamp;
				float4 toon = half4(toonDiffuse + toonSpecular, surf.Alpha);
				return lerp(std, toon, _UnshadeFactor) * half4(1 + highlight.xxx, 1);
			}

			inline void LightingToonPoster_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi) {
				LightingStandard_GI(s, data, gi);
			}

			void surf(Input IN, inout SurfaceOutputStandard o) {
				half4 albedo = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = albedo.rgb * _Color.rgb;
				o.Alpha = albedo.a * _Color.a;
				o.Normal += UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
				half4 metalSample = tex2D(_MainTex, IN.uv_MetallicGlossMap);
				o.Metallic = metalSample.r;
				o.Smoothness = 1 - metalSample.a;
			}

			ENDCG
	}
		Fallback "Diffuse"
}