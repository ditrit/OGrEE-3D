Shader "Hidden/TriLib/BuildHDRPMask"
{
    Properties
    {
        _MetallicTex("_MetallicTex", 2D) = "black" {}
        _OcclusionTex("_OcclusionTex", 2D) = "white" {}
        _DetailMaskTex("_DetailMaskTex", 2D) = "black" {}
        _SmoothnessTex("_SmoothnessTex", 2D) = "gray" {}
    }
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

			sampler2D _MetallicTex;
			sampler2D _OcclusionTex;
			sampler2D _DetailMaskTex;
			sampler2D _SmoothnessTex;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed metallic = tex2D(_MetallicTex, i.uv).x;
				fixed occlusion = tex2D(_OcclusionTex, i.uv).x;
				fixed detail = tex2D(_DetailMaskTex, i.uv).x;
				fixed smoothness = tex2D(_SmoothnessTex, i.uv).x;
				return fixed4(metallic, occlusion, detail, smoothness);
            }
            ENDCG
        }
    }
}
