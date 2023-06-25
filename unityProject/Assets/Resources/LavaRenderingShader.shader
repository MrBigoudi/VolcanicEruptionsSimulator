Shader "Custom/LavaRenderingShader"{
    Properties{
    }
    SubShader{
 
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            struct appdata {
				float4 vertex : POSITION;
				float4 particlePosition : TEXCOORD0;
				float particleHeight : TEXCOORD1;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex.xyz = UnityObjectToClipPos(v.particlePosition);
				o.vertex.y += v.particleHeight;
				return o;
			}

			half4 frag(v2f i) : SV_Target {
				return half4(1, 1, 1, 1);
			}

            ENDCG
		}
    }
    FallBack "Diffuse"
}
