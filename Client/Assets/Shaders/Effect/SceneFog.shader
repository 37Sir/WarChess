// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KP/Effect/SceneFog" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	Category 
	{
		Tags {"Queue"="Transparent" "RenderType" = "Transparent" "IgnoreProjector"="True"}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
//		ZTest Always
		
		SubShader 
		{
			Pass 
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				sampler2D _MainTex;
				fixed4 _Color;

				struct appdata
				{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vert (appdata v)
				{
					v2f o;

					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.texcoord.xy;

					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					half4 col = _Color * tex2D(_MainTex, i.uv);
					return col;
				}
				
				ENDCG
			}
		}
    }
    Fallback "Transparent/VertexLit"
}