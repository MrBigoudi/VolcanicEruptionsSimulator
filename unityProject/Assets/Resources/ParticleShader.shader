Shader "Custom/ParticleShader"{
    Properties{
    }
    SubShader{
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            StructuredBuffer<float3> _ParticlesPositions;
            int _NbCurParticles;

            struct appdata {
                float4 vertex : POSITION;
                uint id : SV_VertexID;
            };

            struct v2g {
                float4 vertex : POSITION;
                float id : TEXCOORD0;
            };

            struct g2f {
                float4 vertex : SV_POSITION;
                float id : TEXCOORD0;
            };

            v2g vert(appdata v) {
                v2g o;
                float3 newVert = _ParticlesPositions[v.id];
                o.vertex = float4(newVert.x, newVert.y, newVert.z, 1);
                // o.vertex = v.vertex;
                // o.vertex.y += v.id;
                o.id = v.id;
                return o;
            }

            [maxvertexcount(6)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                g2f o;
                float length = 5.0f;
                float4 pos;

                // first triangle
                // bottom left
                pos = float4(input[0].vertex.x-length, input[0].vertex.y, input[0].vertex.z-length, 1);
                o.vertex = UnityObjectToClipPos(pos);
                o.id = input[0].id;
                triStream.Append(o);
                // top left
                pos = float4(input[0].vertex.x-length, input[0].vertex.y, input[0].vertex.z+length, 1);
                o.vertex = UnityObjectToClipPos(pos);
                o.id = input[0].id;
                triStream.Append(o);
                // bottom right
                pos = float4(input[0].vertex.x+length, input[0].vertex.y, input[0].vertex.z-length, 1);
                o.vertex = UnityObjectToClipPos(pos);
                o.id = input[0].id;
                triStream.Append(o);
                triStream.RestartStrip();

                // second triangle
                // top left
                pos = float4(input[0].vertex.x-length, input[0].vertex.y, input[0].vertex.z+length, 1);
                o.vertex = UnityObjectToClipPos(pos);
                o.id = input[0].id;
                triStream.Append(o);
                // top right
                pos = float4(input[0].vertex.x+length, input[0].vertex.y, input[0].vertex.z+length, 1);
                o.vertex = UnityObjectToClipPos(pos);
                o.id = input[0].id;
                triStream.Append(o);
                // bottom right
                pos = float4(input[0].vertex.x+length, input[0].vertex.y, input[0].vertex.z-length, 1);
                o.vertex = UnityObjectToClipPos(pos);
                o.id = input[0].id;
                triStream.Append(o);
                triStream.RestartStrip();
            }


            fixed4 frag(g2f i) : SV_Target {
                // bool toDiscard = false;
                bool toDiscard = i.id >= _NbCurParticles;
                if(toDiscard){
                    discard;
                }
                fixed4 col = fixed4(i.id/_NbCurParticles,1,1,1);
                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}