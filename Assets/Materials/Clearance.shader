// Complex Lit is superset of Lit, but provides
// advanced material properties and is always forward rendered.
// It also has higher hardware and shader model requirements.
Shader "Custom/Clearance"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Thickness("Thickness", Range(0.001, 2)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            // -------------------------------------
            // Render State Commands
            //ZTest Always
            Cull false
            
            HLSLPROGRAM 

            // -------------------------------------
            // Shader Stages
            #pragma vertex Vertex
            #pragma fragment Fragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON


            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

            float _Thickness;

            struct VertOut
            {
                float2 uv         : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            struct VertIn
            {
                float4 positionOS : POSITION;
                float2 texcoord   : TEXCOORD0;
            };

            VertOut Vertex(VertIn input)
            {
                VertOut output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = vertexInput.positionCS;

                return output;
            }

            float4 Fragment(VertOut input) : SV_Target0
            {
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData);

                half4 color= _BaseColor;
                color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));

                return color;
            }
            ENDHLSL
        }
    }
}
