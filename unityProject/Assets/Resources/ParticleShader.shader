Shader "Custom/ParticleShader"{
    Properties{
    }
    SubShader{
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "RenderTexture"="True" "LightMode" = "ForwardBase"}
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

            

            [maxvertexcount(3672)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                g2f o;
                uint stackCount = 18;
                uint sectorCount = 36;
                const uint nbVertices = 703; // (1+stackCount)*(1+sectorCount);
                const uint nbIndices = 3672; // 3*2*(stackCount-2)*sectorCount + 2*3*sectorCount;
                float4 vertices[nbVertices];
                uint indices[nbIndices];

                // dumb initialization
                for(uint a=0; a<nbVertices; a++){
                    vertices[a] = float4(0,0,0,0);
                }
                for(uint b=0; b<nbIndices; b++){
                    indices[b] = 0;
                }
                
                float PI = 3.1416;
                float sectorStep = 2 * PI / sectorCount;
                float stackStep = PI / stackCount;
                float sectorAngle, stackAngle;
                float radius = 1.0f;

                // init vertices
                uint idx = 0;
                for(uint c = 0; c <= stackCount; c++){
                    stackAngle = PI / 2 - c * stackStep;        // starting from pi/2 to -pi/2
                    float xy = radius * cos(stackAngle);             // r * cos(u)
                    float z = radius * sin(stackAngle);              // r * sin(u)

                    // add (sectorCount+1) vertices per stack
                    // first and last vertices have same position and normal, but different tex coords
                    for(uint d = 0; d <= sectorCount; d++){
                        sectorAngle = d * sectorStep;           // starting from 0 to 2pi

                        // vertex position (x, y, z)
                        float x = xy * cos(sectorAngle);             // r * cos(u) * cos(v)
                        float y = xy * sin(sectorAngle);             // r * cos(u) * sin(v)

                        vertices[idx++] = float4(x,y,z,0)+input[0].vertex;
                    }
                }

                // init indices
                idx = 0;
                for(uint e=0; e<stackCount; e++){
                    uint k1 = e*(sectorCount+1);
                    uint k2 = k1 + sectorCount + 1;

                    for(uint f=0; f<sectorCount; f++, k1++, k2++){
                        // 2 triangles per sector excluding first and last stacks
                        // k1 => k2 => k1+1
                        if(e != 0){
                            indices[idx++] = k1;
                            indices[idx++] = k2;
                            indices[idx++] = k1 + 1;
                        }

                        // k1+1 => k2 => k2+1
                        if(e != (stackCount-1)){
                            indices[idx++] = k1 + 1;
                            indices[idx++] = k2;
                            indices[idx++] = k2 + 1;
                        }
                    }
                }

                // init triangles
                for(uint g=0; g<nbIndices; g+=3){
                    uint id1 = indices[g];
                    uint id2 = indices[g+1];
                    uint id3 = indices[g+2];

                    if(id1>nbVertices) id1 = 0;
                    if(id2>nbVertices) id2 = 0;
                    if(id3>nbVertices) id3 = 0;

                    o.vertex = vertices[id1];
                    o.id = input[0].id;
                    triStream.Append(o);

                    o.vertex = vertices[id2];
                    o.id = input[0].id;
                    triStream.Append(o);

                    o.vertex = vertices[id3];
                    o.id = input[0].id;
                    triStream.Append(o);
                    triStream.RestartStrip();
                }

                // o.vertex = UnityObjectToClipPos(input[0].vertex);
                // o.id = input[0].id;
                // triStream.Append(o);
                // triStream.RestartStrip();
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