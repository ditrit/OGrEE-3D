Shader "Hidden/TriLib/BuildSpecRoughnessMap"
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

            sampler2D _RoughnessTexture;
			sampler2D _SpecularTexture;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 roughness = tex2D(_RoughnessTexture, i.uv);
				fixed4 specular = tex2D(_SpecularTexture, i.uv);
				return fixed4(specular.xyz, roughness.x);
            }
            ENDCG
        }
    }
}
