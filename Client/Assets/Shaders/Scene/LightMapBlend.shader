Shader "KP/Scene/Transparent/LightMap Blend" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}
	
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 2000

		CGPROGRAM
		#pragma surface surf Lambert alpha

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Transparent/VertexLit"

	SubShader 
	{
//		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off 
		Fog { Mode off }
		Alphatest Greater [_Cutoff]

		// Non-lightmapped
		Pass 
		{
			Tags { "LightMode" = "Vertex" }
			SetTexture [_MainTex] 
			{ 
				constantColor [_Color]
				combine texture * constant 
			} 
		}
	
		// Lightmapped, encoded as dLDR
		Pass 
		{
//			Tags { "LightMode" = "VertexLM" }
			Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "LightMode" = "VertexLM" }

			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord", texcoord1 // main uses 1st uv
			}
		
			SetTexture [unity_Lightmap] 
			{
				matrix [unity_LightmapMatrix]
				combine texture
			}
			
			SetTexture [_MainTex] 
			{
				combine texture * previous DOUBLE, texture * primary
			}
			
			SetTexture [_MainTex] 
			{
				constantColor [_Color]
				combine previous * constant
			}
		}
	
		// Lightmapped, encoded as RGBM
		Pass 
		{
			Tags { "LightMode" = "VertexLMRGBM" }

			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord", texcoord1 // main uses 1st uv
			}
		
			SetTexture [unity_Lightmap] 
			{
				matrix [unity_LightmapMatrix]
				combine texture * texture alpha DOUBLE
			}
			
			SetTexture [_MainTex] 
			{
				combine texture * previous QUAD, texture * primary
			}
			
			SetTexture [_MainTex] 
			{
				constantColor [_Color]
				combine previous * constant
			}
		}	
	}
}
