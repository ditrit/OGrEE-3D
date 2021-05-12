Shader "TriLib/StandardAlpha"
{
    Properties
    {
		//_MetallicGlossMap
		[NoScaleOffset]_MainTex("_MainTex", 2D) = "white" {}
		[HDR]_Color("_Color", Color) = (1, 1, 1, 1)
		_Cutoff("_Cutoff", Range(0, 1)) = 0.5
		[NoScaleOffset][Normal]_BumpMap("_BumpMap", 2D) = "white" {}
		_BumpScale("_BumpScale", Range(0, 1)) = 1
		[NoScaleOffset]_EmissionMap("_EmissionMap", 2D) = "white" {}
		[HDR]_EmissionColor("_EmissionColor", Color) = (0, 0, 0, 0)
		_Metallic("_Metallic", Range(0, 1)) = 0
		_Glossiness("_Glossiness", Range(0, 1)) = 0
		[NoScaleOffset]_OcclusionMap("_OcclusionMap", 2D) = "white" {}
		_OcclusionStrength("_OcclusionStrength", Float) = 1
		_Tilling("_Tilling", Vector) = (1, 1, 0, 0)
		_Offset("_Offset", Vector) = (0, 0, 0, 0)
		[NoScaleOffset]_MetallicGlossMap("_MetallicGlossMap", 2D) = "white" {}
		[NoScaleOffset]_SpecGlossMap("_SpecGlossMap", 2D) = "white" {}
		[KeywordEnum(Fade, Transparent)] _TRANSPARENCY("_TRANSPARENCY", Float) = 1
		[KeywordEnum(Standard, Specular, Roughness)] _TYPE("_TYPE", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}
		Cull Off
        LOD 200

        CGPROGRAM
#if _TRANSPARENCY_FADE
	#if _TYPE_STANDARD
		#pragma surface surf Standard fullforwardshadows alpha:fade 
	#elif _TYPE_SPECULAR
		#pragma surface surf StandardSpecular fullforwardshadows alpha:fade 
	#else
		#pragma surface surf Standard fullforwardshadows alpha:fade 
	#endif
#else
	#if _TYPE_STANDARD
		#pragma surface surf Standard fullforwardshadows alpha:blend 
	#elif _TYPE_SPECULAR
		#pragma surface surf StandardSpecular fullforwardshadows alpha:blend 
	#else
		#pragma surface surf Standard fullforwardshadows alpha:blend 
	#endif
#endif
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
			fixed facing : VFACE;
        };

		sampler2D _MainTex;
		float4 _Color;
		float _Cutoff;
		sampler2D _BumpMap;
		float _BumpScale;
		sampler2D _EmissionMap;
		float4 _EmissionColor;
		float _Metallic;
		float _Glossiness;
		sampler2D _OcclusionMap;
		float _OcclusionStrength;
		float2 _Tilling;
		float2 _Offset;
		sampler2D _MetallicGlossMap;
		sampler2D _SpecGlossMap;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float2 uv = IN.uv_MainTex + _Offset * _Tilling;
            float4 c = tex2D(_MainTex, uv) * _Color;
			if (c.a > _Cutoff) {
				discard;
			}
            o.Albedo = c.xyz;
#if _TYPE_STANDARD
			float4 glossmap = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = glossmap.r * _Metallic;
            o.Smoothness = glossmap.a * _Glossiness;
#else
			float4 specmap = tex2D(_SpecGlossMap, IN.uv_MainTex);
			o.Metallic = 0.0;
			o.Smoothness = specmap.a * _Glossiness;
#endif
			o.Occlusion = tex2D(_OcclusionMap, uv) * _OcclusionStrength;
			o.Normal = tex2D(_BumpMap, uv).xyz * _BumpScale;
			if (IN.facing < 0.5)
			{
				o.Normal *= -1.0;
			}
			o.Emission = tex2D(_EmissionMap, uv).xyz * _EmissionColor.xyz;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
