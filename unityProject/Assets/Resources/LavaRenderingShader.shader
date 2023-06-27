Shader "Custom/LavaRenderingShader"{
    Properties{
    }
    SubShader{
		Tags {"Queue"="Transparent" "RenderType"="Opaque" "RenderTexture"="True"}
        Pass{
            CGPROGRAM
            #pragma vertex vert
			#pragma geometry geom
            #pragma fragment frag

			uniform float dt;
 
            struct appdata {
				float4 vertex : POSITION;
                float2 heights : TEXCOORD0;
				float2 terrain : TEXCOORD1;
				float3 heightsGradients : TEXCOORD2;
				float3 terrainGradients : TEXCOORD3;
			};

            struct v2g {
                float4 vertex : SV_POSITION;
                float2 heights : TEXCOORD0;
				float2 terrain : TEXCOORD1;
				float3 heightsGradients : TEXCOORD2;
				float3 terrainGradients : TEXCOORD3;
            };

			struct g2f {
				float4 worldPos : SV_POSITION;
				float2 heights : TEXCOORD0;
				float2 terrain : TEXCOORD1;
				float3 heightsGradients : TEXCOORD2;
				float3 terrainGradients : TEXCOORD3;
			};

			float4 GetNewPos(appdata v){
				float3 velocity = (-9.81f/50.0f) * (v.heightsGradients + v.terrainGradients);
				float4 pos = float4(v.vertex + dt*velocity, 1);
				// TODO: get height from staggered grid
				return pos;
			}

            v2g vert(appdata v) {
                v2g o;
                o.vertex = GetNewPos(v);

				o.heights          = v.heights;
				o.heightsGradients = v.heightsGradients;

				o.terrain          = v.terrain;
				o.terrainGradients = v.terrainGradients;

                return o;
            }

			g2f GetG2f(float4 pos, v2g v){
				g2f o;
				o.worldPos = UnityObjectToClipPos(pos);
				o.heights          = v.heights;
				o.heightsGradients = v.heightsGradients;
				o.terrain          = v.terrain;
				o.terrainGradients = v.terrainGradients;
				return o;
			}

			[maxvertexcount(6)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                g2f o;
				float length = 0.1f;
				float4 pos;

				// first triangle
				// bottom left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y + input[0].heights.x, input[0].vertex.z-length, 1);
				o = GetG2f(pos, input[0]);
				triStream.Append(o);
				// top left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y + input[0].heights.x, input[0].vertex.z+length, 1);
				o = GetG2f(pos, input[0]);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y + input[0].heights.x, input[0].vertex.z-length, 1);
				o = GetG2f(pos, input[0]);
				triStream.Append(o);
				triStream.RestartStrip();

				// second triangle
				// top left
				pos = float4(input[0].vertex.x-length, input[0].vertex.y + input[0].heights.x, input[0].vertex.z+length, 1);
				o = GetG2f(pos, input[0]);
				triStream.Append(o);
				// top right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y + input[0].heights.x, input[0].vertex.z+length, 1);
				o = GetG2f(pos, input[0]);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].vertex.x+length, input[0].vertex.y + input[0].heights.x, input[0].vertex.z-length, 1);
				o = GetG2f(pos, input[0]);
				triStream.Append(o);
				triStream.RestartStrip();
			}

            fixed4 frag(v2g i) : SV_Target {
				fixed4 col = fixed4(i.heights.x, i.heights.x, i.heights.x, 1);
                return col;
            }

            ENDCG
		}
    }
    FallBack "Diffuse"
}