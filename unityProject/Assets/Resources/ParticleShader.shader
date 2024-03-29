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

            /**
             * From the compute shader, buffer containing particles' positions
            */
            StructuredBuffer<float3> _ParticlesPositions;

            /**
             * From the compute shader, buffer containing particles' temperatures
            */
            StructuredBuffer<float> _ParticlesTemperatures;

            /**
             * The current number of particles
            */
            int _NbCurParticles;

            /**
             * A floating point choosed by the user to move particles up and down
            */
            float _ParticlesMeshHeights;

            /**
             * A boolean to display particles or not
            */
            int _DisplayParticles;

            /**
             * The input data for the shader
            */
            struct appdata {
                float4 vertex : POSITION; // the particle position in object space
                uint id : SV_VertexID; // the id of the current particle
            };

            /**
             * The output of the vertex shader and the input of the geometry shader
            */
            struct v2g {
                float4 vertex : POSITION; // the particle position in object space
                float id : TEXCOORD0; // the id of the current particle
            };

            /**
             * The output of the geometry shader and the input of the fragment shader
            */
            struct g2f {
                float4 vertex : SV_POSITION; // the particle position in world space
                float id : TEXCOORD0; // the id of the current particle
                float3 normal : NORMAL; // the normal of the vertex
            };

            /**
             * A pass through vertex shader
            */
            v2g vert(appdata v) {
                v2g o;
                float3 newVert = _ParticlesPositions[v.id];
                o.vertex = float4(newVert.x, newVert.y, newVert.z, 1);
                o.id = v.id;
                return o;
            }

            /**
             * A geometry shader creating face normals and creating icospheres for each incoming vertex
            */
            [maxvertexcount(60)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                g2f o;
                float radius = 1.0f;
                float4 pos;

                const float PI = 3.1415926f;
                const float H_ANGLE = PI / 180 * 72;    // 72 degree = 360 / 5
                const float V_ANGLE = atan(1.0f / 2);  // elevation = 26.565 degree

                // the vertices of the icosphere
                float4 vertices[12] = {
                    float4(0,0,radius,0), 
                    float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), 
                    float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), float4(0,0,0,0), 
                    float4(0,0,-radius,0)
                };

                // the indices of the vertices in order to draw the sphere
                uint indices[60] = {
                    0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 1,
                    1, 6, 2, 2, 7, 3, 3, 8, 4, 4, 9, 5, 5,10, 1,
                    6, 7, 2, 7, 8, 3, 8, 9, 4, 9,10, 5,10, 6, 1,
                    6,11, 7, 7,11, 8, 8,11, 9, 9,11,10,10,11, 6
                };

                int i1, i2;                             // indices
                float z, xy;                            // coords
                float hAngle1 = -PI / 2 - H_ANGLE / 2;  // start from -126 deg at 1st row
                float hAngle2 = -PI / 2;                // start from -90 deg at 2nd row

                // compute 10 vertices at 1st and 2nd rows
                for(int i = 1; i < 6; i++){
                    i1 = i;         // index for 1st row
                    i2 = (i + 5);   // index for 2nd row

                    z  = radius * sin(V_ANGLE);            // elevaton
                    xy = radius * cos(V_ANGLE);            // length on XY plane

                    vertices[i1].x = xy * cos(hAngle1); // x
                    vertices[i2].x = xy * cos(hAngle2);
                    vertices[i1].y = xy * sin(hAngle1); // y
                    vertices[i2].y = xy * sin(hAngle2);
                    vertices[i1].z = z;                 // z
                    vertices[i2].z = -z;

                    // next horizontal angles
                    hAngle1 += H_ANGLE;
                    hAngle2 += H_ANGLE;
                }

                // create the new triangles
                for(int id=0; id<60; id+=3){
                    // get normal of faces
                    float4 v1 = vertices[indices[id]];
                    float4 v2 = vertices[indices[id+1]];
                    float4 v3 = vertices[indices[id+2]];
                    // create the face normal
                    float3 normal = normalize(cross(v2 - v1, v3 - v1));

                    for(int j=0; j<3; j++){
                        pos = vertices[indices[id+j]] + input[0].vertex + float4(0,_ParticlesMeshHeights,0,0);
                        pos.y += radius;
                        o.vertex = UnityObjectToClipPos(pos);
                        o.id = input[0].id;
                        o.normal = normal;
                        triStream.Append(o);
                    }
                    triStream.RestartStrip();
                }
            }


            /**
             * A fragment shader coloring fragments depending on the particles' ids (uncomment to color in term of particle temperatures)
            */
            fixed4 frag(g2f i) : SV_Target {
                // discard fragments of particles whith indices greater than the current number of particles (or if the ser doen't want to display particles)
                bool toDiscard = i.id >= _NbCurParticles || _DisplayParticles == 0;
                if(toDiscard){
                    discard;
                }
                fixed4 col = fixed4(i.id/_NbCurParticles,1,1,1); // color depending on ids
                // fixed4 red = fixed4(1, _ParticlesTemperatures[(int)i.id] / 200.0f, 0, 1); // color depending on temperature
                // col = red;

                // light calculation using new normals
                fixed light = saturate (dot (normalize(_WorldSpaceLightPos0), i.normal));
                col.rgb *= light;

                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}