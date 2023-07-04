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

            #include "UnityCG.cginc"

            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct g2f{
                float4 vertex : SV_POSITION;
                float3 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            v2g vert(appdata v){
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream){
                g2f o;
                float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));

                for(int i=0; i<3; i++){
                    o.uv.x = input[i].uv.x;
                    o.uv.z = input[i].vertex.y;
                    o.uv.y = input[i].vertex.y - input[i].uv.y;
                    o.vertex = UnityObjectToClipPos(input[i].vertex);
                    o.normal = normal;
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

                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}