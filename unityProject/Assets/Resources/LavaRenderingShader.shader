Shader "Custom/LavaRenderingShader"{
    Properties{
    }
    SubShader{
 
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
 
            #include "UnityCG.cginc"
 
            struct appdata{
				float4 vertex : POSITION;
			};
			
			struct v2g{
				float4 objPos : SV_POSITION;
			};
			
			struct g2f{
				float4 worldPos : SV_POSITION;
				fixed4 col : COLOR;
			};
 
            v2g vert (appdata v){
                v2g o;
                o.objPos = v.vertex;
                return o;
            }
 
            [maxvertexcount(6)]
            void geom(point v2g input[1], inout TriangleStream<g2f> triStream){
                g2f o;
				float length = 0.5f;
				float4 pos;

				// first triangle
				// bottom left
				pos = float4(input[0].objPos.x-length, input[0].objPos.y, input[0].objPos.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.col = fixed4(1,0,0,1);
				triStream.Append(o);
				// top left
				pos = float4(input[0].objPos.x-length, input[0].objPos.y, input[0].objPos.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.col = fixed4(0,1,0,1);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].objPos.x+length, input[0].objPos.y, input[0].objPos.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.col = fixed4(0,0,1,1);
				triStream.Append(o);
				triStream.RestartStrip();

				// second triangle
				// top left
				pos = float4(input[0].objPos.x-length, input[0].objPos.y, input[0].objPos.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.col = fixed4(0,1,0,1);
				triStream.Append(o);
				// top right
				pos = float4(input[0].objPos.x+length, input[0].objPos.y, input[0].objPos.z+length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.col = fixed4(0,1,1,1);
				triStream.Append(o);
				// bottom right
				pos = float4(input[0].objPos.x+length, input[0].objPos.y, input[0].objPos.z-length, 1);
				o.worldPos = UnityObjectToClipPos(pos);
				o.col = fixed4(0,0,1,1);
				triStream.Append(o);
				triStream.RestartStrip();

            }
 
            fixed4 frag (g2f i) : SV_Target{
                fixed4 col = i.col;
				return col;
				// return fixed4(0,0,0,1);
            }

            ENDCG
		}
    }
    FallBack "Diffuse"
}
