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
                float3 normal : NORMAL;
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

            g2f GenerateTriangle(float4 center, float id, float3 v){
                float4 pos = float4(v, 1.0) + center;
                g2f o;
                o.vertex = UnityObjectToClipPos(pos);
                o.normal = v;
                o.id = id;
                return o;
            }            

            [maxvertexcount(60)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                // Define the icosahedron vertices
                float t = (1.0 + sqrt(5.0)) * 0.5;

                float3 icosahedronVertices[12] = {
                    float3(-1,  t,  0),
                    float3( 1,  t,  0),
                    float3(-1, -t,  0),
                    float3( 1, -t,  0),
                    float3( 0, -1,  t),
                    float3( 0,  1,  t),
                    float3( 0, -1, -t),
                    float3( 0,  1, -t),
                    float3( t,  0, -1),
                    float3( t,  0,  1),
                    float3(-t,  0, -1),
                    float3(-t,  0,  1)
                };

                // Define the icosahedron faces
                int3 icosahedronFaces[20] = {
                    int3( 0, 11,  5),
                    int3( 0,  5,  1),
                    int3( 0,  1,  7),
                    int3( 0,  7, 10),
                    int3( 0, 10, 11),
                    int3( 1,  5,  9),
                    int3( 5, 11,  4),
                    int3(11, 10,  2),
                    int3(10,  7,  6),
                    int3( 7,  1,  8),
                    int3( 3,  9,  4),
                    int3( 3,  4,  2),
                    int3( 3,  2,  6),
                    int3( 3,  6,  8),
                    int3( 3,  8,  9),
                    int3( 4,  9,  5),
                    int3( 2,  4, 11),
                    int3( 6,  2, 10),
                    int3( 8,  6,  7),
                    int3( 9,  8,  1)
                };

                // Generate vertices for each face
                for (int i = 0; i < icosahedronFaces.Length; i++){
                    int3 face = icosahedronFaces[i];

                    float3 v0 = normalize(icosahedronVertices[face.x]);
                    float3 v1 = normalize(icosahedronVertices[face.y]);
                    float3 v2 = normalize(icosahedronVertices[face.z]);

                    triStream.Append(GenerateTriangle(input[0].vertex, input[0].id, v0));
                    triStream.Append(GenerateTriangle(input[0].vertex, input[0].id, v1));
                    triStream.Append(GenerateTriangle(input[0].vertex, input[0].id, v2));
                    triStream.RestartStrip();
                }
            }


            fixed4 frag(g2f i) : SV_Target {
                // bool toDiscard = false;
                bool toDiscard = i.id >= _NbCurParticles-1;
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