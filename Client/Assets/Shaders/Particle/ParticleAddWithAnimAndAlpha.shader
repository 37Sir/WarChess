// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KP/Effect/Particle/ParticleAddWithAnimAndAlpha"  
{
	Properties 
	{
		_MainTex ("Particle Texture", 2D) = "white" {}
		_SpeedX ("Speed X", float) = 0.0
		_SpeedY ("Speed Y", float) = 0.0
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	}
		
	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
		Pass 
		{
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;

			
			struct appdata_t 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;
			float _SpeedX;
			float _SpeedY;
			fixed4 _TintColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed x = frac(_SpeedX * _Time);
				fixed y = frac(_SpeedY * _Time);
				
				i.texcoord.x += x;
				i.texcoord.y += y;
				
				return i.color * _TintColor * tex2D(_MainTex, i.texcoord);
			}
			ENDCG 
		}
	}	
	
}
