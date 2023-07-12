Shader "Custom/ParticleShader"{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader{
        Tags {"Queue"="Transparent" "RenderType"="Opaque" "RenderTexture"="True"}
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            //texture and transforms of the texture
            sampler2D _MainTex;
            float4 _MainTex_ST;
 
            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2g {
                float4 vertex : SV_POSITION;
            };

            struct g2f {
                float4 worldPos : SV_POSITION;
            };

            v2g vert(appdata v) {
                v2g o;
                o.vertex = v.vertex;
                return o;
            }

            [maxvertexcount(1)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                g2f o;
                o.worldPos = UnityObjectToClipPos(input[0].vertex);
                triStream.Append(o);
                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target {
                fixed4 col = fixed4(i.worldPos.y/100, 1 - i.worldPos.y/100, 0, 1);
                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}