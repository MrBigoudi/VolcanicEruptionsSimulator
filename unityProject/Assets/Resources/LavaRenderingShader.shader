Shader "Custom/LavaRenderingShader"{
    Properties{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "black" {}
    }
    SubShader{
 
        Pass{
            CGPROGRAM
            #pragma vertex vert
			#pragma geometry geom
            #pragma fragment frag

			#include "UnityCG.cginc"

			//texture and transforms of the texture
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
 
            struct appdata {
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0; 
			};

            struct v2g {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

			struct g2f {
				float4 worldPos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

            v2g vert(appdata v) {
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

			[maxvertexcount(6)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                g2f o;
				float length = 0.5f;
				float4 pos;

				// first triangle
				// bottom left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y + input[0].uv.x, input[0].vertex.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(input[0].uv, _MainTex);
				triStream.Append(o);
				// top left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y + input[0].uv.x, input[0].vertex.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(input[0].uv, _MainTex);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y + input[0].uv.x, input[0].vertex.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(input[0].uv, _MainTex);
				triStream.Append(o);
				triStream.RestartStrip();

				// second triangle
				// top left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y + input[0].uv.x, input[0].vertex.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(input[0].uv, _MainTex);
				triStream.Append(o);
				// top right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y + input[0].uv.x, input[0].vertex.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(input[0].uv, _MainTex);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y + input[0].uv.x, input[0].vertex.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.uv = TRANSFORM_TEX(input[0].uv, _MainTex);
				triStream.Append(o);
				triStream.RestartStrip();
			}

            fixed4 frag(v2g i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
                return col*_Color;
            }

            ENDCG
		}
    }
    FallBack "Diffuse"
}