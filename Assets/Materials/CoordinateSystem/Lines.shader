Shader "Custom/Lines"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
		_Color("First Line Color", Color) = (1,1,1,1)

		[MaterialToggle] _FirstLineToggled("Toggled", Float) = 0
		_FirstLineThickness("First Line Thickness", float) = 0.2
		_FirstLine("First Line", Vector) = (0,0,1,1) // (xa,ya,xb,yb)

		[MaterialToggle] _SecondLineToggled("Toggled", Float) = 0
		_SecondLineThickness("Second Line Thickness", float) = 0.2
		_SecondLine("Second Line", Vector) = (0,1,1,0) // (xa,ya,xb,yb)

		[MaterialToggle] _ThirdLineToggled("Toggled", Float) = 0
		_ThirdLineThickness("Third Line Thickness", float) = 0.2
		_ThirdLine("Third Line", Vector) = (0,1,1,0) // (xa,ya,xb,yb)

		[MaterialToggle] _FourthLineToggled("Toggled", Float) = 0
		_FourthLineThickness("Fourth Line Thickness", float) = 0.2
		_FourthLine("Fourth Line", Vector) = (0,1,1,0) // (xa,ya,xb,yb)
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" }
		Pass{ 
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blend
			Cull[_Cull]
			ZTest [_ZTest]
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
				o.objPos = input.pos.xyz;
				return o;
			}

		fixed4 _Color;

		float _FirstLineThickness;
		float4 _FirstLine;
		float _FirstLineToggled;

		float _SecondLineToggled;
		float _SecondLineThickness;
		float4 _SecondLine;

		float _ThirdLineToggled;
		float _ThirdLineThickness;
		float4 _ThirdLine;

		float _FourthLineToggled;
		float _FourthLineThickness;
		float4 _FourthLine;

		static const float4 lines[4] = { _FirstLine, _SecondLine, _ThirdLine, _FourthLine };
		static const float thicknesses[4] = {_FirstLineThickness, _SecondLineThickness, _ThirdLineThickness, _FourthLineThickness};
		static const float toggles[4] = { _FirstLineToggled, _SecondLineToggled, _ThirdLineToggled, _FourthLineToggled };

		half4 frag(vertOutput output) : SV_Target0
		{
			float3 worldScale = float3(
				length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
				length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
				length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
				);
			float2 coords = (output.objPos.xy + float2(0.5,0.5)) * worldScale;
			float2 firstPoint;
			float2 secondPoint;
			float distance;
			half alpha = 0.0;
			fixed3 color = fixed3(0, 0, 0);

			for (int i = 0; i < 4; i++)
			{
				if (toggles[i] == 0)
				{
					continue;
				}
				firstPoint = lines[i].xy * worldScale;
				secondPoint = lines[i].zw * worldScale;
				if (firstPoint.x == secondPoint.x)
				{
					distance = abs(coords.x - firstPoint.x);
				}
				else // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_an_equation
				{
					distance = abs((secondPoint.y - firstPoint.y) * coords.x + (firstPoint.x - secondPoint.x) * coords.y + (secondPoint.x - firstPoint.x) * firstPoint.y - (secondPoint.y - firstPoint.y) * firstPoint.x) / sqrt(pow(secondPoint.y - firstPoint.y, 2) + pow(firstPoint.x - secondPoint.x, 2));
				}

				if (distance < thicknesses[i])
				{
					alpha = 1;
					color = _Color;
				}
			}
			return half4(color.xyz, alpha);
	}
		ENDCG
	}
	}
}
