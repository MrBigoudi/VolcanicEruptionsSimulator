Shader "Custom/TerrainShader"{
    Properties {
    }
    SubShader{
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "RenderTexture"="True" "LightMode" = "ForwardBase"}
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            // #pragma target 5.0

            #include "UnityCG.cginc"

            StructuredBuffer<float> _TerrainHeights;
            StructuredBuffer<float> _InitialTerrainHeights;

            struct appdata{
                float4 vertex : POSITION;
                uint id : SV_VertexID;
                float3 normal : NORMAL;
            };

            struct v2g{
                float4 vertex : POSITION;
                float id : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct g2f{
                float4 vertex : SV_POSITION;
                float3 uv : TEXCOORD0; // uv = (id, deltaY, curHeight)
                float3 normal : NORMAL;
            };

            v2g vert(appdata v){
                v2g o;
                o.vertex = v.vertex;
                o.vertex.y = _TerrainHeights[v.id];
                o.id = v.id;
                o.normal = v.normal;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream){
                g2f o;
                float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));

                for(int i=0; i<3; i++){
                    float id = input[i].id;
                    float deltaY = input[i].vertex.y - _InitialTerrainHeights[(uint)input[i].id];
                    float curHeight = input[i].vertex.y;
                    o.uv = float3(id, deltaY, curHeight);
                    o.vertex = UnityObjectToClipPos(input[i].vertex);
                    o.normal = normal;
                    // o.normal = input[i].normal;
                    triStream.Append(o);
                }
                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target {
                float deltaY = i.uv.y; // difference of height between current terrain and old terrain
                bool updated = (deltaY >= 0.1f || deltaY <= -0.1f);
                bool isBrown = i.uv.z > 25;

                fixed4 green = fixed4(0.69, 0.86, 0.47, 1);
                fixed4 brown = fixed4(131.0/255, 101.0/255, 57.0/255, 1);
                fixed4 red   = fixed4(1, deltaY, 0, 1);

                // get color
                fixed4 col = green;
                if(updated){
                    col = red;
                } else {
                    if(isBrown){
                        col = brown;
                    }
                }

                fixed light = saturate (dot (normalize(_WorldSpaceLightPos0), i.normal));
                col.rgb *= light;

                // col = updated ? col : fixed4(i.normal * 0.5 + 0.5, 1);

                // if(!updated){
                //     // float r = 0;
                //     float r = i.normal.r > 0 ? i.normal.r : 0;
                //     float g = i.normal.r > 0 ? 0 : -i.normal.r;
                //     float b = 0;
                //     // float b = i.normal.b > 0 ? 1 : 0;

                //     // if(i.normal.r < 0){
                //     //     b = i.normal.r * 0.5 + 0.5;
                //     // } else if (i.normal.r > 0){
                //     //     r = i.normal.r * 0.5 + 0.5;
                //     // } else {
                //     //     g = 0.5;
                //     // }

                //     // r = i.normal.r == 0 ? 1 : 0;
                //     // b = i.normal.b == 0 ? 1 : 0;
                //     col = fixed4(r,g,b,1);
                // }

                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}