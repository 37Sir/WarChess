// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KP/UVAnim_NGUI" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Speed ("Speed", Range (0,1)) = 0
		_YOffset ("YOffset",  float) = 0
		_YValue ("Value", float) = 6000
		_Alpha ("Alpha", float) = 1
	}
	SubShader 
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			ColorMask RGB
			AlphaTest Greater .01
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Speed;
			float _YOffset;
			float _YValue;
			float _Alpha;
			
			struct v2f 
			{
			    float4  pos : SV_POSITION;
			    float2  uv : TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
			    v2f o;
			   	o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
			    return o;
			}

			float4 frag (v2f i) : COLOR
			{
				float2 uv = i.uv;
				uv.x = frac(uv.x - _Time * _Speed);
				uv.y = frac(uv.y - _YOffset /_YValue);
				float4 texCol = tex2D(_MainTex,uv) * _Alpha;
			    return texCol;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
