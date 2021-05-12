Shader "Hidden/TriLib/BuildSimpleMetallicMap"
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
			float _DefaultMetallic;
            float _DefaultSmoothness;
            int _HasSmoothnessTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed smoothness = _HasSmoothnessTex == 1 ? tex2D(_MainTex, i.uv).g : _DefaultSmoothness;
				return float4(_DefaultMetallic, _DefaultMetallic, _DefaultMetallic, smoothness);
            }
            ENDCG
        }
    }
}
