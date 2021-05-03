Shader "Hidden/TriLib/SwapChannelData"
{
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
			int _ChannelIndexA;
			int _ChannelIndexB;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed a = col[_ChannelIndexA];
				fixed b = col[_ChannelIndexB];
				switch (_ChannelIndexA) {
				case 0:
					col.x = b;
					break;
				case 1:
					col.y = b;
					break;
				case 2:
					col.z = b;
					break;
				case 3:
					col.w = b;
					break;
				}
				switch (_ChannelIndexB) {
				case 0:
					col.x = a;
					break;
				case 1:
					col.y = a;
					break;
				case 2:
					col.z = a;
					break;
				case 3:
					col.w = a;
					break;
				}
                return col;
            }
            ENDCG
        }
    }
}
