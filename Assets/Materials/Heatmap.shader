﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Alan Zucconi
// www.alanzucconi.com
Shader "Hidden/Heatmap" 
{
	Properties
	{
		_HeatTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha // Alpha blend

		Pass
		{
			CGPROGRAM
			#pragma	vertex vert             
			#pragma	fragment frag

			struct vertInput {
				float4 pos : POSITION;
			};

			struct vertOutput {
				float4 pos : POSITION;
				fixed3 worldPos : TEXCOORD1;
				fixed3 objPos : TEXCOORD2;
			};

			vertOutput vert(vertInput input)
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.worldPos = mul(unity_ObjectToWorld, input.pos).xyz;
				o.objPos =input.pos.xyz;
				return o;
			}

			uniform int _Points_Length = 0;
			uniform float4 _Points[100];        // (x, y, z) = position
			uniform float4 _Properties[100];    // x = radius, y = intensity

			sampler2D _HeatTex;

			half4 frag(vertOutput output) : COLOR
			{
				// Loops over all the points
				half h = 0;
				half maxValueDistance = -100;
				half coefMax = 0;
				half minValue = 100;
				half totDistance = 0;
				for (int i = 0; i < _Points_Length; i++)
				{
					// Calculates the contribution of each point
					half di = distance(output.worldPos, _Points[i].xyz);

					half ri = _Properties[i].x;
					half hi =saturate(1- di/ri);
	
					if (hi * _Properties[i].y > maxValueDistance) {
						maxValueDistance = hi * _Properties[i].y;
					}
					if (hi > 0) {
						totDistance += 1;
					}
					h += hi * _Properties[i].y;
				}
				// Converts (0-1) according to the heat texture
				/*if (totDistance > 0) {
					h = saturate(h / totDistance);
				}
				*/
				h = saturate(maxValueDistance);
				h = saturate(h);
				half4 color = tex2D(_HeatTex, fixed2(h, 0.5));
				return color;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}