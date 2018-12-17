// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KP/Effect/Melt"
{ 
 	Properties 
 	{ 
        _MainTex 	("_MainTex", 2D) = "white" {}
        _EdgeColor 	("_EdgeColor", Color) = (1,0,0,0)
		_MeltTex 	("_MeltTex", 2D) = "white" {}
		_Melt 		("_Melt", Range (0, 1)) = 1.0
    } 
   
    SubShader 
    { 
        Pass 
        {
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			
			ZWrite On
			Alphatest Greater 0.01
			Blend SrcAlpha OneMinusSrcAlpha
        
            CGPROGRAM 
            // use "vert" function as the vertex shader
            #pragma vertex vert
            // use "frag" function as the pixel (fragment) shader
            #pragma fragment frag
            #include "UnityCG.cginc" 
           

            sampler2D _MainTex; 
            float4 _MainTex_ST; 
            
            sampler2D _MeltTex;
            float4 _MeltTex_ST;
            
            float4 _EdgeColor;
            
            float _Melt;
            
			struct appdata
            {
                float4 vertex : POSITION; // vertex position
                float2 uv : TEXCOORD0; // texture coordinate
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; // texture coordinate
                float4 vertex : SV_POSITION; // clip space position
            };

            // vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                // transform position to clip space
                // (multiply with model*view*projection matrix)
                o.vertex = UnityObjectToClipPos(v.vertex);
                // just pass the texture coordinate
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            { 
                float4 col = tex2D(_MainTex, i.uv); 

                float melt = saturate(tex2D (_MeltTex, i.uv).r * 8.0 - 8.0 + _Melt * 9.0);
                melt = melt * melt;
                
                col.a = saturate(melt * 2.0 - 1.0);
                
                float4 edge = _EdgeColor;
                edge.a = 1.0 - abs(melt * 2.0 - 1.0);
                
				col.rgb = col.rgb * col.a + edge.rgb * edge.a * 2.0;
				col.a = saturate(edge.a + col.a);

                return col;
            } 
            ENDCG 
        } 
    } 
    FallBack "VertexLit" 
} 
