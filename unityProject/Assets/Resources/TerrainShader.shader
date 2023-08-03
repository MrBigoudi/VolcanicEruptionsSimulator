Shader "Custom/TerrainShader"{
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
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

            /**
             * The terrain texture
            */
            sampler2D _MainTex;
            
            /**
             * From the compute shader, buffer containing the current terrain heights
            */
            StructuredBuffer<float> _TerrainHeights;

            /**
             * From the compute shader, buffer containing the initial terrain heights
            */
            StructuredBuffer<float> _InitialTerrainHeights;

            /**
             * From the compute shader, buffer containing the current terrain temperatures
            */
            StructuredBuffer<float> _TerrainTemperatures;

            /**
             * Tells if the texture should be used (the texture is for the St Helen terrain)
            */
            uniform int _UseTerrainTexture;

            /**
             * User defined variable to control the shade of the lava color
            */
            uniform float _ColorShade;

            /**
             * The input data for the shader
            */
            struct appdata{
                float4 vertex : POSITION; // vertex position in object space
                float2 uv_MainTex : TEXCOORD0; // the uv position on the texture
                uint id : SV_VertexID; // the id of the current point in the terrain
            };

            /**
             * The output of the vertex shader and the input of the geometry shader
            */
            struct v2g{
                float4 vertex : POSITION; // vertex position in object space
                float2 uv_MainTex : TEXCOORD0; // the uv position on the texture
                float id : TEXCOORD1; // the id of the current point in the terrain
                float4 color : COLOR; // the color of the lava based on the temperature
            };

            /**
             * The output of the geometry shader and the input of the fragment shader
            */
            struct g2f{
                float4 vertex : SV_POSITION; // vertex position in world space
                float2 uv_MainTex : TEXCOORD0; // the uv position on the texture
                float3 uv : TEXCOORD1; // uv = (id, deltaY, curHeight)
                float3 normal : NORMAL; // the normal of the vertex
                float4 color : COLOR; // the color of the lava based on the temperature
            };

            /**
             * The vertex shader uses the id to create a new color and to update the terrain height
            */
            v2g vert(appdata v){
                v2g o;
                o.vertex = v.vertex;
                o.vertex.y = _TerrainHeights[v.id];
                o.id = v.id;
                o.color = fixed4(1, _TerrainTemperatures[v.id]/_ColorShade, 0, 1);
                o.uv_MainTex = v.uv_MainTex;
                return o;
            }

            /**
             * A geometry shader creating face normals
            */
            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream){
                g2f o;
                float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));

                for(int i=0; i<3; i++){
                    float id = input[i].id;
                    float deltaY = input[i].vertex.y - _InitialTerrainHeights[(uint)input[i].id];
                    float curHeight = input[i].vertex.y;

                    o.uv = float3(id, deltaY, curHeight);
                    o.uv_MainTex = input[0].uv_MainTex;
                    o.vertex = UnityObjectToClipPos(input[i].vertex);
                    o.normal = normal;
                    o.color = input[i].color;
                    triStream.Append(o);
                }
                triStream.RestartStrip();
            }

            /**
             * A fragment shader coloring fragments depending on the temperatures
            */
            fixed4 frag(g2f i) : SV_Target {
                float deltaY = i.uv.y; // difference of height between current terrain and old terrain
                bool updated = (deltaY >= 0.1f || deltaY <= -0.1f);
                bool isBrown = i.uv.z > 25;

                fixed4 green = fixed4(0.69, 0.86, 0.47, 1);
                fixed4 brown = fixed4(131.0/255, 101.0/255, 57.0/255, 1);
                fixed4 red = i.color;

                // set the correct color
                fixed4 col = green;
                if(isBrown){ // if the terrain height is above a certain value change its color from green to brown
                    col = brown;
                }

                // if St Helen use the texture instead
                if(_UseTerrainTexture != 0)
                    col = tex2D(_MainTex, float2(1-i.uv_MainTex.x, i.uv_MainTex.y));

                // if the terrain height has changed, use lava color
                if(updated) col = red;

                // light calculation using new normals
                fixed light = saturate (dot (normalize(_WorldSpaceLightPos0), i.normal));
                // don't use the normals for the lava
                if(!updated){
                    col.rgb *= light;
                }

                return col;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}