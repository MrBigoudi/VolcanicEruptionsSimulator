Shader "Custom/TerrainShader"{
    Properties {
        _SmoothingStrength("The smoothing strength", Float) = 0.1
    }
    SubShader{
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "RenderTexture"="True" "LightMode" = "ForwardBase"}
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2g{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct g2f{
                float4 vertex : SV_POSITION;
                float3 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            float _SmoothingStrength = 0.1;

            float3 SmoothHeight(v2g i[3], int idx){
                float height = i[idx].vertex.y;
                float avgHeight = (i[0].vertex.y + i[1].vertex.y + i[2].vertex.y) / 3.0;
                
                height = lerp(height, avgHeight, _SmoothingStrength);
                return float3(i[idx].vertex.x, height, i[idx].vertex.z);
            }

            v2g vert(appdata v){
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream){
                g2f o;
                float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));

                for(int i=0; i<3; i++){
                    o.uv.x = input[i].uv.x; // random value
                    o.uv.z = input[i].vertex.y; // height of the current terrain
                    o.uv.y = input[i].vertex.y - input[i].uv.y; // difference of height between current terrain and old terrain

                    o.vertex = UnityObjectToClipPos(SmoothHeight(input, i));
                    // o.vertex = UnityObjectToClipPos(input[i].vertex);
                    // o.normal = input[i].normal;
                    o.normal = normal;
                    // o.normal.y = normal.y;
                    triStream.Append(o);
                }
                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target {
                fixed4 green = fixed4(0.69, 0.86, 0.47, 1);
                fixed4 brown = fixed4(131.0/255, 101.0/255, 57.0/255, 1);
                fixed4 red   = fixed4(1, i.uv.y, 0, 1);

                // get color
                fixed4 col = green;
                if(i.uv.y != 0){
                    col = red;
                } else {
                    if(i.uv.z > 25+i.uv.x){
                        col = brown;
                    }
                }

                fixed light = saturate (dot (normalize(_WorldSpaceLightPos0), i.normal));
                col.rgb *= light;  

                // // col = fixed4(i.normal * 0.5 + 0.5, 1);
                // float r = 0;
                // // float r = i.normal.r > 0 ? 1 : 0;
                // float g = 0;
                // float b = 0;
                // // float b = i.normal.b > 0 ? 1 : 0;

                // if(i.normal.r < 0){
                //     b = i.normal.r * 0.5 + 0.5;
                // } else if (i.normal.r > 0){
                //     r = i.normal.r * 0.5 + 0.5;
                // } else {
                //     g = 0.5;
                // }

                // // r = i.normal.r == 0 ? 1 : 0;
                // // b = i.normal.b == 0 ? 1 : 0;
                // col = fixed4(r,g,b,1);

                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}