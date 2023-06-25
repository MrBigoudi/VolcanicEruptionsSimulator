Shader "Custom/LavaRenderingShader"{
    Properties{
    }
    SubShader{
 
        Pass{
            CGPROGRAM
            #pragma vertex vert
			#pragma geometry geom
            #pragma fragment frag
 
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
				pos = float4(input[0].vertex.x-length, input[0].vertex.y, input[0].vertex.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				triStream.Append(o);
				// top left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y, input[0].vertex.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y, input[0].vertex.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				triStream.Append(o);
				triStream.RestartStrip();

				// second triangle
				// top left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y, input[0].vertex.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				triStream.Append(o);
				// top right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y, input[0].vertex.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y, input[0].vertex.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				triStream.Append(o);
				triStream.RestartStrip();
			}

            fixed4 frag(g2f i) : SV_Target {
                return fixed4(1, 1, 1, 1);
            }

            ENDCG
		}
    }
    FallBack "Diffuse"
}
